# Solution Architecture & Design Decisions

## Executive Summary

This document outlines the architectural decisions, trade-offs, and production deployment strategy for the Telemetry Slice multi-tenant SaaS platform.

---

## Local Implementation Design

### Technology Choices

**Backend: ASP.NET Core 8 + Entity Framework Core + SQLite**
- **Rationale**: Modern, performant, and excellent tooling. EF Core provides LINQ queries and automatic parameterization preventing SQL injection.
- **Trade-off**: SQLite is suitable for 10K-100K events locally but needs migration to managed database for production.

**Frontend: React 18 + TypeScript + Vite**
- **Rationale**: Type safety, component reusability, excellent developer experience, and fast build times with Vite.
- **Trade-off**: Adds build step but provides maintainability and scalability.

### Key Features Implementation

#### 1. Idempotency (Duplicate Prevention)
**Implementation**: Unique index on `EventId` field in database
```sql
CREATE UNIQUE INDEX IX_TelemetryEvents_EventId ON TelemetryEvents(EventId);
```
**Benefits**: 
- Database-level guarantee of no duplicates
- O(1) lookup performance
- Works even if multiple API instances process same event

**Trade-off**: Requires devices to generate globally unique event IDs

#### 2. Out-of-Order Event Handling
**Implementation**: Separate `RecordedAt` (device timestamp) and `ReceivedAt` (server timestamp)
- All queries order by `RecordedAt` for correct time-series representation
- `ReceivedAt` tracks actual ingestion time for debugging

**Benefits**:
- Accurate time-series data regardless of network delays
- Audit trail of when events were actually received

#### 3. Multi-Tenant Isolation
**Implementation**: Query-level filtering with `CustomerId` in WHERE clauses
```csharp
var devices = await _context.Devices
    .Where(d => d.CustomerId == customerId)
    .ToListAsync();
```
**Benefits**:
- Simple to implement and understand
- Works with single database
- Index on CustomerId ensures performance

**Limitations**: Requires careful code review to ensure all queries include tenant filter

---

## Cloud Production Architecture

### Recommended Platform: Azure (AWS alternatives noted)

```
┌─────────────────────────────────────────────────────────────┐
│                     Internet / Devices                       │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│  Azure Front Door / CDN (AWS: CloudFront)                   │
│  - SSL termination                                           │
│  - WAF protection                                            │
│  - Global edge caching                                       │
└────────────────────┬────────────────────────────────────────┘
                     │
          ┌──────────┴──────────┐
          │                     │
          ▼                     ▼
┌───────────────────┐  ┌───────────────────┐
│  Static Frontend  │  │  Application      │
│  Azure Storage    │  │  Gateway          │
│  (AWS: S3)        │  │  (AWS: API GW)    │
└───────────────────┘  └─────────┬─────────┘
                                 │
                                 ▼
                       ┌─────────────────────┐
                       │  App Service /      │
                       │  Container Apps     │
                       │  (AWS: ECS/Fargate) │
                       │  - Auto-scaling     │
                       │  - Multiple zones   │
                       └──────────┬──────────┘
                                  │
                    ┌─────────────┼─────────────┐
                    │             │             │
                    ▼             ▼             ▼
          ┌─────────────┐ ┌─────────────┐ ┌─────────────┐
          │ Azure SQL   │ │ Redis Cache │ │ App Insights│
          │ or Postgres │ │ (ElastiCache│ │ (CloudWatch)│
          │             │ │  on AWS)    │ │             │
          └─────────────┘ └─────────────┘ └─────────────┘
```

### Core Services

#### 1. Compute Layer
**Azure**: App Service (PaaS) or Azure Container Apps
- **Auto-scaling**: 2-20 instances based on CPU/memory/request queue
- **Health probes**: Use `/health` endpoint
- **Multi-region**: Active-active deployment in 2+ regions

**AWS Alternative**: Elastic Container Service (ECS) with Fargate
- Similar auto-scaling capabilities
- ALB for health checks and load distribution

