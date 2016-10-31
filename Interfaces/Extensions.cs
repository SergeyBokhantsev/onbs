using System;
using System.IO;
using System.Text;
using System.Threading;
namespace Interfaces
{
    public static class Extensions
    {
        public static bool InvertBoolSetting(this IConfig cfg, string name)
        {
            var value = !cfg.GetBool(name);
            cfg.Set(name, value);
            return value;
        }

        //public static void ExecuteIfFreeAsync(this IOperationGuard guard, Action action, Action<Exception> exceptionHandler = null)
        //{
        //    ThreadPool.QueueUserWorkItem(state =>
        //    {
        //        guard.ExecuteIfFree(action, exceptionHandler);
        //    });
        //}

        public static string GetString(this MemoryStream ms)
        {
            return Encoding.UTF8.GetString(ms.ToArray());
        }

        public static string GetString(this byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }
    }
}
