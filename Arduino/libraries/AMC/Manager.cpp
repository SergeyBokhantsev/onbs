#include "Manager.h"
#include "ButtonProcessor.h"

Manager::Manager(RelayController* _relay, OledController* _oled)
: relay(_relay),
oled(_oled),
screen_timestamp(0),
state_timestamp(0)
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

bool Manager::before_button_send(int button_id, char state)
{
	switch (state)
	{
		case MANAGER_STATE_ON_HOLD:
			if (button_id == cancel_btn_num && state == BTN_STATE_HOLDED)
			{
				set_state(MANAGER_STATE_GUARD);
			}
			return false;
			
		case MANAGER_STATE_GUARD:
			if (button_id == accept_btn_num && state == BTN_STATE_HOLDED)
			{
				set_state(MANAGER_STATE_WAITING);
			}
			return false;
			
		default:
			return true;
	}
}

void Manager::set_state(int newState)
{
	state = newState;
	state_timestamp = millis();
}

void Manager::on_incoming_frame()
{
	set_state(MANAGER_STATE_ACTIVE);
}

int Manager::process_frame(char* frame_array, int frame_len)
{
	if (frame_len == 0)
		return MANAGER_ERROR_INVALID_COMMAND;
	
	switch (frame_array[0])
	{
		case ARDUCOMMAND_EMPTY:
			return ARDUCOMMAND_EMPTY;
			
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
			oled->draw_state_guard();
			break;
	}
}
