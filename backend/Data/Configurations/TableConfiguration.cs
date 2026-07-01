using backend.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Data.Configurations;

public class TableConfiguration: IEntityTypeConfiguration<Table>
{
    public void Configure(EntityTypeBuilder<Table> builder)
    {
        builder.HasOne(t => t.Location)
            .WithMany(l => l.Tables)
            .HasForeignKey(t => t.LocationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}