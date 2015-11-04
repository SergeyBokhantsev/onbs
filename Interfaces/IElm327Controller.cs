﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IElm327Controller : IController
    {
        string Error { get; }
        void Reset();
        byte[] GetPIDValue(uint pid);
        Nullable<T> GetPIDValue<T>(uint pid, int expectedBytesCount, Func<byte[], T> formula) where T : struct;
        byte[] GetTroubleCodes();
    }
}
