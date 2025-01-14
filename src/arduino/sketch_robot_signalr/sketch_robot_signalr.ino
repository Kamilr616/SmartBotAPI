/*
  SmartBotDevice
  By: Kamil Rataj,
      Mateusz Ciszek,
      Izabela Panek.
*/


#include <WiFi.h>  // "WiFi" by Arduino
#include <WiFiMulti.h>
#include <WebSocketsClient.h>  //"WebSockets" by Markus Sattler  TODO: check deprecated func
//#include <WiFiClientSecure.h>
#include <Wire.h>                       // I2C
#include <SparkFun_VL53L5CX_Library.h>  // "SparkFun VL53L5CX Arduino Library" by SparkFun
#include <ArduinoJson.h>
#include <Adafruit_MPU6050.h>
#include <Adafruit_Sensor.h>

#include "arduino_secrets.h"
#include "config.h"


const char ssid[] = SECRET_SSID;        // WiFi SSID
const char password[] = SECRET_PASS;    // WiFi Password
const char ssid2[] = SECRET_SSID2;      // WiFi SSID2
const char password2[] = SECRET_PASS2;  // WiFi Password2
// const char ssid3[] = SECRET_SSID3;      // WiFi SSID3
// const char password3[] = SECRET_PASS3;  // WiFi Password3

const char websocketServer[] = SERVER_IP;  // API URL
const int websocketPort = SERVER_PORT;     // API PORT

sensors_event_t a, g, temp;

const int imageWidth = 8;
const int imageResolution = 64;

WebSocketsClient webSocket;
SparkFun_VL53L5CX myImager;
VL53L5CX_ResultsData measurementData;  // Result data class structure, 1356 bytes of RAM
Adafruit_MPU6050 mpu;
WiFiMulti wifiMulti;
WiFiClientSecure secureClient;

static unsigned long lastTime = 0;
static unsigned long currentMillis = 0;
static bool stopMotor = true;


void setLEDColor(uint8_t r = 0, uint8_t g = 0, uint8_t b = 0) {
  rgbLedWrite(PIN_NEOPIXEL, g, r, b);
}

void controlMotors(int valA = 0, int valB = 0) {
  bool output3 = 0;
  bool output4 = 0;
  int pwmOutput5 = 0;
  bool output2 = 0;
  bool output1 = 0;
  int pwmOutput0 = 0;

  if (valA > 255) valA = 255;
  if (valA < -255) valA = -255;
  if (valB > 255) valB = 255;
  if (valB < -255) valB = -255;

  if (valA > 0 && stopMotor == false) {
    output3 = 1;
    output4 = 0;
    pwmOutput5 = valA;
  } else if (valA < 0) {
    output3 = 0;
    output4 = 1;
    pwmOutput5 = -valA;
  }

  if (valB > 0 && stopMotor == false) {
    output2 = 0;
    output1 = 1;
    pwmOutput0 = valB;
  } else if (valB < 0) {
    output2 = 1;
    output1 = 0;
    pwmOutput0 = -valB;
  }

  USE_SERIAL.printf("Motor A -> %d  |  Motor B -> %d\n", valA, valB);
  lastTime = millis();

  digitalWrite(MOTOR_A_IN1, output3);  // motor A (left)
  digitalWrite(MOTOR_A_IN2, output4);
  analogWrite(MOTOR_A_PWM, pwmOutput5);
  digitalWrite(MOTOR_B_IN1, output2);  // motor B (right)
  digitalWrite(MOTOR_B_IN2, output1);
  analogWrite(MOTOR_B_PWM, pwmOutput0);
}

void handleIncomingMessage(uint8_t *payload, size_t payloadLength) {
  char jsonString[payloadLength + 1];

  strncpy(jsonString, reinterpret_cast<const char *>(payload), payloadLength);
  jsonString[payloadLength] = '\0';  // null-terminator

  JsonDocument doc;
  DeserializationError error = deserializeJson(doc, jsonString);

  if (error) {
    USE_SERIAL.printf("[WS] JSON Error: %s\n", error.f_str());
    return;
  }

  int arg1 = 0;
  int arg2 = 0;

  if (strcmp(doc["target"], "ReceiveRobotCommand") == 0) {
    JsonArray arguments = doc["arguments"];
    arg1 = arguments[0];
    arg2 = arguments[1];
  }
  controlMotors(arg1, arg2);
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
      if (length > 48) {
        handleIncomingMessage(payload, length);
      }
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
  doc["arguments"][0] = "Robot_KI";    //user

  JsonArray measurements = doc["arguments"][1].to<JsonArray>();
  JsonArray distances = doc["arguments"][2].to<JsonArray>();

  measurements[0] = (double_t)-a.acceleration.z;
  measurements[1] = (double_t)-a.acceleration.y;
  measurements[2] = (double_t)-a.acceleration.x;
  measurements[3] = (double_t)-g.gyro.z;
  measurements[4] = (double_t)-g.gyro.y;
  measurements[5] = (double_t)-g.gyro.x;
  measurements[6] = (double_t)temp.temperature;

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

  if (avgDistance < MIN_DISTANCE) {
    stopMotor = true;
    setLEDColor(255, 0, 0);
  } else {
    stopMotor = false;
    setLEDColor(0, 255, 0);
  }

  doc["arguments"][3] = avgDistance;
  String jsonString;
  serializeJson(doc, jsonString);
  jsonString += "";

  return jsonString;
}

