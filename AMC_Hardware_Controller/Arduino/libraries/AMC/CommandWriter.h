#ifndef commandwriter_h
#define commandwriter_h

#include "Arduino.h"

#define FRAME_BEGIN_LEN 3
#define FRAME_END_LEN 1

class CommandWriter
{
	public:	
	CommandWriter(HardwareSerial* _buffer);
	void write(char c);
	void write_line(char* line);
	void open_command(char command_type);
	void close_command();

	private:
	HardwareSerial* buffer;

	char comm_frame_begin[FRAME_BEGIN_LEN];// = { 'A', 'C', '{' };
	//const char comm_frame_begin_len = 3;

	char comm_frame_end[FRAME_END_LEN];// = { '}' };
	//const char comm_frame_end_len = 1;
};

#endif