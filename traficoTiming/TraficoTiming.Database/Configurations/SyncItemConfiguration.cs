using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TraficoTiming.Database.Entities;

namespace TraficoTiming.Database.Configurations;

public class SyncItemConfiguration : IEntityTypeConfiguration<SyncItem>
{
    public void Configure(EntityTypeBuilder<SyncItem> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.EntityType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.PayloadJson)
            .IsRequired();

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(2000);

        builder.HasIndex(x => x.SyncBatchId);

        builder.HasIndex(x => x.ClientGeneratedId)
            .IsUnique();
    }
}
