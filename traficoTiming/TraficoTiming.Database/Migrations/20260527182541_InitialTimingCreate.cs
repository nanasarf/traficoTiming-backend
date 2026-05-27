using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TraficoTiming.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialTimingCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SyncBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TimingSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ItemCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncBatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TimingSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MeetId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    HeatNumber = table.Column<int>(type: "integer", nullable: false),
                    Round = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SyncQualityScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimingSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SyncItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SyncBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientGeneratedId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SyncItems_SyncBatches_SyncBatchId",
                        column: x => x.SyncBatchId,
                        principalTable: "SyncBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClockSyncRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TimingSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    StarterDeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    FinishDeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    OffsetMs = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: false),
                    RoundTripDelayMs = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: false),
                    DriftMs = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: true),
                    SyncQualityScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClockSyncRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClockSyncRecords_TimingSessions_TimingSessionId",
                        column: x => x.TimingSessionId,
                        principalTable: "TimingSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FinishCaptures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TimingSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    FinishDeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    VideoFileUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LocalFileId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FrameRate = table.Column<decimal>(type: "numeric(8,3)", precision: 8, scale: 3, nullable: false),
                    Resolution = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RecordingStartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RecordingEndedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FinishLineCalibrationDataJson = table.Column<string>(type: "text", nullable: true),
                    UploadStatus = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinishCaptures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinishCaptures_TimingSessions_TimingSessionId",
                        column: x => x.TimingSessionId,
                        principalTable: "TimingSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RaceStartEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TimingSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    StarterDeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartTimestampUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartMethod = table.Column<int>(type: "integer", nullable: false),
                    GunSoundPlayed = table.Column<bool>(type: "boolean", nullable: false),
                    FlashTriggered = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaceStartEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RaceStartEvents_TimingSessions_TimingSessionId",
                        column: x => x.TimingSessionId,
                        principalTable: "TimingSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RawTimingResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TimingSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AthleteId = table.Column<Guid>(type: "uuid", nullable: true),
                    Lane = table.Column<int>(type: "integer", nullable: true),
                    BibNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DetectedFinishTimestampUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RawTimeSeconds = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    AdjustedTimeSeconds = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    DetectionMethod = table.Column<int>(type: "integer", nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    FrameNumber = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ReviewerNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawTimingResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RawTimingResults_TimingSessions_TimingSessionId",
                        column: x => x.TimingSessionId,
                        principalTable: "TimingSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimingAuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TimingSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DetailsJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimingAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimingAuditLogs_TimingSessions_TimingSessionId",
                        column: x => x.TimingSessionId,
                        principalTable: "TimingSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimingDevices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TimingSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DeviceRole = table.Column<int>(type: "integer", nullable: false),
                    DeviceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ConnectionType = table.Column<int>(type: "integer", nullable: false),
                    ClockOffsetMs = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: true),
                    ClockDriftMs = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: true),
                    LastSyncedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BatteryLevel = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimingDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimingDevices_TimingSessions_TimingSessionId",
                        column: x => x.TimingSessionId,
                        principalTable: "TimingSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClockSyncRecords_CreatedAtUtc",
                table: "ClockSyncRecords",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ClockSyncRecords_TimingSessionId",
                table: "ClockSyncRecords",
                column: "TimingSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_FinishCaptures_FinishDeviceId",
                table: "FinishCaptures",
                column: "FinishDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_FinishCaptures_TimingSessionId",
                table: "FinishCaptures",
                column: "TimingSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_RaceStartEvents_StartTimestampUtc",
                table: "RaceStartEvents",
                column: "StartTimestampUtc");

            migrationBuilder.CreateIndex(
                name: "IX_RaceStartEvents_TimingSessionId",
                table: "RaceStartEvents",
                column: "TimingSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_RawTimingResults_AthleteId",
                table: "RawTimingResults",
                column: "AthleteId");

            migrationBuilder.CreateIndex(
                name: "IX_RawTimingResults_TimingSessionId",
                table: "RawTimingResults",
                column: "TimingSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_RawTimingResults_TimingSessionId_Lane",
                table: "RawTimingResults",
                columns: new[] { "TimingSessionId", "Lane" });

            migrationBuilder.CreateIndex(
                name: "IX_SyncBatches_DeviceId",
                table: "SyncBatches",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncBatches_TimingSessionId",
                table: "SyncBatches",
                column: "TimingSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncItems_ClientGeneratedId",
                table: "SyncItems",
                column: "ClientGeneratedId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SyncItems_SyncBatchId",
                table: "SyncItems",
                column: "SyncBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_TimingAuditLogs_CreatedAtUtc",
                table: "TimingAuditLogs",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_TimingAuditLogs_DeviceId",
                table: "TimingAuditLogs",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_TimingAuditLogs_TimingSessionId",
                table: "TimingAuditLogs",
                column: "TimingSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_TimingAuditLogs_UserId",
                table: "TimingAuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TimingDevices_TimingSessionId",
                table: "TimingDevices",
                column: "TimingSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_TimingDevices_TimingSessionId_DeviceRole",
                table: "TimingDevices",
                columns: new[] { "TimingSessionId", "DeviceRole" });

            migrationBuilder.CreateIndex(
                name: "IX_TimingSessions_EventId",
                table: "TimingSessions",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_TimingSessions_MeetId",
                table: "TimingSessions",
                column: "MeetId");

            migrationBuilder.CreateIndex(
                name: "IX_TimingSessions_MeetId_EventId_HeatNumber",
                table: "TimingSessions",
                columns: new[] { "MeetId", "EventId", "HeatNumber" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClockSyncRecords");

            migrationBuilder.DropTable(
                name: "FinishCaptures");

            migrationBuilder.DropTable(
                name: "RaceStartEvents");

            migrationBuilder.DropTable(
                name: "RawTimingResults");

            migrationBuilder.DropTable(
                name: "SyncItems");

            migrationBuilder.DropTable(
                name: "TimingAuditLogs");

            migrationBuilder.DropTable(
                name: "TimingDevices");

            migrationBuilder.DropTable(
                name: "SyncBatches");

            migrationBuilder.DropTable(
                name: "TimingSessions");
        }
    }
}
