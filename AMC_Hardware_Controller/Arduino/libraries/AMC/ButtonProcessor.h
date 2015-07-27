#ifndef buttonprocessor_h
#define buttonprocessor_h

#include "Arduino.h"
#include "CommandWriter.h"

#define BTN_STATE_RELEASED 45
#define BTN_STATE_PRESSED 43
#define BTN_STATE_HOLDED 44

#define BTN_PRESSED_FIRST_INTERVAL 800
#define BTN_PRESSED_SECOND_INTERVAL 300

#define accept_btn_pin 49
#define cancel_btn_pin 41
#define f1_btn_pin 47
#define f2_btn_pin 50
#define f3_btn_pin 48
#define f4_btn_pin 45
#define f5_btn_pin 46
#define f6_btn_pin 51
#define f7_btn_pin 53
#define f8_btn_pin 43

#define accept_btn_num 0
#define cancel_btn_num 9
#define f1_btn_num 1
#define f2_btn_num 2
#define f3_btn_num 3
#define f4_btn_num 4
#define f5_btn_num 5
#define f6_btn_num 6
#define f7_btn_num 7
#define f8_btn_num 8

class ButtonProcessor
{
	public:	
	ButtonProcessor(CommandWriter* _writer);
	void init();
	void process();

	private:
	CommandWriter* writer;

	unsigned long buttons_process_interval;
	unsigned long buttons_last_processed;
	const static int buttons_count = 10;
	byte button_pins[buttons_count];
	char button_states[buttons_count];
	unsigned long button_process_times[buttons_count];

	void init_button_pin(int pin);
	void send_button_state(int button_id, char state);
	bool check_button(int pin, char* state, unsigned long* last_processed_time);
};

#endif