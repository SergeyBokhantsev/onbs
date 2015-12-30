#ifndef relay_controller_h
#define relay_controller_h

#include "Arduino.h"

#define RELAY_MASTER_PIN 30
#define RELAY_OBD_PIN 31
#define RELAY_3_PIN 32
#define RELAY_4_PIN 33

#define RELAY_MASTER 0
#define RELAY_OBD 1
#define RELAY_3 2
#define RELAY_4 3

#define RELAY_ENABLE true
#define RELAY_DISABLE false

#define RELAY_COMMAND_TURN_RELAY 0

#define RELAY_ERROR_INVALID_FRAME 20
#define RELAY_ERROR_ARGUMENTS_MISMATCH 21
#define RELAY_ERROR_UNKNOWN_COMMAND 22

struct RelayDescriptor
{
	int pin;
	bool state;
};

class RelayController
{
	public:
	RelayController();
	~RelayController();
	void init();
	int process_frame(const char* frame_array, int frame_len);
	void turn_relay(int relay, bool action);
	
	private:
	RelayDescriptor descriptors[4];
};

#endif