namespace HotelSearch.Domain.ValueObjects;

/// <summary>
/// Represents a geographic location with latitude and longitude
/// </summary>
public class GeoLocation : IEquatable<GeoLocation>
{
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }

    // Private parameterless constructor for EF Core
    private GeoLocation() { }

    /// <summary>
    /// Creates a new geographic location
    /// </summary>
    /// <param name="latitude">Latitude in degrees (-90 to 90)</param>
    /// <param name="longitude">Longitude in degrees (-180 to 180)</param>
    /// <exception cref="ArgumentException">Thrown when coordinates are out of valid range</exception>
    public GeoLocation(double latitude, double longitude)
    {
        if (latitude is < -90 or > 90)
            throw new ArgumentException("Latitude must be between -90 and 90 degrees", nameof(latitude));

        if (longitude is < -180 or > 180) 
            throw new ArgumentException("Longitude must be between -180 and 180 degrees", nameof(longitude));

        Latitude = latitude;
        Longitude = longitude;
    }

    /// <summary>
    /// Calculates the great-circle distance to another location using the Haversine formula
    /// </summary>
    /// <param name="other">Target location</param>
    /// <returns>Distance in kilometers</returns>
    /// <exception cref="ArgumentNullException">Thrown when other location is null</exception>
    public double DistanceTo(GeoLocation other)
    {
        if (other is null)
            throw new ArgumentNullException(nameof(other));

        const double earthRadiusKm = 6371.0;

        var lat1Rad = DegreesToRadians(Latitude);
        var lat2Rad = DegreesToRadians(other.Latitude);
        var deltaLat = DegreesToRadians(other.Latitude - Latitude);
        var deltaLon = DegreesToRadians(other.Longitude - Longitude);

        // Haversine formula
        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }
    
    public bool Equals(GeoLocation? other)
    {
        if (other is null)
            return false;
        
        if (ReferenceEquals(this, other)) 
            return true;
        
        // Use tolerance for floating-point comparison
        return Math.Abs(Latitude - other.Latitude) < 0.000001 && 
               Math.Abs(Longitude - other.Longitude) < 0.000001;
    }

    public override bool Equals(object? obj) => Equals(obj as GeoLocation);

    public override int GetHashCode() => HashCode.Combine(Latitude, Longitude);

    public static bool operator ==(GeoLocation? gl1, GeoLocation? gl2)
    {
        if (gl1 is null)
            return gl2 is null;
        
        return gl1.Equals(gl2);
    }

    public static bool operator !=(GeoLocation? gl1, GeoLocation? gl2) => !(gl1 == gl2);

    public override string ToString() => $"({Latitude:F6}, {Longitude:F6})";

    /// <summary>
    /// Converts degrees to radians
    /// </summary>
    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;
}