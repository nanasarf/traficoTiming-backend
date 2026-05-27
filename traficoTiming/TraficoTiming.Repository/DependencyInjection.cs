using Microsoft.Extensions.DependencyInjection;
using TraficoTiming.Repository.Implementations;
using TraficoTiming.Repository.Interfaces;

namespace TraficoTiming.Repository;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<ITimingSessionRepository, TimingSessionRepository>();
        services.AddScoped<ITimingDeviceRepository, TimingDeviceRepository>();
        services.AddScoped<IClockSyncRecordRepository, ClockSyncRecordRepository>();
        services.AddScoped<IRaceStartEventRepository, RaceStartEventRepository>();
        services.AddScoped<IFinishCaptureRepository, FinishCaptureRepository>();
        services.AddScoped<IRawTimingResultRepository, RawTimingResultRepository>();
        services.AddScoped<ISyncBatchRepository, SyncBatchRepository>();
        services.AddScoped<ISyncItemRepository, SyncItemRepository>();
        services.AddScoped<ITimingAuditLogRepository, TimingAuditLogRepository>();

        return services;
    }
}
