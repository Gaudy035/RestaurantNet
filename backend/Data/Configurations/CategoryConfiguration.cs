using backend.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Data.Configurations;

public class CategoryConfiguration: IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasMany(c => c.MenuItems)
            .WithOne(mu => mu.Category)
            .HasForeignKey(mu => mu.CategoryId)
            .IsRequired(true)
            .OnDelete(DeleteBehavior.NoAction);
    }
}