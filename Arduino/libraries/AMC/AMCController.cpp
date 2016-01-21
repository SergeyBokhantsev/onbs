#include "AMCController.h"

AMCController::AMCController(HardwareSerial* _gsmPort, HardwareSerial* _gpsPort, UARTClass* _comPort) :
buzzer(),
oled(),
relay(),
outcom_writer(&out_buffer),
manager(&outcom_writer, &relay, &oled, &buzzer),
gsmPort(_gsmPort),
comPort(_comPort),
frame_sender(_comPort, ports, ports_types, 3),
frame_receiver(_comPort),
button_processor(&outcom_writer, &manager)
{
	ports[0] = _gsmPort;
	ports[1] = _gpsPort;
	ports[2] = &out_buffer;

	ports_types[0] = GSM_FRAME_TYPE;
	ports_types[1] = GPS_PART_FRAME_TYPE;
	ports_types[2] = ARDUINO_COMMAND_FRAME_TYPE;

	outcom_writer.open_command(ARDUINO_COMMAND_FRAME_TYPE);
    outcom_writer.write_line("ARDUINO STARTED");
    outcom_writer.close_command();
	
	//oled.draw_clock();
}

AMCController::~AMCController()
{
}

void AMCController::init()
{
	buzzer.init();
	
	button_processor.init();
	
	relay.init();	
}

void AMCController::run()
{
	buzzer.tick();
	
	manager.tick();
	
	frame_sender.send_byte();

	process_incoming();

	button_processor.process();
}

void AMCController::process_incoming()
{
	char* frame_array = 0;
	int frame_len = 0;
	char frame_type = 0;
	unsigned short out_frame_id = 0;

	if (frame_receiver.get_frame(&frame_array, &frame_len, &frame_type, &out_frame_id))
	{
		manager.dispatch_frame(frame_array, frame_len, frame_type, out_frame_id);
	}
}