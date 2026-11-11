namespace backend.Data.Configurations;

using backend.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.HasKey(c => new { c.UserId, c.ItemId });

        builder.HasOne(c => c.User)
            .WithMany(u => u.CartItems)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(c => c.MenuItem)
            .WithMany(m => m.CartItems)
            .HasForeignKey(c => c.ItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
