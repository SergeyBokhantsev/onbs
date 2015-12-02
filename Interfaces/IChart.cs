using System;
using System.Collections.Generic;
using System.Linq;

namespace Interfaces
{
    public interface IChart<out T>
    {
        string Title { get; }
        string UnitText { get; }
        T Scale { get; }
        T Max { get; }
        int Count { get; }
        T Last { get; }
        /// <summary>
        /// visits last count number of stored points
        /// </summary>
        void Visit(Action<T> visitor, int count);
    }

    public class ChartOfDouble : IChart<double>
    {
        private readonly List<double> data = new List<double>();

        public string Title
        {
            get;
            set;
        }

        public string UnitText
        {
            get;
            set;
        }

        public double Scale
        {
            get;
            set;
        }

        public double Max
        {
            get;
            private set;
        }

        public int Count
        {
            get
            {
                lock (data)
                {
                    return data.Count;
                }
            }
        }

        public double Last
        {
            get
            {
                lock (data)
                {
                    return data.Count > 0 ? data.Last() : 0d;
                }
            }
        }

        public void Visit(Action<double> visitor, int count)
        {
            lock (data)
            {
                count = Math.Min(data.Count, count);

                while (count > 0)
                {
                    visitor(data[data.Count - count--]);
                }
            }
        }

        public void Add(int? value)
        {
            Add(value ?? 0d);
        }

        public void Add(double? value)
        {
            Add(value ?? 0d);
        }

        public void Add(double value)
        {
            lock (data)
            {
                data.Add(value);

                if (value > Max)
                    Max = value;

                if (value > Scale)
                    Scale = value;
            }
        }

        public void DuplicateLast()
        {
            lock (data)
            {
                data.Add(data.Count > 0 ? data.Last() : 0);
            }
        }
    }
}
