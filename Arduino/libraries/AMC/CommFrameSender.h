#ifndef commframesender_h
#define commframesender_h

#include "CommFrameProcessor.h"
#include "OutBuffer.h"

#define FRAME_SEND_TIMEOUT_MS 10

class CommFrameSender : public CommFrameProcessor
{
	public:	
	CommFrameSender(UARTClass* _out, HardwareSerial** _in, char* _in_types, int _in_count);
	void send_byte();
	
	private:
	OutBuffer out_buffer;
	UARTClass* out;
	HardwareSerial** in;
	char* in_types;
	int in_count;
	unsigned short frame_id;
	int max_frame_size;

	int current_buffer_index;
	int current_frame_size;
	int current_frame_send_time;

	void open_frame(char type);
	void close_frame(int size);
	void set_next_buffer();

	void write_frame();

	int frames_count;
};

#endif