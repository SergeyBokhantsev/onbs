
#include "ButtonProcessor.h"
#include "CommFrameProcessor.h"

ButtonProcessor::ButtonProcessor(HardwareSerial* _out_buffer, Manager* _manager) :
out_buffer(_out_buffer),
manager(_manager),
buttons_last_processed(0)
{
}

void ButtonProcessor::init()
{
  button_pins[accept_btn_num] = accept_btn_pin;
  button_pins[cancel_btn_num] = cancel_btn_pin;
  button_pins[f1_btn_num] = f1_btn_pin;
  button_pins[f2_btn_num] = f2_btn_pin;
  button_pins[rotary_select_num] = rotary_select_pin;
  
  for (int i=0; i<buttons_count; ++i)
  {
    init_button_pin(button_pins[i]);
    button_states[i] = BTN_STATE_RELEASED;
    button_process_times[i] = 0;
  }
}

void ButtonProcessor::process()
{
  unsigned long now = millis();

	if (now >= buttons_last_processed + BTN_PROCESS_INTERVAL)
	  {
	    for (int i=0; i<buttons_count; ++i)
	    {
	      if (check_button(button_pins[i], button_states+i, button_process_times+i))
		  {
			  on_button(i, button_states[i]);
		  }
	    }
	    
	    buttons_last_processed = now;
	  }
}

void ButtonProcessor::on_button(int button_id, uint8_t state)
{
	if (manager->before_button_send(button_id, state))
	 {
		send_button_state(button_id, state);
	}
}

void ButtonProcessor::init_button_pin(int pin)
{
  if (pin > 0)
  {
    pinMode(pin, INPUT);
    digitalWrite(pin, HIGH);
  }
}

void ButtonProcessor::send_button_state(int button_id, uint8_t state)
{
	out_buffer->write((uint8_t)'[');
	out_buffer->write((uint8_t)'<');
	out_buffer->write((uint8_t)']');
	out_buffer->write(button_id);
	out_buffer->write(state);
	out_buffer->write((uint8_t)'[');
	out_buffer->write((uint8_t)'>');
	out_buffer->write((uint8_t)']');
}

bool ButtonProcessor::check_button(int pin, uint8_t* state, unsigned long* last_processed_time)
{
  if (pin <= 0)
    return false;
  
  int pressed = !digitalRead(pin);

  if (pressed)
  {
    unsigned long now = millis();

    switch(*state)
    {
      case BTN_STATE_RELEASED:
        *last_processed_time = now;
        *state = BTN_STATE_PRESSED;
        return true;
        
       case BTN_STATE_PRESSED:
        if (now >= *last_processed_time + BTN_PRESSED_FIRST_INTERVAL)
        {
          *last_processed_time = now;
          *state = BTN_STATE_HOLDED;
          return true;
        }
        return false;
        
       case BTN_STATE_HOLDED:
        if (now >= *last_processed_time + BTN_PRESSED_SECOND_INTERVAL)
        {
          *last_processed_time = now;
          return true;
        }
        return false;
    }
  }
  else
  {
    if (*state == BTN_STATE_PRESSED || *state == BTN_STATE_HOLDED)
    {
      *state = BTN_STATE_RELEASED;
      return true;
    }
    return false;
  }
}