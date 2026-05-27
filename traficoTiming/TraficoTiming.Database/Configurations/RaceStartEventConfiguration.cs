using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TraficoTiming.Database.Entities;

namespace TraficoTiming.Database.Configurations;

public class RaceStartEventConfiguration : IEntityTypeConfiguration<RaceStartEvent>
{
    public void Configure(EntityTypeBuilder<RaceStartEvent> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.TimingSessionId);
        builder.HasIndex(x => x.StartTimestampUtc);
    }
}
