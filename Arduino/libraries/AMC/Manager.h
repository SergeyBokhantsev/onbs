#ifndef manager_h
#define manager_h

#include "Arduino.h"
#include "RelayController.h"
#include "OledController.h"

// RPi env is up and connected (evaluated by PING signal)
#define MANAGER_STATE_ACTIVE 0
// RPi env is up, but no PING signal for N seconds. Preparing to turn of RPi. PING will switch to ACTIVE.
#define MANAGER_STATE_WAITING 1
// RPi env is up regardless of PING. [RED] will immediately turn off RPi.
#define MANAGER_STATE_ON_HOLD 2
// RPi env is off, [GREEN] will immediately turn on RPi
#define MANAGER_STATE_GUARD 3

#define MANAGER_STATE_WAITING_TIMEOUT_SECONDS 120
#define MANAGER_STATE_ACTIVE_TIMEOUT_SECONDS 15

#define MANAGER_SCREEN_UPDATE_MS 200

#define ARDUCOMMAND_EMPTY 100
#define ARDUCOMMAND_HOLD 101

#define MANAGER_ERROR_UNKNOWN_COMMAND 22
#define MANAGER_ERROR_INVALID_COMMAND 23

class Manager
{
	public:
	Manager(RelayController* _relay, OledController* _oled);
	~Manager();
	
	void on_incoming_frame();
	bool before_button_send(int button_id, char state);
	void tick();
	int process_frame(char* frame_array, int frame_len);
	
	private:
	RelayController* relay;
	OledController* oled;
	
	int state;
	unsigned long state_timestamp;
	unsigned long screen_timestamp;
	
	void set_state(int newState);
	
	void update_screen();
};

#endif