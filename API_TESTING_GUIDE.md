# TraficoTiming API - Testing Guide

## 🚀 How to Run and Test the API

### Prerequisites
- .NET 10 SDK installed
- PostgreSQL database running
- Connection string configured in `appsettings.json`

### Running the Application

#### Option 1: Visual Studio
1. Open the solution in Visual Studio
2. Set `TraficoTiming.Api` as the startup project
3. Press `F5` or click the "Start" button
4. **Swagger UI will automatically open in your browser at the root URL**

#### Option 2: Command Line
```bash
cd TraficoTiming.Api
dotnet run
```
Then navigate to: **https://localhost:7005** or **http://localhost:5158**

---

## 📋 Swagger UI Usage

When you run the application, Swagger UI will appear at the root URL showing all available endpoints.

### Complete Race Flow Test Sequence

Follow this exact order in Swagger to test a complete timing session:

#### 1️⃣ **Create a Timing Session**
```
POST /api/timing/sessions
```
**Request Body:**
```json
{
  "meetId": "00000000-0000-0000-0000-000000000001",
  "eventId": "00000000-0000-0000-0000-000000000002",
  "heatNumber": 1,
  "round": "Finals",
  "createdByUserId": "00000000-0000-0000-0000-000000000003"
}
```
✅ **Copy the `sessionId` from the response**

---

#### 2️⃣ **Pair Starter Device**
```
POST /api/timing/sessions/{sessionId}/devices
```
**Request Body:**
```json
{
  "deviceName": "iPhone 14 Pro - Starter",
  "deviceRole": 0,
  "deviceType": "iOS",
  "connectionType": 1,
  "batteryLevel": 85
}
```
✅ **Copy the `starterDeviceId` from the response**

---

#### 3️⃣ **Pair Finish Device**
```
POST /api/timing/sessions/{sessionId}/devices
```
**Request Body:**
```json
{
  "deviceName": "iPhone 13 - Finish Lane 1",
  "deviceRole": 1,
  "deviceType": "iOS",
  "connectionType": 1,
  "batteryLevel": 90
}
```
✅ **Copy the `finishDeviceId` from the response**

---

#### 4️⃣ **Sync Clocks**
```
POST /api/timing/sessions/{sessionId}/clock-sync
```
**Request Body:**
```json
{
  "starterDeviceId": "{starterDeviceId from step 2}",
  "finishDeviceId": "{finishDeviceId from step 3}",
  "offsetMs": 150,
  "roundTripDelayMs": 25,
  "driftMs": 2,
  "syncQualityScore": 0.95
}
```

---

#### 5️⃣ **Start the Race**
```
POST /api/timing/sessions/{sessionId}/start
```
**Request Body:**
```json
{
  "starterDeviceId": "{starterDeviceId from step 2}",
  "startTimestampUtc": "2025-01-26T10:30:00Z",
  "startMethod": 0,
  "gunSoundPlayed": true,
  "flashTriggered": true
}
```

---

#### 6️⃣ **Create Finish Capture**
```
POST /api/timing/sessions/{sessionId}/finish-captures
```
**Request Body:**
```json
{
  "finishDeviceId": "{finishDeviceId from step 3}",
  "localFileId": "capture_2025_01_26_10_30_15.mp4",
  "frameRate": 240,
  "resolution": "1920x1080",
  "recordingStartedAtUtc": "2025-01-26T10:30:10Z",
  "finishLineCalibrationDataJson": "{\"line_position\": 0.75}"
}
```
✅ **Copy the `captureId` from the response**

---

#### 7️⃣ **Create Raw Timing Results**
```
POST /api/timing/sessions/{sessionId}/raw-results
```
**Request Body (Athlete 1 - Lane 1):**
```json
{
  "athleteId": "00000000-0000-0000-0000-000000000010",
  "lane": 1,
  "bibNumber": "101",
  "detectedFinishTimestampUtc": "2025-01-26T10:30:09.850Z",
  "rawTimeSeconds": 9.85,
  "adjustedTimeSeconds": 9.85,
  "detectionMethod": 1,
  "confidenceScore": 0.98,
  "frameNumber": 2364
}
```
**Request Body (Athlete 2 - Lane 2):**
```json
{
  "athleteId": "00000000-0000-0000-0000-000000000011",
  "lane": 2,
  "bibNumber": "102",
  "detectedFinishTimestampUtc": "2025-01-26T10:30:10.120Z",
  "rawTimeSeconds": 10.12,
  "adjustedTimeSeconds": 10.12,
  "detectionMethod": 1,
  "confidenceScore": 0.95,
  "frameNumber": 2429
}
```

