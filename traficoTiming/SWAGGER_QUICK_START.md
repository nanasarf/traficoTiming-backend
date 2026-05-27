# 🚀 Quick Start - Swagger Access

## When You Run the Application

### Automatic Browser Launch
When you press F5 in Visual Studio or run `dotnet run`, your browser will automatically open to:

**HTTPS (Recommended):**
```
https://localhost:7005
```

**HTTP (Alternative):**
```
http://localhost:5158
```

Both URLs will show the **Swagger UI** at the root.

---

## What You'll See

### Swagger UI Interface
The Swagger UI will display all API endpoints organized by controller:

1. **AuditLogs** - `/api/timing`
   - GET audit logs for a session

2. **ClockSync** - `/api/timing`
   - POST clock synchronization
   - GET latest sync record

3. **FinishCaptures** - `/api/timing`
   - POST create finish capture
   - GET list captures
   - PATCH update upload status

4. **RaceStart** - `/api/timing`
   - POST start race
   - GET race start event

5. **RawTimingResults** - `/api/timing`
   - POST create result
   - GET list results
   - GET specific result
   - PATCH update result
   - POST submit result
   - POST reject result

6. **Sync** - `/api/timing/sync`
   - POST process sync batch
   - GET batch status

7. **TimingDevices** - `/api/timing`
   - POST pair device
   - GET list devices
   - PATCH update heartbeat
   - PATCH disconnect device

8. **TimingSessions** - `/api/timing/sessions`
   - POST create session
   - GET session by ID
   - GET sessions by meet
   - PATCH end session
   - PATCH cancel session

---

## How to Test an Endpoint

1. Click on any endpoint (e.g., `POST /api/timing/sessions`)
2. Click the **"Try it out"** button on the right
3. Fill in the request body (Swagger shows the expected format)
4. Click **"Execute"**
5. See the response below with status code and data

---

## Sample First Request

### Create a Timing Session

1. Expand: `POST /api/timing/sessions`
2. Click: **"Try it out"**
3. Paste this JSON:

```json
{
  "meetId": "11111111-1111-1111-1111-111111111111",
  "eventId": "22222222-2222-2222-2222-222222222222",
  "heatNumber": 1,
  "round": "Finals",
  "createdByUserId": "33333333-3333-3333-3333-333333333333"
}
```

4. Click: **"Execute"**
5. You should see a `200 OK` response with your new session data!

---

## Alternative Access (If Browser Doesn't Auto-Open)

If the browser doesn't open automatically:

1. Look at the console output in Visual Studio
2. Find the line that says: `Now listening on: https://localhost:7005`
3. Manually open your browser and navigate to that URL
4. The Swagger UI will appear

---

## Swagger JSON Endpoint

If you need the raw OpenAPI specification:
```
https://localhost:7005/swagger/v1/swagger.json
```

---

## Ready to Test! 🎯

Just press **F5** in Visual Studio and Swagger will open automatically at the root URL.

Happy Testing! 🚀
