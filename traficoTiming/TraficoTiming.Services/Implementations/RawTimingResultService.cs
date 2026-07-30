using TraficoTiming.Database.Entities;
using TraficoTiming.Database.Enums;
using TraficoTiming.Repository.Interfaces;
using TraficoTiming.Services.Interfaces;
using TraficoTiming.Services.Models;

namespace TraficoTiming.Services.Implementations;

public class RawTimingResultService(
    IRawTimingResultRepository resultRepo,
    IAuditLogService auditLog) : IRawTimingResultService
{
    public async Task<RawTimingResult> CreateResultAsync(CreateRawTimingResultModel model, CancellationToken ct = default)
    {
        if (model.RawTimeSeconds <= 0)
            throw new ArgumentException("RawTimeSeconds must be greater than 0.");
        if (model.AdjustedTimeSeconds.HasValue && model.AdjustedTimeSeconds <= 0)
            throw new ArgumentException("AdjustedTimeSeconds must be greater than 0.");
        if (model.ConfidenceScore.HasValue && (model.ConfidenceScore < 0 || model.ConfidenceScore > 100))
            throw new ArgumentException("ConfidenceScore must be between 0 and 100.");
        if (model.FrameNumber is < int.MinValue or > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(model.FrameNumber), "FrameNumber must fit in a 32-bit integer.");
        if (model.Lane.HasValue && await resultRepo.LaneExistsInSessionAsync(model.TimingSessionId, model.Lane.Value, ct))
            throw new InvalidOperationException($"Lane {model.Lane} already has a result in this session.");

        var result = new RawTimingResult
        {
            TimingSessionId            = model.TimingSessionId,
            AthleteId                  = model.AthleteId,
            Lane                       = model.Lane,
            BibNumber                  = model.BibNumber,
            DetectedFinishTimestampUtc = model.DetectedFinishTimestampUtc ?? DateTime.UtcNow,
            RawTimeSeconds             = model.RawTimeSeconds,
            AdjustedTimeSeconds        = model.AdjustedTimeSeconds ?? model.RawTimeSeconds,
            DetectionMethod            = model.DetectionMethod,
            ConfidenceScore            = model.ConfidenceScore,
            FrameNumber                = model.FrameNumber.HasValue ? (int)model.FrameNumber.Value : null,
            Status                     = RawResultStatus.Pending
        };
        await resultRepo.CreateRawResultAsync(result, ct);
        await auditLog.LogAsync(result.TimingSessionId, AuditActions.RawResultCreated,
            details: new
            {
                rawResultId = result.Id,
                lane = result.Lane,
                newResultStatus = result.Status.ToString()
            }, ct: ct);
        return result;
    }

    public async Task<RawTimingResult> GetResultAsync(Guid resultId, CancellationToken ct = default) =>
        await resultRepo.GetRawResultByIdAsync(resultId, ct)
            ?? throw new KeyNotFoundException($"Result {resultId} not found.");

    public async Task<IReadOnlyList<RawTimingResult>> GetSessionResultsAsync(Guid sessionId, CancellationToken ct = default) =>
        (await resultRepo.GetResultsBySessionAsync(sessionId, ct)).AsReadOnly();

    public async Task<RawTimingResult> UpdateResultAsync(UpdateRawTimingResultModel model, CancellationToken ct = default)
    {
        var result = await GetResultAsync(model.ResultId, ct);
        if (result.Status == RawResultStatus.Submitted)
            throw new InvalidOperationException("A submitted result cannot be edited unless reopened.");
        if (model.AdjustedTimeSeconds.HasValue && model.AdjustedTimeSeconds <= 0)
            throw new ArgumentException("AdjustedTimeSeconds must be greater than 0.");

        var previousStatus = result.Status;
        if (model.AdjustedTimeSeconds.HasValue) result.AdjustedTimeSeconds = model.AdjustedTimeSeconds.Value;
        if (model.ReviewerNote is not null)      result.ReviewerNote = model.ReviewerNote;
        result.Status = RawResultStatus.Reviewed;

        await resultRepo.UpdateRawResultAsync(result, ct);
        await auditLog.LogAsync(result.TimingSessionId, AuditActions.RawResultUpdated,
            details: new
            {
                rawResultId = result.Id,
                lane = result.Lane,
                previousResultStatus = previousStatus.ToString(),
                newResultStatus = result.Status.ToString()
            }, ct: ct);
        return result;
    }

    public async Task<RawTimingResult> SubmitResultAsync(Guid resultId, CancellationToken ct = default)
    {
        var result = await GetResultAsync(resultId, ct);
        if (result.Status == RawResultStatus.Rejected)
            throw new InvalidOperationException("A rejected result must be reviewed before submission.");
        var previousStatus = result.Status;
        result.Status = RawResultStatus.Submitted;
        await resultRepo.UpdateRawResultAsync(result, ct);
        await auditLog.LogAsync(result.TimingSessionId, AuditActions.RawResultSubmitted,
            details: new
            {
                rawResultId = result.Id,
                lane = result.Lane,
                previousResultStatus = previousStatus.ToString(),
                newResultStatus = result.Status.ToString()
            }, ct: ct);
        return result;
    }

    public async Task<RawTimingResult> RejectResultAsync(Guid resultId, string note, CancellationToken ct = default)
    {
        var result = await GetResultAsync(resultId, ct);
        var previousStatus = result.Status;
        result.Status       = RawResultStatus.Rejected;
        result.ReviewerNote = note;
        await resultRepo.UpdateRawResultAsync(result, ct);
        await auditLog.LogAsync(result.TimingSessionId, AuditActions.RawResultRejected,
            details: new
            {
                rawResultId = result.Id,
                lane = result.Lane,
                previousResultStatus = previousStatus.ToString(),
                newResultStatus = result.Status.ToString()
            }, ct: ct);
        return result;
    }
}
