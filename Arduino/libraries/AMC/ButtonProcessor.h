#ifndef buttonprocessor_h
#define buttonprocessor_h

#include "Arduino.h"
#include "Manager.h"

#define BTN_STATE_RELEASED 45
#define BTN_STATE_PRESSED 43
#define BTN_STATE_HOLDED 44

#define BTN_PRESSED_FIRST_INTERVAL 800
#define BTN_PRESSED_SECOND_INTERVAL 300

#define BTN_PROCESS_INTERVAL 50

#define accept_btn_pin 50
#define cancel_btn_pin 48
#define f1_btn_pin 53
#define f2_btn_pin 49
#define rotary_select_pin 43

#define ROTARY_PIN_SIGNAL 0
#define ROTARY_PIN_VALUE 0

#define accept_btn_num 0
#define cancel_btn_num 9
#define f1_btn_num 1
#define f2_btn_num 2
#define rotary_select_num 3

#define rotary_prev_num 10
#define rotary_next_num 11

class ButtonProcessor
{
	public:	
	ButtonProcessor(HardwareSerial* _out_buffer, Manager* _manager);
	void init();
	void process();
	void send_button_state(int button_id, uint8_t state);

	private:
	HardwareSerial* out_buffer;
	Manager* manager;	
	
	unsigned long buttons_last_processed;
	const static int buttons_count = 10;
	byte button_pins[buttons_count];
	uint8_t button_states[buttons_count];
	unsigned long button_process_times[buttons_count];

	void init_button_pin(int pin);	
	bool check_button(int pin, uint8_t* state, unsigned long* last_processed_time);
};

#endif