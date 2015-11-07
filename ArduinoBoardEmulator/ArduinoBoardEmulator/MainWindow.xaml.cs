using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ArduinoBoardEmulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Controller controller;
        private readonly Dispatcher disp;

        public MainWindow(Controller controller)
        {
            this.controller = controller;
            InitializeComponent();

            disp = Dispatcher.CurrentDispatcher;

            controller.Arduino.PingSignal += Arduino_PingSignal;
        }

        private void Arduino_PingSignal()
        {
            disp.Invoke(async () =>
            {
                var origColor = lPing.Background;
                lPing.Background = Brushes.Blue;
                await Task.Delay(1000);
                lPing.Background = origColor;
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;

            var name = btn.Content as string;

            var number = (int)Enum.Parse(typeof(Interfaces.Input.Buttons), name);

            controller.Arduino.SendButtonPress(number);
        }


    }
}
