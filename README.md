# Device Management System

Small full-stack app: **ASP.NET Core** Web API + **SQL Server** + **Angular** (devices, users, JWT auth, search, optional AI descriptions).

---

## What you need installed

- [.NET SDK](https://dotnet.microsoft.com/download) 8 or 9  
- [Node.js LTS](https://nodejs.org/) (includes npm)  
- **SQL Server** [LocalDB](https://learn.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb) or Express (default connection string targets LocalDB)  
- [Git](https://git-scm.com/)

---

## Open and run (local)

**1. Database** — in a terminal, go to the `src` folder inside this repo:

```powershell
cd src
dotnet tool restore
dotnet tool run dotnet-ef database update --project DeviceManagement.Api
```

**2. API** — same repo, new terminal (or stop after step 1 and use one terminal):

```powershell
cd src\DeviceManagement.Api
dotnet run
```

Wait until you see a URL like `http://localhost:5117`. Open **Swagger**: [http://localhost:5117/swagger](http://localhost:5117/swagger).

**3. Web UI** — another terminal:

```powershell
cd client\device-management-ui
npm install
npm start
```

Open [http://localhost:4200](http://localhost:4200). Log in with a seeded user (**`alice@company.test`** / **`Passw0rd!`**) or use **Register**.

**Tip:** The Angular app calls **`http://localhost:5117/api`**. If your API uses another port, edit `client/device-management-ui/src/environments/environment.ts` (`apiUrl`).

---

## Check that it works

1. **`dotnet test`** from the `src` folder — should report passing tests.  
2. **Swagger** — `GET /api/devices` should return **401** without a token; after login, call with **Authorize** (JWT) and you should get **200**.  
3. **Browser** — login → device list loads → open a device → create/edit/delete if you want.  
4. **(Optional)** AI description: set an API key — `dotnet user-secrets set "OpenAI:ApiKey" "YOUR_KEY" --project src/DeviceManagement.Api` — then use “Generate description” in the UI.

---

## Repo layout

| Path | Purpose |
|------|--------|
| `src/DeviceManagement.sln` | Open in Visual Studio or build with `dotnet build` |
| `src/DeviceManagement.Api` | Web API |
| `client/device-management-ui` | Angular app |
| `database/` | Optional SQL scripts (`01-create.sql`, `02-seed.sql`, migration baseline) |

---

## More detail

- Connection string: `src/DeviceManagement.Api/appsettings.json` → `ConnectionStrings:DefaultConnection`  
- Seeded logins: `alice@company.test` / `bob@company.test`, password **`Passw0rd!`**
