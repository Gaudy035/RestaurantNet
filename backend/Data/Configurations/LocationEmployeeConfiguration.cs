using backend.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Data.Configurations;

public class LocationEmployeeConfiguration : IEntityTypeConfiguration<LocationEmployee>
{
    public void Configure(EntityTypeBuilder<LocationEmployee> builder)
    {
        builder.HasKey(le => new { le.UserId, le.LocationId });

        builder.HasOne(le => le.Employee)
            .WithMany(e => e.LocationEmployees)
            .HasForeignKey(le => le.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(le => le.Location)
            .WithMany(l => l.LocationEmployees)
            .HasForeignKey(le => le.LocationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(le => le.Position)
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();
    }
}