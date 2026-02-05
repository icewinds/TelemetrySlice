# Telemetry Slice - Multi-Tenant SaaS Telemetry Platform

A focused slice of a multi-tenant SaaS platform that receives telemetry from connected devices, stores it, and presents simple insights to end users. Built with ASP.NET Core 8, SQLite, React 18, and TypeScript.

## 🎯 Features

### Core Capabilities
- **Telemetry Ingestion**: Accepts telemetry events from devices via REST API
- **Idempotency**: Prevents duplicate event processing using unique event IDs
- **Out-of-Order Support**: Handles events arriving out of order by recorded timestamp
- **Multi-Tenant Isolation**: Complete data isolation between customers
- **Real-Time Insights**: Displays latest, min, max, and average values
- **Historical Data**: Shows last 24 hours of telemetry with interactive charts
- **Health Monitoring**: Basic health check endpoint for observability

### Architecture Highlights
- **Backend**: ASP.NET Core 8 Web API with Entity Framework Core
- **Database**: SQLite for local development (cloud-ready design)
- **Frontend**: React 18 with TypeScript and Vite
- **Data Visualization**: Recharts for interactive telemetry charts
- **Tenant Scoping**: Query-level isolation via customer ID filtering

## 🚀 Quick Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- Windows/macOS/Linux

### 1. Clone the Repository
```bash
git clone <repository-url>
cd TelemetrySlice
```

### 2. Run the Backend API

Navigate to the backend folder and run:

```bash
cd backend/TelemetrySlice.Api
dotnet restore
dotnet run
```

The API will start at:
- **HTTP**: `http://localhost:5177`
- **HTTPS**: `https://localhost:7177`
- **Swagger UI**: `http://localhost:5177/swagger`

The database will be automatically created and seeded with test data on first run.

### 3. Run the Frontend

In a new terminal, navigate to the frontend folder:

```bash
cd frontend
npm install
npm run dev
```

The frontend will start at:
- **URL**: `http://localhost:5173`

### 4. Explore the Application

1. Open your browser to `http://localhost:5173`
2. Select a customer from the dropdown (acme-123 or beta-456)
3. Click on a device to view its telemetry data
4. Explore the interactive chart and insights panel
5. Enable auto-refresh to see live updates

## 📊 Seeded Test Data

The application includes pre-seeded data for demonstration:

### Customers
- **Acme Corporation** (acme-123)
- **Beta Industries** (beta-456)

### Devices
- **acme-123**:
  - dev-001: Boiler #3 (Plant A)
  - dev-002: Chiller #1 (Plant A)
- **beta-456**:
  - dev-100: Pump #9 (Site B)

### Telemetry Events
- 10 events for Boiler #3 (temperature readings)
- 5 events for Chiller #1 (temperature readings)
- 5 events for Pump #9 (temperature readings)
- All events are timestamped within the last 24 hours for realistic demonstration

## 🧪 Testing Features

### Testing Idempotency (Duplicate Prevention)

You can test duplicate event handling using curl or Postman:

```bash
# Submit an event
curl -X POST http://localhost:5177/api/telemetry \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "acme-123",
    "deviceId": "dev-001",
    "eventId": "test-evt-1",
    "recordedAt": "2026-02-05T10:00:00Z",
    "type": "temperature",
    "value": 25.5,
    "unit": "C"
  }'

# Submit the same event again (should be ignored)
curl -X POST http://localhost:5177/api/telemetry \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "acme-123",
    "deviceId": "dev-001",
    "eventId": "test-evt-1",
    "recordedAt": "2026-02-05T10:00:00Z",
    "type": "temperature",
    "value": 25.5,
    "unit": "C"
  }'
```

The second request will return `isDuplicate: true` and won't create a new record.

### Testing Out-of-Order Events

Submit events with earlier timestamps than existing ones:

```bash
curl -X POST http://localhost:5177/api/telemetry \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "acme-123",
    "deviceId": "dev-001",
    "eventId": "old-evt-1",
    "recordedAt": "2026-02-04T08:00:00Z",
    "type": "temperature",
    "value": 20.0,
    "unit": "C"
  }'
```

The event will be stored and displayed in chronological order based on `recordedAt`.

### Testing Tenant Isolation

Try to access another customer's data:

