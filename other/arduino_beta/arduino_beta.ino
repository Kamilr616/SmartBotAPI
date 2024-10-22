#include "WiFi.h"
#include "AsyncUDP.h"
#include <Wire.h>
#include <SparkFun_VL53L5CX_Library.h>

const char *ssid = "ROBOT_01";
const char *password = "*****";

AsyncUDP udpBroadcast;  // UDP object for broadcasting

SparkFun_VL53L5CX myImager;
VL53L5CX_ResultsData measurementData;

int imageResolution = 0;
int imageWidth = 0;

void setup() {
  Serial.begin(115200);
  delay(1000);
  Serial.println("SparkFun VL53L5CX Imager Example");

  Wire.begin(8, 7);
  Wire.setClock(400000);

  pinMode(9, OUTPUT);
  digitalWrite(9, LOW);

  pinMode(0, OUTPUT);
  pinMode(1, OUTPUT);
  pinMode(2, OUTPUT);
  pinMode(3, OUTPUT);
  pinMode(4, OUTPUT);
  pinMode(5, OUTPUT);



  Serial.println("Initializing sensor board. This can take up to 10s. Please wait.");
  if (!myImager.begin()) {
    Serial.println(F("Sensor not found - check your wiring. Freezing"));
    while (1)
      ;
  }

  myImager.setResolution(8 * 8);  // Enable all 64 pads

  imageResolution = myImager.getResolution();
  imageWidth = sqrt(imageResolution);

  myImager.setRangingFrequency(15);
  myImager.startRanging();

  WiFi.mode(WIFI_AP);
  WiFi.softAP(ssid, password);


  if (udpBroadcast.listen(1215)) {
    Serial.println("UDP Receive Listening on port 1215");
    udpBroadcast.onPacket([](AsyncUDPPacket packet) {
      Serial.printf("Received %u bytes from %s:%d\n", packet.length(), packet.remoteIP().toString().c_str(), packet.remotePort());

      if (packet.length() > 0) {
        for (size_t i = 0; i < packet.length(); i++) {
          Serial.printf("%02X ", packet.data()[i]);
        }
        Serial.println();
        char command = packet.data()[0];
        Serial.println(command);
        switch (command) {
          case '0':
            digitalWrite(3, LOW);
            digitalWrite(4, HIGH);
            digitalWrite(5, HIGH);
            digitalWrite(0, LOW);
            digitalWrite(1, HIGH);
            digitalWrite(2, HIGH);
            break;

          case '1':
            digitalWrite(3, HIGH);
            digitalWrite(4, LOW);
            digitalWrite(5, HIGH);
            digitalWrite(0, HIGH);
            digitalWrite(1, LOW);
            digitalWrite(2, HIGH);
            ;
            break;

          case '2':
            digitalWrite(3, HIGH);
            digitalWrite(4, LOW);
            digitalWrite(5, HIGH);
            digitalWrite(0, LOW);
            digitalWrite(1, HIGH);
            digitalWrite(2, HIGH);
            ;
            break;

          case '3':
            digitalWrite(0, HIGH);
            digitalWrite(1, LOW);
            digitalWrite(2, HIGH);
            digitalWrite(3, LOW);
            digitalWrite(4, HIGH);
            digitalWrite(5, HIGH);
            ;
            break;
          default:
            digitalWrite(3, LOW);
            digitalWrite(4, LOW);
            digitalWrite(5, HIGH);
            digitalWrite(0, LOW);
            digitalWrite(1, LOW);
            digitalWrite(2, HIGH);
            break;
        }
        delay(250);
        digitalWrite(0, LOW);
        digitalWrite(1, LOW);
        digitalWrite(2, HIGH);
        digitalWrite(3, LOW);
        digitalWrite(4, LOW);
        digitalWrite(5, HIGH);
      }
    });
  } else {
    Serial.println("Failed to bind UDP server for receiving on port 1215");
  }
}

void loop() {
  if (myImager.isDataReady()) {
    if (myImager.getRangingData(&measurementData)) {
      uint8_t data[128];
      int z = 0;

      for (int y = 0; y <= imageWidth * (imageWidth - 1); y += imageWidth) {
        for (int x = imageWidth - 1; x >= 0; x--) {
          data[z] = measurementData.distance_mm[x + y] >> 8;
          data[z + 1] = measurementData.distance_mm[x + y] & 0xFF;
          z += 2;
        }
      }


      udpBroadcast.broadcast(data, 128);
    }
  }

  delay(1);
}
