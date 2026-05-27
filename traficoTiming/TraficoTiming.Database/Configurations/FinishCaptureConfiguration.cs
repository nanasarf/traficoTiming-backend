using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TraficoTiming.Database.Entities;

namespace TraficoTiming.Database.Configurations;

public class FinishCaptureConfiguration : IEntityTypeConfiguration<FinishCapture>
{
    public void Configure(EntityTypeBuilder<FinishCapture> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.VideoFileUrl)
            .HasMaxLength(1000);

        builder.Property(x => x.LocalFileId)
            .HasMaxLength(200);

        builder.Property(x => x.FrameRate)
            .HasPrecision(8, 3);

        builder.Property(x => x.Resolution)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.TimingSessionId);
        builder.HasIndex(x => x.FinishDeviceId);
    }
}
