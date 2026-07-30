namespace TraficoTiming.Services;

public static class AuditActions
{
    public const string SessionCreated = "SESSION_CREATED";
    public const string SessionCompleted = "SESSION_COMPLETED";
    public const string SessionCancelled = "SESSION_CANCELLED";
    public const string DevicePaired = "DEVICE_PAIRED";
    public const string DeviceHeartbeatUpdated = "DEVICE_HEARTBEAT_UPDATED";
    public const string DeviceDisconnected = "DEVICE_DISCONNECTED";
    public const string ClockSyncRecorded = "CLOCK_SYNC_RECORDED";
    public const string SessionMarkedReady = "SESSION_MARKED_READY";
    public const string RaceStarted = "RACE_STARTED";
    public const string FinishCaptureCreated = "FINISH_CAPTURE_CREATED";
    public const string FinishCaptureUploadStatusUpdated = "FINISH_CAPTURE_UPLOAD_STATUS_UPDATED";
    public const string RawResultCreated = "RAW_RESULT_CREATED";
    public const string RawResultUpdated = "RAW_RESULT_UPDATED";
    public const string RawResultSubmitted = "RAW_RESULT_SUBMITTED";
    public const string RawResultRejected = "RAW_RESULT_REJECTED";
    public const string SyncBatchReceived = "SYNC_BATCH_RECEIVED";
    public const string SyncBatchCompleted = "SYNC_BATCH_COMPLETED";
    public const string SyncBatchFailed = "SYNC_BATCH_FAILED";
}
