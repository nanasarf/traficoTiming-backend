using Microsoft.AspNetCore.Mvc;
using TraficoTiming.Api.Models.Requests;
using TraficoTiming.Api.Models.Responses;
using TraficoTiming.Database.Entities;
using TraficoTiming.Services.Interfaces;
using TraficoTiming.Services.Models;

namespace TraficoTiming.Api.Controllers;

[ApiController]
[Route("api/timing")]
public class TimingDevicesController : ControllerBase
{
    private readonly IDevicePairingService _devicePairingService;

    public TimingDevicesController(IDevicePairingService devicePairingService)
    {
        _devicePairingService = devicePairingService;
    }

    [HttpPost("sessions/{sessionId:guid}/devices")]
    public async Task<IActionResult> PairDevice(
        Guid sessionId,
        [FromBody] PairDeviceRequest request,
        CancellationToken cancellationToken)
    {
        var model = new PairDeviceModel
        {
            TimingSessionId = sessionId,
            DeviceName = request.DeviceName,
            DeviceRole = request.DeviceRole,
            DeviceType = request.DeviceType,
            ConnectionType = request.ConnectionType,
            BatteryLevel = request.BatteryLevel
        };

        var device = await _devicePairingService.PairDeviceAsync(model, cancellationToken);

        return Ok(ApiResponse<TimingDeviceResponse>.Ok(ToResponse(device), "Device paired successfully."));
    }

    [HttpGet("sessions/{sessionId:guid}/devices")]
    public async Task<IActionResult> GetDevices(Guid sessionId, CancellationToken cancellationToken)
    {
        var devices = await _devicePairingService.GetSessionDevicesAsync(sessionId, cancellationToken);

        var responses = devices.Select(ToResponse).ToList();

        return Ok(ApiResponse<List<TimingDeviceResponse>>.Ok(responses));
    }

    [HttpPatch("devices/{deviceId:guid}/heartbeat")]
    public async Task<IActionResult> UpdateHeartbeat(
        Guid deviceId,
        [FromBody] int? batteryLevel,
        CancellationToken cancellationToken)
    {
        await _devicePairingService.UpdateHeartbeatAsync(deviceId, batteryLevel, cancellationToken);

        return Ok(ApiResponse<object>.Ok(null, "Heartbeat updated."));
    }

    [HttpPatch("devices/{deviceId:guid}/disconnect")]
    public async Task<IActionResult> DisconnectDevice(Guid deviceId, CancellationToken cancellationToken)
    {
        await _devicePairingService.DisconnectDeviceAsync(deviceId, cancellationToken);

        return Ok(ApiResponse<object>.Ok(null, "Device disconnected."));
    }

    private static TimingDeviceResponse ToResponse(TimingDevice device)
    {
        return new TimingDeviceResponse
        {
            Id = device.Id,
            TimingSessionId = device.TimingSessionId,
            DeviceName = device.DeviceName,
            DeviceRole = device.DeviceRole,
            DeviceType = device.DeviceType,
            ConnectionType = device.ConnectionType,
            ClockOffsetMs = device.ClockOffsetMs,
            ClockDriftMs = device.ClockDriftMs,
            LastSyncedAtUtc = device.LastSyncedAtUtc,
            BatteryLevel = device.BatteryLevel,
            Status = device.Status,
            CreatedAtUtc = device.CreatedAtUtc
        };
    }
}
