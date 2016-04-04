#ifndef AMC_Controller_h
#define AMC_Controller_h

#include "Arduino.h"
#include "Manager.h"
#include "ArrayToSerialAdapter.h"
#include "CommFrameSender.h"
#include "CommFrameReceiver.h"
#include "CommandWriter.h"
#include "ButtonProcessor.h"
#include "Rotary.h"
#include "OledController.h"
#include "RelayController.h"
#include "Buzzer.h"
#include "LightSensor.h"

#define ARDUINO_OUT_BUFFER_SIZE 2048
#define BUTTONS_OUT_BUFFER_SIZE 128

class AMCController
{
public:
	AMCController(HardwareSerial* _gsmPort, HardwareSerial* _gpsPort, UARTClass* _commPort);
	~AMCController();
	void init();
	void run();

private:
	HardwareSerial* gsmPort;
	HardwareSerial* comPort;

	HardwareSerial* ports[4];
	uint8_t ports_types[4];

	CommFrameSender frame_sender;

	CommFrameReceiver frame_receiver;

	ArrayToSerialAdapter arduino_out_port;
	uint8_t arduino_out_buffer[ARDUINO_OUT_BUFFER_SIZE];

	ArrayToSerialAdapter buttons_out_port;
	uint8_t buttons_out_buffer[BUTTONS_OUT_BUFFER_SIZE];
	
	ButtonProcessor button_processor;
	Rotary rotary_encoder;
	
	Manager manager;
	
	OledController oled;

	RelayController relay;
	
	Buzzer buzzer;

	LightSensor light_sensor;
	
	void process_incoming();
};



#endif