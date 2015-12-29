using System;
using System.Threading;
namespace Interfaces
{
    public static class Extensions
    {
        public static void InvertBoolSetting(this IConfig cfg, string name)
        {
            var value = cfg.GetBool(name);
            cfg.Set(name, !value);
        }

        public static void ExecuteIfFreeAsync(this IOperationGuard guard, Action action, Action<Exception> exceptionHandler = null)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                guard.ExecuteIfFree(action, exceptionHandler);
            });
        }
    }
}
