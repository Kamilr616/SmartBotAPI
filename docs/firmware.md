# Firmware & Hardware Reference

The production firmware lives in `src/arduino/sketch_robot_signalr/` and targets the **ESP32-C3 DevKitM-1**. Legacy and experimental sketches (plain Arduino, WebSocket tests, a PlatformIO project) are kept under `other/`.

## Bill of Materials

| Part | Role | Interface | Datasheet |
|---|---|---|---|
| ESP32-C3 DevKitM-1 | MCU, Wi-Fi, control loop | — | — |
| ST VL53L5CX | 8×8 multizone time-of-flight depth sensor | I²C (400 kHz) | [`vl53l5cx.pdf`](vl53l5cx.pdf), [`um2884`](um2884-a-guide-to-using-the-vl53l5cx.pdf), [`um2887`](um2887-software-integration-guide.pdf) |
| InvenSense MPU6050 | 6-axis IMU (accelerometer + gyroscope) + die temperature | I²C | — |
| Toshiba TB6612FNG | Dual H-bridge motor driver (2 DC motors) | GPIO + PWM | [`TB6612FNG_datasheet_en_20121101.pdf`](TB6612FNG_datasheet_en_20121101.pdf) |
| NeoPixel RGB LED | Connection status indicator | Single-wire (GPIO 10) | — |

The full circuit schematic is in [`schemat.pdf`](schemat.pdf); [`adafruit_tb6612_schem.png`](adafruit_tb6612_schem.png) shows the motor-driver breakout reference.

## Pinout (from `config.h`)

| Signal | GPIO |
|---|---|
| Motor A IN1 / IN2 / PWM | 3 / 4 / 5 |
| Motor B IN1 / IN2 / PWM | 2 / 1 / 0 |
| VL53L5CX enable (`TOF_PIN`) | 9 |
| NeoPixel data | 10 |
| I²C SDA / SCL | 8 / 7 (bus at 400 kHz) |

## Configuration (`config.h`)

| Constant | Default | Meaning |
|---|---|---|
| `SERVER_IP` / `SERVER_PORT` | `your-server.example.com` / `443` | SignalR hub address; replace the placeholder with the current server hostname or LAN IP |
| `SERIAL_BAUDRATE` | 115200 | Debug serial speed |
| `WS_RECONNECT_INTERVAL` | 5000 ms | WebSocket reconnect period |
| `MAX_IDLE_TIME` | 700 ms | Motors auto-stop if no command arrives within this window |
| `MIN_DISTANCE` | 400 mm | Obstacle threshold used by the safety logic |
| `TOF_RANGING_FREQ` | 15 Hz | Depth frame rate |
| `TOF_INTEGRATION_TIME` | 5 ms | Defined for ToF integration, but the corresponding `setIntegrationTime` call is currently commented out; the sensor library default remains active |
| `TOF_SHARPENER_PERCENT` | 18 % | On-chip sharpening |
| `TOF_TARGET_ORDER` | `CLOSEST` | Multi-target priority |
| `MPU_G_RANGE` | ±2 g | Accelerometer full scale |
| `MPU_DEG_RANGE` | ±250 °/s | Gyroscope full scale |
| `I2C_FREQ` | 400 kHz | Sensor bus clock |

The robot API key and Wi-Fi credentials come from `arduino_secrets.h` (git-ignored). `WiFiMulti` picks whichever configured network is reachable, and the API key authenticates the WebSocket connection. See [getting-started.md](getting-started.md) or `arduino_secrets.example.h` for the template.

## Control Loop

1. **Connect** — join Wi-Fi (multi-network), open an API-key-authenticated WebSocket to `SERVER_IP:SERVER_PORT` (TLS on 443), perform the SignalR JSON handshake, and set the status LED.
2. **Sense** — read the 8×8 VL53L5CX frame at 15 Hz and the MPU6050 (acceleration, rotation, temperature). The average distance is computed over the central 4×4 zone of the frame.
3. **Publish** — invoke `ReceiveRobotData(user, measurements[7], rawMatrix[64], avgDistance)` on the hub.
4. **Actuate** — on each `ReceiveRobotCommand(motorB, motorA)` message, clamp values to `-255…+255`, set direction pins and PWM duty on the TB6612FNG, and reset the idle timer.

### Safety behavior

- **Dead-man timer:** if no movement command arrives for `MAX_IDLE_TIME` (700 ms), both motors stop. The dashboard therefore sends commands continuously while the joystick is held.
- **Obstacle guard:** forward motion is inhibited when the central 4×4 average distance drops below `MIN_DISTANCE` (400 mm); reversing away from the obstacle remains possible.
- **Link watchdog:** the WebSocket client retries the connection every 5 s; with no incoming commands the dead-man timer stops the motors within 700 ms of a link loss. If Wi-Fi cannot be joined at boot (~40 attempts), the board reboots itself.

### Status LED colors

| Color | Meaning |
|---|---|
| Yellow | Connecting to Wi-Fi |
| Blue | Wi-Fi connected |
| Cyan | Sensors initialized |
| Green | Hub connected / path clear |
| Red | Disconnected or obstacle below 400 mm |
| Magenta | WebSocket error / boot |

## Building

**Arduino IDE 2.x** — install the ESP32 board package, select *ESP32C3 Dev Module* (DevKitM-1), set *Tools → Partition Scheme → Huge APP (3MB No OTA/1MB SPIFFS)*, install the libraries below, open `sketch_robot_signalr.ino`, and upload. The default 1.2 MB application partition is too small for the current firmware and libraries.

The PlatformIO project in `other/241111-183051-esp32-c3-devkitm-1/` is an archived development snapshot and does not contain the current robot authentication flow. Do not use it as a build source for the production firmware.

**Reference library versions** (recorded in the archived PlatformIO project):

- `ArduinoJson` 7.2.0
- `SparkFun VL53L5CX Arduino Library` 1.0.3
- `WebSockets` (Markus Sattler) 2.6.1
- `Adafruit MPU6050` 2.0.3
- `Adafruit Unified Sensor` 1.1.4
