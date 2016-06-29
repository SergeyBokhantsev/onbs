#include "Manager.h"
#include "ButtonProcessor.h"
#include "CommFrameProcessor.h"
#include "Buzzer.h"

#include <TimeLib.h>

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

Manager::Manager(HardwareSerial* _arduino_out_port, RelayController* _relay, OledController* _oled, Buzzer* _buzzer, LightSensor* _light_sensor, GpsController* _gpsController)
: command_writer(_arduino_out_port),
relay(_relay),
oled(_oled),
buzzer(_buzzer),
light_sensor(_light_sensor),
gpsController(_gpsController),
screen_timestamp(0),
state_timestamp(0),
shutdown_signal_timestamp(0),
gps_guard_lat(0),
gps_guard_lon(0),
gps_guard_location_valid(false),
gps_guard_last_check_time(0),
gps_exceeding_distance_count(0)
{
	set_state(MANAGER_STATE_GUARD);
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
			
			if (check_gps_guard())
			{
				set_state(MANAGER_STATE_WAITING);
			}
			
			break;
	}
	
	update_screen();
}

bool Manager::check_gps_guard()
{
	unsigned long now = millis();
	
	if (now > gps_guard_last_check_time + MANAGER_GPS_GUARD_CHECK_MS)
	{
		if(gpsController->GPS()->location.isValid())
		{
			if (gps_guard_location_valid)
			{
				double lat = gpsController->GPS()->location.lat();
				double lon = gpsController->GPS()->location.lng();
				
				double dist = gpsController->distance(lat, lon, gps_guard_lat, gps_guard_lon);
				
				if (dist > MANAGER_GPS_GUARD_TRIGGER_DISTANCE_METERS)
					gps_exceeding_distance_count++;
				else
					gps_exceeding_distance_count = 0;
				
				return gps_exceeding_distance_count >= MANAGER_GPS_GUARD_TRIGGER_DISTANCE_EXCEEDING_MAX;
			}
			else
			{
				gps_guard_lat = gpsController->GPS()->location.lat();
				gps_guard_lon = gpsController->GPS()->location.lng();
				gps_guard_location_valid = true;
				gps_exceeding_distance_count = 0;
			}
		}
		
		gps_guard_last_check_time = now;
	}
	
	return false;
}

