using backend.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Data.Configurations;

public class ClientConfiguration: IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.HasOne(c => c.User)
            .WithOne(u => u.Client)
            .HasForeignKey<Client>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}