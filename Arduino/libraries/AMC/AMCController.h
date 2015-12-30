#ifndef AMC_Controller_h
#define AMC_Controller_h

#include "Arduino.h"
#include "Manager.h"
#include "OutBuffer.h"
#include "CommFrameSender.h"
#include "CommFrameReceiver.h"
#include "CommandWriter.h"
#include "ButtonProcessor.h"
#include "OledController.h"
#include "RelayController.h"

#define COMM_FRAME_MAX_SIZE 512

#define ARDU_ERROR_UNKNOWN_FRAME_TYPE 20
#define ARDU_ERROR_FRAME_TYPE_DISABLED 21

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

	HardwareSerial* ports[3];
	char ports_types[3];

	CommFrameSender frame_sender;

	CommFrameReceiver frame_receiver;

	OutBuffer out_buffer;
	CommandWriter outcom_writer;

	ButtonProcessor button_processor;

	Manager manager;
	
	OledController oled;

	RelayController relay;
	
	void process_incoming();
	void process_frame(char* frame_array, int frame_len, char frame_type);
	int process_incoming_arduino_command(char* data, int len, char* operation_descriptor);
};



#endif