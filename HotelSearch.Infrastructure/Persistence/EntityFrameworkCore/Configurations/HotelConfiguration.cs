using HotelSearch.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelSearch.Infrastructure.Persistence.EntityFrameworkCore.Configurations;

/// <summary>
/// Entity Framework Core configuration for Hotel domain entity
/// </summary>
public class HotelConfiguration : IEntityTypeConfiguration<Hotel>
{
    public void Configure(EntityTypeBuilder<Hotel> builder)
    {
        builder.ToTable("hotels");
        
        builder.HasKey(h => h.Id);
        
        builder.Property(h => h.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(h => h.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(h => h.CreatedAt)
            .IsRequired();

        builder.Property(h => h.UpdatedAt);
        
        builder.OwnsOne(h => h.Location, location =>
        {
            location.Property(l => l.Latitude)
                .HasColumnName("latitude")
                .IsRequired();

            location.Property(l => l.Longitude)
                .HasColumnName("longitude")
                .IsRequired();
        });
        
        builder.HasIndex(h => h.Name)
            .HasDatabaseName("ix_hotels_name");
        
        builder.HasIndex(h => new { h.Location.Latitude, h.Location.Longitude })
            .HasDatabaseName("ix_hotels_location");
        
        builder.HasIndex(h => h.Price)
            .HasDatabaseName("ix_hotels_price");
    }
}