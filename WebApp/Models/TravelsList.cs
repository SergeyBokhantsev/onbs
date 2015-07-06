using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class TravelsList
    {
        public IEnumerable<LinkItem> Today { get; set; }
        public IEnumerable<LinkItem> Yesterday { get; set; }
        public TravelsList()
        {
        }
    }
}