using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;

namespace ArduinoController
{
    //http://www.xappsoftware.com/wordpress/2013/06/24/three-ways-to-reset-an-arduino-board-by-code/

    public class ArduinoCommandProcessor
    {
        private readonly IDispatcherTimer timer;

        public ArduinoCommandProcessor(IDispatcher dispatcher)
        {
            if (dispatcher == null)
                throw new ArgumentNullException("dispatcher");

            this.timer = dispatcher.CreateTimer(1000, TimerTick);
        }

        private void TimerTick(object sender, EventArgs e)
        {

        }
    }
}
