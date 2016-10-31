using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StdInOutTester
{
    class Program
    {
        static void Main(string[] args)
        {
            while(true)
            {
                var b = Console.Read();

                if (b >= 0)
                {
                    Console.Write((char)b);
                }

                Thread.Sleep(10);
            }
        }
    }
}
