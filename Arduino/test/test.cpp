#include "..\libraries\AMC\OutBuffer.h"
#include "..\libraries\AMC\CommFrameSender.h"

unsigned long time_now = 0;

unsigned long millis()
{
    return time_now++;
}

void main()
{
    OutBuffer* ob = new OutBuffer();
    UARTClass* out_uart = new UARTClass();

    HardwareSerial* in[1];
    in[0] = new HardwareSerial();

    char in_types[1];
    in_types[0] = (char)70;

    CommFrameSender* sender = new CommFrameSender(out_uart, in, in_types, 1);

    while (1)
    {
        sender->send_byte();
    }
}