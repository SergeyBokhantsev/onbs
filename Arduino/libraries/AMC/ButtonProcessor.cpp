
#include "ButtonProcessor.h"
#include "CommFrameProcessor.h"

ButtonProcessor::ButtonProcessor(CommandWriter* _writer, Manager* _manager) :
writer(_writer),
manager(_manager),
buttons_last_processed(0)
{
}

void ButtonProcessor::init()
{
	buttons_process_interval = 50;
  
  button_pins[accept_btn_num] = accept_btn_pin;
  button_pins[cancel_btn_num] = cancel_btn_pin;
  button_pins[f1_btn_num] = f1_btn_pin;
  button_pins[f2_btn_num] = f2_btn_pin;
  button_pins[f3_btn_num] = f3_btn_pin;
  button_pins[f4_btn_num] = f4_btn_pin;
  button_pins[f5_btn_num] = f5_btn_pin;
  button_pins[f6_btn_num] = f6_btn_pin;
  button_pins[f7_btn_num] = f7_btn_pin;
  button_pins[f8_btn_num] = f8_btn_pin;
  
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

	if (now >= buttons_last_processed + buttons_process_interval)
	  {
	    for (int i=0; i<buttons_count; ++i)
	    {
	      if (check_button(button_pins[i], button_states+i, button_process_times+i))
		  {
			  if (manager->before_button_send(i, button_states[i]))
			  {
				send_button_state(i, button_states[i]);
			  }
		  }
	    }
	    
	    buttons_last_processed = now;
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

void ButtonProcessor::send_button_state(int button_id, char state)
{
	writer->open_command((char)BUTTON_FRAME_TYPE);
	writer->write((char)button_id);
	writer->write(state);
	writer->close_command();
}

bool ButtonProcessor::check_button(int pin, char* state, unsigned long* last_processed_time)
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