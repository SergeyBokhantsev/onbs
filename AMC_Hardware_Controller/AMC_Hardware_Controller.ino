
//To enable watchdog - comment WDT_Disable() call within the following file:
//C:\Users\Mau\AppData\Roaming\Arduino15\packages\arduino\hardware\sam\1.6.4\variants\arduino_due_x

#include <AMCController.h>

#include <DueTimer.h>

#define comm_port Serial

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

#define __WDP_MS 2048 // edit this number accordingly

//##########################################
//############## SETUP #####################
//##########################################

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
   
  WDT_Enable(WDT, 0x2000 | __WDP_MS | ( __WDP_MS << 16 ));
}

//##########################################
//############## LOOP ######################
//##########################################

void loop() 
{
 ctrl.run();
}
