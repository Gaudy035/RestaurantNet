using backend.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Data.Configurations;

public class ItemIngredientConfiguration: IEntityTypeConfiguration<ItemIngredient>
{
    public void Configure(EntityTypeBuilder<ItemIngredient> builder)
    {
        builder.HasKey(ii => new {ii.IngredientId, ii.ItemId});

        builder.HasOne(ii => ii.Ingredient)
            .WithMany(i => i.ItemIngredients)
            .HasForeignKey(ii => ii.IngredientId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(ii => ii.MenuItem)
            .WithMany(mi => mi.ItemIngredients)
            .HasForeignKey(ii => ii.ItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}