using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class TravelPoint
    {
        public int ID { get; set; }
        [Required]
        public double Lat { get; set; }
        [Required]
        public double Lon { get; set; }
        [Required]
        public DateTime Time { get; set; }
        public double Speed { get; set; }
        [MaxLength(50)]
        public string Description { get; set; }
    }
}