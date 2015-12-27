#include <AMCController.h>

#include <DueTimer.h>

#define comm_port Serial1

#define gps_port Serial3
#define gps_port_speed 38400
#define gps_port_enabled true

#define gsm_port Serial2
#define gsm_port_speed 460800
#define gsm_port_enabled false
#define gsm_enable_pin 8
#define gsm_reset_pin 9

AMCController ctrl(gsm_port_enabled ? &gsm_port : 0,
                   gps_port_enabled ? &gps_port : 0,
                   &comm_port);

//##########################################
//############## SETUP #####################
//##########################################

void watchdogSetup (void)
{
  // watchdog for 5 seconds
  watchdogEnable(5000);
}

void setup() 
{
  //Timer3.attachInterrupt(time_tick).start(TIMESTAMPDELTA * 1000);
  
  comm_port.begin(115200, SERIAL_8N1);
  
  if (gps_port_enabled)
  {
    gps_port.begin(gps_port_speed);
    gps_port.setInterruptPriority(100);
  }
  
  if (gsm_port_enabled)
  {
    gsm_port.begin(gsm_port_speed);
    gsm_port.setInterruptPriority(0);
  }
  
   ctrl.init();
}

//##########################################
//############## LOOP ######################
//##########################################

void loop() 
{
 ctrl.run();
 watchdogReset();
}
