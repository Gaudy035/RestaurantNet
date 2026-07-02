using backend.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Data.Configurations;

public class OrderConfiguration: IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasOne(o => o.Location)
            .WithMany(l => l.Orders)
            .HasForeignKey(o => o.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Client)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(o => o.PaymentMethod)
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();
        
        builder.Property(o => o.OrderType)
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();
    }
}