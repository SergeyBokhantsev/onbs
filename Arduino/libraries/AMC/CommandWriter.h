#ifndef commandwriter_h
#define commandwriter_h

#include "Arduino.h"

#define FRAME_BEGIN_LEN 3
#define FRAME_END_LEN 3

class CommandWriter
{
	public:	
	CommandWriter(HardwareSerial* _buffer);

	void send_frame_confirmation(unsigned short id);
	void send_frame_fail(uint8_t frame_type, uint8_t result_code);
	void send_shutdown_signal();
	
	void send_command(uint8_t command);
	void send_command(uint8_t command, uint8_t arg1, uint8_t arg2);
	void send_command(uint8_t command, uint8_t arg1, uint8_t arg2, uint8_t arg3, uint8_t arg4, uint8_t arg5, uint8_t arg6);
	
	private:
	HardwareSerial* buffer;

	void write(uint8_t c);
	void open_command_frame(uint8_t ardu_command);
	void close_command_frame();
	
	char comm_frame_begin[FRAME_BEGIN_LEN];// = { 'A', 'C', '{' };
	//const char comm_frame_begin_len = 3;

	char comm_frame_end[FRAME_END_LEN];// = { '}' };
	//const char comm_frame_end_len = 1;
};

#endif