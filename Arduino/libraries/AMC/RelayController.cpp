#include "RelayController.h"

RelayController::RelayController()
{
	descriptors[RELAY_MASTER].scheduled = false;
	descriptors[RELAY_OBD].scheduled = false;
	descriptors[RELAY_3].scheduled = false;
	descriptors[RELAY_4].scheduled = false;
	
	descriptors[RELAY_MASTER].pin = RELAY_MASTER_PIN;
	descriptors[RELAY_OBD_PIN].pin = RELAY_OBD_PIN;
	descriptors[RELAY_3_PIN].pin = RELAY_3_PIN;
	descriptors[RELAY_4_PIN].pin = RELAY_4_PIN;
	
	for (int i=0; i<4; ++i)
	{
		if (descriptors[i].pin != -1)
			pinMode(descriptors[i].pin, OUTPUT);  
	} 
}

RelayController::~RelayController()
{
}

void RelayController::schedule(int relay, bool action, int delaySec)
{
	if (relay < 0 || relay > 3 || delaySec < 0)
		return;
	
	descriptors[relay].scheduled = true;
	descriptors[relay].scheduledTime = millis() + delaySec * 1000;
	descriptors[relay].action = action;
}

void RelayController::unschedule(int relay)
{
	if (relay < 0 || relay > 3)
		return;
	
	descriptors[relay].scheduled = false;
}

void RelayController::tick()
{
	unsigned long now = millis();
	
	for (int i=0; i<4; ++i)
	{
		if (descriptors[i].scheduled && descriptors[i].scheduledTime <= now)
		{
			descriptors[i].scheduled = false;
			turn_relay(i, descriptors[i].action);
		}
	}
}

void RelayController::turn_relay(int relay, bool action)
{
	digitalWrite(descriptors[relay].pin, action ? LOW : HIGH);
}

bool RelayController::process_frame(const char* frame_array, int frame_len)
{
	if (frame_len == 0)
		return false;
	
	switch (frame_array[0])
	{
		case RELAY_COMMAND_SHEDULE:
			if (frame_len == 4)
			{
				int relay = frame_array[1];
				bool action = frame_array[2] > 0;
				int delaySec = frame_array[3];				
				schedule(relay, action, delaySec);		
				return true;
			}
			else
				return false;
			
		case RELAY_COMMAND_UNSHEDULE:
			if (frame_len == 2)
			{
				int relay = frame_array[1];		
				unschedule(relay);		
				return true;
			}
			else
				return false;
			
		default:
			return false;
	}
}