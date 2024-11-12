
/*
  SmartBotDevice
  By: Kamil Rataj,
      Mateusz Ciszek,
      Izabela Panek.
*/

#include <WiFi.h>                       // "WiFi" by Arduino
#include <WebSocketsClient.h>           //"WebSockets" by Markus Sattler  TODO: check deprecated func
#include <Wire.h>                       // I2C
#include <SparkFun_VL53L5CX_Library.h>  // "SparkFun VL53L5CX Arduino Library" by SparkFun
#include <ArduinoJson.h>
#include <Adafruit_MPU6050.h>
#include <Adafruit_Sensor.h>

#include "arduino_secrets.h"
#include "config.h"


const char ssid[] = SECRET_SSID;           // WiFi SSID
const char password[] = SECRET_PASS;       // WiFi Password
const char websocketServer[] = SERVER_IP;  // API URL
const int websocketPort = SERVER_PORT;     // API PORT

WebSocketsClient webSocket;
SparkFun_VL53L5CX myImager;
VL53L5CX_ResultsData measurementData;  // Result data class structure, 1356 bytes of RAM
Adafruit_MPU6050 mpu;

sensors_event_t a, g, temp;

const int imageWidth = 8;
const int imageResolution = 64;
//const size_t dataSize = 128;

//static unsigned long lastTime = 0;
//unsigned long currentMillis;


void setLEDColor(uint8_t r = 0, uint8_t g = 0, uint8_t b = 0) {
  rgbLedWrite(PIN_NEOPIXEL, g, r, b);
}

void webSocketEvent(WStype_t type, uint8_t *payload, size_t length) {
  switch (type) {
    case WStype_DISCONNECTED:
      USE_SERIAL.println("[WS] Disconnected from WebSocket.");
      USE_SERIAL.printf("[WS] Payload from server: %s\n", payload);
      setLEDColor(255, 0, 0);  // Red
      break;
    case WStype_CONNECTED:
      USE_SERIAL.printf("[WS] Connected to url: %s\n", payload);
      webSocket.sendTXT("{\"protocol\": \"json\", \"version\": 1}");  // Signalr protocol
      setLEDColor(0, 255, 0);                                          // Green
      break;
    case WStype_TEXT:
      USE_SERIAL.printf("[WS] Message from server: %s\n", payload);
      // TODO: Handle incoming messages
      break;
    case WStype_BIN:
      USE_SERIAL.printf("[WS] Binary data received, length: %u\n", length);
      break;
    case WStype_ERROR:
      USE_SERIAL.println("[WS] Error occured!");
      setLEDColor(255, 0, 128);
      break;
    case WStype_FRAGMENT_TEXT_START:
      USE_SERIAL.println("[WS] Text fragment start received!");
      break;
    case WStype_FRAGMENT_BIN_START:
      USE_SERIAL.println("[WS] Binary fragment start received!");
      break;
    case WStype_FRAGMENT:
      USE_SERIAL.println("[WS] Fragment received!");
      break;
    case WStype_FRAGMENT_FIN:
      USE_SERIAL.println("[WS] Fragment finished received!");
      break;
    case WStype_PING:
      USE_SERIAL.println("[WS] PING recieved!");
      break;
    case WStype_PONG:
      USE_SERIAL.println("[WS] PONG recieved!");
      break;
    default:
      USE_SERIAL.println("[WS] Event not defined!");
      setLEDColor(255, 0, 128);
      break;
  }
}

String createDataString(const VL53L5CX_ResultsData &measurementData, sensors_event_t &a, sensors_event_t &g, sensors_event_t &temp, const int width = 8, const int resolution = 64) {
  JsonDocument doc;
  uint32_t totalCenterDistance = 0;
  int centerCount = 0;
  uint16_t avgDistance = 0;

  doc["type"] = 1;
  doc["target"] = "ReceiveRobotData";  //method
  doc["arguments"][0] = "Robot";       //user
  doc["arguments"][1] = (double_t)temp.temperature;          //tempMeasure

  JsonArray measurements = doc["arguments"][2].to<JsonArray>();
  JsonArray distances = doc["arguments"][3].to<JsonArray>();

  measurements[0] = (double_t)a.acceleration.x;
  measurements[1] = (double_t)a.acceleration.y;
  measurements[2] = (double_t)a.acceleration.z;
  measurements[3] = (double_t)g.gyro.x;
  measurements[4] = (double_t)g.gyro.y;
  measurements[5] = (double_t)g.gyro.z;

  for (int y = (width * (width - 1)); y >= 0; y -= width) {
    for (int x = 0; x < width; x++) {
      uint16_t distance = measurementData.distance_mm[x + y];
      distances.add(distance);

      // Calculate avgDistance only for the central 4x4 block
      if (x >= 2 && x <= 5 && (y / width) >= 2 && (y / width) <= 5) {
        totalCenterDistance += distance;
        centerCount++;
      }
    }
  }

  avgDistance = (centerCount > 0) ? (totalCenterDistance / centerCount) : 0;
  doc["arguments"][4] = avgDistance;  //avgDistance

  String jsonString;
  serializeJson(doc, jsonString);
  jsonString += "";

  return jsonString;
}

