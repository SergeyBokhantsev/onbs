#include "LightSensor.h"

LightSensor::LightSensor() 
{

}

LightSensor::~LightSensor()
{
}

void LightSensor::init()
{
	pinMode(LIGHT_SENSOR_A_PIN, INPUT);
	pinMode(LIGHT_SENSOR_B_PIN, INPUT);
}

int LightSensor::read_sensor(uint8_t pin)
{
	switch(pin)
	{
		case 0:
			return analogRead(LIGHT_SENSOR_A_PIN);

		case 1:
			return analogRead(LIGHT_SENSOR_B_PIN);
	}

	return 0;
}
