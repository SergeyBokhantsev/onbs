#pragma once

#include "types.h"

class HardwareSerial
{
public:
    HardwareSerial();
    ~HardwareSerial();

    virtual void begin(unsigned long) { };
    virtual void end() { };
    virtual int available(void) { return 1; };
    virtual int peek(void) { return 0; };
    virtual int read(void) { return 2; };
    virtual void flush(void) {};
    virtual size_t write(uint8_t)  { return 0; };
    //virtual operator bool() {  };
};

