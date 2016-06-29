#ifndef gps_controller_h
#define gps_controller_h

#include "Arduino.h"
#include "TinyGPS++.h"

#define GPS_PROCESS_INTERVAL 20
#define GPS_SEND_INTERVAL 1000

#define GPS_CONTROLLER_VALUE_LOCATION 0

class GpsController
{
	public:
	GpsController(HardwareSerial* _nmeaGpsPort, HardwareSerial* _decodedGpsPort);
	~GpsController();
	
	void process();
	
	TinyGPSPlus* GPS() { return &tinyGps; }
	
	double distance(double lat1, double lon1, double lat2, double lon2);
	
	private:
	TinyGPSPlus tinyGps;
	HardwareSerial* nmeaGpsPort;
	HardwareSerial* decodedGpsPort;
	unsigned long last_process_time;
	unsigned long last_send_time;
	
	void write_c(char c);
	void write_d(double val);
};

#endif