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

bool OledController::process(const char* frame_array, int frame_len)
{
	int command = (int)frame_array[0];
	
	switch(command)
	{
		case OLED_COMMAND_UPDATE:
			display.update();
			return true;
		
		case OLED_COMMAND_CLS:
			display.clrScr();
			return true;
			
		case OLED_COMMAND_FILL:
			display.fillScr(); 
			return true;
			
		case OLED_COMMAND_INVERT:
			display.invert(OLED_1_ARG > 0);
			return true;
			
		case OLED_COMMAND_PIXEL:
			if (OLED_1_ARG > 0)
				display.setPixel(OLED_2_ARG, OLED_3_ARG);
			else
				display.clrPixel(OLED_2_ARG, OLED_3_ARG);
			return true;
			
		case OLED_COMMAND_INVERT_PIXEL:
			display.invPixel(OLED_1_ARG, OLED_2_ARG);
			return true;
			
		case OLED_COMMAND_PRINT:
			// 1 - X Align mode
			// 2 - X
			// 3 - Y
			if (frame_len > 4)
			{
				int strLen = frame_len - 4;
				
				int i;
				for (i=0; i< strLen; ++i)
				{
					if (i == TEXT_BUFFER_SIZE)
						break;
					
					text_buffer[i] = frame_array[i + 3];
				}
				text_buffer[i] = 0;
				
				display.print(text_buffer, get_x_aligned(OLED_2_ARG, OLED_1_ARG), OLED_3_ARG);
				
				return true;
			}
			else
				return false;
			
		case OLED_COMMAND_FONT:
			switch (OLED_1_ARG)
			{
				case 0:
					display.setFont(SmallFont);
					return true;
					
				case 1:
					display.setFont(MediumNumbers);
					return true;
					
				case 2:
					display.setFont(BigNumbers);
					return true;
				
				default:
					return false;
			}
			
		case OLED_COMMAND_DRAW_LINE:
			display.drawLine(OLED_1_ARG, OLED_2_ARG, OLED_3_ARG, OLED_4_ARG);
			return true;
			
		case OLED_COMMAND_CLR_LINE:
			display.clrLine(OLED_1_ARG, OLED_2_ARG, OLED_3_ARG, OLED_4_ARG);
			return true;
			
		case OLED_COMMAND_DRAW_RECT:
			display.drawRect(OLED_1_ARG, OLED_2_ARG, OLED_3_ARG, OLED_4_ARG);
			return true;
			
		case OLED_COMMAND_CLR_RECT:
			display.clrRect(OLED_1_ARG, OLED_2_ARG, OLED_3_ARG, OLED_4_ARG);
			return true;
			
		case OLED_COMMAND_DRAW_ROUND_RECT:
			display.drawRoundRect(OLED_1_ARG, OLED_2_ARG, OLED_3_ARG, OLED_4_ARG);
			return true;
			
		case OLED_COMMAND_CLR_ROUND_RECT:
			display.clrRoundRect(OLED_1_ARG, OLED_2_ARG, OLED_3_ARG, OLED_4_ARG);
			return true;
			
		case OLED_COMMAND_DRAW_CIRCLE:
			display.drawCircle(OLED_1_ARG, OLED_2_ARG, OLED_3_ARG);
			return true;
			
		case OLED_COMMAND_CLR_CIRCLE:
			display.clrCircle(OLED_1_ARG, OLED_2_ARG, OLED_3_ARG);
			return true;
	}
	
	return false;
}
