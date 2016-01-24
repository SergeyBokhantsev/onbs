#include "ArrayToSerialAdapter.h"

ArrayToSerialAdapter::ArrayToSerialAdapter(uint8_t* _buffer, int _size) :
buffer(_buffer),
size(_size),
head(0),
tail(0)
{
}

int ArrayToSerialAdapter::read( void )
{
	if (available())
	{
		uint8_t ret = *(buffer + (tail++));

		if (!available())
		{
			head = 0;
			tail = 0;
		}

		return ret;
	}
	else
		return -1;
}

int ArrayToSerialAdapter::available()
{ 
	return head - tail;
}

size_t ArrayToSerialAdapter::write(uint8_t c)
{
	if (head == size)
		return 0;

	*(buffer + (head++)) = c;

	return 1;
}