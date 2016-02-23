#include "RotaryEncoder.h"

RotaryEncoder::RotaryEncoder(ButtonProcessor* _btn_processor) :
btn_processor(_btn_processor),
last_processed(0),
previous_signal(-1)
{
}

void RotaryEncoder::init()
{
  if (ROTARY_PIN_SIGNAL > 0 && ROTARY_PIN_VALUE > 0)
  {
    pinMode(ROTARY_PIN_SIGNAL, INPUT);
	pinMode(ROTARY_PIN_VALUE, INPUT);
    digitalWrite(ROTARY_PIN_SIGNAL, HIGH);
	digitalWrite(ROTARY_PIN_VALUE, HIGH);
  }
}

void RotaryEncoder::process()
{
	//btn_processor->send_button_state(rotary_prev_num, BTN_STATE_PRESSED);
	
	if (ROTARY_PIN_SIGNAL == 0 || ROTARY_PIN_VALUE == 0)
		return;
	  
	unsigned long now = millis();

	int signal = digitalRead(ROTARY_PIN_SIGNAL);
	
	if (now >= last_processed + ROTARY_PROCESS_INTERVAL)
	{
		if (signal == LOW && previous_signal == HIGH)
		{
			generate_event();
			last_processed = now + ROTARY_AFTER_EVENT_DELAY;
		}
	}
	
	previous_signal = signal;
}

void RotaryEncoder::generate_event()
{
	int value = digitalRead(ROTARY_PIN_VALUE);
	
	if (value == LOW)
		btn_processor->on_button(rotary_prev_num, BTN_STATE_PRESSED);
	else
		btn_processor->on_button(rotary_next_num, BTN_STATE_PRESSED);
}