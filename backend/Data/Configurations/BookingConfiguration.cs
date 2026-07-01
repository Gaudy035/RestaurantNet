using backend.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Data.Configurations;

public class BookingConfiguration: IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasOne(b => b.Table)
            .WithMany(t => t.Bookings)
            .HasForeignKey(b => b.TableId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(b => b.Client)
            .WithMany(c => c.Bookings)
            .HasForeignKey(b => b.ClientId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}