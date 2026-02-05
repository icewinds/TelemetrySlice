# API Examples - Postman & cURL

Complete collection of API endpoints with cURL commands and Postman examples.

**Base URL**: `http://localhost:5075`

---

## 🏥 Health Check

### Get Health Status
```bash
curl --location 'http://localhost:5075/health'
```

**Response:**
```json
{
  "status": "healthy",
  "timestamp": "2026-02-05T16:45:00.000Z"
}
```

---

## 👥 Customers API

### Get All Customers
```bash
curl --location 'http://localhost:5075/api/customers'
```

**Response:**
```json
[
  {
    "customerId": "acme-123",
    "name": "Acme Corporation"
  },
  {
    "customerId": "beta-456",
    "name": "Beta Industries"
  }
]
```

---

## 🔧 Devices API

### Get All Devices for a Customer
```bash
curl --location 'http://localhost:5075/api/devices/acme-123'
```

**Response:**
```json
[
  {
    "customerId": "acme-123",
    "deviceId": "dev-001",
    "label": "Boiler #3",
    "location": "Plant A"
  },
  {
    "customerId": "acme-123",
    "deviceId": "dev-002",
    "label": "Chiller #1",
    "location": "Plant A"
  }
]
```

### Get Specific Device
```bash
curl --location 'http://localhost:5075/api/devices/acme-123/dev-001'
```

**Response:**
```json
{
  "customerId": "acme-123",
  "deviceId": "dev-001",
  "label": "Boiler #3",
  "location": "Plant A"
}
```

### Get Devices for Different Customer (Tenant Isolation Demo)
```bash
curl --location 'http://localhost:5075/api/devices/beta-456'
```

**Response:**
```json
[
  {
    "customerId": "beta-456",
    "deviceId": "dev-100",
    "label": "Pump #9",
    "location": "Site B"
  }
]
```

---

## 📊 Telemetry API

### Submit Telemetry Event
```bash
curl --location 'http://localhost:5075/api/telemetry' \
--header 'Content-Type: application/json' \
--data '{
  "customerId": "acme-123",
  "deviceId": "dev-001",
  "eventId": "evt-new-001",
  "recordedAt": "2026-02-05T16:30:00Z",
  "type": "temperature",
  "value": 24.5,
  "unit": "C"
}'
```

**Response (First Submission):**
```json
{
  "message": "Event processed successfully",
  "eventId": "evt-new-001",
  "isDuplicate": false
}
```

### Submit Duplicate Event (Idempotency Test)
```bash
curl --location 'http://localhost:5075/api/telemetry' \
--header 'Content-Type: application/json' \
--data '{
  "customerId": "acme-123",
  "deviceId": "dev-001",
  "eventId": "evt-new-001",
  "recordedAt": "2026-02-05T16:30:00Z",
  "type": "temperature",
  "value": 24.5,
  "unit": "C"
}'
```

**Response (Duplicate):**
```json
{
  "message": "Event already processed",
  "eventId": "evt-new-001",
  "isDuplicate": true
}
```

### Submit Out-of-Order Event
```bash
curl --location 'http://localhost:5075/api/telemetry' \
--header 'Content-Type: application/json' \
--data '{
  "customerId": "acme-123",
  "deviceId": "dev-001",
  "eventId": "evt-old-001",
  "recordedAt": "2026-02-04T08:00:00Z",
  "type": "temperature",
  "value": 19.5,
  "unit": "C"
}'
```

**Response:**
```json
{
  "message": "Event processed successfully",
  "eventId": "evt-old-001",
  "isDuplicate": false
}
```

### Get Telemetry Events (Last 24 Hours)
```bash
curl --location 'http://localhost:5075/api/telemetry/acme-123/dev-001?hours=24'
```

**Response:**
```json
[
  {
    "eventId": "evt-a0",
    "recordedAt": "2026-02-04T17:10:00Z",
    "receivedAt": "2026-02-04T17:11:00Z",
    "type": "temperature",
    "value": 21.0,
    "unit": "C"
  },
  {
    "eventId": "evt-a1",
    "recordedAt": "2026-02-04T17:40:00Z",
    "receivedAt": "2026-02-04T17:41:00Z",
    "type": "temperature",
    "value": 21.5,
    "unit": "C"
  }
]
```

