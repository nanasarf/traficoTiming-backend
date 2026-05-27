using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TraficoTiming.Database.Entities;

namespace TraficoTiming.Database.Configurations;

public class TimingAuditLogConfiguration : IEntityTypeConfiguration<TimingAuditLog>
{
    public void Configure(EntityTypeBuilder<TimingAuditLog> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Action)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.TimingSessionId);
        builder.HasIndex(x => x.DeviceId);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.CreatedAtUtc);
    }
}
