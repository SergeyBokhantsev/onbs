using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class LinkItem
    {
        public string Caption { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public object Args { get; set; }
    }

    public class LinkItemsBag
    {
        public List<LinkItem> Items { get; set; } 
    }
}