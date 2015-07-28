using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Interfaces
{
    public class InterlockedScope
    {
        private int busy;

        public bool ExecuteIfFree(Action action, Action<Exception> exceptionHandler = null)
        {
            if (Interlocked.Exchange(ref busy, 1) == 0)
            {
                try
                {
                    action();
                }
                catch(Exception ex)
                {
                    if (exceptionHandler != null)
                        exceptionHandler(ex);
                }
                finally
                {
                    Interlocked.Exchange(ref busy, 0);
                }

                return true;
            }
            else
                return false;
        }
    }
}
