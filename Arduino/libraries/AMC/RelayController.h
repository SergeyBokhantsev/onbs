#ifndef relay_controller_h
#define relay_controller_h

#include "Arduino.h"

#define RELAY_1 0
#define RELAY_2 0
#define RELAY_3 0
#define RELAY_4 0

struct RelayDescriptor
{
	
};

class RelayController
{
	public:
	RelayController();
	~RelayController();
	//void Tick();
	//void ProcessFrame(const char* frame_array, int frame_len);
	
	//void Enable(int relayNum);
	//void Disable(int relayNum);
	
	private:
	RelayDescriptor rd1;
};

#endif