# LeadPilot

LeadPilot is a .NET 8 vertical slice for protected lead ingestion. This baseline includes an ASP.NET Core Web API, EF Core with SQL Server, a RabbitMQ-backed worker, health checks, OpenAPI in development, UTC timestamps through `TimeProvider`, and xUnit unit plus integration tests.

The current slice supports:

- administrator authentication and authorization
- tenant, project, and manager creation
- streamed CSV contact imports via `CsvHelper`
- phone normalization to E.164 via `libphonenumber-csharp`
- email normalization
- deduplication on `TenantId + ContactType + keyed HMAC hash`
- encrypted raw contact storage at rest
- masked contact listing for ordinary APIs
- manager-only reveal with immutable audit rows

No scraping or paid-provider connectors are included.

## Project Layout

- `src/LeadPilot.Domain`: domain entities and enums
- `src/LeadPilot.Application`: use-case services and abstractions
- `src/LeadPilot.Infrastructure`: EF Core, auth, crypto, RabbitMQ, health checks
- `src/LeadPilot.Server`: ASP.NET Core Web API host
- `src/LeadPilot.Worker`: RabbitMQ consumer host
- `tests/LeadPilot.UnitTests`: focused unit tests
- `tests/LeadPilot.IntegrationTests`: API-level integration tests

## Local Setup

1. Install .NET 8 SDK and Docker Engine with Compose.
2. Copy the environment template and replace every placeholder with local-only secrets.

```bash
cp .env.example .env
```

3. Start SQL Server, Redis, and RabbitMQ.

```bash
docker compose --env-file .env up -d
```

4. Export runtime configuration for the API and worker.

```bash
export ConnectionStrings__SqlServer="Server=localhost,1433;Database=LeadPilot;User Id=sa;Password=<your SQL_SA_PASSWORD>;TrustServerCertificate=True;Encrypt=False"
export ConnectionStrings__Redis="localhost:6379"
export Jwt__Issuer="LeadPilot"
export Jwt__Audience="LeadPilot"
export Jwt__SigningKey="<your JWT_SIGNING_KEY>"
export ContactProtection__EncryptionKey="<your CONTACT_ENCRYPTION_KEY>"
export ContactProtection__HashKey="<your CONTACT_HASH_KEY>"
export RabbitMq__HostName="localhost"
export RabbitMq__Port="5672"
export RabbitMq__UserName="<your RABBITMQ_DEFAULT_USER>"
export RabbitMq__Password="<your RABBITMQ_DEFAULT_PASS>"
export BootstrapAdmin__Email="<your BOOTSTRAP_ADMIN_EMAIL>"
export BootstrapAdmin__Password="<your BOOTSTRAP_ADMIN_PASSWORD>"
export PhoneNormalization__DefaultRegion="US"
```

5. Restore tools, apply the migration, then start the worker and API in separate terminals.

```bash
dotnet tool restore
dotnet ef database update --project src/LeadPilot.Infrastructure --startup-project src/LeadPilot.Server
dotnet run --project src/LeadPilot.Worker
dotnet run --project src/LeadPilot.Server
```

The API exposes Swagger UI in development at `/swagger` and health checks at `/health`.

## CSV Shape

Use a header row with `email` and `phone`. Either column may be empty, but synthetic test data only should be used.

```csv
email,phone
alice.synthetic@example.test,2025550101
```

## Example Flow

1. Authenticate as the bootstrap administrator through `POST /api/auth/token`.
2. Create a tenant through `POST /api/tenants`.
3. Create a project through `POST /api/tenants/{tenantId}/projects`.
4. Create a manager through `POST /api/tenants/{tenantId}/managers`.
5. Authenticate as the manager.
6. Import a CSV through `POST /api/projects/{projectId}/contacts/import`.
7. List masked contacts through `GET /api/projects/{projectId}/contacts`.
8. Reveal a full contact through `POST /api/projects/{projectId}/contacts/{contactId}/reveal`.

## Validation

Run the standard validation commands from the repository root:

```bash
dotnet format
dotnet build LeadPilot.sln
dotnet test LeadPilot.sln
docker compose --env-file .env ps
```

To verify container health explicitly:

```bash
docker inspect --format='{{json .State.Health}}' leadpilot-sqlserver
docker inspect --format='{{json .State.Health}}' leadpilot-redis
docker inspect --format='{{json .State.Health}}' leadpilot-rabbitmq
```