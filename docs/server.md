# Server Application Reference

The web application lives in `src/server/` and consists of two projects:

| Project | Type | Purpose |
|---|---|---|
| `SmartBotBlazorApp` | ASP.NET Core 8.0 (Blazor Web App host) | SignalR hub, EF Core persistence, Identity, image processing, server-rendered pages |
| `SmartBotBlazorApp.Client` | Blazor WebAssembly 8.0 | Browser-side interactive pages (robot control) |

## Pages

| Route | Component | Render mode | Description |
|---|---|---|---|
| `/` | `Home.razor` | Server | Landing page and project overview |
| `/chat` | `Client/Pages/Chat.razor` | WebAssembly | Robot control: virtual joystick, keyboard input (arrow keys, throttled to one command per 250 ms), dual speedometer gauges, live telemetry readout |
| `/image-receiver-server` | `ImageReceiver_server.razor` | Server | Live 512×512 depth heatmap streamed as base64 PNG |
| `/matrix-receiver-server` | `MatrixReceiver_server.razor` | Server | Live 32×32 interpolated depth grid with per-cell coloring (supports `[Authorize]`) |
| `/measurement-charts` | `MeasurementCharts.razor` | Server | Historical MudBlazor line charts: temperature, average distance, acceleration (X/Y/Z), rotation (X/Y/Z); date-range picker, defaults to the last 5 minutes |
| `/weather` | `Weather.razor` | Server (stream rendering) | Air-quality data pulled from an external GreenCity/InPost sensor API |
| `/Account/*` | Identity pages | Server | Registration, login, 2FA, password reset, account management |

## Services

### `Hubs/SignalHub.cs` — SignalR hub (`/signalhub`)

The single real-time endpoint shared by robots and dashboards. See [architecture.md](architecture.md) for the full message contract. On each `ReceiveRobotData` invocation it concurrently:

1. saves the measurement (`MeasurementService`),
2. broadcasts a heatmap frame (`ReceiveBase64Frame`),
3. broadcasts the interpolated matrix (`ReceiveMatrix`).

### `ImageProcessor.cs`

- `GenerateHeatmapBase64Image(ushort[64])` — renders the 8×8 depth frame to a PNG heatmap (ImageSharp) using a red→yellow→green→blue→purple gradient normalized over 10–3500 mm, returned base64-encoded.
- `InterpolateData(ushort[64], 32)` — bilinear upsampling of the frame to 32×32 for the matrix view.

### `Data/MeasurementService.cs`

- `SaveMeasurementsToDatabase(user, measurements, avgDistance)` — inserts a `Measurement` row, throttled to at most one write per second.
- `RoundMeasurements(measurements, digits)` — display rounding (2 decimal places).
- Date-range query used by the charts page.

### `Data/ApplicationDbContext.cs`

EF Core context combining ASP.NET Core Identity tables with the `Measurement` telemetry table. Migrations live in `Data/Migrations/` and are applied automatically at startup.

## Data Model

`Measurement` (`Data/Measurement.cs`):

| Column | Type | Notes |
|---|---|---|
| `Id` | int | Primary key |
| `RobotId` | string (≤ 50) | Required; identifies the sending robot |
| `TemperatureC` | double | From the MPU6050 die sensor |
| `AccelerationX/Y/Z` | double | m/s² |
| `RotationX/Y/Z` | double | °/s |
| `AvgDistance` | ushort | Average ToF distance, mm |
| `Timestamp` | DateTime | Defaults to insert time |

## Configuration Reference

### `appsettings.json`

| Key | Purpose |
|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection (LocalDB by default) |
| `Logging:LogLevel:*` | Standard ASP.NET Core logging levels |
| `Api:Url` | External API base URL placeholder |
| `AllowedHosts` | Host filtering (`*` by default) |

### Environment variables

| Variable | Purpose |
|---|---|
| `SmartBotDBConnectionString` | Overrides the connection string (used in Docker/Azure) |
| `ASPNETCORE_ENVIRONMENT` | `Development` / `Production` |
| `ASPNETCORE_HTTP_PORTS` / `ASPNETCORE_HTTPS_PORTS` | Container port bindings (8080/8081 in Docker) |

### Ports (`Properties/launchSettings.json`)

| Profile | HTTP | HTTPS |
|---|---|---|
| Kestrel (`http`/`https`) | 5221 | 7297 |
| IIS Express | 24385 | 44312 |
| Docker | 8080 | 8081 |

## Middleware Pipeline (from `Program.cs`)

Response compression (including `application/octet-stream` for SignalR payloads) → CORS (`CorsPolicy`, currently allow-all origins) → HTTPS redirection → static files → antiforgery → Razor components (Server + WASM render modes) → SignalR hub at `/signalhub` → Identity endpoints.

Notes for hardening before production use:

- CORS is configured with `WithOrigins("*")` — restrict to known origins.
- `IEmailSender` is a no-op stub (`IdentityNoOpEmailSender`) — plug in a real sender for account confirmation emails.
- Authorization on the receiver pages is partially commented out — enable `@attribute [Authorize]` where access should be restricted.

## Deployment

- **Docker:** `src/server/SmartBotBlazorApp/Dockerfile` (multi-stage: SDK build → publish → `mcr.microsoft.com/dotnet/aspnet:8.0` runtime, exposing 8080/8081). The project also defines .NET SDK container metadata (`kamilr616/smartbotblazorapp:latest`).
- **CI/CD:** `.github/workflows/smartbotweb.yml` builds, tests, publishes, and deploys to the `smartbotweb` Azure App Service on every push to `main`, authenticated with a publish-profile secret.
