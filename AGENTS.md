## Cursor Cloud specific instructions

### Overview

This is a .NET 10 / ASP.NET Core + Blazor WASM application for managing miniature painting projects ("Painting Projects Management"). It uses .NET Aspire to orchestrate all services (PostgreSQL, RabbitMQ, API, UI, pgAdmin).

### Prerequisites (installed by VM snapshot)

- .NET 10 SDK (installed at `/usr/share/dotnet`, linked to `/usr/local/bin/dotnet`)
- Docker (required for Aspire containers and Testcontainers in integration tests)
- A symlink at `/workspace/back/PaintingProjectsManagment.Database` pointing to `/workspace/back/Database/PaintingProjectsManagment.Database` (needed for database seed file path resolution)

### Key commands

All commands run from `/workspace/back`.

| Task | Command |
|------|---------|
| Restore | `dotnet restore PaintingProjectsManagment.slnx` |
| Build | `dotnet build PaintingProjectsManagment.slnx` |
| Test | `dotnet test --solution PaintingProjectsManagment.slnx` |
| Run (dev) | `dotnet run --project Aspire/PaintingProjectsManagement.AppHost -- --allow-unsecured-transport` |

### Running the application

The Aspire AppHost orchestrates everything. Pass `--allow-unsecured-transport` to avoid HTTPS cert issues in headless environments. The Aspire dashboard (at `https://localhost:17257`) shows all resource endpoints.

**Important**: Always access the UI via the **HTTPS** endpoint `https://localhost:7114`, not the HTTP endpoint on port 5251. The API enforces HTTPS redirect, so the Blazor WASM client must be served over HTTPS for API calls to work through Aspire's service discovery. Using `http://localhost:5251` will cause login and all API calls to fail silently.

| Service | URL |
|---------|-----|
| UI | `https://localhost:7114` |
| API | `https://localhost:7236` |
| Aspire Dashboard | `https://localhost:17257` |
| pgAdmin | `http://localhost:5050` |
| RabbitMQ Management | `http://localhost:15672` |

### Docker startup

Docker must be running before starting Aspire. In this Cloud VM, start dockerd manually:

```
sudo dockerd &>/tmp/dockerd.log &
sudo chmod 666 /var/run/docker.sock
```

### Database seed gotcha

The API's database seed code walks up parent directories looking for `PaintingProjectsManagment.Database/Seed/models_seed.sql`. The actual file is under `Database/PaintingProjectsManagment.Database/`, so a symlink is needed at `/workspace/back/PaintingProjectsManagment.Database` → `Database/PaintingProjectsManagment.Database`. Without this symlink, the API crashes on startup with a `NullReferenceException` during seed.

Additionally, the Projects seed depends on Materials being propagated via RabbitMQ integration events. On first startup with a clean database, the Projects seed may fail because the event-driven material projections aren't ready yet. Workaround: manually populate `projects.projections.materials` from `materials.materials`, then restart.

### Testing notes

- Tests use **TUnit** with `Microsoft.Testing.Platform` (configured in `global.json`). The `--filter` flag is not supported; use `--treenode-filter` for TUnit-based filtering.
- Integration tests use **Testcontainers** and automatically spin up ephemeral PostgreSQL + RabbitMQ Docker containers.
- Playwright UI tests require browser binaries (`pwsh bin/Debug/net10/playwright.ps1 install`); they will fail without them.
- The project explicitly sets `TreatWarningsAsErrors=false` in `Directory.Build.props`. Some NU1903/NU1506 warnings are expected.

### Seeded user accounts

| Username | Password | Tenant |
|----------|----------|--------|
| `rodrigo.basniak` | `trustno1` | RODRIGO.BASNIAK |
| `ricardo.smarzaro` | `zemiko987` | RICARDO.SMARZARO |
| `superuser` | `admin` | (admin, all tenants) |
