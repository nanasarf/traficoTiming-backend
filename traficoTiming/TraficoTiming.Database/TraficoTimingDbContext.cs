using Microsoft.EntityFrameworkCore;
using TraficoTiming.Database.Entities;

namespace TraficoTiming.Database;

public class TraficoTimingDbContext(DbContextOptions<TraficoTimingDbContext> options)
    : DbContext(options)
{
    public DbSet<TimingSession>   TimingSessions   => Set<TimingSession>();
    public DbSet<TimingDevice>    TimingDevices    => Set<TimingDevice>();
    public DbSet<ClockSyncRecord> ClockSyncRecords => Set<ClockSyncRecord>();
    public DbSet<RaceStartEvent>  RaceStartEvents  => Set<RaceStartEvent>();
    public DbSet<FinishCapture>   FinishCaptures   => Set<FinishCapture>();
    public DbSet<RawTimingResult> RawTimingResults => Set<RawTimingResult>();
    public DbSet<SyncBatch>       SyncBatches      => Set<SyncBatch>();
    public DbSet<SyncItem>        SyncItems        => Set<SyncItem>();
    public DbSet<TimingAuditLog>  TimingAuditLogs  => Set<TimingAuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TraficoTimingDbContext).Assembly);
    }
}
