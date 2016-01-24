#ifndef commframesender_h
#define commframesender_h

#include "CommFrameProcessor.h"
#include "ArrayToSerialAdapter.h"

#define FRAME_SEND_TIMEOUT_MS 10

#define OUTCOMING_BUFFER_SIZE 128

class CommFrameSender : public CommFrameProcessor
{
	public:	
	CommFrameSender(UARTClass* _out, HardwareSerial** _in, uint8_t* _in_types, int _in_count);
	void send_byte();
	
	private:
	ArrayToSerialAdapter out_buffer;
	uint8_t out_array[OUTCOMING_BUFFER_SIZE];
	
	UARTClass* out;
	HardwareSerial** in;
	uint8_t* in_types;
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