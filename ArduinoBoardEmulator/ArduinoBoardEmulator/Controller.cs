using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoBoardEmulator
{
    public class Controller
    {
        private readonly Logger logger;

        internal ArduinoServer Arduino {get; private set;}

        public Controller()
        {
            logger = new Logger();

            Arduino = new ArduinoServer(logger);
        }
        
        [STAThread]
        public static void Main(string[] args)
        {
            var controller = new Controller();

            var app = new System.Windows.Application();
            app.Run(new MainWindow(controller));
        }
    }
}
