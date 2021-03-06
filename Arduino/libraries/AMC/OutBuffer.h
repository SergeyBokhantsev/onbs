#ifndef outbuffer_h
#define outbuffer_h

#include "HardwareSerial.h"

#define OUT_BUFFER_SIZE 128

class OutBuffer : public HardwareSerial
{
	public:
	OutBuffer();
	int available(void);
    int read(void);
    size_t write(uint8_t);

	void begin(unsigned long) { }
    void end() { }
    int peek(void) { return -1; }
    void flush(void) { }
    operator bool() { return true; }

	uint8_t* get_buffer() { return &buffer[0]; }
    void reset() { head = head = 0; };

	private:
	uint8_t buffer[OUT_BUFFER_SIZE];
	int head; //point to write to
	int tail; //point to read from
};

#endif