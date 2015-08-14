using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elm327
{
    public interface IElm327Response
    {
        Elm327FunctionTypes Type { get; }
    }

    public class Elm327Response<T> : IElm327Response
    {
        public T Value { get; private set; }

        public Elm327FunctionTypes Type { get; private set; }

        public Elm327Response(Elm327FunctionTypes type, T value)
        {
            Type = type;
            Value = value;
        }
    }
}
