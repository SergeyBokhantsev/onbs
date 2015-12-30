#include "OledController.h"

extern uint8_t SmallFont[]; // FONT 0
extern uint8_t MediumNumbers[]; // FONT 1
extern uint8_t BigNumbers[]; // FONT 2

extern uint8_t clock_64_img[];

OledController::OledController() : display(SDA, SCL)
{
	display.begin();
	display.setBrightness(0);
}

OledController::~OledController()
{
}

void OledController::draw_clock()
{
	display.drawBitmap (32, 0, clock_64_img, 64, 64);
	display.update();
}

int get_x_aligned(int x, int mode)
{
	if (mode == OLED_TEXT_X_ALIGN_MODE_LEFT)
		return LEFT;
	else if (mode == OLED_TEXT_X_ALIGN_MODE_CENTER)
		return CENTER;
	else if (mode == OLED_TEXT_X_ALIGN_MODE_RIGHT)
		return RIGHT;
	else 
		return x;
}

int OledController::process(const char* frame_array, int frame_len)
{
	int command = (int)frame_array[0];
	
	switch(command)
	{
		case OLED_COMMAND_UPDATE:
			display.update();
			return 0;
		
		case OLED_COMMAND_CLS:
			display.clrScr();
			return 0;
			
		case OLED_COMMAND_FILL:
			display.fillScr(); 
			return 0;
			
		case OLED_COMMAND_INVERT:
			if (frame_len != 2)
				return OLED_ERROR_EXECUTING_COMMAND_ARGS_MISMATCH;			
			display.invert(OLED_1_ARG > 0);
			return 0;
			
		case OLED_COMMAND_PIXEL:
			if (frame_len != 4)
				return OLED_ERROR_EXECUTING_COMMAND_ARGS_MISMATCH;			
			if (OLED_1_ARG > 0)
				display.setPixel(OLED_2_ARG, OLED_3_ARG);
			else
				display.clrPixel(OLED_2_ARG, OLED_3_ARG);
			return 0;
			
		case OLED_COMMAND_INVERT_PIXEL:
			if (frame_len != 3)
				return OLED_ERROR_EXECUTING_COMMAND_ARGS_MISMATCH;			
			display.invPixel(OLED_1_ARG, OLED_2_ARG);
			return 0;
			
		case OLED_COMMAND_PRINT:
			// 1 - X Align mode
			// 2 - X
			// 3 - Y			
			if (frame_len < 4)
				return OLED_ERROR_EXECUTING_COMMAND_ARGS_MISMATCH;
			
			if (frame_len > 4)
			{
				int strLen = frame_len - 4;
				
				int i;
				for (i=0; i< strLen; ++i)
				{
					if (i == TEXT_BUFFER_SIZE)
						break;
					
					text_buffer[i] = frame_array[i + 4];
				}
				text_buffer[i] = 0;
				
				display.print(text_buffer, get_x_aligned(OLED_2_ARG, OLED_1_ARG), OLED_3_ARG);
			}
			return 0;
			
		case OLED_COMMAND_FONT:
			if (frame_len != 2)
				return OLED_ERROR_EXECUTING_COMMAND_ARGS_MISMATCH;
			
			switch (OLED_1_ARG)
			{
				case 0:
					display.setFont(SmallFont);
					return 0;
					
				case 1:
					display.setFont(MediumNumbers);
					return 0;
					
				case 2:
					display.setFont(BigNumbers);
					return 0;
				
				default:
					return OLED_ERROR_EXECUTING_COMMAND_UNKN_FONT;
			}
			
		case OLED_COMMAND_DRAW_LINE:
			if (frame_len != 5)
				return OLED_ERROR_EXECUTING_COMMAND_ARGS_MISMATCH;
			display.drawLine(OLED_1_ARG, OLED_2_ARG, OLED_3_ARG, OLED_4_ARG);
			return 0;
			
		case OLED_COMMAND_CLR_LINE:
			if (frame_len != 5)
				return OLED_ERROR_EXECUTING_COMMAND_ARGS_MISMATCH;
			display.clrLine(OLED_1_ARG, OLED_2_ARG, OLED_3_ARG, OLED_4_ARG);
			return 0;
			
		case OLED_COMMAND_DRAW_RECT:
			if (frame_len != 5)
				return OLED_ERROR_EXECUTING_COMMAND_ARGS_MISMATCH;
			display.drawRect(OLED_1_ARG, OLED_2_ARG, OLED_3_ARG, OLED_4_ARG);
			return 0;
			
		case OLED_COMMAND_CLR_RECT:
			if (frame_len != 5)
				return OLED_ERROR_EXECUTING_COMMAND_ARGS_MISMATCH;
			display.clrRect(OLED_1_ARG, OLED_2_ARG, OLED_3_ARG, OLED_4_ARG);
			return 0;
			
		case OLED_COMMAND_DRAW_ROUND_RECT:
			if (frame_len != 5)
				return OLED_ERROR_EXECUTING_COMMAND_ARGS_MISMATCH;
			display.drawRoundRect(OLED_1_ARG, OLED_2_ARG, OLED_3_ARG, OLED_4_ARG);
			return 0;
			
		case OLED_COMMAND_CLR_ROUND_RECT:
			if (frame_len != 5)
				return OLED_ERROR_EXECUTING_COMMAND_ARGS_MISMATCH;
			display.clrRoundRect(OLED_1_ARG, OLED_2_ARG, OLED_3_ARG, OLED_4_ARG);
			return 0;
			
		case OLED_COMMAND_DRAW_CIRCLE:
			if (frame_len != 4)
				return OLED_ERROR_EXECUTING_COMMAND_ARGS_MISMATCH;
			display.drawCircle(OLED_1_ARG, OLED_2_ARG, OLED_3_ARG);
			return 0;
			
		case OLED_COMMAND_CLR_CIRCLE:
			if (frame_len != 4)
				return OLED_ERROR_EXECUTING_COMMAND_ARGS_MISMATCH;
			display.clrCircle(OLED_1_ARG, OLED_2_ARG, OLED_3_ARG);
			return 0;
			
		case OLED_COMMAND_BRIGHTNESS:
			if (frame_len != 2)
				return OLED_ERROR_EXECUTING_COMMAND_ARGS_MISMATCH;
			display.setBrightness(OLED_1_ARG);
			return 0;
			
		default:
			return OLED_ERROR_UNKNOWN_COMMAND;
	}
}

void OledController::draw_state_waiting(int remainingSeconds)
{
	display.clrScr();
	display.invert(false);
	display.setFont(SmallFont);
	display.print((char*)"Initializing...", CENTER, 10);
	display.setFont(MediumNumbers);
	display.printNumI(remainingSeconds, CENTER, 20);
	display.update();
}

void OledController::draw_state_hold()
{
	display.clrScr();
	display.invert(false);
	display.setFont(SmallFont);
	display.print((char*)"On HOLD", CENTER, 10);
	display.print((char*)"[RED] to turn OFF", CENTER, 20);
	display.update();
}

void OledController::draw_state_guard()
{
	display.clrScr();
	display.invert(false);
	display.setFont(SmallFont);
	display.print((char*)"...", CENTER, 10);
	display.print((char*)"[GRN] to launch", CENTER, 20);
	display.update();
}

void OledController::messageI(int i)
{
	display.clrScr();
	display.invert(false);
	display.setFont(MediumNumbers);
	display.printNumI(i, CENTER, 20);
	display.update();
}