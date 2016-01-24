#ifndef OledController_h
#define OledController_h

#include "Arduino.h"
#include <OLED_I2C.h>

#define TEXT_BUFFER_SIZE 128

#define OLED_COMMAND_CLS 0
#define OLED_COMMAND_FILL 1
#define OLED_COMMAND_INVERT 2
#define OLED_COMMAND_PIXEL 3
#define OLED_COMMAND_INVERT_PIXEL 4
#define OLED_COMMAND_PRINT 5
#define OLED_COMMAND_FONT 6
#define OLED_COMMAND_DRAW_LINE 7
#define OLED_COMMAND_CLR_LINE 8
#define OLED_COMMAND_DRAW_RECT 9
#define OLED_COMMAND_CLR_RECT 10
#define OLED_COMMAND_DRAW_ROUND_RECT 11
#define OLED_COMMAND_CLR_ROUND_RECT 12
#define OLED_COMMAND_DRAW_CIRCLE 13
#define OLED_COMMAND_CLR_CIRCLE 14
#define OLED_COMMAND_UPDATE 15
#define OLED_COMMAND_BRIGHTNESS 16

#define OLED_TEXT_X_ALIGN_MODE_NONE 0
#define OLED_TEXT_X_ALIGN_MODE_LEFT 1
#define OLED_TEXT_X_ALIGN_MODE_CENTER 2
#define OLED_TEXT_X_ALIGN_MODE_RIGHT 3

#define OLED_1_ARG (int)frame_array[1]
#define OLED_2_ARG (int)frame_array[2]
#define OLED_3_ARG (int)frame_array[3]
#define OLED_4_ARG (int)frame_array[4]
#define OLED_5_ARG (int)frame_array[5]

#define OLED_ERROR_EXECUTING_COMMAND_UNKN_FONT 20
#define OLED_ERROR_EXECUTING_COMMAND_ARGS_MISMATCH 21
#define OLED_ERROR_UNKNOWN_COMMAND 22

#define OLED_ICON_CLOCK 0
#define OLED_ICON_CAR_GUARD_1 1
#define OLED_ICON_CAR_GUARD_2 2
#define OLED_ICON_CAR_GUARD_3 3
#define OLED_ICON_CAR_GUARD_4 4
#define OLED_ICON_CAR_GUARD_5 5
#define OLED_ICON_CAR_GUARD_6 6

class OledController
{
public:
	OledController();
	~OledController();

	uint8_t process(const uint8_t* frame_array, int frame_len);
	
	void draw_icon(int icon);
	void draw_state_waiting(int remainingSeconds);
	void draw_state_hold();
	void draw_state_guard_hint();
	
	void messageI(int i);
	
	OLED display;
private:

	char text_buffer[TEXT_BUFFER_SIZE + 1];
};

#endif