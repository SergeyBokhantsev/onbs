#ifndef buzzer_h
#define buzzer_h

#include "Arduino.h"

#define BUZZER_PIN 23

class Buzzer
{
	public:
	Buzzer();
	~Buzzer();
	void init();
	void tick();
	void beep(int beepMs, int pauseMs, int count);
	
	private:
	bool isBeeping;
	int beepTime;
	int pauseTime;
	int beepCount;
	unsigned long beepTimestamp;
};

#endif