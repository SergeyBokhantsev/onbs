#ifndef oled_animation_h
#define oled_animation_h

#include "Arduino.h"

class OledAnimation
{
	public:
	OledAnimation();
	~OledAnimation();

	private:
	uint8_t** frames;
	int count;
};

#endif