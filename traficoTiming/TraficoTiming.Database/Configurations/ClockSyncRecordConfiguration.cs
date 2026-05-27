using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TraficoTiming.Database.Entities;

namespace TraficoTiming.Database.Configurations;

public class ClockSyncRecordConfiguration : IEntityTypeConfiguration<ClockSyncRecord>
{
    public void Configure(EntityTypeBuilder<ClockSyncRecord> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OffsetMs)
            .HasPrecision(10, 3);

        builder.Property(x => x.RoundTripDelayMs)
            .HasPrecision(10, 3);

        builder.Property(x => x.DriftMs)
            .HasPrecision(10, 3);

        builder.Property(x => x.SyncQualityScore)
            .HasPrecision(5, 2);

        builder.HasIndex(x => x.TimingSessionId);
        builder.HasIndex(x => x.CreatedAtUtc);
    }
}
