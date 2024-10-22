/*
  Test WebSocket
  By: Kamil Rataj.
*/

#include <WiFi.h> // "WiFi" by Arduino
#include <WebSocketsClient.h> //"WebSockets" by Markus Sattler

const char *ssid = "*****";              // WiFi SSID
const char *password = "*****";               // WiFi Password
const char *websocketServer = "192.168.1.100";  // API URL
const int websocketPort = 443;                 // API PORT


static unsigned long lastSendTime = 0;
unsigned long currentMillis;

WebSocketsClient webSocket;

void webSocketEvent(WStype_t type, uint8_t * payload, size_t length) {
    switch(type) {
        case WStype_DISCONNECTED:
            Serial.println("[WS] Disconnected from WebSocket.");
            break;
        case WStype_CONNECTED:
            Serial.println("[WS] Connected to WebSocket.");
            // Send a greeting message after connection
            webSocket.sendTXT("Hello from Robot");
            break;
        case WStype_TEXT:
            Serial.printf("[WS] Message from server: %s\n", payload);
            break;
        case WStype_BIN:
            Serial.printf("[WS] Binary data received, length: %u\n", length);
            break;
        default:
            break;
    }
}

void setup()
{
    Serial.begin(115200);

    while (!Serial); // Wait for the serial port to connect (only needed for native USB)
    Serial.println("Serial communication initialized.");

    // Connect to WiFi
    WiFi.begin(ssid, password);
    do {
        delay(500);
        Serial.printf("[WIFI] Connecting to %s ...\n", ssid);
    } while (WiFi.status() != WL_CONNECTED);
    Serial.printf("[WIFI] Connected to %s\n", ssid);

    // Initialize WebSocket client
    webSocket.beginSSL(websocketServer, websocketPort, "/api/WebSocket/ws");
    webSocket.onEvent(webSocketEvent); // Set event handler
}

void loop()
{
    currentMillis = millis(); // Get the current time
    if (currentMillis - lastSendTime >= 2000) {
        lastSendTime = currentMillis;
        webSocket.sendTXT("[TEST] Hello it's ESP!"); // Send the message
    }
    // Handle WebSocket events and communication
    webSocket.loop();
}
