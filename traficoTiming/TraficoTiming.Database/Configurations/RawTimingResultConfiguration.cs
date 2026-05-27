using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TraficoTiming.Database.Entities;

namespace TraficoTiming.Database.Configurations;

public class RawTimingResultConfiguration : IEntityTypeConfiguration<RawTimingResult>
{
    public void Configure(EntityTypeBuilder<RawTimingResult> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.BibNumber)
            .HasMaxLength(50);

        builder.Property(x => x.RawTimeSeconds)
            .HasPrecision(10, 4);

        builder.Property(x => x.AdjustedTimeSeconds)
            .HasPrecision(10, 4);

        builder.Property(x => x.ConfidenceScore)
            .HasPrecision(5, 2);

        builder.Property(x => x.ReviewerNote)
            .HasMaxLength(1000);

        builder.HasIndex(x => x.TimingSessionId);
        builder.HasIndex(x => x.AthleteId);
        builder.HasIndex(x => new { x.TimingSessionId, x.Lane });
    }
}
