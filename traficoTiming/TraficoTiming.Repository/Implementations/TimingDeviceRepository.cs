using Microsoft.EntityFrameworkCore;
using TraficoTiming.Database;
using TraficoTiming.Database.Entities;
using TraficoTiming.Database.Enums;
using TraficoTiming.Repository.Interfaces;

namespace TraficoTiming.Repository.Implementations;

public class TimingDeviceRepository(TraficoTimingDbContext db) : ITimingDeviceRepository
{
    public async Task<TimingDevice> CreateAsync(TimingDevice device, CancellationToken ct = default)
    {
        db.TimingDevices.Add(device);
        await db.SaveChangesAsync(ct);
        return device;
    }

    public Task<TimingDevice?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.TimingDevices.FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<List<TimingDevice>> GetDevicesBySessionAsync(Guid sessionId, CancellationToken ct = default) =>
        await db.TimingDevices
            .Where(d => d.TimingSessionId == sessionId)
            .OrderBy(d => d.DeviceRole)
            .ToListAsync(ct);

    public Task<TimingDevice?> GetStarterDeviceAsync(Guid sessionId, CancellationToken ct = default) =>
        db.TimingDevices
            .FirstOrDefaultAsync(d => d.TimingSessionId == sessionId && d.DeviceRole == DeviceRole.Starter, ct);

    public async Task<List<TimingDevice>> GetFinishDevicesAsync(Guid sessionId, CancellationToken ct = default) =>
        await db.TimingDevices
            .Where(d => d.TimingSessionId == sessionId && d.DeviceRole == DeviceRole.Finish)
            .ToListAsync(ct);

    public async Task<List<TimingDevice>> GetConnectedDevicesAsync(Guid sessionId, CancellationToken ct = default) =>
        await db.TimingDevices
            .Where(d => d.TimingSessionId == sessionId && d.Status == DeviceStatus.Connected)
            .ToListAsync(ct);

    public Task<bool> RoleExistsInSessionAsync(Guid sessionId, DeviceRole role, CancellationToken ct = default) =>
        db.TimingDevices.AnyAsync(d => d.TimingSessionId == sessionId && d.DeviceRole == role, ct);

    public Task<bool> DeviceNameExistsInSessionAsync(Guid sessionId, string deviceName, CancellationToken ct = default) =>
        db.TimingDevices.AnyAsync(d => d.TimingSessionId == sessionId && d.DeviceName == deviceName, ct);

    public async Task UpdateAsync(TimingDevice device, CancellationToken ct = default)
    {
        db.TimingDevices.Update(device);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(TimingDevice device, CancellationToken ct = default)
    {
        db.TimingDevices.Remove(device);
        await db.SaveChangesAsync(ct);
    }
}
