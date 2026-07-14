# AGENTS.md

Guidance for AI coding agents working in this repository.

## Project
**SmartBot** platform — an **ASP.NET Core 8 / Blazor** (Server + WASM client) app that
receives a live depth/matrix image stream from a robot over **SignalR** and visualizes
it on a dashboard. Uses **EF Core** for persistence; was deployed on **Azure App
Service**. Robot firmware is an Arduino sketch that connects to the SignalR hub.

## Layout
- `src/server/SmartBotBlazorApp/` — server app (hub, data, migrations, seed).
- `src/server/SmartBotBlazorApp.Client/` — Blazor WASM client.
- `src/server/SmartBotBlazorApp.Tests/` — unit tests.
- `src/arduino/sketch_robot_signalr/` — robot firmware (+ `arduino_secrets.example.h`).
- `docs/` — architecture / getting-started / server / firmware notes.
- `other/media/` — README screenshots, photos, and demo video.

## Build / run / test
- Build/run with the **.NET 8 SDK** (`dotnet build` / `dotnet run`).
- Tests: `dotnet test` (see `docs/getting-started.md`).

## Conventions & good practices
- The SignalR hub is **access-controlled** (authenticated Identity users, or a robot
  `RobotApiKey` compared in constant time). Don't remove the authorization gate.
- **Never commit secrets** — `RobotApiKey`, connection strings, and `arduino_secrets.h`
  stay out of source (`*.example` templates only).
- **Do not edit existing EF migrations' `Migration` IDs** or `Up/Down` bodies; add a new
  migration instead. Keep the model snapshot in sync.
- Update **both** `README.md` and `README.pl.md` together.

## Documentation
- [README.md](README.md) · [README.pl.md](README.pl.md)
- [`docs/architecture.md`](docs/architecture.md) · [`docs/getting-started.md`](docs/getting-started.md) · [`docs/firmware.md`](docs/firmware.md)
- License — see [LICENSE](LICENSE).

_Educational / portfolio project._
