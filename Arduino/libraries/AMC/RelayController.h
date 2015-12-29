#ifndef relay_controller_h
#define relay_controller_h

#include "Arduino.h"

#define RELAY_MASTER_PIN -1
#define RELAY_OBD_PIN -1
#define RELAY_3_PIN -1
#define RELAY_4_PIN -1

#define RELAY_MASTER 0
#define RELAY_OBD 1
#define RELAY_3 2
#define RELAY_4 3

#define RELAY_ENABLE true
#define RELAY_DISABLE false

#define RELAY_COMMAND_SHEDULE 0
#define RELAY_COMMAND_UNSHEDULE 1

struct RelayDescriptor
{
	bool scheduled;
	unsigned long scheduledTime;
	bool action;
	int pin;
};

class RelayController
{
	public:
	RelayController();
	~RelayController();
	void tick();
	bool process_frame(const char* frame_array, int frame_len);
	
	void schedule(int relay, bool action, int delaySec);
	void unschedule(int relay);
	
	private:
	RelayDescriptor descriptors[4];
	
	void turn_relay(int relay, bool action);
};

#endif