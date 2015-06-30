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
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public virtual ICollection<TravelPoint> Track { get; set; }

        public Travel()
        {
        }

        public Travel(string name)
        {
            this.Name = name;
            this.StartTime = this.EndTime = DateTime.Now;
        }
    }
}