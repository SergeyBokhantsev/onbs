#ifndef commframeprocessor_h
#define commframeprocessor_h

#include "Arduino.h"

#define GPS_PART_FRAME_TYPE 65
#define GSM_FRAME_TYPE 67
#define BUTTON_FRAME_TYPE 66
#define ARDUINO_COMMAND_FRAME_TYPE 68
#define OLED_COMMAND_FRAME_TYPE 69
#define RELAY_COMMAND_FRAME_TYPE 70

#define INFO_FRAME_TYPE 105

#define COMM_FRAME_BEGIN_LEN 3
#define COMM_FRAME_END_LEN 3

class CommFrameProcessor
{
	public:	
	CommFrameProcessor()
	{
		comm_frame_begin[0] = ':';
		comm_frame_begin[1] = '<';
		comm_frame_begin[2] = ':';

		comm_frame_end[0] = ':';
		comm_frame_end[1] = '>';
		comm_frame_end[2] = ':';
	}
	
	protected:
	uint8_t comm_frame_begin[COMM_FRAME_BEGIN_LEN];
	uint8_t comm_frame_end[COMM_FRAME_END_LEN];
	
	uint8_t calculate_checksum(uint8_t* frame_data, int data_len) 
	{
		uint8_t res = 0;
		if (data_len > 0)
		{
			for (int i=0; i<data_len; ++i)
			{
				res ^= frame_data[i];
			}
		}
		return res;
	}
};

#endif