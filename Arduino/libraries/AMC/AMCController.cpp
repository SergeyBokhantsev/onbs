#include "AMCController.h"

#define GSM_ENABLE_PIN 8
#define GSM_RESET_PIN 9

#define BACK_DESCRIPTOR_LEN 8

bool enable_gsm()
{
  pinMode(GSM_ENABLE_PIN, OUTPUT);
	// power on pulse
  digitalWrite(GSM_ENABLE_PIN,HIGH);
  delay(2000);
  digitalWrite(GSM_ENABLE_PIN,LOW);
  return true;
}

bool reset_gsm()
{
  pinMode(GSM_RESET_PIN, OUTPUT);
	// power on pulse
  digitalWrite(GSM_RESET_PIN,HIGH);
  delay(1000);
  digitalWrite(GSM_RESET_PIN,LOW);
  return true;
}

void set_descr(char* descr, const char* what)
{
	for (int i=0; i<BACK_DESCRIPTOR_LEN; ++i)
	{
		descr[i] = what[i];
		if (what[i] == 0)
			break;
	}

	descr[BACK_DESCRIPTOR_LEN-1] = 0;
}

AMCController::AMCController(HardwareSerial* _gsmPort, HardwareSerial* _gpsPort, UARTClass* _comPort) :
gsmPort(_gsmPort),
comPort(_comPort),
frame_sender(_comPort, ports, ports_types, 3, COMM_FRAME_MAX_SIZE),
frame_receiver(_comPort),
outcom_writer(&out_buffer),
button_processor(&outcom_writer),
disable_buttons(false),
oled(),
relay()
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
	
	relay.turn_relay(RELAY_MASTER, RELAY_ENABLE);
	
	relay.schedule(RELAY_MASTER, RELAY_DISABLE, 120);
}

void AMCController::run()
{
	relay.tick();
	
	frame_sender.send_byte();

	process_incoming();

	if (!disable_buttons)
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
	bool result = false;

	char descr[BACK_DESCRIPTOR_LEN] = "UNKNOWN";

	switch (frame_type)
	{
		case GSM_FRAME_TYPE:
			set_descr(descr, "GSM_SND");
			if (frame_len > 0 && gsmPort != 0)
			{
				for (int i =0; i<frame_len; ++i)
				{
					gsmPort->write((uint8_t)frame_array[i]);
				}

				result = true;
			}
			break;
			
		case ARDUINO_COMMAND_FRAME_TYPE:
			result = process_incoming_arduino_command(frame_array, frame_len, descr);
			break;
			
		case OLED_COMMAND_FRAME_TYPE:
			result = oled.process(frame_array, frame_len);
			break;
			
		case RELAY_COMMAND_FRAME_TYPE:
			result = relay.process_frame(frame_array, frame_len);
			break;
	}

     outcom_writer.open_command(ARDUINO_COMMAND_FRAME_TYPE);
     outcom_writer.write(frame_type);
     outcom_writer.write(result ? '+' : '-');
     outcom_writer.write_line(descr);
     outcom_writer.close_command();
}

bool AMCController::process_incoming_arduino_command(char* data, int len, char* operation_descr)
{
	switch (*data)
	{
		case ARDUCOMMAND_EMPTY:
			//WDT_Restart(WDT);
			set_descr(operation_descr, "PING");
			return true;
			
		case ARDUCOMMAND_ENABLE_GSM:
			set_descr(operation_descr, "GSM_ENB");
			return enable_gsm();

        case ARDUCOMMAND_RESET_GSM:
        	set_descr(operation_descr, "GSM_RST");
            return reset_gsm();

        case ARDUCOMMAND_GSM_DUMP:
        	set_descr(operation_descr, "DUMP_2");
        	return true;

        case ARDUCOMMAND_DISABLE_BUTTONS:
        	disable_buttons = true;
        	set_descr(operation_descr, "BTNoff");
        	return true;

        case ARDUCOMMAND_ENABLE_BUTTONS:
        	disable_buttons = false;
        	set_descr(operation_descr, "BTNon");
        	return true;
	}
	
	return false;
}