using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebApp.Models
{
    public enum TravelPointTypes { AutoTrackPoint = 0, ManualTrackPoint = 1 }

    public class TravelPoint
    {
        public int ID { get; set; }
        public TravelPointTypes Type { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public DateTime Time { get; set; }
        public double Speed { get; set; }
        [MaxLength(50)]
        public string Description { get; set; }
    }
}