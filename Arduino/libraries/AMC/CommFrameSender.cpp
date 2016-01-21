
#include "CommFrameSender.h"

CommFrameSender::CommFrameSender(UARTClass* _out, HardwareSerial** _in, char* _in_types, int _in_count) : CommFrameProcessor(),
out_buffer(),
out(_out),
in(_in),
in_types(_in_types),
in_count(_in_count),
current_buffer_index(0),
current_frame_size(0),
frames_count(0),
frame_id(0)
{
	set_next_buffer();
	max_frame_size = OUT_BUFFER_SIZE - (COMM_FRAME_BEGIN_LEN + 4 + COMM_FRAME_END_LEN + 2);
}

void CommFrameSender::set_next_buffer()
{
	if (++current_buffer_index == in_count)
		current_buffer_index = 0;

	current_frame_size = 0;
	current_frame_send_time = 0;// = time_now();
}

void CommFrameSender::send_byte()
{
	unsigned long now = millis();

	if (in[current_buffer_index] == 0)
		set_next_buffer();

	if (in[current_buffer_index]->available())// && out->availableForWrite())
	{
		if (current_frame_size++ == 0)
		{
			open_frame(in_types[current_buffer_index]);
		}

		out_buffer.write((uint8_t)in[current_buffer_index]->read());

		current_frame_send_time = now;

		if (current_frame_size == max_frame_size)
		{
			close_frame(current_frame_size);
			set_next_buffer();
		}
	}
	else if (now > current_frame_send_time + FRAME_SEND_TIMEOUT_MS)
	{
		if (current_frame_size > 0)
		{
			close_frame(current_frame_size);
		}

		set_next_buffer();
	}
}

void CommFrameSender::open_frame(char type)
{
	//START MARKER
	for (int i=0; i<COMM_FRAME_BEGIN_LEN; ++i)
	{
		out_buffer.write((uint8_t)comm_frame_begin[i]);
	}
	//TYPE
	out_buffer.write((uint8_t)type);
	//ID
	frame_id++;
	out_buffer.write((uint8_t)((frame_id >> 8) & 0xFF));
	out_buffer.write((uint8_t)(frame_id & 0xFF));
	//CHECKSUM (placeholder)
	out_buffer.write(0);
}

void CommFrameSender::close_frame(int frameSize)
{
	int frame_data_offset = COMM_FRAME_BEGIN_LEN + 4;
	uint8_t* frame_data = (out_buffer.get_buffer()) + frame_data_offset;
	int frame_data_len = out_buffer.available() - frame_data_offset;
	char checksum = calculate_checksum((char*)frame_data, frame_data_len);
	
	int frame_checksum_offset = COMM_FRAME_BEGIN_LEN + 3; 
	uint8_t* frame_checksum = (out_buffer.get_buffer()) + frame_checksum_offset;
	*frame_checksum = (uint8_t)checksum;
	
	for (int i=0; i<COMM_FRAME_END_LEN; ++i)
	{
		out_buffer.write((uint8_t)comm_frame_end[i]);
	}
	
	write_frame();
}

void CommFrameSender::write_frame()
{
	while(out_buffer.available())
	{
		out->write(out_buffer.read());
	}
}