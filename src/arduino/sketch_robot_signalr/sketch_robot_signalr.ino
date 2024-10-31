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

#include "arduino_secrets.h"
#include "config.h"


const char ssid[] = SECRET_SSID;           // WiFi SSID
const char password[] = SECRET_PASS;       // WiFi Password
const char websocketServer[] = SERVER_IP;  // API URL
const int websocketPort = SERVER_PORT;     // API PORT

WebSocketsClient webSocket;
SparkFun_VL53L5CX myImager;
VL53L5CX_ResultsData measurementData;  // Result data class structure, 1356 bytes of RAM

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

String createRangingDataString(const VL53L5CX_ResultsData &measurementData, const int width = 8, const int resolution = 64) {
  JsonDocument doc;

  // Ustawienie podstawowych danych JSON
  doc["type"] = 1;
  doc["target"] = "ReceiveRawMatrix";
  doc["arguments"][0] = "Robot";

  // Serializacja danych odległości do tablicy JSON
  JsonArray distances = doc["arguments"][1].to<JsonArray>();

  // Iteruj przez dane i dodaj je do tablicy JSON
  for (int y = (width * (width - 1)); y >= 0; y -= width) {
    for (int x = 0; x < width; x++) {
      uint16_t distance = measurementData.distance_mm[x + y];
      distances.add(distance);
    }
  }

  // Serializuj JSON do stringu
  String jsonString;
  serializeJson(doc, jsonString);
  jsonString += ""; // Dodanie znacznika zakończenia, jeśli jest wymagany

  return jsonString;
}

void waitForWiFiConnectOrReboot(bool printOnSerial = true, int numOfAttempts = 50) {
  int notConnectedCounter = 0;
  setLEDColor(255, 255, 0);  // Yellow
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
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
  pinMode(0, OUTPUT);
  pinMode(1, OUTPUT);
  pinMode(2, OUTPUT);
  pinMode(3, OUTPUT);
  pinMode(4, OUTPUT);
  pinMode(5, OUTPUT);

  digitalWrite(9, LOW);

  //while (!USE_SERIAL);  // Wait for the USE_SERIAL port to connect (only needed for native USB)
  USE_SERIAL.println("USE_SERIAL communication initialized!");

  WiFi.begin(ssid, password);  // Connect to WiFi
  waitForWiFiConnectOrReboot(USE_SERIAL, 50);

  if (myImager.begin() == false) {
    USE_SERIAL.println("Failed to initialize VL53L5CX sensor. Restarting ...");
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

  webSocket.beginSSL(websocketServer, websocketPort, "/signalhub");  // Initialize WebSocket client
  webSocket.setReconnectInterval(WS_RECONNECT_INTERVAL);
  //webSocket.enableHeartbeat(pingInterval,pongTimeout,disconnectTimeoutCount); // TODO
  webSocket.onEvent(webSocketEvent);  // Set event handler

  myImager.startRanging();  // Start ranging
  delay(100);
}

void loop() {
  webSocket.loop();  // Handle WebSocket events and communication
  //currentMillis = millis();                                                                      // Get the current time
  if (myImager.isDataReady() && webSocket.isConnected()) {  // Poll the VL53L5CX sensor for new data  TODO: Attach the interrupt
    setLEDColor(64, 0, 64);                                 // White
    if (myImager.getRangingData(&measurementData))          // Read distance data into array
    {
      String data = createRangingDataString(measurementData, imageWidth, imageResolution);
      webSocket.sendTXT(data);
    }
    setLEDColor(0, 128, 0);  // Green
  }
}