void waitForWiFiConnectOrReboot(bool printOnSerial = true, int numOfAttempts = 50) {
  int notConnectedCounter = 0;
  setLEDColor(255, 255, 0);  // Yellow
  while (WiFi.status() != WL_CONNECTED) {
    delay(800);
    if (printOnSerial) {
      USE_SERIAL.printf("[WIFI] Connecting to %s ...\n", ssid);
    }

    notConnectedCounter++;
    if (notConnectedCounter > numOfAttempts) {  // Reset board if not connected
      if (printOnSerial) {
        USE_SERIAL.println("[WIFI] Resetting due to Wifi not connecting...");
      }
      ESP.restart();
    }
  }
  if (printOnSerial) {
    USE_SERIAL.printf("[WIFI] Connected to %s ! Robot IP address: ", ssid);
    USE_SERIAL.println(WiFi.localIP());
    setLEDColor(0, 0, 255);  // Blue
  }
}


void setup() {
  setLEDColor(255, 0, 128);
  USE_SERIAL.begin(SERIAL_BAUDRATE);
  USE_SERIAL.println();

  for (uint8_t t = 4; t > 0; t--) {
    USE_SERIAL.printf("[SETUP] BOOT WAIT %d...\n", t);
    USE_SERIAL.flush();
    delay(500);
  }

  Wire.begin(8, 7);         // Use pins 8 (SDA), 7 (SCL) for I2C
  Wire.setClock(I2C_FREQ);  // Set I2C frequency

  pinMode(9, OUTPUT);
  // pinMode(0, OUTPUT);
  // pinMode(1, OUTPUT);
  // pinMode(2, OUTPUT);
  // pinMode(3, OUTPUT);
  // pinMode(4, OUTPUT);
  // pinMode(5, OUTPUT);

  digitalWrite(9, LOW);

  USE_SERIAL.println("USE_SERIAL communication initialized!");

  WiFi.begin(ssid, password);  // Connect to WiFi
  waitForWiFiConnectOrReboot(USE_SERIAL, 50);

  if (myImager.begin() == false) {
    USE_SERIAL.println("Failed to initialize VL53L5CX sensor. Restarting ...");
    ESP.restart();
  } else if (!mpu.begin()) {
    Serial.println("Failed to find MPU6050 chip. Restarting ...");
    ESP.restart();
  } else {
    setLEDColor(0, 255, 255);  // Cyan
  }

  myImager.setResolution(imageResolution);
  myImager.setRangingFrequency(TOF_RANGING_FREQ);
  myImager.setRangingMode(TOF_RANGING_MODE);
  myImager.setSharpenerPercent(TOF_SHARPENER_PERCENT);
  myImager.setTargetOrder(TOF_TARGET_ORDER);
  myImager.setIntegrationTime(TOF_INTEGRATION_TIME);

  mpu.setAccelerometerRange(MPU_G_RANGE);
  mpu.setGyroRange(MPU_DEG_RANGE);
  mpu.setFilterBandwidth(MPU_HZ_BAND);
  mpu.setCycleRate(MPU_HZ_CYCLE);
  mpu.enableSleep(false);

  webSocket.beginSSL(websocketServer, websocketPort, "/signalhub");  // Initialize WebSocket client
  webSocket.setReconnectInterval(WS_RECONNECT_INTERVAL);
  //webSocket.enableHeartbeat(pingInterval,pongTimeout,disconnectTimeoutCount); // TODO
  webSocket.onEvent(webSocketEvent);  // Set event handler

  myImager.startRanging();  // Start ranging
  mpu.enableCycle(true);
  delay(100);
}

void loop() {
  webSocket.loop();  // Handle WebSocket events and communication
  //currentMillis = millis();                                                                      // Get the current time
  if (myImager.isDataReady() && webSocket.isConnected()) {  // Poll the VL53L5CX sensor for new data  TODO: Attach the interrupt
    setLEDColor(64, 0, 64);                                 // White

    if (myImager.getRangingData(&measurementData) && mpu.getEvent(&a, &g, &temp))  // Read distance data into array
    {
      String data = createDataString(measurementData, a, g, temp, imageWidth, imageResolution);
      webSocket.sendTXT(data);
    }

    setLEDColor(0, 128, 0);  // Green
  }
}