bool Manager::before_button_send(int buttonId, char buttonState)
{
	if (buttonId == cancel_btn_num 
		&& buttonState == BTN_STATE_HOLDED
		&& state == MANAGER_STATE_ACTIVE)
	{
		if (is_raise_shutdown_signal(&shutdown_signal_timestamp))
		{
			command_writer.send_command(ARDUCOMMAND_SHUTDOWN_SIGNAL);
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
	
	return true;
}

void Manager::set_state(int newState)
{
	state = newState;
	state_timestamp = millis();
	
	guard_animation_counter = 0;
	guard_animation_mode = 0;
	
	switch (state)
	{
		case MANAGER_STATE_GUARD:
			oled->display.setBrightness(0);
			gps_guard_location_valid = false;
			break;
	}
}

void Manager::dispatch_frame(uint8_t* frame_array, int frame_len, uint8_t frame_type, unsigned short frame_id)
{
	command_writer.send_command(ARDUCOMMAND_CONFIRMATION, (uint8_t)((frame_id >> 8) & 0xFF), (uint8_t)(frame_id & 0xFF));
	
	uint8_t result = MANAGER_ERROR_UNKNOWN_FRAME_TYPE;

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
		command_writer.send_command(ARDUCOMMAND_COMMAND_FAILED, frame_type, result);
	}
}

uint8_t Manager::process_frame(uint8_t* frame_array, int frame_len)
{
	if (frame_len == 0)
		return MANAGER_ERROR_INVALID_COMMAND;
	
	switch (frame_array[0])
	{
		case ARDUCOMMAND_PING_REQUEST:
			set_state(MANAGER_STATE_ACTIVE);
			command_writer.send_command(ARDUCOMMAND_PING_RESPONSE);
			return 0;
			
		case ARDUCOMMAND_HOLD:
			set_state(MANAGER_STATE_ON_HOLD);
			return 0;
			
		case ARDUCOMMAND_SET_TIME:
			if (frame_len == 7)
			{
				int hr = (int)frame_array[1];
				int min = (int)frame_array[2];
				int sec = (int)frame_array[3];
				int day = (int)frame_array[4];
				int mnth = (int)frame_array[5];
				int yr = (int)frame_array[6];
				
				setTime(hr,min,sec,day,mnth,yr); 
				
				return 0;
			}
			else return MANAGER_ERROR_INVALID_COMMAND;
			
		case ARDUCOMMAND_GET_TIME_REQUEST:
			{
			time_t t = now(); 
			command_writer.send_command(ARDUCOMMAND_GET_TIME_RESPONSE, 
			(uint8_t)hour(t), 
			(uint8_t)minute(t),
			(uint8_t)second(t),
			(uint8_t)day(t),
			(uint8_t)month(t),
			(uint8_t)(year(t) - 2000));
			}
			return 0;
			
		case ARDUCOMMAND_BEEP:
			if (frame_len == 6)
			{
				char beep_lb = frame_array[1];
				char beep_sb = frame_array[2];
				int beepTimeMs = (beep_lb << 8) + beep_sb;
				
				char pause_lb = frame_array[3];
				char pause_sb = frame_array[4];
				int pauseTimeMs = (pause_lb << 8) + pause_sb;
				
				int count = (int)frame_array[5];				
				buzzer->beep(beepTimeMs, pauseTimeMs, count);
				return 0;
			}
			else return MANAGER_ERROR_INVALID_COMMAND;
			
		case ARDUCOMMAND_GET_LIGHT_SENSOR_REQUEST:
			if (frame_len == 2)
			{
				char sensorIndex = frame_array[1];
				int value = light_sensor->read_sensor((uint8_t)sensorIndex);
				char byteValue = value / 4;
				command_writer.send_command(ARDUCOMMAND_GET_LIGHT_SENSOR_RESPONSE, sensorIndex, byteValue);
				return 0;
			}
			else return MANAGER_ERROR_INVALID_COMMAND;

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
			if (remainingMs <= 5000)
			{
				if ((int)remainingMs/1000 != remaining_beep)
				{
					remaining_beep = (int)remainingMs/1000;
					buzzer->beep(70, 40, 2);
				}
			}
			break;
			
		case MANAGER_STATE_ON_HOLD:
			oled->draw_state_hold();
			break;
			
		case MANAGER_STATE_GUARD:
			if (guard_animation_counter++ == 1)
			{
				oled->draw_icon(OLED_ICON_CAR_GUARD_1);
			}
			else if (guard_animation_counter == 2)
			{
				oled->draw_icon(OLED_ICON_CAR_GUARD_2);
			}
			else if (guard_animation_counter == 3)
			{
				oled->draw_icon(OLED_ICON_CAR_GUARD_3);
			}
			else if (guard_animation_counter == 4)
			{
				oled->draw_icon(OLED_ICON_CAR_GUARD_4);
			}
			else if (guard_animation_counter == 5)
			{
				oled->draw_icon(OLED_ICON_CAR_GUARD_5);
			}
			else if (guard_animation_counter == 6)
			{
				oled->draw_icon(OLED_ICON_CAR_GUARD_6);
			}
			else if (guard_animation_counter == 7)
			{
				oled->draw_icon(OLED_ICON_CAR_GUARD_1);
			}
			else if (guard_animation_counter == 13)
			{
				oled->draw_state_guard_hint();
			}
			else if (guard_animation_counter == 20)
			{
				oled->display.clrScr();
				oled->display.update();
			}
			else if (guard_animation_counter > 50)
			{
				guard_animation_counter = 0;
			}
			break;
	}
}
