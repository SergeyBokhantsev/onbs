#include "Manager.h"
#include "ButtonProcessor.h"
#include "CommFrameProcessor.h"

void reset_shutdown_signal(unsigned long* ts)
{
	*ts = 0;
}

bool is_raise_shutdown_signal(unsigned long* ts)
{
	if (*ts == 0)
	{
		*ts = millis();
		return false;
	}
	else if (*ts + MANAGER_SHUTDOWN_SIGNAL_TIMEOUT_MS < millis())
	{
		reset_shutdown_signal(ts);
		return true;
	}
	else
	{
		return false;
	}
}

Manager::Manager(CommandWriter* _outcom_writer, RelayController* _relay, OledController* _oled)
: outcom_writer(_outcom_writer),
relay(_relay),
oled(_oled),
screen_timestamp(0),
state_timestamp(0),
shutdown_signal_timestamp(0)
{
	set_state(MANAGER_STATE_WAITING);
}

Manager::~Manager()
{
}

void Manager::tick()
{
	switch(state)
	{
		case MANAGER_STATE_ACTIVE:
			relay->turn_relay(RELAY_MASTER, RELAY_ENABLE);
			if (millis() > state_timestamp + MANAGER_STATE_ACTIVE_TIMEOUT_SECONDS * 1000)
			{
				set_state(MANAGER_STATE_WAITING);
			}
			break;
		
		case MANAGER_STATE_WAITING:
			relay->turn_relay(RELAY_MASTER, RELAY_ENABLE);
			if (millis() > state_timestamp + MANAGER_STATE_WAITING_TIMEOUT_SECONDS * 1000)
			{
				set_state(MANAGER_STATE_GUARD);
			}
			break;
			
		case MANAGER_STATE_ON_HOLD:
			relay->turn_relay(RELAY_MASTER, RELAY_ENABLE);
			break;
			
		case MANAGER_STATE_GUARD:
			relay->turn_relay(RELAY_MASTER, RELAY_DISABLE);
			break;
	}
	
	update_screen();
}

bool Manager::before_button_send(int buttonId, char buttonState)
{
	if (buttonId == cancel_btn_num 
		&& buttonState == BTN_STATE_HOLDED
		&& state == MANAGER_STATE_ACTIVE)
	{
		if (is_raise_shutdown_signal(&shutdown_signal_timestamp))
		{
			outcom_writer->open_command(ARDUINO_COMMAND_FRAME_TYPE);
			outcom_writer->write(ARDUCOMMAND_SHUTDOWN_SIGNAL);
			outcom_writer->close_command();
		}
	}
	else
	{
		reset_shutdown_signal(&shutdown_signal_timestamp);
	}
	
	switch (state)
	{
		case MANAGER_STATE_WAITING:
			if (buttonId == cancel_btn_num && buttonState == BTN_STATE_PRESSED)
			{
				set_state(MANAGER_STATE_GUARD);
			}
			else if (buttonId == accept_btn_num && buttonState == BTN_STATE_PRESSED)
			{
				set_state(MANAGER_STATE_ON_HOLD);
			}
			return false;
			
		case MANAGER_STATE_ON_HOLD:
			if (buttonId == cancel_btn_num && buttonState == BTN_STATE_PRESSED)
			{
				set_state(MANAGER_STATE_GUARD);
			}
			return false;
			
		case MANAGER_STATE_GUARD:
			if (buttonId == accept_btn_num && buttonState == BTN_STATE_PRESSED)
			{
				set_state(MANAGER_STATE_WAITING);
			}
			return false;
	}
}

void Manager::set_state(int newState)
{
	state = newState;
	state_timestamp = millis();
	
	guard_animation_counter = 0;
	guard_animation_mode = 0;
}

void Manager::dispatch_frame(char* frame_array, int frame_len, char frame_type)
{
	int result = MANAGER_ERROR_UNKNOWN_FRAME_TYPE;

	set_state(MANAGER_STATE_ACTIVE);
	
	switch (frame_type)
	{
		case GSM_FRAME_TYPE:
			result = MANAGER_ERROR_FRAME_TYPE_DISABLED;
			break;
			
		case ARDUINO_COMMAND_FRAME_TYPE:
			result = process_frame(frame_array, frame_len);
			break;
			
		case OLED_COMMAND_FRAME_TYPE:
			result = oled->process(frame_array, frame_len);
			break;
			
		case RELAY_COMMAND_FRAME_TYPE:
			result = relay->process_frame(frame_array, frame_len);
			break;			
	}

	if (result > 0)
	{
     outcom_writer->open_command(ARDUINO_COMMAND_FRAME_TYPE);
     outcom_writer->write(ARDUCOMMAND_COMMAND_RESULT);
	 outcom_writer->write(frame_type);
     outcom_writer->write((char)result);
     outcom_writer->close_command();
	}
}

int Manager::process_frame(char* frame_array, int frame_len)
{
	if (frame_len == 0)
		return MANAGER_ERROR_INVALID_COMMAND;
	
	switch (frame_array[0])
	{
		case ARDUCOMMAND_PING_REQUEST:
			return ARDUCOMMAND_PING_RESPONSE;
			
		case ARDUCOMMAND_HOLD:
			set_state(MANAGER_STATE_ON_HOLD);
			return 0;
			
		default:
			return MANAGER_ERROR_UNKNOWN_COMMAND;
	}
}

void Manager::update_screen()
{
	if (millis() < screen_timestamp + MANAGER_SCREEN_UPDATE_MS)
		return;
	
	screen_timestamp = millis();
	
	unsigned long remainingMs;
	
	switch(state)
	{
		case MANAGER_STATE_WAITING:
			remainingMs = ((unsigned long)MANAGER_STATE_WAITING_TIMEOUT_SECONDS * 1000) - (millis() - state_timestamp);
			oled->draw_state_waiting((int)(remainingMs / (unsigned long)1000));
			break;
			
		case MANAGER_STATE_ON_HOLD:
			oled->draw_state_hold();
			break;
			
		case MANAGER_STATE_GUARD:
			if (++guard_animation_counter < 15)
			{
				oled->draw_state_guard_icon();
			}
			else if (guard_animation_counter < 20)
			{
				oled->draw_state_guard_hint();
			}
			else
			{
				guard_animation_counter = 0;
			}
			break;
	}
}
