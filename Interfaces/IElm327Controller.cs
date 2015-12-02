using System;
using System.Collections.Generic;

namespace Interfaces
{
    public interface IElm327Controller : IController
    {
        string Error { get; }
        void Reset();
        byte[] GetPIDValue(uint pid);
        T? GetPIDValue<T>(uint pid, int expectedBytesCount, Func<byte[], T> formula) where T : struct;
        IEnumerable<string> GetTroubleCodeFrames();
        bool ResetTroubleCodes();
    }
}
