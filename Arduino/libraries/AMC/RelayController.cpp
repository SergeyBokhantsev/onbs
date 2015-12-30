#include "RelayController.h"

RelayController::RelayController()
{ 
	descriptors[RELAY_MASTER].pin = RELAY_MASTER_PIN;
	descriptors[RELAY_OBD].pin = RELAY_OBD_PIN;
	descriptors[RELAY_3].pin = RELAY_3_PIN;
	descriptors[RELAY_4].pin = RELAY_4_PIN;
	
	for (int i=0; i<4; ++i)
	{
		descriptors[i].state = RELAY_DISABLE;
	}
}

RelayController::~RelayController()
{
}

void RelayController::init()
{
	for (int i=0; i<4; ++i)
	{
		if (descriptors[i].pin != -1)
			pinMode(descriptors[i].pin, OUTPUT);  
	}
}

void RelayController::turn_relay(int relay, bool action)
{
	if (relay < 0 || relay > 3)
		return;
	
	if (descriptors[relay].state == action)
		return;
	
	digitalWrite(descriptors[relay].pin, action ? LOW : HIGH);
	descriptors[relay].state = action;
}

int RelayController::process_frame(const char* frame_array, int frame_len)
{
	if (frame_len == 0)
		return RELAY_ERROR_INVALID_FRAME;
	
	switch (frame_array[0])
	{
		case RELAY_COMMAND_TURN_RELAY:
			if (frame_len == 3)
			{
				int relay = frame_array[1];
				bool action = frame_array[2] > 0;				
				turn_relay(relay, action);				
				return 0;
			}
			else
				return RELAY_ERROR_ARGUMENTS_MISMATCH;
			
		default:
			return RELAY_ERROR_UNKNOWN_COMMAND;
	}
}