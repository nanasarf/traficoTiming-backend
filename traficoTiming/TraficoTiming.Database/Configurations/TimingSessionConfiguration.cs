using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TraficoTiming.Database.Entities;

namespace TraficoTiming.Database.Configurations;

public class TimingSessionConfiguration : IEntityTypeConfiguration<TimingSession>
{
    public void Configure(EntityTypeBuilder<TimingSession> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Round)
            .HasMaxLength(50);

        builder.Property(x => x.SyncQualityScore)
            .HasPrecision(5, 2);

        builder.HasMany(x => x.Devices)
            .WithOne(x => x.TimingSession)
            .HasForeignKey(x => x.TimingSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.ClockSyncRecords)
            .WithOne(x => x.TimingSession)
            .HasForeignKey(x => x.TimingSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.RaceStartEvents)
            .WithOne(x => x.TimingSession)
            .HasForeignKey(x => x.TimingSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.FinishCaptures)
            .WithOne(x => x.TimingSession)
            .HasForeignKey(x => x.TimingSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.RawTimingResults)
            .WithOne(x => x.TimingSession)
            .HasForeignKey(x => x.TimingSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.AuditLogs)
            .WithOne(x => x.TimingSession)
            .HasForeignKey(x => x.TimingSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.MeetId);
        builder.HasIndex(x => x.EventId);
        builder.HasIndex(x => new { x.MeetId, x.EventId, x.HeatNumber });
    }
}
