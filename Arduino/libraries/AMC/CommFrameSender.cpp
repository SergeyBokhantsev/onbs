
#include "CommFrameSender.h"

CommFrameSender::CommFrameSender(UARTClass* _out, HardwareSerial** _in, char* _in_types, int _in_count, int _frame_max_size) : CommFrameProcessor(),
out(_out),
in(_in),
in_types(_in_types),
in_count(_in_count),
frame_max_size(_frame_max_size),
current_buffer_index(0),
current_frame_size(0),
frames_count(0)
{
	set_next_buffer();
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

		out->write((uint8_t)in[current_buffer_index]->read());

		current_frame_send_time = now;

		if (current_frame_size == frame_max_size)
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
	write_open(type);
}

void CommFrameSender::close_frame(int frameSize)
{
	write_close();
	//frames_count++;

	// write_open(INFO_FRAME_TYPE);
	// out->print(frames_count);
	// out->write('|');
	// out->print(frameSize);
	// write_close();
}

void CommFrameSender::write_open(char type)
{
	for (int i=0; i<COMM_FRAME_BEGIN_LEN; ++i)
	{
		out->write((uint8_t)comm_frame_begin[i]);
	}

	out->write((uint8_t)type);
}

void CommFrameSender::write_close()
{
	for (int i=0; i<COMM_FRAME_END_LEN; ++i)
	{
		out->write((uint8_t)comm_frame_end[i]);
	}
}