```bash
# This will return devices for acme-123 only
curl http://localhost:5177/api/devices/acme-123

# This will return devices for beta-456 only
curl http://localhost:5177/api/devices/beta-456
```

Each customer can only see their own devices and telemetry.

## 📁 Project Structure

```
TelemetrySlice/
├── backend/
│   └── TelemetrySlice.Api/
│       ├── Controllers/          # API endpoints
│       │   ├── CustomersController.cs
│       │   ├── DevicesController.cs
│       │   └── TelemetryController.cs
│       ├── Data/                 # Database models and context
│       │   ├── Customer.cs
│       │   ├── Device.cs
│       │   ├── TelemetryEvent.cs
│       │   └── TelemetryDbContext.cs
│       ├── Models/               # DTOs
│       │   └── TelemetryEventDto.cs
│       ├── Services/             # Business logic
│       │   └── DatabaseSeeder.cs
│       ├── Program.cs            # Application entry point
│       └── appsettings.json      # Configuration
├── frontend/
│   └── src/
│       ├── components/           # React components
│       │   ├── InsightsPanel.tsx
│       │   ├── TelemetryChart.tsx
│       │   └── TelemetryTable.tsx
│       ├── services/             # API client
│       │   └── api.ts
│       ├── types/                # TypeScript types
│       │   └── telemetry.ts
│       ├── App.tsx               # Main app component
│       ├── App.css               # Styles
│       └── main.tsx              # Entry point
├── README.md                     # This file
└── SOLUTION.md                   # Architecture and design decisions
```

## 🔌 API Endpoints

### Health Check
- `GET /health` - Returns health status

### Customers
- `GET /api/customers` - List all customers

### Devices
- `GET /api/devices/{customerId}` - List devices for a customer
- `GET /api/devices/{customerId}/{deviceId}` - Get specific device

### Telemetry
- `POST /api/telemetry` - Submit telemetry event
- `GET /api/telemetry/{customerId}/{deviceId}?hours=24` - Get telemetry data
- `GET /api/telemetry/{customerId}/{deviceId}/insights?hours=24` - Get insights

## 🛠️ Development

### Backend Development

```bash
cd backend/TelemetrySlice.Api

# Build the project
dotnet build

# Run tests (if available)
dotnet test

# Run with hot reload
dotnet watch run

# Create a new migration
dotnet ef migrations add MigrationName

# View Swagger documentation
# Navigate to http://localhost:5177/swagger
```

### Frontend Development

```bash
cd frontend

# Install dependencies
npm install

# Start dev server with hot reload
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview

# Type checking
npm run tsc

# Linting
npm run lint
```

## 🔒 Security Notes

⚠️ **Local Development Only**: This implementation is designed for local development and demonstration purposes.

For production deployment, you should implement:
- Proper authentication (OAuth 2.0, JWT tokens)
- Authorization middleware
- HTTPS everywhere
- API rate limiting
- Input validation and sanitization
- SQL injection protection (EF Core provides this)
- CORS policy restrictions
- Secrets management (Azure Key Vault, AWS Secrets Manager)
- Logging and monitoring (Application Insights, CloudWatch)

See [SOLUTION.md](SOLUTION.md) for production architecture recommendations.

## 📈 Performance Characteristics

- **Idempotency**: O(1) lookup via unique index on EventId
- **Time-series queries**: Optimized with composite index on (CustomerId, DeviceId, RecordedAt)
- **Tenant isolation**: Enforced at query level with WHERE clauses
- **Database**: SQLite is suitable for 10K-100K events; use PostgreSQL/SQL Server for production scale

## 🤝 Contributing

This is a technical vetting assignment. For production use, consider:
- Unit and integration tests
- CI/CD pipeline
- Containerization (Docker)
- Infrastructure as Code (Terraform, Bicep)
- Monitoring and alerting
- Load testing
- Security scanning

## 📄 License

This project is created as a technical assessment and is provided as-is for evaluation purposes.

## 📞 Support

For questions or issues during evaluation:
- Check SOLUTION.md for architectural decisions
- Review API documentation at `/swagger`
- Inspect browser console for frontend errors
- Check backend logs in the console output

---

**Built with** ❤️ **using ASP.NET Core 8, React 18, and TypeScript**
