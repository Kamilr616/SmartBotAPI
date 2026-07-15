# Server Application Reference

The web application lives in `src/server/` and consists of two projects:

| Project | Type | Purpose |
|---|---|---|
| `SmartBotBlazorApp` | ASP.NET Core 8.0 (Blazor Web App host) | SignalR hub, EF Core persistence, Identity, image processing, server-rendered pages |
| `SmartBotBlazorApp.Client` | Blazor WebAssembly 8.0 | Browser-side diagnostic/auth pages; `/chat` connects to SignalR from WASM |

## Pages

| Route | Component | Render mode | Description |
|---|---|---|---|
| `/` | `Home.razor` | Server | Landing page and project overview |
| `/chat` | `Client/Pages/Chat.razor` | WebAssembly | Text-message diagnostics for the SignalR connection |
| `/image-receiver-server` | `ImageReceiver_server.razor` | Server | Authorized live 512×512 depth heatmap plus joystick/keyboard robot control and speed gauges |
| `/matrix-receiver-server` | `MatrixReceiver_server.razor` | Server | Authorized live 32×32 interpolated depth grid with per-cell coloring and robot control |
| `/measurement-charts` | `MeasurementCharts.razor` | Server | Authorized historical MudBlazor charts: temperature, average distance, acceleration (X/Y/Z), rotation (X/Y/Z); date-range picker, defaults to the last 5 minutes |
| `/weather` | `Weather.razor` | Server (stream rendering) | Air-quality data pulled from an external GreenCity/InPost sensor API |
| `/Account/*` | Identity pages | Server | Registration, login, 2FA, password reset, account management |

## Robot Control Input

The control panel in `ImageReceiver_server.razor` accepts pointer events from mouse
or touch and keyboard events from the arrow keys:

- `Components/RobotMovementInput/JoystickInputHandler.cs` normalizes the two joystick
  axes, applies stop/straight-driving dead zones, and mixes them as `left = y + x`
  and `right = y - x`. This provides proportional straight motion, curved motion,
  and rotation around the robot's own axis from one joystick.
- `Components/RobotMovementInput/keyboardInputHandler.cs` maps the arrow keys to
  full-range `-255`, `0`, or `255` motor commands. Up/down drive both motors in the
  same direction; left/right drive them in opposite directions for rotation in place.
- `ImageReceiver_server.razor` sends commands through `SendMovementCommand`, updates
  both speed gauges, limits normal command transmission to one message per 250 ms,
  and sends `(0, 0)` when pointer or keyboard input is released.

See [architecture.md](architecture.md#motion-control) for the equations, exact key
mapping, and the interaction with the firmware dead-man timer.

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

- `SaveMeasurementsToDatabase(user, measurements, avgDistance)` — validates the seven-value IMU payload and inserts at most one row per second for each robot ID. The in-process throttle is synchronized across concurrent hub calls and uses UTC for elapsed-time comparisons while retaining local timestamps for the existing chart filters.
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
| `AccountAccess:AllowRegistration` | Enables the registration endpoint; `false` by default, `true` in Development |
| `AccountAccess:ShowSelfConfirmationLink` | Shows the no-email confirmation link; for local development only |
| `AllowedHosts` | Host filtering (`*` by default) |

### Environment variables

| Variable | Purpose |
|---|---|
| `SmartBotDBConnectionString` | Overrides the connection string (used in Docker/Azure) |
| `RobotApiKey` | URL-safe key (minimum 32 characters) accepted from robot connections to `/signalhub` |
| `ASPNETCORE_ENVIRONMENT` | `Development` / `Production` |
| `AccountAccess__AllowRegistration` / `AccountAccess__ShowSelfConfirmationLink` | Environment-variable overrides for account provisioning |
| `ASPNETCORE_HTTP_PORTS` / `ASPNETCORE_HTTPS_PORTS` | Optional container port bindings; the image serves HTTP on 8080 by default |

### Ports (`Properties/launchSettings.json`)

| Profile | HTTP | HTTPS |
|---|---|---|
| Kestrel (`http`/`https`) | 5221 | 7297 |
| IIS Express | 24385 | 44312 |
| Docker (default runtime) | 8080 | — |

## Middleware Pipeline (from `Program.cs`)

Response compression (including `application/octet-stream` for SignalR payloads) → HTTPS redirection → static files → Identity authentication/authorization → role-aware SignalR access guard → antiforgery → Razor components (Server + WASM render modes) → SignalR hub at `/signalhub` → Identity endpoints.

Notes for hardening before production use:

- Registration is disabled outside Development. Temporarily provision accounts in a trusted environment or connect a controlled administrative flow.
- `IEmailSender` is a no-op stub (`IdentityNoOpEmailSender`) — plug in a real sender before enabling registration publicly. Never enable the self-confirmation link publicly.

## Deployment

- **Docker:** `src/server/SmartBotBlazorApp/Dockerfile` (multi-stage .NET 10 SDK build → publish → non-root `mcr.microsoft.com/dotnet/aspnet:8.0` runtime, serving HTTP on 8080). The project also defines .NET SDK container metadata (`kamilr616/smartbotblazorapp:latest`).
- **CI:** `.github/workflows/smartbotweb.yml` performs locked restore, formatting and vulnerability checks, build, tests with coverage, publish, and a container build for pushes and pull requests targeting `main`. External deployment is intentionally manual; the Azure App Service used for the project presentation is no longer hosted.
