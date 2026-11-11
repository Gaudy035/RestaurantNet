using backend.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Data.Configurations;

public class UserConfiguration: IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(u => u.Email).IsUnique();

        builder.HasMany(u => u.CartItems)
            .WithOne(c => c.User)
            .HasForeignKey(c => c.UserId)
            .IsRequired(true)
            .OnDelete(DeleteBehavior.Cascade);
    }
}