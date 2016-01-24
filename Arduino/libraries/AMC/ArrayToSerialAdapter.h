#ifndef arraytoserialadapter_h
#define arraytoserialadapter_h

#include "HardwareSerial.h"

class ArrayToSerialAdapter : public HardwareSerial
{
	public:
	ArrayToSerialAdapter(uint8_t* _buffer, int _size);
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
	uint8_t* buffer;
	int size;
	int head; //point to write to
	int tail; //point to read from
};

#endif