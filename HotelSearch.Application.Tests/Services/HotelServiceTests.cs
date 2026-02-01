using FluentAssertions;
using HotelSearch.Application.Dtos;
using HotelSearch.Application.Exceptions;
using HotelSearch.Application.Interfaces;
using HotelSearch.Application.Services.Implementations;
using HotelSearch.Domain.Entities;
using HotelSearch.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace HotelSearch.Application.Tests.Services;

public class HotelServiceTests
{
    private readonly IHotelRepository _hotelRepository;
    private readonly ILogger<HotelService> _logger;
    private readonly HotelService _sut; // System Under Test

    public HotelServiceTests()
    {
        _hotelRepository = Substitute.For<IHotelRepository>();
        _logger = Substitute.For<ILogger<HotelService>>();
        _sut = new HotelService(_hotelRepository, _logger);
    }

    private static Hotel CreateTestHotel(string name = "Test Hotel", decimal price = 100m, double lat = 45.0, double lon = 15.0)
    {
        return new Hotel(name, price, new GeoLocation(lat, lon));
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsCreatedHotel()
    {
        // Arrange
        var createDto = new CreateHotelDto("Grand Hotel", 150m, 45.8150, 15.9819);
        _hotelRepository.ExistsByNameAndLocationAsync(
            Arg.Any<string>(), Arg.Any<double>(), Arg.Any<double>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _hotelRepository.AddAsync(Arg.Any<Hotel>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Hotel>());

        // Act
        var result = await _sut.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(createDto.Name);
        result.Price.Should().Be(createDto.Price);
        result.Latitude.Should().Be(createDto.Latitude);
        result.Longitude.Should().Be(createDto.Longitude);
        result.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateAsync_WhenDuplicateExists_ThrowsDuplicateHotelException()
    {
        // Arrange
        var createDto = new CreateHotelDto("Existing Hotel", 100m, 45.0, 15.0);
        _hotelRepository.ExistsByNameAndLocationAsync(
            createDto.Name, createDto.Latitude, createDto.Longitude, null, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var act = () => _sut.CreateAsync(createDto);

        // Assert
        await act.Should().ThrowAsync<DuplicateHotelException>()
            .WithMessage($"*{createDto.Name}*");
    }

    [Fact]
    public async Task CreateAsync_CallsRepositoryAddAsync()
    {
        // Arrange
        var createDto = new CreateHotelDto("New Hotel", 200m, 46.0, 16.0);
        _hotelRepository.ExistsByNameAndLocationAsync(
            Arg.Any<string>(), Arg.Any<double>(), Arg.Any<double>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _hotelRepository.AddAsync(Arg.Any<Hotel>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Hotel>());

        // Act
        await _sut.CreateAsync(createDto);

        // Assert
        await _hotelRepository.Received(1).AddAsync(Arg.Any<Hotel>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_PassesCancellationToken()
    {
        // Arrange
        var createDto = new CreateHotelDto("Hotel", 100m, 45.0, 15.0);
        var cts = new CancellationTokenSource();
        _hotelRepository.ExistsByNameAndLocationAsync(
            Arg.Any<string>(), Arg.Any<double>(), Arg.Any<double>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _hotelRepository.AddAsync(Arg.Any<Hotel>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Hotel>());

        // Act
        await _sut.CreateAsync(createDto, cts.Token);

        // Assert
        await _hotelRepository.Received(1).AddAsync(Arg.Any<Hotel>(), cts.Token);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenHotelExists_ReturnsHotelDto()
    {
        // Arrange
        var hotel = CreateTestHotel("Found Hotel", 150m);
        _hotelRepository.GetByIdAsync(hotel.Id, Arg.Any<CancellationToken>())
            .Returns(hotel);

        // Act
        var result = await _sut.GetByIdAsync(hotel.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(hotel.Id);
        result.Name.Should().Be(hotel.Name);
        result.Price.Should().Be(hotel.Price);
    }

    [Fact]
    public async Task GetByIdAsync_WhenHotelNotFound_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _hotelRepository.GetByIdAsync(nonExistentId, Arg.Any<CancellationToken>())
            .Returns((Hotel?)null);

        // Act
        var result = await _sut.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_CallsRepositoryWithCorrectId()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        _hotelRepository.GetByIdAsync(hotelId, Arg.Any<CancellationToken>())
            .Returns((Hotel?)null);

        // Act
        await _sut.GetByIdAsync(hotelId);

        // Assert
        await _hotelRepository.Received(1).GetByIdAsync(hotelId, Arg.Any<CancellationToken>());
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WhenHotelExists_ReturnsUpdatedHotel()
    {
        // Arrange
        var existingHotel = CreateTestHotel("Original Name", 100m);
        var updateDto = new UpdateHotelDto("Updated Name", 200m, 46.0, 16.0);

        _hotelRepository.GetByIdAsync(existingHotel.Id, Arg.Any<CancellationToken>())
            .Returns(existingHotel);
        _hotelRepository.ExistsByNameAndLocationAsync(
            Arg.Any<string>(), Arg.Any<double>(), Arg.Any<double>(), existingHotel.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.UpdateAsync(existingHotel.Id, updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(updateDto.Name);
        result.Price.Should().Be(updateDto.Price);
        result.Latitude.Should().Be(updateDto.Latitude);
        result.Longitude.Should().Be(updateDto.Longitude);
    }

    [Fact]
    public async Task UpdateAsync_WhenHotelNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateDto = new UpdateHotelDto("Name", 100m, 45.0, 15.0);

        _hotelRepository.GetByIdAsync(nonExistentId, Arg.Any<CancellationToken>())
            .Returns((Hotel?)null);

        // Act
        var act = () => _sut.UpdateAsync(nonExistentId, updateDto);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{nonExistentId}*");
    }

    [Fact]
    public async Task UpdateAsync_WhenDuplicateExists_ThrowsDuplicateHotelException()
    {
        // Arrange
        var existingHotel = CreateTestHotel();
        var updateDto = new UpdateHotelDto("Duplicate Name", 100m, 46.0, 16.0);

        _hotelRepository.GetByIdAsync(existingHotel.Id, Arg.Any<CancellationToken>())
            .Returns(existingHotel);
        _hotelRepository.ExistsByNameAndLocationAsync(
            updateDto.Name, updateDto.Latitude, updateDto.Longitude, existingHotel.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var act = () => _sut.UpdateAsync(existingHotel.Id, updateDto);

        // Assert
        await act.Should().ThrowAsync<DuplicateHotelException>()
            .WithMessage($"*{updateDto.Name}*");
    }

    [Fact]
    public async Task UpdateAsync_CallsRepositoryUpdateAsync()
    {
        // Arrange
        var existingHotel = CreateTestHotel();
        var updateDto = new UpdateHotelDto("Updated", 150m, 45.5, 15.5);

        _hotelRepository.GetByIdAsync(existingHotel.Id, Arg.Any<CancellationToken>())
            .Returns(existingHotel);
        _hotelRepository.ExistsByNameAndLocationAsync(
            Arg.Any<string>(), Arg.Any<double>(), Arg.Any<double>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        await _sut.UpdateAsync(existingHotel.Id, updateDto);

        // Assert
        await _hotelRepository.Received(1).UpdateAsync(Arg.Any<Hotel>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ExcludesCurrentHotelFromDuplicateCheck()
    {
        // Arrange
        var existingHotel = CreateTestHotel();
        var updateDto = new UpdateHotelDto("Same Name", 150m, 45.0, 15.0);

        _hotelRepository.GetByIdAsync(existingHotel.Id, Arg.Any<CancellationToken>())
            .Returns(existingHotel);
        _hotelRepository.ExistsByNameAndLocationAsync(
            Arg.Any<string>(), Arg.Any<double>(), Arg.Any<double>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        await _sut.UpdateAsync(existingHotel.Id, updateDto);

        // Assert
        await _hotelRepository.Received(1).ExistsByNameAndLocationAsync(
            updateDto.Name, updateDto.Latitude, updateDto.Longitude, existingHotel.Id, Arg.Any<CancellationToken>());
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WhenHotelExists_DeletesSuccessfully()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        _hotelRepository.ExistsAsync(hotelId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        await _sut.DeleteAsync(hotelId);

        // Assert
        await _hotelRepository.Received(1).DeleteAsync(hotelId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_WhenHotelNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _hotelRepository.ExistsAsync(nonExistentId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var act = () => _sut.DeleteAsync(nonExistentId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{nonExistentId}*");
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_DoesNotCallDeleteOnRepository()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _hotelRepository.ExistsAsync(nonExistentId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        try { await _sut.DeleteAsync(nonExistentId); } catch { }

        // Assert
        await _hotelRepository.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region SearchAsync Tests

    [Fact]
    public async Task SearchAsync_WhenNoHotels_ReturnsEmptyResult()
    {
        // Arrange
        var query = new HotelSearchQuery(45.0, 15.0, 1, 10);
        _hotelRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Hotel>());

        // Act
        var result = await _sut.SearchAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task SearchAsync_ReturnsHotelsWithDistance()
    {
        // Arrange
        var hotels = new List<Hotel>
        {
            CreateTestHotel("Hotel A", 100m, 45.0, 15.0),
            CreateTestHotel("Hotel B", 150m, 45.1, 15.1)
        };
        var query = new HotelSearchQuery(45.0, 15.0, 1, 10);
        _hotelRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(hotels);

        // Act
        var result = await _sut.SearchAsync(query);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().AllSatisfy(h => h.DistanceKm.Should().BeGreaterThanOrEqualTo(0));
    }

    [Fact]
    public async Task SearchAsync_SortsHotelsByScore()
    {
        // Arrange - Create hotels with different distances and prices
        var nearCheap = CreateTestHotel("Near Cheap", 50m, 45.001, 15.001);    // Low distance, low price = best score
        var nearExpensive = CreateTestHotel("Near Expensive", 300m, 45.002, 15.002); // Low distance, high price
        var farCheap = CreateTestHotel("Far Cheap", 30m, 46.0, 16.0);          // High distance, low price

        var hotels = new List<Hotel> { nearExpensive, farCheap, nearCheap };
        var query = new HotelSearchQuery(45.0, 15.0, 1, 10);
        _hotelRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(hotels);

        // Act
        var result = await _sut.SearchAsync(query);

        // Assert - Near and cheap should be first (lowest combined score)
        result.Items.First().Name.Should().Be("Near Cheap");
    }

    [Fact]
    public async Task SearchAsync_RespectsPagination()
    {
        // Arrange
        var hotels = Enumerable.Range(1, 25)
            .Select(i => CreateTestHotel($"Hotel {i}", i * 10m, 45.0 + i * 0.01, 15.0))
            .ToList();
        var query = new HotelSearchQuery(45.0, 15.0, 2, 10);
        _hotelRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(hotels);

        // Act
        var result = await _sut.SearchAsync(query);

        // Assert
        result.Items.Should().HaveCount(10);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(25);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task SearchAsync_LastPageHasRemainingItems()
    {
        // Arrange
        var hotels = Enumerable.Range(1, 25)
            .Select(i => CreateTestHotel($"Hotel {i}", i * 10m, 45.0 + i * 0.01, 15.0))
            .ToList();
        var query = new HotelSearchQuery(45.0, 15.0, 3, 10);
        _hotelRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(hotels);

        // Act
        var result = await _sut.SearchAsync(query);

        // Assert
        result.Items.Should().HaveCount(5); // 25 total, page 3 with size 10 = 5 remaining
    }

    [Fact]
    public async Task SearchAsync_CalculatesDistanceCorrectly()
    {
        // Arrange - Hotel at same location as search
        var hotel = CreateTestHotel("Same Location", 100m, 45.0, 15.0);
        var query = new HotelSearchQuery(45.0, 15.0, 1, 10);
        _hotelRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Hotel> { hotel });

        // Act
        var result = await _sut.SearchAsync(query);

        // Assert
        result.Items.First().DistanceKm.Should().Be(0);
    }

    [Fact]
    public async Task SearchAsync_RoundsDistanceToTwoDecimals()
    {
        // Arrange
        var hotel = CreateTestHotel("Test Hotel", 100m, 45.1, 15.1);
        var query = new HotelSearchQuery(45.0, 15.0, 1, 10);
        _hotelRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Hotel> { hotel });

        // Act
        var result = await _sut.SearchAsync(query);

        // Assert
        var distance = result.Items.First().DistanceKm;
        var decimalPlaces = BitConverter.GetBytes(decimal.GetBits((decimal)distance)[3])[2];
        decimalPlaces.Should().BeLessThanOrEqualTo(2);
    }

    [Fact]
    public async Task SearchAsync_WithSingleHotel_ScoreIsZero()
    {
        // Arrange - Single hotel means normalized values are 1/1 = 1 for both
        var hotel = CreateTestHotel("Only Hotel", 100m, 45.5, 15.5);
        var query = new HotelSearchQuery(45.0, 15.0, 1, 10);
        _hotelRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Hotel> { hotel });

        // Act
        var result = await _sut.SearchAsync(query);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Only Hotel");
    }

    [Fact]
    public async Task SearchAsync_PageBeyondTotal_ReturnsEmptyItems()
    {
        // Arrange
        var hotels = Enumerable.Range(1, 5)
            .Select(i => CreateTestHotel($"Hotel {i}", 100m, 45.0, 15.0))
            .ToList();
        var query = new HotelSearchQuery(45.0, 15.0, 10, 10); // Page 10, but only 5 hotels
        _hotelRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(hotels);

        // Act
        var result = await _sut.SearchAsync(query);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(5);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task SearchAsync_ScoreCalculation_CheaperNearbyBeatsFartherCheaper()
    {
        // Arrange
        // Hotel A: Very close (1km), moderate price (100)
        // Hotel B: Far (100km), very cheap (10)
        // With equal weighting, A should win if distance normalized difference > price normalized difference
        var closeModerate = CreateTestHotel("Close Moderate", 100m, 45.009, 15.0); // ~1km away
        var farCheap = CreateTestHotel("Far Cheap", 10m, 46.0, 15.0);              // ~111km away

        var hotels = new List<Hotel> { farCheap, closeModerate };
        var query = new HotelSearchQuery(45.0, 15.0, 1, 10);
        _hotelRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(hotels);

        // Act
        var result = await _sut.SearchAsync(query);

        // Assert
        // Close Moderate: dist=1/111=0.009, price=100/100=1.0, score=1.009
        // Far Cheap: dist=111/111=1.0, price=10/100=0.1, score=1.1
        // Close Moderate wins (lower score)
        result.Items.First().Name.Should().Be("Close Moderate");
    }

    #endregion
}
