#include "Buzzer.h"

Buzzer::Buzzer()
: beepTimestamp(0),
isBeeping(false)
{ 
}

Buzzer::~Buzzer()
{
}

void Buzzer::init()
{
	pinMode(BUZZER_PIN, OUTPUT);
	
	beep(30, 30, 4);
}

void Buzzer::tick()
{
	if (beepTimestamp > 0)
	{
		if (isBeeping && millis() > beepTimestamp + beepTime)
		{
			digitalWrite(BUZZER_PIN, HIGH);
			isBeeping = false;
			
			beepTimestamp = (--beepCount > 0) ? millis() : 0;
		}
		else if (!isBeeping && millis() > beepTimestamp + pauseTime)
		{
			digitalWrite(BUZZER_PIN, LOW);
			isBeeping = true;
			beepTimestamp = millis();
		}
	}
}

void Buzzer::beep(int beepMs, int pauseMs, int count)
{
	beepTimestamp = millis();
	beepTime = beepMs;
	pauseTime = pauseMs;
	beepCount = count;
	digitalWrite(BUZZER_PIN, LOW);
}