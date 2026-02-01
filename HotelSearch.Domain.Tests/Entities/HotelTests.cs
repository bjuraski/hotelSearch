using FluentAssertions;
using HotelSearch.Domain.Entities;
using HotelSearch.Domain.ValueObjects;
using Xunit;

namespace HotelSearch.Domain.Tests.Entities;

public class HotelTests
{
    private static GeoLocation ValidLocation => new(45.8150, 15.9819);

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidInputs_CreatesHotel()
    {
        // Arrange
        var name = "Grand Hotel";
        var price = 150.00m;
        var location = ValidLocation;

        // Act
        var hotel = new Hotel(name, price, location);

        // Assert
        hotel.Should().NotBeNull();
        hotel.Id.Should().NotBeEmpty();
        hotel.Name.Should().Be(name);
        hotel.Price.Should().Be(price);
        hotel.Location.Should().Be(location);
        hotel.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        hotel.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithZeroPrice_CreatesHotel()
    {
        // Arrange
        var name = "Free Hotel";
        var price = 0m;

        // Act
        var hotel = new Hotel(name, price, ValidLocation);

        // Assert
        hotel.Price.Should().Be(0m);
    }

    [Fact]
    public void Constructor_WithHighPrice_CreatesHotel()
    {
        // Arrange
        var name = "Luxury Hotel";
        var price = 10000.99m;

        // Act
        var hotel = new Hotel(name, price, ValidLocation);

        // Assert
        hotel.Price.Should().Be(price);
    }

    [Fact]
    public void Constructor_GeneratesUniqueIds()
    {
        // Arrange & Act
        var hotel1 = new Hotel("Hotel 1", 100m, ValidLocation);
        var hotel2 = new Hotel("Hotel 2", 100m, ValidLocation);

        // Assert
        hotel1.Id.Should().NotBe(hotel2.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Constructor_WithInvalidName_ThrowsArgumentException(string? invalidName)
    {
        // Act
        var act = () => new Hotel(invalidName!, 100m, ValidLocation);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("name")
            .WithMessage("*Hotel name cannot be empty*");
    }

    [Fact]
    public void Constructor_WithNegativePrice_ThrowsArgumentException()
    {
        // Act
        var act = () => new Hotel("Hotel", -1m, ValidLocation);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("price")
            .WithMessage("*Hotel price cannot be negative*");
    }

    [Fact]
    public void Constructor_WithNullLocation_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new Hotel("Hotel", 100m, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("location");
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithValidInputs_UpdatesHotelProperties()
    {
        // Arrange
        var hotel = new Hotel("Original Name", 100m, new GeoLocation(45.0, 15.0));
        var newName = "Updated Name";
        var newPrice = 200m;
        var newLocation = new GeoLocation(46.0, 16.0);

        // Act
        hotel.Update(newName, newPrice, newLocation);

        // Assert
        hotel.Name.Should().Be(newName);
        hotel.Price.Should().Be(newPrice);
        hotel.Location.Should().Be(newLocation);
        hotel.UpdatedAt.Should().NotBeNull();
        hotel.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Update_PreservesOriginalId()
    {
        // Arrange
        var hotel = new Hotel("Original", 100m, ValidLocation);
        var originalId = hotel.Id;

        // Act
        hotel.Update("Updated", 200m, new GeoLocation(46.0, 16.0));

        // Assert
        hotel.Id.Should().Be(originalId);
    }

    [Fact]
    public void Update_PreservesCreatedAt()
    {
        // Arrange
        var hotel = new Hotel("Original", 100m, ValidLocation);
        var originalCreatedAt = hotel.CreatedAt;

        // Act
        hotel.Update("Updated", 200m, new GeoLocation(46.0, 16.0));

        // Assert
        hotel.CreatedAt.Should().Be(originalCreatedAt);
    }

    [Fact]
    public void Update_CalledMultipleTimes_UpdatesUpdatedAtEachTime()
    {
        // Arrange
        var hotel = new Hotel("Original", 100m, ValidLocation);

        // Act
        hotel.Update("First Update", 150m, ValidLocation);
        var firstUpdateTime = hotel.UpdatedAt;

        Thread.Sleep(10); // Small delay to ensure different timestamps

        hotel.Update("Second Update", 200m, ValidLocation);
        var secondUpdateTime = hotel.UpdatedAt;

        // Assert
        firstUpdateTime.Should().NotBeNull();
        secondUpdateTime.Should().NotBeNull();
        secondUpdateTime.Should().BeOnOrAfter(firstUpdateTime.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidName_ThrowsArgumentException(string? invalidName)
    {
        // Arrange
        var hotel = new Hotel("Original", 100m, ValidLocation);

        // Act
        var act = () => hotel.Update(invalidName!, 100m, ValidLocation);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("name")
            .WithMessage("*Hotel name cannot be empty*");
    }

    [Fact]
    public void Update_WithNegativePrice_ThrowsArgumentException()
    {
        // Arrange
        var hotel = new Hotel("Original", 100m, ValidLocation);

        // Act
        var act = () => hotel.Update("Updated", -1m, ValidLocation);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("price")
            .WithMessage("*Hotel price cannot be negative*");
    }

    [Fact]
    public void Update_WithNullLocation_ThrowsArgumentNullException()
    {
        // Arrange
        var hotel = new Hotel("Original", 100m, ValidLocation);

        // Act
        var act = () => hotel.Update("Updated", 100m, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("location");
    }

    [Fact]
    public void Update_WithSameValues_StillUpdatesTimestamp()
    {
        // Arrange
        var hotel = new Hotel("Hotel", 100m, ValidLocation);

        // Act
        hotel.Update("Hotel", 100m, ValidLocation);

        // Assert
        hotel.UpdatedAt.Should().NotBeNull();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Constructor_WithVeryLongName_CreatesHotel()
    {
        // Arrange
        var longName = new string('A', 500);

        // Act
        var hotel = new Hotel(longName, 100m, ValidLocation);

        // Assert
        hotel.Name.Should().Be(longName);
    }

    [Fact]
    public void Constructor_WithSpecialCharactersInName_CreatesHotel()
    {
        // Arrange
        var specialName = "Hôtel & Café 日本語 🏨";

        // Act
        var hotel = new Hotel(specialName, 100m, ValidLocation);

        // Assert
        hotel.Name.Should().Be(specialName);
    }

    [Fact]
    public void Constructor_WithDecimalPrice_PreservesPrecision()
    {
        // Arrange
        var precisePrice = 99.99999m;

        // Act
        var hotel = new Hotel("Hotel", precisePrice, ValidLocation);

        // Assert
        hotel.Price.Should().Be(precisePrice);
    }

    #endregion
}