---

#### 8️⃣ **Get All Raw Results for Session**
```
GET /api/timing/sessions/{sessionId}/raw-results
```

---

#### 9️⃣ **Submit a Result**
```
POST /api/timing/raw-results/{resultId}/submit
```
(No body required)

---

#### 🔟 **End the Session**
```
PATCH /api/timing/sessions/{sessionId}/end
```
(No body required)

---

#### 1️⃣1️⃣ **View Audit Logs**
```
GET /api/timing/sessions/{sessionId}/audit-logs
```

---

## 📊 Other Available Endpoints

### Device Management
- `GET /api/timing/sessions/{sessionId}/devices` - List all paired devices
- `PATCH /api/timing/devices/{deviceId}/heartbeat` - Update device battery/connection
- `PATCH /api/timing/devices/{deviceId}/disconnect` - Disconnect device

### Clock Sync
- `GET /api/timing/sessions/{sessionId}/clock-sync/latest` - Get latest sync record

### Race Start
- `GET /api/timing/sessions/{sessionId}/start` - Get race start event

### Finish Captures
- `GET /api/timing/sessions/{sessionId}/finish-captures` - List all finish captures
- `PATCH /api/timing/finish-captures/{captureId}/upload-status` - Update upload status

### Raw Results Management
- `GET /api/timing/raw-results/{resultId}` - Get specific result
- `PATCH /api/timing/raw-results/{resultId}` - Update result (reviewer corrections)
- `POST /api/timing/raw-results/{resultId}/reject` - Reject result

### Offline Sync
- `POST /api/timing/sync/batch` - Upload offline sync batch
- `GET /api/timing/sync/batch/{batchId}` - Check sync batch status

### Session Management
- `GET /api/timing/sessions/{sessionId}` - Get specific session
- `GET /api/timing/sessions/meet/{meetId}` - Get all sessions for a meet
- `PATCH /api/timing/sessions/{sessionId}/cancel` - Cancel session

---

## 🎯 Enum Values Reference

### DeviceRole
- `0` = Starter
- `1` = Finish

### ConnectionType
- `0` = Bluetooth
- `1` = WiFi
- `2` = Cellular

### StartMethod
- `0` = GunButton
- `1` = VoiceCommand
- `2` = AutoTimer

### DetectionMethod
- `0` = Manual
- `1` = AI
- `2` = FrameByFrame

### SessionStatus
- `0` = Created
- `1` = DevicesPaired
- `2` = Ready
- `3` = Running
- `4` = Completed
- `5` = Cancelled

### RawResultStatus
- `0` = Pending
- `1` = UnderReview
- `2` = Approved
- `3` = Rejected
- `4` = Submitted

### UploadStatus
- `0` = Pending
- `1` = Uploading
- `2` = Completed
- `3` = Failed

---

## ⚠️ Important Notes

1. **Database Must Be Running**: Make sure PostgreSQL is running before starting the API
2. **Run Migrations**: Ensure EF Core migrations are applied:
   ```bash
   cd TraficoTiming.Api
   dotnet ef database update
   ```
3. **Connection String**: Check `appsettings.json` for correct database connection string
4. **Test Data**: Use valid GUIDs when testing (you can generate them or use placeholder ones)
5. **Sequence Matters**: Follow the test flow sequence above for a complete race timing scenario

---

## 🐛 Troubleshooting

### Swagger doesn't open
- Make sure you're running `TraficoTiming.Api` project
- Check browser isn't blocking localhost
- Manually navigate to `https://localhost:7005` or `http://localhost:5158`

### Database errors
- Verify PostgreSQL is running
- Check connection string in `appsettings.json`
- Run: `dotnet ef database update` from the API project folder

### Build errors
- Clean solution: `dotnet clean`
- Rebuild: `dotnet build`
- Check all NuGet packages are restored

---

## 📧 API Response Format

All API responses follow this consistent format:

```json
{
  "success": true,
  "message": "Success message here",
  "data": { /* actual response data */ }
}
```

Error responses:
```json
{
  "success": false,
  "message": "Error description",
  "data": null
}
```

---

## 🎉 You're Ready!

Run the application and start testing the complete race timing flow in Swagger UI!
