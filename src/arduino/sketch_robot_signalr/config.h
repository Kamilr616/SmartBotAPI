/**
 * @file config.h
 * @date 20.10.2024
 * @author Kamil Rataj
 *
 * Compile time configuration file.
 *
 * Copyright (c) 2024 Kamil Rataj. All rights reserved.
**/

#ifndef config_h
#define config_h


#define USE_SERIAL Serial
#define SERIAL_BAUDRATE 115200

#define SERVER_IP "smartbotapi.azurewebsites.net"
#define SERVER_PORT 443

#define PIN_NEOPIXEL 10  // GPIO10

#define I2C_FREQ 1000000  // 1MHz

#define WS_RECONNECT_INTERVAL 1000

#define TOF_RANGING_MODE SF_VL53L5CX_RANGING_MODE::CONTINUOUS
#define TOF_SHARPENER_PERCENT 20
#define TOF_TARGET_ORDER SF_VL53L5CX_TARGET_ORDER::CLOSEST
#define TOF_INTEGRATION_TIME 5
#define TOF_RANGING_FREQ 15

//#define RGB_BRIGHTNESS 10 // Change white brightness (max 255)


#endif