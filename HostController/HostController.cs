using Interfaces;
using System;
using System.Threading.Tasks;

namespace HostController
{
	public class HostController : IHostController
	{
		public HostController()
		{
		}

        public void foo()
        {
            internal_foo();
        }

        private async void internal_foo()
        {
            await Task.Delay(1000);
        }
    }
}

