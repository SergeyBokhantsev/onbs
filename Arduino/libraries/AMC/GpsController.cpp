#include "GpsController.h"

GpsController::GpsController(HardwareSerial* _nmeaGpsPort, HardwareSerial* _decodedGpsPort)
: nmeaGpsPort(_nmeaGpsPort),
decodedGpsPort(_decodedGpsPort),
last_process_time(0),
last_send_time(0)
{ 
	
}

GpsController::~GpsController()
{
}

void GpsController::process()
{
	unsigned long now = millis();
	
	if (now > last_process_time + GPS_PROCESS_INTERVAL)
	{
		int count = nmeaGpsPort->available();
		
		for (int i=0; i<count; ++i)
		{
			char v = nmeaGpsPort->read();
			write_c(v);
			tinyGps.encode(v);
		}
		
		last_process_time = now;
	}
	
	//!!!!
	return;
	
	if (now > last_send_time + GPS_SEND_INTERVAL)
	{	
		write_c('G');write_c('P');write_c('S');write_c(':');

		write_c((char)GPS_CONTROLLER_VALUE_LOCATION);
		write_d(tinyGps.location.isValid() ? '1' : '0');
		write_d(tinyGps.location.lat());
		write_d(tinyGps.location.lng());		
		
		write_c('E');write_c('N');write_c('D');
		
		last_send_time = now;
	}
}

void GpsController::write_c(char c)
{
	decodedGpsPort->write((uint8_t)c);
}

void GpsController::write_d(double val)
{
	uint8_t b = 0;
	uint64_t ival = (uint64_t)val;
	
	for(int i=0; i<8; ++i)
	{
		b = (uint8_t)(ival & 0xFF);
		decodedGpsPort->write(b);
		ival >> 8;
	}
}

double GpsController::distance(double lat1, double lon1, double lat2, double lon2)
	{
		double R = 6371000; // m
		double degreeToRad = 0.0174444444444444;
		
        double f1 = lat1 * degreeToRad;
        double f2 = lat2 * degreeToRad;
		double df = degreeToRad * (lat2 - lat1);
        double dl = degreeToRad * (lon2 - lon1);

        double a = sin(df / 2) * sin(df / 2) +
                    cos(f1) * cos(f2) *
                    sin(dl / 2) * sin(dl / 2);
					
        double c = 2 * atan2(sqrt(a), sqrt(1 - a));

        return R * c;
	}
