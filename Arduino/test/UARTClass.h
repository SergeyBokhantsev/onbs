#pragma once

#include "HardwareSerial.h"

class UARTClass : public HardwareSerial
{
public:
    UARTClass();
    ~UARTClass();
};