**Why not serverless (Functions/Lambda)?**
- HTTP connection pooling more efficient for database
- Predictable cold start behavior
- Better for sustained throughput

#### 2. Database Layer
**Azure SQL Database** (or **Azure Database for PostgreSQL**)
- **Tier**: General Purpose, 4-8 vCores
- **Geo-replication**: Read replicas in secondary regions
- **Tenant isolation**: Same query-level filtering, but add:
  - Row-level security policies for defense-in-depth
  - Separate schemas per customer (if <100 customers)
  - Connection string per tenant (if very high isolation needed)

**AWS Alternative**: RDS PostgreSQL or Aurora PostgreSQL
- Similar capabilities with read replicas
- Aurora provides better scaling for reads

**Data Model Evolution Strategy**:
```
v1.0: Current schema
v1.1: Add migration script with ALTER TABLE commands
v2.0: New columns added with defaults, old columns deprecated
v3.0: Remove deprecated columns after 6 months
```
- Use EF Core Migrations with versioning
- Apply during maintenance window or use online schema changes
- Keep migrations in source control

#### 3. Caching Layer
**Azure Redis Cache** (AWS: ElastiCache Redis)
- Cache device metadata (rarely changes)
- Cache recent telemetry for quick dashboard loads
- TTL: 5 minutes for telemetry, 1 hour for devices
- Reduces database load by 70-80%

#### 4. Observability
**Azure Application Insights** (AWS: CloudWatch + X-Ray)
- Request tracking, dependency calls, exceptions
- Custom metrics: events/sec per tenant, duplicate rate
- Alerts: API error rate >1%, database CPU >80%, queue depth >1000
- Distributed tracing for multi-service requests

---

## Multi-Tenant Isolation Strategy

### Current Approach: Shared Database, Query Filtering
✅ **Pros**: Simple, cost-effective, easy to backup
❌ **Cons**: Noisy neighbor risk, requires careful coding

### Production Enhancements

#### Level 1: Database-Level Row Security (PostgreSQL)
```sql
CREATE POLICY tenant_isolation ON telemetry_events
    USING (customer_id = current_setting('app.current_tenant'));
```
- Even if developer forgets WHERE clause, policy enforces isolation
- Performance impact: minimal with proper indexes

#### Level 2: Separate Schemas per Tenant (Medium Isolation)
```
database: telemetry_prod
  schema: acme_123
    tables: devices, telemetry_events
  schema: beta_456
    tables: devices, telemetry_events
```
- Stronger isolation, easier to backup single customer
- Works well for <100 tenants

#### Level 3: Database per Tenant (Highest Isolation)
- Use for compliance requirements (HIPAA, PCI-DSS)
- Connection pooling per tenant
- Most expensive, complex to manage

**Recommendation**: Start with Level 1 (row security), move to Level 2 if >50 customers or compliance requires it.

---

## Handling Scale & Burst Protection

### Event Volume Strategy

#### Volumes
- **Current**: ~20 events/min across 5 devices
- **Target Year 1**: 10,000 devices × 1 event/min = 10,000 events/min (167/sec)
- **Target Year 3**: 100,000 devices = 1,667 events/sec

### Architecture for Scale

#### 1. API Gateway Rate Limiting
```yaml
Per API key: 100 requests/second
Per tenant: 1000 requests/second
Global: 10,000 requests/second
```
- Returns HTTP 429 (Too Many Requests)
- Devices should implement exponential backoff

#### 2. Message Queue for Decoupling
**Azure**: Service Bus or Event Hubs
**AWS**: SQS or Kinesis

```
Device → API → Queue → Worker Service → Database
```

**Benefits**:
- API returns 202 Accepted immediately
- Worker processes batch inserts (100 events at a time)
- Handles bursts up to 10x normal load
- Poison message queue for failed events

**Trade-off**: Slightly delayed insights (2-5 seconds), but system doesn't crash

#### 3. Database Optimizations
- **Batch Inserts**: Insert 100 events in single transaction
- **Partitioning**: Partition telemetry_events by date (monthly partitions)
- **Archival**: Move data >90 days to cold storage (Azure Blob/S3)
- **Read Replicas**: Route dashboard queries to read replica

