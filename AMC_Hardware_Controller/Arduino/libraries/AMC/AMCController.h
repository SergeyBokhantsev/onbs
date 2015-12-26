#ifndef AMC_Controller_h
#define AMC_Controller_h

#include "Arduino.h"
#include "OutBuffer.h"
#include "CommFrameSender.h"
#include "CommFrameReceiver.h"
#include "CommandWriter.h"
#include "ButtonProcessor.h"
#include "OledController.h"

#define COMM_FRAME_MAX_SIZE 512

#define ARDUCOMMAND_EMPTY 100

#define ARDUCOMMAND_GSM_DUMP 102
#define ARDUCOMMAND_DISABLE_BUTTONS 104
#define ARDUCOMMAND_ENABLE_BUTTONS 105

#define ARDUCOMMAND_ENABLE_GSM 10
#define ARDUCOMMAND_DISABLE_GSM 11
#define ARDUCOMMAND_RESET_GSM 12

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
	
	OledController oled;

	void process_incoming();
	void process_frame(char* frame_array, int frame_len, char frame_type);
	bool process_incoming_arduino_command(char* data, int len, char* operation_descriptor);

	bool disable_buttons;
};



#endif