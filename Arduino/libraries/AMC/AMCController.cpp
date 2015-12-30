#include "AMCController.h"

AMCController::AMCController(HardwareSerial* _gsmPort, HardwareSerial* _gpsPort, UARTClass* _comPort) :
oled(),
relay(),
manager(&relay, &oled),
gsmPort(_gsmPort),
comPort(_comPort),
frame_sender(_comPort, ports, ports_types, 3, COMM_FRAME_MAX_SIZE),
frame_receiver(_comPort),
outcom_writer(&out_buffer),
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
	
	oled.draw_clock();
}

AMCController::~AMCController()
{
}

void AMCController::init()
{
	button_processor.init();
	
	relay.init();	
}

void AMCController::run()
{
	frame_sender.send_byte();

	process_incoming();

	button_processor.process();
}

void AMCController::process_incoming()
{
	char* frame_array = 0;
	int frame_len = 0;
	char frame_type = 0;

	if (frame_receiver.get_frame(&frame_array, &frame_len, &frame_type))
	{
		process_frame(frame_array, frame_len, frame_type);
	}
}

void AMCController::process_frame(char* frame_array, int frame_len, char frame_type)
{
	int result = ARDU_ERROR_UNKNOWN_FRAME_TYPE;

	manager.on_incoming_frame();
	
	switch (frame_type)
	{
		case GSM_FRAME_TYPE:
			result = ARDU_ERROR_FRAME_TYPE_DISABLED;
			break;
			
		case ARDUINO_COMMAND_FRAME_TYPE:
			result = manager.process_frame(frame_array, frame_len);
			break;
			
		case OLED_COMMAND_FRAME_TYPE:
			result = oled.process(frame_array, frame_len);
			break;
			
		case RELAY_COMMAND_FRAME_TYPE:
			result = relay.process_frame(frame_array, frame_len);
			break;			
	}

	if (result > 0)
	{
     outcom_writer.open_command(ARDUINO_COMMAND_FRAME_TYPE);
     outcom_writer.write(frame_type);
     outcom_writer.write((char)result);
     outcom_writer.close_command();
	}
}