#ifndef commframereceiver_h
#define commframereceiver_h

#include "CommFrameProcessor.h"

#define FRAME_DATA_SIZE 2048

class CommFrameReceiver : public CommFrameProcessor
{
	public:	
	CommFrameReceiver(HardwareSerial* _inputPort);
	bool get_frame(char** out_frame_array, int* out_frame_len, char* out_frame_type);
	
	private:
	HardwareSerial* inputPort;

	char frame_type;
	int frame_len;
	char frame_data[FRAME_DATA_SIZE];

	uint8_t readingIncomingDataState;
	int matchedFrameMarkCount;
};

#endif