### Get Telemetry Events (Last 48 Hours)
```bash
curl --location 'http://localhost:5075/api/telemetry/acme-123/dev-001?hours=48'
```

### Get Telemetry Events (Last 1 Hour)
```bash
curl --location 'http://localhost:5075/api/telemetry/acme-123/dev-001?hours=1'
```

### Get Insights for Device
```bash
curl --location 'http://localhost:5075/api/telemetry/acme-123/dev-001/insights?hours=24'
```

**Response:**
```json
{
  "latest": 21.0,
  "min": 21.0,
  "average": 22.03,
  "max": 23.0,
  "count": 10,
  "unit": "C"
}
```

### Get Insights for Different Device
```bash
curl --location 'http://localhost:5075/api/telemetry/acme-123/dev-002/insights?hours=24'
```

**Response:**
```json
{
  "latest": 6.9,
  "min": 6.5,
  "average": 6.88,
  "max": 7.2,
  "count": 5,
  "unit": "C"
}
```

### Get Insights for Beta Customer Device (Tenant Isolation)
```bash
curl --location 'http://localhost:5075/api/telemetry/beta-456/dev-100/insights?hours=24'
```

**Response:**
```json
{
  "latest": 55.0,
  "min": 54.8,
  "average": 55.3,
  "max": 56.0,
  "count": 5,
  "unit": "C"
}
```

---

## 🧪 Test Scenarios

### Scenario 1: Complete Device Telemetry Flow

**Step 1: Check device exists**
```bash
curl --location 'http://localhost:5075/api/devices/acme-123/dev-001'
```

**Step 2: Submit new telemetry**
```bash
curl --location 'http://localhost:5075/api/telemetry' \
--header 'Content-Type: application/json' \
--data '{
  "customerId": "acme-123",
  "deviceId": "dev-001",
  "eventId": "test-flow-001",
  "recordedAt": "2026-02-05T17:00:00Z",
  "type": "temperature",
  "value": 26.0,
  "unit": "C"
}'
```

**Step 3: Get updated telemetry**
```bash
curl --location 'http://localhost:5075/api/telemetry/acme-123/dev-001?hours=24'
```

**Step 4: Get updated insights**
```bash
curl --location 'http://localhost:5075/api/telemetry/acme-123/dev-001/insights?hours=24'
```

### Scenario 2: Tenant Isolation Verification

**Get Acme devices (should return 2)**
```bash
curl --location 'http://localhost:5075/api/devices/acme-123'
```

**Get Beta devices (should return 1, different devices)**
```bash
curl --location 'http://localhost:5075/api/devices/beta-456'
```

**Try to access Acme device with Beta customer ID (should fail or return empty)**
```bash
curl --location 'http://localhost:5075/api/telemetry/beta-456/dev-001?hours=24'
```

### Scenario 3: Batch Event Submission

**Submit multiple events**
```bash
# Event 1
curl --location 'http://localhost:5075/api/telemetry' \
--header 'Content-Type: application/json' \
--data '{
  "customerId": "acme-123",
  "deviceId": "dev-002",
  "eventId": "batch-001",
  "recordedAt": "2026-02-05T17:00:00Z",
  "type": "temperature",
  "value": 7.5,
  "unit": "C"
}'

# Event 2
curl --location 'http://localhost:5075/api/telemetry' \
--header 'Content-Type: application/json' \
--data '{
  "customerId": "acme-123",
  "deviceId": "dev-002",
  "eventId": "batch-002",
  "recordedAt": "2026-02-05T17:05:00Z",
  "type": "temperature",
  "value": 7.8,
  "unit": "C"
}'

# Event 3
curl --location 'http://localhost:5075/api/telemetry' \
--header 'Content-Type: application/json' \
--data '{
  "customerId": "acme-123",
  "deviceId": "dev-002",
  "eventId": "batch-003",
  "recordedAt": "2026-02-05T17:10:00Z",
  "type": "temperature",
  "value": 8.0,
  "unit": "C"
}'
```

---

## 🐛 Error Cases

### Device Not Found
```bash
curl --location 'http://localhost:5075/api/devices/acme-123/dev-999'
```

