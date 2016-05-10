#include "AMCController.h"

AMCController::AMCController(HardwareSerial* _gsmPort, HardwareSerial* _nmeaGpsPort, UARTClass* _comPort) :
buzzer(),
oled(),
relay(),
light_sensor(),
arduino_out_port(arduino_out_buffer, ARDUINO_OUT_BUFFER_SIZE),
buttons_out_port(buttons_out_buffer, BUTTONS_OUT_BUFFER_SIZE),
gps_out_port(gps_out_buffer, GPS_OUT_BUFFER_SIZE),
gpsController(_nmeaGpsPort, &gps_out_port),
manager(&arduino_out_port, &relay, &oled, &buzzer, &light_sensor, &gpsController),
gsmPort(_gsmPort),
comPort(_comPort),
frame_sender(_comPort, ports, ports_types, 4),
frame_receiver(_comPort),
button_processor(&buttons_out_port, &manager),
rotary_encoder(&button_processor)
{
	ports[0] = _gsmPort;
	ports[1] = &gps_out_port;
	ports[2] = &arduino_out_port;
	ports[3] = &buttons_out_port;

	ports_types[0] = GSM_FRAME_TYPE;
	ports_types[1] = GPS_PART_FRAME_TYPE;
	ports_types[2] = ARDUINO_COMMAND_FRAME_TYPE;
	ports_types[3] = BUTTON_FRAME_TYPE;
}

AMCController::~AMCController()
{
}

void AMCController::init()
{
	buzzer.init();
	
	button_processor.init();
	
	rotary_encoder.init();
	
	relay.init();	

	light_sensor.init();
}

void AMCController::run()
{
	buzzer.tick();
	
	manager.tick();
	
	frame_sender.send_byte();

	process_incoming();

	button_processor.process();
	
	gpsController.process();
	
	rotary_encoder.process();
}

void AMCController::process_incoming()
{
	uint8_t* frame_array = 0;
	int frame_len = 0;
	uint8_t frame_type = 0;
	unsigned short out_frame_id = 0;

	if (frame_receiver.get_frame(&frame_array, &frame_len, &frame_type, &out_frame_id))
	{
		manager.dispatch_frame(frame_array, frame_len, frame_type, out_frame_id);
	}
}