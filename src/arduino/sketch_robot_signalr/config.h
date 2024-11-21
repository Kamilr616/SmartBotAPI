/**
 * @file config.h
 * @date 20.10.2024
 * @author Kamil Rataj
 *
 * Compile time configuration file.
 *
 * Copyright (c) 2024 Kamil Rataj. All rights reserved.
**/

#ifndef CONFIG_H
#define CONFIG_H


#define USE_SERIAL Serial
#define SERIAL_BAUDRATE 115200

#define SERVER_IP "192.168.50.100"//"smartbotapi.azurewebsites.net"
#define SERVER_PORT 32775//443

#define PIN_NEOPIXEL 10  // GPIO10

#define I2C_FREQ 1000000  // 1MHz

#define WS_RECONNECT_INTERVAL 7500

#define TOF_RANGING_MODE SF_VL53L5CX_RANGING_MODE::CONTINUOUS
#define TOF_SHARPENER_PERCENT 18
#define TOF_TARGET_ORDER SF_VL53L5CX_TARGET_ORDER::CLOSEST
#define TOF_INTEGRATION_TIME 5
#define TOF_RANGING_FREQ 10

#define MPU_G_RANGE MPU6050_RANGE_2_G
#define MPU_DEG_RANGE MPU6050_RANGE_250_DEG
#define MPU_HZ_BAND MPU6050_BAND_21_HZ
#define MPU_HZ_CYCLE MPU6050_CYCLE_20_HZ

//#define RGB_BRIGHTNESS 10


#endif  // CONFIG_H