**Response (404):**
```json
{
  "error": "Device dev-999 not found for customer acme-123"
}
```

### Customer Not Found
```bash
curl --location 'http://localhost:5075/api/devices/invalid-customer'
```

**Response (404):**
```json
{
  "error": "Customer invalid-customer not found"
}
```

### Submit Event to Non-Existent Device
```bash
curl --location 'http://localhost:5075/api/telemetry' \
--header 'Content-Type: application/json' \
--data '{
  "customerId": "acme-123",
  "deviceId": "dev-999",
  "eventId": "error-test-001",
  "recordedAt": "2026-02-05T17:00:00Z",
  "type": "temperature",
  "value": 25.0,
  "unit": "C"
}'
```

**Response (404):**
```json
{
  "error": "Device dev-999 not found for customer acme-123"
}
```

---

## 📦 Postman Collection Import

To import these into Postman:

1. Open Postman
2. Click "Import" button
3. Select "Raw text"
4. Copy and paste the cURL commands above
5. Postman will automatically convert them to requests

Or create a collection manually:

1. Create new Collection: "Telemetry Slice API"
2. Add folders: "Health", "Customers", "Devices", "Telemetry"
3. Add each cURL as a new request in the appropriate folder
4. Set base URL as environment variable: `{{baseUrl}}` = `http://localhost:5075`

---

## 🔑 PowerShell Examples

### Test Idempotency (Windows)
```powershell
# First submission
$body = @{
    customerId = "acme-123"
    deviceId = "dev-001"
    eventId = "ps-test-001"
    recordedAt = "2026-02-05T17:00:00Z"
    type = "temperature"
    value = 25.0
    unit = "C"
} | ConvertTo-Json

$response1 = Invoke-RestMethod -Uri "http://localhost:5075/api/telemetry" -Method POST -Body $body -ContentType "application/json"
Write-Host "First submission: isDuplicate = $($response1.isDuplicate)" -ForegroundColor Green

# Second submission (duplicate)
$response2 = Invoke-RestMethod -Uri "http://localhost:5075/api/telemetry" -Method POST -Body $body -ContentType "application/json"
Write-Host "Second submission: isDuplicate = $($response2.isDuplicate)" -ForegroundColor Yellow
```

### Get All Data for Customer
```powershell
# Get customer info
$customers = Invoke-RestMethod -Uri "http://localhost:5075/api/customers"
$acme = $customers | Where-Object { $_.customerId -eq "acme-123" }
Write-Host "Customer: $($acme.name)" -ForegroundColor Cyan

# Get devices
$devices = Invoke-RestMethod -Uri "http://localhost:5075/api/devices/acme-123"
Write-Host "Devices: $($devices.Count)" -ForegroundColor Cyan

# Get telemetry for each device
foreach ($device in $devices) {
    $telemetry = Invoke-RestMethod -Uri "http://localhost:5075/api/telemetry/acme-123/$($device.deviceId)?hours=24"
    Write-Host "$($device.label): $($telemetry.Count) events" -ForegroundColor Green
}
```

---

## 📊 Swagger UI

For interactive API documentation, open your browser to:

**http://localhost:5075/swagger**

You can test all endpoints directly from the Swagger UI with a visual interface.

---

## 🌐 JavaScript/Fetch Examples

### Get Customers (Fetch API)
```javascript
fetch('http://localhost:5075/api/customers')
  .then(response => response.json())
  .then(data => console.log(data));
```

### Submit Telemetry (Fetch API)
```javascript
fetch('http://localhost:5075/api/telemetry', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
  },
  body: JSON.stringify({
    customerId: 'acme-123',
    deviceId: 'dev-001',
    eventId: 'js-test-001',
    recordedAt: new Date().toISOString(),
    type: 'temperature',
    value: 25.0,
    unit: 'C'
  })
})
  .then(response => response.json())
  .then(data => console.log(data));
```

---

## 📝 Notes

- All timestamps should be in ISO 8601 format (UTC)
- EventId must be unique across all events (globally unique)
- CustomerId and DeviceId are case-sensitive
- The `hours` query parameter defaults to 24 if not specified
- All POST requests require `Content-Type: application/json` header
