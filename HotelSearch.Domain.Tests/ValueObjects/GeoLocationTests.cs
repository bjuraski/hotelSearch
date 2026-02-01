using FluentAssertions;
using HotelSearch.Domain.ValueObjects;
using Xunit;

namespace HotelSearch.Domain.Tests.ValueObjects;

public class GeoLocationTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidCoordinates_CreatesGeoLocation()
    {
        // Arrange
        var latitude = 45.8150;
        var longitude = 15.9819;

        // Act
        var location = new GeoLocation(latitude, longitude);

        // Assert
        location.Latitude.Should().Be(latitude);
        location.Longitude.Should().Be(longitude);
    }

    [Theory]
    [InlineData(0, 0)]           // Equator and Prime Meridian
    [InlineData(90, 0)]          // North Pole
    [InlineData(-90, 0)]         // South Pole
    [InlineData(0, 180)]         // Date Line (East)
    [InlineData(0, -180)]        // Date Line (West)
    [InlineData(90, 180)]        // Extreme NE
    [InlineData(-90, -180)]      // Extreme SW
    public void Constructor_WithBoundaryCoordinates_CreatesGeoLocation(double latitude, double longitude)
    {
        // Act
        var location = new GeoLocation(latitude, longitude);

        // Assert
        location.Latitude.Should().Be(latitude);
        location.Longitude.Should().Be(longitude);
    }

    [Theory]
    [InlineData(90.001)]
    [InlineData(91)]
    [InlineData(100)]
    [InlineData(double.MaxValue)]
    public void Constructor_WithLatitudeAbove90_ThrowsArgumentException(double invalidLatitude)
    {
        // Act
        var act = () => new GeoLocation(invalidLatitude, 0);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("latitude")
            .WithMessage("*Latitude must be between -90 and 90 degrees*");
    }

    [Theory]
    [InlineData(-90.001)]
    [InlineData(-91)]
    [InlineData(-100)]
    [InlineData(double.MinValue)]
    public void Constructor_WithLatitudeBelow90_ThrowsArgumentException(double invalidLatitude)
    {
        // Act
        var act = () => new GeoLocation(invalidLatitude, 0);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("latitude")
            .WithMessage("*Latitude must be between -90 and 90 degrees*");
    }

    [Theory]
    [InlineData(180.001)]
    [InlineData(181)]
    [InlineData(200)]
    [InlineData(double.MaxValue)]
    public void Constructor_WithLongitudeAbove180_ThrowsArgumentException(double invalidLongitude)
    {
        // Act
        var act = () => new GeoLocation(0, invalidLongitude);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("longitude")
            .WithMessage("*Longitude must be between -180 and 180 degrees*");
    }

    [Theory]
    [InlineData(-180.001)]
    [InlineData(-181)]
    [InlineData(-200)]
    [InlineData(double.MinValue)]
    public void Constructor_WithLongitudeBelow180_ThrowsArgumentException(double invalidLongitude)
    {
        // Act
        var act = () => new GeoLocation(0, invalidLongitude);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("longitude")
            .WithMessage("*Longitude must be between -180 and 180 degrees*");
    }

    #endregion

    #region DistanceTo Tests

    [Fact]
    public void DistanceTo_SameLocation_ReturnsZero()
    {
        // Arrange
        var location = new GeoLocation(45.8150, 15.9819);

        // Act
        var distance = location.DistanceTo(location);

        // Assert
        distance.Should().Be(0);
    }

    [Fact]
    public void DistanceTo_NullLocation_ThrowsArgumentNullException()
    {
        // Arrange
        var location = new GeoLocation(45.0, 15.0);

        // Act
        var act = () => location.DistanceTo(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("other");
    }

    [Fact]
    public void DistanceTo_KnownDistance_ZagrebToSplit()
    {
        // Arrange - Zagreb to Split is approximately 270-280 km
        var zagreb = new GeoLocation(45.8150, 15.9819);
        var split = new GeoLocation(43.5081, 16.4402);

        // Act
        var distance = zagreb.DistanceTo(split);

        // Assert
        distance.Should().BeInRange(255, 260); // Haversine gives ~257 km
    }

    [Fact]
    public void DistanceTo_KnownDistance_LondonToNewYork()
    {
        // Arrange - London to New York is approximately 5570 km
        var london = new GeoLocation(51.5074, -0.1278);
        var newYork = new GeoLocation(40.7128, -74.0060);

        // Act
        var distance = london.DistanceTo(newYork);

        // Assert
        distance.Should().BeInRange(5550, 5600);
    }

    [Fact]
    public void DistanceTo_EquatorCircumference_QuarterEarth()
    {
        // Arrange - Quarter of Earth's circumference along equator (~10,000 km)
        var point1 = new GeoLocation(0, 0);
        var point2 = new GeoLocation(0, 90);

        // Act
        var distance = point1.DistanceTo(point2);

        // Assert
        distance.Should().BeInRange(10000, 10020);
    }

    [Fact]
    public void DistanceTo_PoleToEquator_QuarterMeridian()
    {
        // Arrange - North Pole to Equator (~10,000 km)
        var northPole = new GeoLocation(90, 0);
        var equator = new GeoLocation(0, 0);

        // Act
        var distance = northPole.DistanceTo(equator);

        // Assert
        distance.Should().BeInRange(10000, 10020);
    }

    [Fact]
    public void DistanceTo_IsSymmetric()
    {
        // Arrange
        var locationA = new GeoLocation(45.8150, 15.9819);
        var locationB = new GeoLocation(43.5081, 16.4402);

        // Act
        var distanceAtoB = locationA.DistanceTo(locationB);
        var distanceBtoA = locationB.DistanceTo(locationA);

        // Assert
        distanceAtoB.Should().BeApproximately(distanceBtoA, 0.0001);
    }

    [Fact]
    public void DistanceTo_ShortDistance_HighPrecision()
    {
        // Arrange - Two points 1km apart (approximately)
        var point1 = new GeoLocation(45.0, 15.0);
        var point2 = new GeoLocation(45.009, 15.0); // ~1km north

        // Act
        var distance = point1.DistanceTo(point2);

        // Assert
        distance.Should().BeInRange(0.9, 1.1);
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_SameCoordinates_ReturnsTrue()
    {
        // Arrange
        var location1 = new GeoLocation(45.8150, 15.9819);
        var location2 = new GeoLocation(45.8150, 15.9819);

        // Act & Assert
        location1.Equals(location2).Should().BeTrue();
        (location1 == location2).Should().BeTrue();
        (location1 != location2).Should().BeFalse();
    }

    [Fact]
    public void Equals_DifferentLatitude_ReturnsFalse()
    {
        // Arrange
        var location1 = new GeoLocation(45.8150, 15.9819);
        var location2 = new GeoLocation(45.8151, 15.9819);

        // Act & Assert
        location1.Equals(location2).Should().BeFalse();
        (location1 == location2).Should().BeFalse();
        (location1 != location2).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentLongitude_ReturnsFalse()
    {
        // Arrange
        var location1 = new GeoLocation(45.8150, 15.9819);
        var location2 = new GeoLocation(45.8150, 15.9820);

        // Act & Assert
        location1.Equals(location2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithinTolerance_ReturnsTrue()
    {
        // Arrange - Coordinates within 0.000001 tolerance
        var location1 = new GeoLocation(45.8150000, 15.9819000);
        var location2 = new GeoLocation(45.81500009, 15.98190009);

        // Act & Assert
        location1.Equals(location2).Should().BeTrue();
    }

    [Fact]
    public void Equals_Null_ReturnsFalse()
    {
        // Arrange
        var location = new GeoLocation(45.0, 15.0);

        // Act & Assert
        location.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Equals_SameReference_ReturnsTrue()
    {
        // Arrange
        var location = new GeoLocation(45.0, 15.0);

        // Act & Assert
        location.Equals(location).Should().BeTrue();
        ReferenceEquals(location, location).Should().BeTrue();
    }

    [Fact]
    public void Equals_ObjectOverload_WorksCorrectly()
    {
        // Arrange
        var location1 = new GeoLocation(45.0, 15.0);
        object location2 = new GeoLocation(45.0, 15.0);
        object notALocation = "not a location";

        // Act & Assert
        location1.Equals(location2).Should().BeTrue();
        location1.Equals(notALocation).Should().BeFalse();
    }

    [Fact]
    public void OperatorEquals_BothNull_ReturnsTrue()
    {
        // Arrange
        GeoLocation? location1 = null;
        GeoLocation? location2 = null;

        // Act & Assert
        (location1 == location2).Should().BeTrue();
    }

    [Fact]
    public void OperatorEquals_OneNull_ReturnsFalse()
    {
        // Arrange
        GeoLocation? location1 = new GeoLocation(45.0, 15.0);
        GeoLocation? location2 = null;

        // Act & Assert
        (location1 == location2).Should().BeFalse();
        (location2 == location1).Should().BeFalse();
    }

    #endregion

    #region GetHashCode Tests

    [Fact]
    public void GetHashCode_SameCoordinates_ReturnsSameHash()
    {
        // Arrange
        var location1 = new GeoLocation(45.8150, 15.9819);
        var location2 = new GeoLocation(45.8150, 15.9819);

        // Act & Assert
        location1.GetHashCode().Should().Be(location2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentCoordinates_ReturnsDifferentHash()
    {
        // Arrange
        var location1 = new GeoLocation(45.8150, 15.9819);
        var location2 = new GeoLocation(43.5081, 16.4402);

        // Act & Assert
        location1.GetHashCode().Should().NotBe(location2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_CanBeUsedInHashSet()
    {
        // Arrange
        var set = new HashSet<GeoLocation>
        {
            new GeoLocation(45.0, 15.0),
            new GeoLocation(46.0, 16.0)
        };

        // Act
        var containsFirst = set.Contains(new GeoLocation(45.0, 15.0));
        var containsNew = set.Contains(new GeoLocation(47.0, 17.0));

        // Assert
        containsFirst.Should().BeTrue();
        containsNew.Should().BeFalse();
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var location = new GeoLocation(45.815, 15.9819);

        // Act
        var result = location.ToString();

        // Assert
        result.Should().Be("(45.815000, 15.981900)");
    }

    [Fact]
    public void ToString_NegativeCoordinates_ReturnsFormattedString()
    {
        // Arrange
        var location = new GeoLocation(-33.8688, -151.2093);

        // Act
        var result = location.ToString();

        // Assert
        result.Should().Contain("-33.868800");
        result.Should().Contain("-151.209300");
    }

    #endregion
}
