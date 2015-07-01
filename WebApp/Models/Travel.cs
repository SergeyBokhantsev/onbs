using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class Travel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Vehicle { get; set; }
        public bool Closed { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public virtual ICollection<TravelPoint> Track { get; set; }

        public Travel()
        {
        }

        public Travel(string name, string vehicle)
        {
            this.Name = name;
            this.Vehicle = vehicle;
            this.StartTime = this.EndTime = DateTime.Now;
        }
    }
}