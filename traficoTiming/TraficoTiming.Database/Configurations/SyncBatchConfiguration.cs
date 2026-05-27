using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TraficoTiming.Database.Entities;

namespace TraficoTiming.Database.Configurations;

public class SyncBatchConfiguration : IEntityTypeConfiguration<SyncBatch>
{
    public void Configure(EntityTypeBuilder<SyncBatch> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(2000);

        builder.HasMany(x => x.SyncItems)
            .WithOne(x => x.SyncBatch)
            .HasForeignKey(x => x.SyncBatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.TimingSessionId);
        builder.HasIndex(x => x.DeviceId);
    }
}
