using HotelSearch.Domain.ValueObjects;

namespace HotelSearch.Domain.Entities;

/// <summary>
/// Represents a hotel aggregate root
/// </summary>
public class Hotel
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    public GeoLocation Location { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Private parameterless constructor for EF Core
    private Hotel()
    {
        Name = string.Empty;
        Location = null!;
    } 
    
    /// <summary>
    /// Creates a new hotel instance
    /// </summary>
    /// <param name="name">Hotel name</param>
    /// <param name="price">Hotel price per night</param>
    /// <param name="location">Geographic location</param>
    public Hotel(string name, decimal price, GeoLocation location)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Hotel name cannot be empty", nameof(name));
        
        if (price < 0)
            throw new ArgumentException("Hotel price cannot be negative", nameof(price));

        if (location == null)
            throw new ArgumentNullException(nameof(location));

        Id = Guid.NewGuid();
        Name = name;
        Price = price;
        Location = location;
        CreatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Updates hotel information
    /// </summary>
    public void Update(string name, decimal price, GeoLocation location)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Hotel name cannot be empty", nameof(name));
        
        if (price < 0)
            throw new ArgumentException("Hotel price cannot be negative", nameof(price));

        if (location == null)
            throw new ArgumentNullException(nameof(location));

        Name = name;
        Price = price;
        Location = location;
        UpdatedAt = DateTime.UtcNow;
    }
}