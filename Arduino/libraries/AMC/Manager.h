#ifndef manager_h
#define manager_h

#include "Arduino.h"
#include "RelayController.h"
#include "OledController.h"
#include "CommandWriter.h"

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

//Incoming commands
#define ARDUCOMMAND_PING_REQUEST 100
#define ARDUCOMMAND_HOLD 102

//Outcoming commands
#define ARDUCOMMAND_PING_RESPONSE 101
#define ARDUCOMMAND_COMMAND_RESULT 103
#define ARDUCOMMAND_SHUTDOWN_SIGNAL 104

#define MANAGER_ERROR_UNKNOWN_FRAME_TYPE 20
#define MANAGER_ERROR_FRAME_TYPE_DISABLED 21
#define MANAGER_ERROR_UNKNOWN_COMMAND 22
#define MANAGER_ERROR_INVALID_COMMAND 23

class Manager
{
	public:
	Manager(CommandWriter* _outcom_writer, RelayController* _relay, OledController* _oled);
	~Manager();
	
	bool before_button_send(int button_id, char state);
	void tick();
	
	void dispatch_frame(char* frame_array, int frame_len, char frame_type);
	
	
	private:
	RelayController* relay;
	OledController* oled;
	CommandWriter* outcom_writer;
	
	int state;
	unsigned long state_timestamp;
	unsigned long screen_timestamp;
	unsigned long shutdown_signal_timestamp;
	
	int process_frame(char* frame_array, int frame_len);
	void set_state(int newState);
	
	void update_screen();
	
	int guard_animation_counter;
	int guard_animation_mode;
};

#endif