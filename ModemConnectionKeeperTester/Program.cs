using ModemConnectionKeeper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModemConnectionKeeperTester
{
    class Program
    {
        static void Main(string[] args)
        {
            var pinger = new Pinger("google.com", 0, 0);

            pinger.State += (name, line) => Console.WriteLine(name + " | " + line);

            pinger.Start();

            Thread.Sleep(-1);
        }
    }
}