### Burst Protection Example
```csharp
// Pseudo-code for worker service
while (true) {
    var events = await queue.ReceiveBatch(maxCount: 100, timeout: 5s);
    if (events.Any()) {
        using var transaction = db.BeginTransaction();
        foreach (var evt in events) {
            // Duplicate check happens here via unique constraint
            db.TelemetryEvents.Add(evt);
        }
        await db.SaveChangesAsync();
        transaction.Commit();
        queue.CompleteBatch(events);
    }
}
```

---

## CI/CD Pipeline

### Pipeline Stages

```
┌─────────────┐
│  Developer  │
│  git push   │
└──────┬──────┘
       │
       ▼
┌─────────────────────────────────────────────┐
│ Stage 1: Build & Test (5 min)              │
│ - dotnet build                              │
│ - dotnet test                               │
│ - npm build                                 │
│ - npm test                                  │
│ - SonarQube code analysis                   │
│ - OWASP dependency check                    │
└──────┬──────────────────────────────────────┘
       │ ✅ Pass
       ▼
┌─────────────────────────────────────────────┐
│ Stage 2: Package (2 min)                    │
│ - Docker build                              │
│ - Push to Azure Container Registry          │
│ - Version tag (semver)                      │
└──────┬──────────────────────────────────────┘
       │
       ▼
┌─────────────────────────────────────────────┐
│ Stage 3: Deploy to Staging (10 min)        │
│ - Deploy to staging environment             │
│ - Run smoke tests                           │
│ - Run integration tests                     │
│ - Run load tests (100 req/s for 5 min)     │
└──────┬──────────────────────────────────────┘
       │ ✅ Pass
       ▼
┌─────────────────────────────────────────────┐
│ Stage 4: Deploy to Production (Manual)     │
│ - Approval required                         │
│ - Blue-green deployment                     │
│ - Gradual traffic shift: 10% → 50% → 100%  │
│ - Automatic rollback on error rate spike    │
└─────────────────────────────────────────────┘
```

### Automated Quality Gates

1. **Unit Test Coverage**: Minimum 80%
2. **Integration Tests**: All API endpoints tested
3. **Load Test**: Must handle 2x expected load
4. **Security Scan**: No high/critical vulnerabilities
5. **Code Quality**: SonarQube Quality Gate passes

### Database Migration Strategy
```yaml
# In deployment pipeline
- name: Apply Database Migrations
  run: |
    dotnet ef database update --connection "$PROD_CONNECTION_STRING"
  # Runs before new app version deployed
  # Uses migration scripts in source control
```

**Safety Measures**:
- Migrations tested in staging first
- Always additive (add columns, never remove immediately)
- Background jobs for data backfills
- Rollback script prepared for each migration

---

## Data Model Evolution

### Scenario: Adding a new telemetry type (e.g., "pressure")

#### Phase 1: Expand (Week 1)
```csharp
// Add nullable column
public string? SecondaryType { get; set; }
public double? SecondaryValue { get; set; }
```
- Deploy migration
- Old code continues working (ignores new columns)
- New devices can start sending pressure data

#### Phase 2: Migrate (Week 2-4)
- Update API to handle new type
- Update frontend to display new type
- Monitor for issues

#### Phase 3: Contract (Week 8+)
- If replacing old field, mark as deprecated
- After 3 months, remove if no usage

### Breaking Changes Strategy
- **Version API**: `/api/v1/telemetry` and `/api/v2/telemetry`
- Support v1 for 6 months after v2 release
- Sunset emails to customers 30/60/90 days before deprecation

---

## Security Considerations

### Authentication & Authorization
**Current**: Customer ID in request (demo only)
**Production**: 
- Device authentication via API keys (1 per device, rotatable)
- User authentication via OAuth 2.0 / OpenID Connect (Azure AD, Auth0)
- API keys stored hashed in database
- JWT tokens for user sessions (15 min expiry, refresh tokens for 7 days)

