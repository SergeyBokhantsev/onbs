using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Interfaces.UI;

namespace HostController
{
    public static class EntryPoint
    {
        public static void Main (string[] args)
        {
            var ass = Assembly.LoadFrom("GtkApplication.dll");
            var appType = ass.GetType("GtkApplication.App");
            var appConstructor = appType.GetConstructor(new Type[] { typeof(IHostController) });

            var hostController = new HostController();

            var ui = appConstructor.Invoke(new object[] { hostController });
        }
    }
}
