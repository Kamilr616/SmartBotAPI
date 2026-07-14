# Getting Started

This guide walks through setting up the full SmartBotAPI stack: the web server, the database, and the robot firmware.

## Prerequisites

**Server:**

- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server — LocalDB (installed with Visual Studio) is enough for development; any SQL Server instance works in production
- Optional: Docker Desktop, Visual Studio 2022

**Firmware:**

- ESP32-C3 DevKitM-1 board and the robot hardware (see [firmware.md](firmware.md))
- Arduino IDE 2.x with the ESP32 board package
- Libraries: `ArduinoJson` (≥ 7.2), `SparkFun VL53L5CX Arduino Library` (≥ 1.0.3), `WebSockets` by Markus Sattler (≥ 2.6), `Adafruit MPU6050` (≥ 2.0.3), `Adafruit Unified Sensor`

## 1. Run the Web Server

```powershell
git clone https://github.com/Kamilr616/SmartBotAPI.git
cd SmartBotAPI/src/server/SmartBotBlazorApp
$env:RobotApiKey = "replace-with-a-url-safe-random-key-of-at-least-32-characters"
dotnet restore
dotnet run --launch-profile https
```

On first start the app creates the database and applies all EF Core migrations automatically. Endpoints:

| Profile | URL |
|---|---|
| `https` | `https://localhost:7297` (+ `http://localhost:5221`) |
| `http` | `http://localhost:5221` |
| Docker | `http://localhost:8080`, `https://localhost:8081` |

Open the site in a browser — the **Home** page and the navigation menu list all dashboard pages.

### Custom database

Set the environment variable before starting; it overrides `appsettings.json`:

```powershell
$env:SmartBotDBConnectionString = "Server=...;Database=SmartBot;..."
dotnet run --launch-profile https
```

### Docker

```bash
cd SmartBotAPI
docker build -t smartbotblazorapp -f src/server/SmartBotBlazorApp/Dockerfile .
docker run -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e SmartBotDBConnectionString="<connection-string>" \
  -e RobotApiKey="<same-long-random-key-as-the-firmware>" \
  smartbotblazorapp
```

> The container cannot reach LocalDB — always provide a real SQL Server connection string when running in Docker.

## 2. Flash the Robot Firmware

1. **Create the secrets file** `src/arduino/sketch_robot_signalr/arduino_secrets.h` (git-ignored):

   ```cpp
   #define SECRET_API_KEY "same-url-safe-random-key-as-the-server"

   #define SECRET_SSID  "primary-wifi"
   #define SECRET_PASS  "primary-password"
   #define SECRET_SSID2 "fallback-wifi"
   #define SECRET_PASS2 "fallback-password"
   #define SECRET_SSID3 "third-wifi"
   #define SECRET_PASS3 "third-password"
   ```

   All three Wi-Fi pairs must be defined (they may repeat). `SECRET_API_KEY` must be URL-safe, at least 32 characters long, and identical to the server's `RobotApiKey`.

2. **Point the firmware at your server** in `config.h`:

   ```cpp
   #define SERVER_IP   "your-server.example.com"  // or your LAN IP / hostname
   #define SERVER_PORT 443                        // 443 = TLS; use the HTTP port for local testing
   ```

   For local development, use the machine's LAN IP and the server's HTTP port, and make sure the firewall allows inbound connections.

3. **Upload** — Arduino IDE: select board *ESP32C3 Dev Module / DevKitM-1*, set *Tools → Partition Scheme → Huge APP (3MB No OTA/1MB SPIFFS)*, open `sketch_robot_signalr.ino`, and click Upload. The default 1.2 MB application partition is too small for the current firmware and libraries.

4. **Verify** — open the serial monitor at **115200 baud**. The firmware logs Wi-Fi association, the WebSocket/SignalR handshake, and each motor command it receives. The NeoPixel LED also indicates connection status.

## 3. Drive the Robot

1. Sign in to the dashboard and navigate to **Image Receiver** (`/image-receiver-server`).
2. When the robot is connected, telemetry values (acceleration, rotation, temperature, average distance) update live.
3. Drive with the on-screen joystick (mouse/touch) or the arrow keys. The speedometer gauges show the PWM value sent to each motor.
4. Watch the depth camera output on **Image Receiver** (`/image-receiver-server`, rendered heatmap) or **Matrix Receiver** (`/matrix-receiver-server`, interpolated 32×32 grid).
5. Review history on **Measurement Charts** (`/measurement-charts`) — pick a date range to plot temperature, distance, acceleration, and rotation.

### Control reference

The joystick supports proportional differential steering across its full circular
range. Move it vertically for straight driving, diagonally for a progressively tighter
turn, or horizontally to run the motors in opposite directions and rotate around the
robot's own axis. Returning it to the center or releasing the pointer stops the robot.

To use the keyboard, focus the control panel and use:

| Key | Result |
|---|---|
| `↑` | Full-speed forward |
| `↓` | Full-speed reverse |
| `←` | Rotate left in place |
| `→` | Rotate right in place |
| Release the key | Stop |

The speedometer gauges show the actual `-255…+255` PWM command for each motor. The
joystick is proportional; the keyboard mapping is intentionally discrete and uses
full power.

## Troubleshooting

| Symptom | Likely cause / fix |
|---|---|
| Robot never connects | Missing/mismatched `RobotApiKey` and `SECRET_API_KEY`; wrong `SERVER_IP`/`SERVER_PORT`; TLS port used against a plain-HTTP server; firewall blocking the port |
| Firmware build reports `text section exceeds available space` | Select *Tools → Partition Scheme → Huge APP (3MB No OTA/1MB SPIFFS)* for the ESP32-C3 target |
| `JSON Error` in serial log | Server and firmware SignalR protocol mismatch — confirm the hub is mapped at `/signalhub` |
| Robot connects, motors don't move | Motors auto-stop after 700 ms idle — commands must arrive continuously; check the `stopMotor` safety state and wiring against `config.h` pins |
| Dashboard shows no data | Robot not connected, or the hub connection failed — check the browser console for SignalR negotiation errors |
| Startup DB error | LocalDB not installed or connection string invalid — set `SmartBotDBConnectionString` |