void waitForWiFiConnectOrReboot(bool printOnSerial = true, int numOfAttempts = 50) {
  int notConnectedCounter = 0;
  setLEDColor(255, 255, 0);  // Yellow
  if (printOnSerial) {
    USE_SERIAL.printf("[WIFI] Connecting to %s |", ssid);
  }

  while (wifiMulti.run() != WL_CONNECTED) {
    delay(2500);
    if (printOnSerial) {
      USE_SERIAL.print(" |\n");
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
  delay(100);

  for (uint8_t t = 4; t > 0; t--) {
    USE_SERIAL.printf("\n[SETUP] BOOT WAIT %d...\n", t);
    USE_SERIAL.flush();
    delay(500);
  }

  Wire.begin(8, 7);         // Use pins 8 (SDA), 7 (SCL) for I2C
  Wire.setClock(I2C_FREQ);  // Set I2C frequency

  pinMode(9, OUTPUT);  //ToF

  pinMode(MOTOR_A_IN1, OUTPUT);  //H-Bridge
  pinMode(MOTOR_A_IN2, OUTPUT);
  pinMode(MOTOR_A_PWM, OUTPUT);
  pinMode(MOTOR_B_IN1, OUTPUT);
  pinMode(MOTOR_B_IN2, OUTPUT);
  pinMode(MOTOR_B_PWM, OUTPUT);

  digitalWrite(TOF_PIN, LOW);

  USE_SERIAL.println("USE_SERIAL communication initialized!");

  WiFi.mode(WIFI_STA);
  wifiMulti.addAP(ssid, password);
  wifiMulti.addAP(ssid2, password2);
  //wifiMulti.addAP(ssid3, password3);
  waitForWiFiConnectOrReboot(USE_SERIAL, 50);

  //secureClient.setCACert(root_ca);

  if (!myImager.begin()) {
    USE_SERIAL.println("Failed to initialize VL53L5CX sensor. Restarting ...");
    ESP.restart();
  } else if (!mpu.begin()) {
    Serial.println("Failed to initialize MPU6050 chip. Restarting ...");
    ESP.restart();
  } else {
    setLEDColor(0, 255, 255);  // Cyan
  }

  myImager.setResolution(imageResolution);
  myImager.setRangingFrequency(TOF_RANGING_FREQ);
  myImager.setRangingMode(TOF_RANGING_MODE);
  myImager.setSharpenerPercent(TOF_SHARPENER_PERCENT);
  myImager.setTargetOrder(TOF_TARGET_ORDER);
  //myImager.setIntegrationTime(TOF_INTEGRATION_TIME);

  mpu.setAccelerometerRange(MPU_G_RANGE);
  mpu.setGyroRange(MPU_DEG_RANGE);
  mpu.setFilterBandwidth(MPU_HZ_BAND);
  //mpu.setCycleRate(MPU_HZ_CYCLE);
  //mpu.enableSleep(false);

  webSocket.setReconnectInterval(WS_RECONNECT_INTERVAL);
  webSocket.onEvent(webSocketEvent);                                 // Set event handler
  webSocket.beginSSL(websocketServer, websocketPort, "/signalhub");  // Initialize WebSocket client

  myImager.startRanging();  // Start ranging
  //mpu.enableCycle(true);
  delay(100);
}

void loop() {
  webSocket.loop();  // Handle WebSocket events and communication

  currentMillis = millis();  // Get the current time

  if (myImager.isDataReady() && webSocket.isConnected()) {  // Poll the VL53L5CX sensor for new data  TODO: Attach the interrupt
    setLEDColor(0, 0, 32);
    if (myImager.getRangingData(&measurementData) && mpu.getEvent(&a, &g, &temp))  // Read data
    {
      String data = createDataString(measurementData, a, g, temp, imageWidth, imageResolution);
      webSocket.sendTXT(data);
    }
  }
  if (currentMillis - lastTime >= MAX_IDLE_TIME) {
    stopMotor = true;
    controlMotors(0, 0);  // force stop
  }
}
