#pragma once

#include "Arduino.h"

class RingBuffer
{
public:
    RingBuffer();
    ~RingBuffer();
public:
    volatile uint8_t _aucBuffer[1];
    volatile int _iHead;
    volatile int _iTail;

public:
    void store_char(uint8_t c);
};

