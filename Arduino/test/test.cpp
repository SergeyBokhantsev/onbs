#include "..\libraries\AMC\AMCRingBuffer.h"
#include "..\libraries\AMC\AMCController.h"

int test_ringbuffer()
{
	RB b(0);

	/// 1
	for (int i = 0; i < RING_BUFFER_SIZE * 4; ++i)
	{
		if (b.get_len() != 0)
			return 1;

		b.add((unsigned char)i);

		if (b.get_len() != 1)
			return 2;

		if (b.get() != (unsigned char)i)
			return 3;

		if (b.get_len() != 0)
			return 4;
	}

	/// 2
	for (int i = 0; i < RING_BUFFER_SIZE * 4; ++i)
	{
		if (b.get_len() != 0)
			return 5;

		b.add((unsigned char)i);
		b.add((unsigned char)(i+1));
		b.add((unsigned char)(i+2));

		if (b.get_len() != 3)
			return 6;

		if (b.get() != (unsigned char)i)
			return 7;
		if (b.get() != (unsigned char)(i+1))
			return 8;
		if (b.get() != (unsigned char)(i+2))
			return 9;

		if (b.get_len() != 0)
			return 10;
	}

	/// 3
	for (int i = 0; i < RING_BUFFER_SIZE * 4; ++i)
	{
		b.add(1);
	}

	if (b.get_len() != RING_BUFFER_SIZE)
		return 11;

	for (int i = 0; i < RING_BUFFER_SIZE; ++i)
	{
		if (b.get() != 1)
			return 12;
	}

	if (b.get_len() != 0)
			return 13;

	if (b.get() != 0)
			return 14;

	return 0;
}

int assert(int res)
{
	if (res != 0)
	{
		return res;
	}
}

void main()
{
	assert(test_ringbuffer());

	AMCController c;
}