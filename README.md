# Device Management System

Company device inventory with assignments, JWT authentication, ranked search, and an optional OpenAI-powered description generator. Backend: **ASP.NET Core 9** Web API, **EF Core**, **SQL Server**. Frontend: **Angular 19**.

## Prerequisites

- [.NET SDK 9](https://dotnet.microsoft.com/download) (or 8+)
- [Node.js LTS](https://nodejs.org/) (includes npm)
- **SQL Server** — [LocalDB](https://learn.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb), SQL Express, or Docker SQL Server
- [Git](https://git-scm.com/)
- (Optional, Phase 4) OpenAI API key or compatible endpoint

## Clone and layout

```powershell
git clone <your-repo-url>
cd DeviceManagement
```

Important paths:

- `src/DeviceManagement.sln` — Visual Studio / `dotnet` solution
- `src/DeviceManagement.Api` — Web API
- `client/device-management-ui` — Angular SPA
- `database/` — idempotent SQL helpers + generated EF script

## Database

**Recommended (matches the running app):** from the repo root:

```powershell
cd src
dotnet tool restore
dotnet tool run dotnet-ef database update --project DeviceManagement.Api
```

You can also install the EF CLI globally: `dotnet tool install --global dotnet-ef` and then run `dotnet ef database update --project DeviceManagement.Api` from `src`.

**Alternative scripts:**

1. Run `database/01-create.sql` to create the `DeviceManagement` database (idempotent).
2. Apply schema using `database/ef-migration-baseline.sql` in SSMS or `sqlcmd` (creates Identity + `Devices` + history table), **or** use `dotnet ef database update` as above (preferred).
3. Optionally run `database/02-seed.sql` to idempotently insert sample **devices** (assignments are seeded when the API runs).

Connection string (default LocalDB) is in `src/DeviceManagement.Api/appsettings.json` under `ConnectionStrings:DefaultConnection`. Override with environment variable `ConnectionStrings__DefaultConnection` for other hosts.

## Run the API

```powershell
cd src/DeviceManagement.Api
dotnet run
```

- HTTP (used by the default Angular `environment.ts`): `http://localhost:5117`
- HTTPS: `https://localhost:7188` (trust dev certs: `dotnet dev-certs https --trust`)

Swagger UI (Development): `http://localhost:5117/swagger` (or the HTTPS URL printed in the console).

### JWT signing key (production)

Replace `Jwt:SigningKey` in configuration with a long random secret (32+ characters). For local dev the sample value in `appsettings.json` is only for convenience.

### OpenAI / LLM (Phase 4)

Configure the API key via [user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) (development):

```powershell
cd src/DeviceManagement.Api
dotnet user-secrets set "OpenAI:ApiKey" "YOUR_KEY_HERE"
```

Optional overrides: `OpenAI:BaseUrl` (OpenAI-compatible API), `OpenAI:Model`, `OpenAI:Temperature`.

The endpoint `POST /api/devices/{id}/generate-description` updates the stored `Description` after a successful response.

## Run the Angular app

```powershell
cd client/device-management-ui
npm install
npm start
```

Open `http://localhost:4200`. The dev build points to `http://localhost:5117/api` (`src/environments/environment.ts`). Adjust `apiUrl` for HTTPS or production deployments; production builds replace `environment.ts` with `environment.prod.ts` (see `angular.json` `fileReplacements`).

## Demo accounts (after API seed runs)

Seeded users (password `Passw0rd!`):

- `alice@company.test`
- `bob@company.test`

You can also **Register** a new account from the UI (email + password + profile fields).

## Tests

```powershell
cd src
dotnet test
```

Integration tests use an in-memory database and the `IntegrationTest` host environment (see `DatabaseSeeder.IntegrationTestEnvironment`).

## Features checklist (assignment)

- CRUD devices, list shows assignee, duplicate guard on **name + manufacturer**
- JWT login/register; devices require authentication
- Assign / unassign rules enforced server-side
- Ranked search: `GET /api/devices/search?q=...` (deterministic scoring, no AI)
- AI description generation with persistence

## Video demo

Record a short English voiceover walkthrough (CRUD, auth, assign/unassign, search, AI, optional DB scripts) and upload it to your preferred host; link it from your GitHub repository description or README.
