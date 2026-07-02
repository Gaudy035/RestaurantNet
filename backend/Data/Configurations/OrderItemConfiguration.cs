using backend.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Data.Configurations;

public class OrderItemConfiguration: IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(oi => new { oi.OrderId, oi.ItemId });

        builder.HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasOne(oi => oi.MenuItem)
            .WithMany(mi => mi.OrderItems)
            .HasForeignKey(oi => oi.ItemId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }
}