using Microsoft.Extensions.DependencyInjection;
using TraficoTiming.Services.Implementations;
using TraficoTiming.Services.Interfaces;

namespace TraficoTiming.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<ITimingSessionService, TimingSessionService>();
        services.AddScoped<IDevicePairingService, DevicePairingService>();
        services.AddScoped<IClockSyncService, ClockSyncService>();
        services.AddScoped<IRaceStartService, RaceStartService>();
        services.AddScoped<IFinishCaptureService, FinishCaptureService>();
        services.AddScoped<IRawTimingResultService, RawTimingResultService>();
        services.AddScoped<IOfflineSyncService, OfflineSyncService>();
        services.AddScoped<IAuditLogService, AuditLogService>();

        return services;
    }
}
