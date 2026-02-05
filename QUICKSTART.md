# 🚀 Quick Start Guide

Get the application running in 3 simple steps!

## Step 1: Start the Backend API
Open PowerShell and run:
```powershell
cd c:\Users\Admin\source\repos\TelemetrySlice
.\run-backend.ps1
```

Wait for the message: `Now listening on: http://localhost:5177`

The database will be automatically created and seeded with test data.

## Step 2: Start the Frontend
Open a **NEW** PowerShell window and run:
```powershell
cd c:\Users\Admin\source\repos\TelemetrySlice
.\run-frontend.ps1
```

Wait for the message: `Local: http://localhost:5173/`

## Step 3: Open Your Browser
Navigate to: **http://localhost:5173**

### Try These Actions:
1. ✅ Select "Acme Corporation (acme-123)" from the dropdown
2. ✅ Click on "Boiler #3" device
3. ✅ View the interactive chart and insights
4. ✅ Check the "Auto-refresh" box
5. ✅ Switch to "Beta Industries (beta-456)" to see different data

---

## 🧪 Test Features

### Test Duplicate Prevention
Open a third PowerShell window:
```powershell
# Send the same event twice
$body = @{
    customerId = "acme-123"
    deviceId = "dev-001"
    eventId = "test-duplicate-123"
    recordedAt = "2026-02-05T12:00:00Z"
    type = "temperature"
    value = 25.0
    unit = "C"
} | ConvertTo-Json

# First submission
Invoke-RestMethod -Uri "http://localhost:5177/api/telemetry" -Method POST -Body $body -ContentType "application/json"

# Second submission (should be marked as duplicate)
Invoke-RestMethod -Uri "http://localhost:5177/api/telemetry" -Method POST -Body $body -ContentType "application/json"
```

The second response will show `isDuplicate: true`

### View API Documentation
Open: **http://localhost:5177/swagger**

---

## 📚 Documentation

- **README.md** - Complete setup instructions
- **SOLUTION.md** - Cloud architecture and design decisions  
- **PROJECT_SUMMARY.md** - Project status and demo script

---

## 🎥 Ready for Demo Video!

See PROJECT_SUMMARY.md for detailed demo script.

---

## ❓ Troubleshooting

### Backend won't start
- Ensure .NET 8 SDK is installed: `dotnet --version`
- Check port 5177 is not in use

### Frontend won't start  
- Ensure Node.js 18+ is installed: `node --version`
- Run `npm install` in the frontend folder
- Check port 5173 is not in use

### Can't see data
- Make sure backend started first
- Check browser console (F12) for errors
- Verify API is running: http://localhost:5177/health
