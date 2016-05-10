#ifndef manager_h
#define manager_h

#include "Arduino.h"
#include "ArduinoCommands.h"
#include "RelayController.h"
#include "OledController.h"
#include "CommandWriter.h"
#include "Buzzer.h"
#include "LightSensor.h"
#include "GpsController.h"

// RPi env is up and connected (evaluated by PING signal)
#define MANAGER_STATE_ACTIVE 0
// RPi env is up, but no PING signal for N seconds. Preparing to turn of RPi. PING will switch to ACTIVE.
#define MANAGER_STATE_WAITING 1
// RPi env is up regardless of PING. [RED] will immediately turn off RPi.
#define MANAGER_STATE_ON_HOLD 2
// RPi env is off, [GREEN] will immediately turn on RPi
#define MANAGER_STATE_GUARD 3

#define MANAGER_STATE_WAITING_TIMEOUT_SECONDS 60
#define MANAGER_STATE_ACTIVE_TIMEOUT_SECONDS 15

#define MANAGER_SHUTDOWN_SIGNAL_TIMEOUT_MS 3000
#define MANAGER_SCREEN_UPDATE_MS 200

#define MANAGER_GPS_GUARD_CHECK_MS 3000
#define MANAGER_GPS_GUARD_TRIGGER_DISTANCE_METERS 50

#define MANAGER_ERROR_UNKNOWN_FRAME_TYPE 20
#define MANAGER_ERROR_FRAME_TYPE_DISABLED 21
#define MANAGER_ERROR_UNKNOWN_COMMAND 22
#define MANAGER_ERROR_INVALID_COMMAND 23

class Manager
{
	public:
	Manager(HardwareSerial* _arduino_out_port, RelayController* _relay, OledController* _oled, Buzzer* _buzzer, LightSensor* _light_sensor, GpsController* _gpsController);
	~Manager();
	
	bool before_button_send(int button_id, char state);
	void tick();
	
	void dispatch_frame(uint8_t* frame_array, int frame_len, uint8_t frame_type, unsigned short frame_id);
	
	
	private:
	RelayController* relay;
	OledController* oled;
	CommandWriter command_writer;
	Buzzer* buzzer;
	LightSensor* light_sensor;

	GpsController* gpsController;
	
	int temp;
	
	int state;
	unsigned long state_timestamp;
	unsigned long screen_timestamp;
	unsigned long shutdown_signal_timestamp;
	int remaining_beep;
	
	uint8_t process_frame(uint8_t* frame_array, int frame_len);
	void set_state(int newState);
	
	void update_screen();
	
	bool check_gps_guard();
	
	int guard_animation_counter;
	int guard_animation_mode;
	
	double gps_guard_lat;
	double gps_guard_lon;
	bool gps_guard_location_valid;
	unsigned long gps_guard_last_check_time;
};

#endif