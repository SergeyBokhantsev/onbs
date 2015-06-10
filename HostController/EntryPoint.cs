using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Interfaces.UI;
using System.Configuration;

namespace HostController
{
    public static class EntryPoint
    {
        public static void Main (string[] args)
        {
            new HostController().Run();
        }
    }
}
