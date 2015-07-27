#include "OutBuffer.h"

OutBuffer::OutBuffer() :
head(0),
tail(0)
{
}

int OutBuffer::read( void )
{
	if (available())
	{
		uint8_t ret = buffer[tail++];

		if (!available())
		{
			head = 0;
			tail = 0;
		}

		return ret;
	}
}

int OutBuffer::available()
{ 
	return head - tail;
}

size_t OutBuffer::write(uint8_t c)
{
	if (head == OUT_BUFFER_SIZE)
		return 0;

	buffer[head++] = c;

	return 1;
}