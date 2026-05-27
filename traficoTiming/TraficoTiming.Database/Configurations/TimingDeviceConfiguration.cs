using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TraficoTiming.Database.Entities;

namespace TraficoTiming.Database.Configurations;

public class TimingDeviceConfiguration : IEntityTypeConfiguration<TimingDevice>
{
    public void Configure(EntityTypeBuilder<TimingDevice> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DeviceName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.DeviceType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.ClockOffsetMs)
            .HasPrecision(10, 3);

        builder.Property(x => x.ClockDriftMs)
            .HasPrecision(10, 3);

        builder.HasIndex(x => x.TimingSessionId);
        builder.HasIndex(x => new { x.TimingSessionId, x.DeviceRole });
    }
}
