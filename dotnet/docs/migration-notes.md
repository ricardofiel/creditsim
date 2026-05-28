# Migration Notes: Node.js → .NET Framework 4.7.2

## Package Mapping

| npm package | NuGet package | Version | Notes |
|---|---|---|---|
| `express` | `Microsoft.AspNet.WebApi` | 5.3.0 | Web API 2 runtime |
| `cors` (middleware) | `Microsoft.AspNet.WebApi.Cors` | 5.3.0 | `[EnableCors]` attribute |
| `helmet` | Custom HTTP headers in `Web.config` `<customHeaders>` | — | See Web.config `<httpProtocol>` |
| `express-validator` | `System.ComponentModel.DataAnnotations` | BCL | `[Required]`, `[Range]`, custom `ValidationAttribute` |
| `sqlite3` | `System.Data.SQLite` | 1.0.118 | Windows/Linux native SQLite binding |
| `async/await` (Node) | `async Task<T>` / `await` (C# 5+) | — | Same paradigm |
| `process.env.PORT` | `<appSettings>` in `Web.config` | — | |
| `console.error` | `log4net` | 2.0.15 | Configured via `Web.config` `<log4net>` section |
| `jest` | `MSTest.TestFramework` + `MSTest.TestAdapter` | 3.6.4 | MSTest v2 |
| `supertest` | `System.Web.Http` test helpers + `Moq` | 4.20.72 | Unit tests with mocked dependencies |
| N/A (no ORM) | `Dapper` | 2.1.35 | Lightweight micro-ORM for SQL queries |

---

## Behavioral Differences

### 1. `POST /api/simulate` → Returns 201 in both implementations

The original Node.js code returns HTTP 201. The C# controller uses `Content(HttpStatusCode.Created, ...)` to match this.

### 2. Validation error response shape

The original `express-validator` response includes `details` as an array with `msg`, `path`, `location` fields.  
The C# implementation returns:
```json
{ "error": "Validation failed", "details": [{ "field": "...", "message": "..." }] }
```
The shape is slightly different (field/message vs msg/path/location) but functionally equivalent.

### 3. `GET /api/health` — `uptime` field

Node.js: `process.uptime()` — time since the Node.js process started.  
C#: `(DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime()).TotalSeconds` — time since the IIS Express worker process started. The numeric value will differ but the semantics are the same.

### 4. `createdAt` timestamp format

Node.js SQLite stores `CURRENT_TIMESTAMP` which uses the `YYYY-MM-DD HH:MM:SS` format (UTC).  
The C# API serializes `DateTime` to ISO 8601 (`o` format specifier), producing `2024-01-15T10:30:00.0000000Z`.  
Frontend JavaScript (`new Date(str)`) parses both formats correctly.

### 5. Score rounding

Node.js: `Math.round(score)` — result is always an integer.  
C#: `(int)Math.Round((double)score)` — identical behavior.

### 6. WebForm vs. static index.html

The original `public/index.html` served both the simulation form and a dynamic simulations list.  
In the .NET version:
- `Default.aspx` — server-rendered customer list with `GridView` paging (replaces the "Previous Simulations" section)
- `public/simulate.html` — the same simulation form as before (was `public/index.html`), still powered by `public/app.js` calling `/api/*`

---

## Known Limitations

| Limitation | Impact |
|---|---|
| `System.Data.SQLite` ships Windows/Linux x86/x64 native DLLs; ARM not supported | Cannot run on ARM servers |
| `System.Web` (WebForms) is Windows-only at runtime | IIS Express required; cannot deploy to Linux IIS |
| `Microsoft.AspNet.WebApi` 5.x does not support .NET Core/5+ | Project is locked to Windows deployment |
| `log4net` 2.0.15 has a known moderate CVE ([GHSA-4f7c-pmjv-c25w](https://github.com/advisories/GHSA-4f7c-pmjv-c25w)) | Consider upgrading to 2.0.16+ when compatible |
| No built-in DI container | Controllers use constructor overloads: parameterless for IIS, injectable for tests |

---

## How to Run Locally

### Prerequisites

- Visual Studio 2022 (Community or higher) with **ASP.NET and web development** workload
- .NET Framework 4.7.2 Developer Pack
- Windows 10/11 or Windows Server 2019+

### Steps

1. **Open the solution**
   ```
   File → Open → Solution → dotnet/CreditSim.sln
   ```

2. **Restore NuGet packages**  
   Visual Studio restores packages automatically on build. Or run:
   ```
   dotnet restore dotnet/CreditSim.sln
   ```

3. **Set startup project**  
   Right-click `CreditSim.Web` → Set as Startup Project.

4. **Run (F5)**  
   IIS Express will start at `http://localhost:5000/`.  
   - Customer list: `http://localhost:5000/`  
   - Simulation form: `http://localhost:5000/public/simulate.html`  
   - API: `http://localhost:5000/api/health`

5. **Pre-existing data**  
   The repository's `data/creditsim.db` is reused. On first run, 30 seed records are inserted if the table is empty.

---

## How to Run Tests

### On Windows (full suite — net472)

**Visual Studio Test Explorer:**
```
Test → Run All Tests
```

**Command line:**
```bash
dotnet test dotnet/CreditSim.sln
```

### On Linux / macOS (CI — net8.0 unit tests only)

```bash
dotnet test dotnet/tests/CreditSim.Core.Tests/CreditSim.Core.Tests.csproj --framework net8.0
```

This runs the 31 `CreditScoringService` unit tests (no Windows/Mono dependency).  
`SimulationControllerTests` and `DatabaseRepositoryTests` require .NET Framework (net472) and run on Windows.

### Test count

| Test class | Count | Runs on |
|---|---|---|
| `CreditScoringServiceTests` | 20 | net8.0 + net472 |
| `SimulationControllerTests` | 11 | net472 (Windows) |
| `DatabaseRepositoryTests` | 5 | net472 (Windows) |
| **Total** | **36** | |

Jest test count (Node.js): `creditScoring.test.js` (18) + `api.test.js` (14) = **32**  
C# test count: **36** ≥ 32 ✅