### Tenant Isolation Verification
```csharp
// Middleware to extract tenant from JWT claims
app.Use(async (context, next) => {
    var tenantId = context.User.FindFirst("tenant_id")?.Value;
    if (string.IsNullOrEmpty(tenantId)) {
        context.Response.StatusCode = 401;
        return;
    }
    context.Items["TenantId"] = tenantId;
    await next();
});
```

### Secrets Management
- Connection strings in Azure Key Vault (AWS Secrets Manager)
- Managed Identity for API to access Key Vault (no credentials in code)
- Rotate secrets every 90 days

---

## Cost Estimates (Azure)

### Assumptions: 10,000 devices, 1 event/min, 100 users

| Service | Tier | Monthly Cost |
|---------|------|--------------|
| App Service (3 × P2v3) | Auto-scale 2-6 | $600 |
| Azure SQL (8 vCores) | General Purpose | $1,200 |
| Redis Cache (6 GB) | Standard | $150 |
| Application Insights | Pay-as-you-go | $200 |
| Service Bus (Premium) | 1 messaging unit | $700 |
| Blob Storage (archival) | Cool tier, 1 TB | $20 |
| **Total** | | **~$2,900/month** |

**Scaling**: Costs scale ~linearly with event volume up to 1M events/min, then optimization needed.

---

## Trade-Offs Made

### 1. Shared Database vs. Database-per-Tenant
**Chose**: Shared database with row-level filtering
**Why**: Simplicity, cost-effective for <1000 tenants, easier backups
**When to change**: If a customer requires dedicated infrastructure for compliance

### 2. SQLite vs. PostgreSQL for Local Dev
**Chose**: SQLite
**Why**: Zero configuration, fast setup, good for demos
**Trade-off**: Doesn't support all PostgreSQL features (full-text search, JSON operators)

### 3. REST API vs. gRPC/GraphQL
**Chose**: REST API
**Why**: Simple, widely supported, good for HTTP-based device communication
**When to change**: gRPC for high-throughput device streams (>10K events/sec per device)

### 4. Query-Time Filtering vs. Middleware Tenant Context
**Chose**: Query-time filtering with customer ID
**Why**: Explicit, easy to audit, works with EF Core
**Improvement**: Add middleware to set db context tenant filter automatically

---

## Monitoring & Alerts

### Key Metrics Dashboard
1. **Event Ingestion Rate** (events/sec per tenant)
2. **API Response Time** (p50, p95, p99)
3. **Duplicate Event Rate** (should be <5%)
4. **Database CPU/Memory** (alert if >80%)
5. **Queue Depth** (alert if >10,000 messages)

### Alert Rules
```yaml
- name: High Error Rate
  condition: error_rate > 1% for 5 minutes
  action: Page on-call engineer

- name: Slow API Response
  condition: p95_latency > 500ms for 10 minutes
  action: Email dev team

- name: Database Overload
  condition: db_cpu > 90% for 5 minutes
  action: Scale up database, page on-call
```

---

## Future Enhancements

### Phase 2 (Months 3-6)
- WebSocket support for real-time dashboard updates
- Device command and control (send commands back to devices)
- Advanced analytics (anomaly detection, predictive maintenance)
- Multi-region active-active setup

### Phase 3 (Months 6-12)
- Time-series database (InfluxDB, TimescaleDB) for better performance
- Data lake for long-term analytics (Azure Data Lake, S3)
- Machine learning pipeline for predictive insights
- Mobile app for monitoring

---

## Conclusion

This solution provides a production-ready foundation for a multi-tenant telemetry platform. Key strengths:
- ✅ Scales from 1 to 100,000 devices with architectural adjustments
- ✅ Strong tenant isolation with query-level filtering
- ✅ Idempotency and out-of-order handling built-in
- ✅ Observable and debuggable with proper logging
- ✅ Clear path to cloud deployment with managed services
- ✅ Cost-effective for startups, scalable for growth

The local implementation demonstrates all core capabilities while the cloud architecture provides a roadmap for production scale and reliability.
