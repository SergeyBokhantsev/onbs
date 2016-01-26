
#include "CommandWriter.h"

CommandWriter::CommandWriter(HardwareSerial* _buffer) : 
buffer(_buffer)
{
	// "<-!"
	comm_frame_begin[0] = '<';
	comm_frame_begin[1] = '-';
	comm_frame_begin[2] = '!';
	
	// "!->"
	comm_frame_end[0] = '!';
	comm_frame_end[1] = '-';
	comm_frame_end[2] = '>';
}

void CommandWriter::send_command(uint8_t command)
{
	open_command_frame(command);
	close_command_frame();
}

void CommandWriter::send_command(uint8_t command, uint8_t arg1, uint8_t arg2)
{
	open_command_frame(command);
	write(arg1);
	write(arg2);
	close_command_frame();
}

void CommandWriter::send_command(uint8_t command, uint8_t arg1, uint8_t arg2, uint8_t arg3, uint8_t arg4, uint8_t arg5, uint8_t arg6)
{
	open_command_frame(command);
	write(arg1);
	write(arg2);
	write(arg3);
	write(arg4);
	write(arg5);
	write(arg6);
	close_command_frame();
}

void CommandWriter::write(uint8_t c)
{
	buffer->write(c);
}

void CommandWriter::open_command_frame(uint8_t ardu_command)
{
	for (int i = 0; i < FRAME_BEGIN_LEN; ++i)
	{
		buffer->write((uint8_t)comm_frame_begin[i]);
	}

	buffer->write(ardu_command);
}

void CommandWriter::close_command_frame()
{
	for (int i = 0; i < FRAME_END_LEN; ++i)
	{
		buffer->write((uint8_t)comm_frame_end[i]);
	}
}