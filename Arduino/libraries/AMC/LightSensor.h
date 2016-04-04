#ifndef LightSensor_h
#define LightSensor_h

#include "Arduino.h"

#define LIGHT_SENSOR_A_PIN A8
#define LIGHT_SENSOR_B_PIN A9

class LightSensor
{
public:
	LightSensor();
	~LightSensor();

	void init();

	int read_sensor(uint8_t index);
private:
	
};

#endif