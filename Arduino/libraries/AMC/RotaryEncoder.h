#ifndef rotaryencoder_h
#define rotaryencoder_h

#include "Arduino.h"
#include "ButtonProcessor.h"

#define ROTARY_PROCESS_INTERVAL 5
#define ROTARY_AFTER_EVENT_DELAY 50

class RotaryEncoder
{
	public:	
	RotaryEncoder(ButtonProcessor* _btn_processor);
	void init();
	void process();

	private:
	ButtonProcessor* btn_processor;
	unsigned long last_processed;
	int previous_signal;
	
	void generate_event();
};

#endif