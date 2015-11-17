using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class LogEntry
    {
        public int ID { get; set; }
        public DateTime Date { get; set; }
        public string Vehicle { get; set; }
        public string Message { get; set; }
    }
}