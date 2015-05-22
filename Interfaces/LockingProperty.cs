using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public class LockingProperty<T>
    {
        private readonly object locker = new object();

        private T value;

        public T Value
        {
            get
            {
                lock(locker)
                {
                    return value;
                }
            }
            set
            {
                lock(locker)
                {
                    this.value = value;
                }
            }
        }
    }
}
