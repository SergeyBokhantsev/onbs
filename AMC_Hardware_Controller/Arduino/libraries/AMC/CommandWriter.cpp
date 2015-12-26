
#include "CommandWriter.h"

CommandWriter::CommandWriter(HardwareSerial* _buffer) : 
buffer(_buffer)
{
	comm_frame_begin[0] = 'a';
	comm_frame_begin[1] = 'c';
	comm_frame_begin[2] = '{';

	comm_frame_end[0] = '}';
}

void CommandWriter::write(char c)
{
	buffer->write((uint8_t)c);
}

void CommandWriter::write_line(const char* line)
{
	int i = 0;
	while(line[i] != 0)
	{
		buffer->write((uint8_t)line[i++]);
	}
}

void CommandWriter::open_command(char command_type)
{
	for (int i = 0; i < FRAME_BEGIN_LEN; ++i)
	{
		buffer->write((uint8_t)comm_frame_begin[i]);
	}

	buffer->write(command_type);
}

void CommandWriter::close_command()
{
	for (int i = 0; i < FRAME_END_LEN; ++i)
	{
		buffer->write((uint8_t)comm_frame_end[i]);
	}
}