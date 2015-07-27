#ifndef commframeprocessor_h
#define commframeprocessor_h

#include "Arduino.h"

#define GPS_PART_FRAME_TYPE 65
#define GSM_FRAME_TYPE 67
#define BUTTON_FRAME_TYPE 66
#define ARDUINO_COMMAND_FRAME_TYPE 68

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
	char comm_frame_begin[COMM_FRAME_BEGIN_LEN];
	char comm_frame_end[COMM_FRAME_END_LEN];
};

#endif