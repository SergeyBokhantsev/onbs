﻿namespace Interfaces
{
    public class LockingProperty<T>
    {
        private readonly object locker = new object();

        private T value;

        public LockingProperty()
        {
        }

        public LockingProperty(T initialValue)
        {
            Value = initialValue;
        }

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
