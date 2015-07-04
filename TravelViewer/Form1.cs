using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebApp.Models;

namespace TravelViewer
{
    public partial class Form1 : Form
    {
        private class TravelInfoDecorator
        {
            public Tuple<int, string> Info;

            public override string ToString()
            {
                return Info.Item2;
            }
        }

        private Client client;
        private readonly Timer timerMapUpdater = new Timer();

        private Travel travel;

        public Form1()
        {
            InitializeComponent();
            webBrowser1.ScriptErrorsSuppressed = true;
            timerMapUpdater.Interval = 30000;
            timerMapUpdater.Enabled = false;
            timerMapUpdater.Tick += timerMapUpdater_Tick;

            client = new Client(Properties.Settings.Default.DBConnectionString);

            this.Load += Form1_Load;
        }

        private void timerMapUpdater_Tick(object sender, EventArgs e)
        {
            UpdateTravel(true);
        }

        async private void UpdateTravel(bool refreshClient)
        {
            if (refreshClient)
            {
                if (client != null)
                    client.Dispose();

                client = new Client(Properties.Settings.Default.DBConnectionString);
            }

            if (travel != null)
            {
                travel = await client.GetTravel(travel.ID);

                label_travel_name.Text = travel.Name;
                label_travel_vehicle.Text = travel.Vehicle;
                label_travel_time.Text = string.Format("{0}{1}{2}", travel.StartTime, Environment.NewLine, travel.EndTime);
                label_travel_points_count.Text = travel.Track.Count.ToString();

                var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data", "map.html");
                webBrowser1.DocumentText = GoogleMapGenerator.CreateHtml(path, travel.Track);
            }
            else
            {
                webBrowser1.DocumentText = null;
            }
        }

        async private void Form1_Load(object sender, EventArgs e)
        {
            button_refresh_travels.Enabled = false;
            
            var travels = await client.GetTravelInfos();

            listBox_travels.Items.Clear();

            foreach (var travel in travels)
                listBox_travels.Items.Add(new TravelInfoDecorator { Info = travel });

            button_refresh_travels.Enabled = true;
        }

        async private void listBox_travels_SelectedValueChanged(object sender, EventArgs e)
        {
            listBox_travels.Enabled = false;

            var travelInfo = listBox_travels.SelectedItem as TravelInfoDecorator;

            if (travelInfo != null)
            {
                travel = await client.GetTravel(travelInfo.Info.Item1);
                UpdateTravel(false);
                timerMapUpdater.Enabled = true;
            }
            else
            {
                timerMapUpdater.Enabled = false;
            }

            listBox_travels.Enabled = true;
        }
    }
}
