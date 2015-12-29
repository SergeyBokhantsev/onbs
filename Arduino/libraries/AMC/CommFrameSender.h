#ifndef commframesender_h
#define commframesender_h

#include "CommFrameProcessor.h"

#define FRAME_SEND_TIMEOUT_MS 50

class CommFrameSender : public CommFrameProcessor
{
	public:	
	CommFrameSender(UARTClass* _out, HardwareSerial** _in, char* _in_types, int _in_count, int _frame_max_size);
	void send_byte();
	
	private:
	int frame_max_size;
	UARTClass* out;
	HardwareSerial** in;
	char* in_types;
	int in_count;

	int current_buffer_index;
	int current_frame_size;
	int current_frame_send_time;

	void open_frame(char type);
	void close_frame(int size);
	void set_next_buffer();
	void write_open(char type);
	void write_close();

	int frames_count;
};

#endif