using System.Threading;

namespace HostController
{
    public static class EntryPoint
    {
        public static void Main (string[] args)
        {
			Thread.CurrentThread.Name = "Main";

            new HostController().Run();
        }
    }
}
