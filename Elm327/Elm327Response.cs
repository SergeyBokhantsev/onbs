using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elm327
{
    public class Elm327Response<T> : IElm327Response
    {
        public T Value { get; private set; }

        public Elm327FunctionTypes Type { get; private set; }

        public Elm327Response(Elm327FunctionTypes type, T value)
        {
            Type = type;
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class MonitorStatusResponse : IElm327Response
    {
        private readonly byte A;
        private readonly byte B;
        private readonly byte C;
        private readonly byte D;

        public Elm327FunctionTypes Type
        {
            get { return Elm327FunctionTypes.MonitorStatus; }
        }

        public bool CheckEngine
        {
            get { return (A & (1 << 7)) != 0; }
        }

        public int TroubleCodesCount
        {
            get { return (A & 127); }
        }

        internal MonitorStatusResponse(byte a, byte b, byte c, byte d)
        {
            A = a; B = b; C = c; D = d;
        }

        public override string ToString()
        {
            return string.Concat(CheckEngine ? "CHECK ENGINE: " : string.Empty, TroubleCodesCount);
        }
    }

    public class FuelSystemStatusResponse : IElm327Response
    {
        private readonly byte A;
        private readonly byte B;

        public Elm327FunctionTypes Type
        {
            get { return Elm327FunctionTypes.FuelSystemStatus; }
        }

        public string Status
        {
            get 
            {
                switch(A)
                {
                    case 1: 
                        return "Open loop due to insufficient engine temperature";
                    case 2:
                        return "Closed loop, using oxygen sensor feedback to determine fuel mix";
                    case 4:
                        return "Open loop due to engine load OR fuel cut due to deceleration";
                    case 8:
                        return "Open loop due to system failure";
                    case 16:
                        return "Closed loop, using at least one oxygen sensor but there is a fault in the feedback system";
                    default:
                        return string.Format("Invalid response: {0}", A);
                }
            }
        }

        internal FuelSystemStatusResponse(byte a, byte b)
        {
            A = a; B = b;
        }

        public override string ToString()
        {
            return Status;
        }
    }
}
