using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Interfaces;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class OBDProcessorTest
    {
        private class MockElm327 : IElm327Controller
        {
            public string Error
            {
                get { throw new NotImplementedException(); }
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }

            public byte[] GetPIDValue(uint pid)
            {
                throw new NotImplementedException();
            }

            public T? GetPIDValue<T>(uint pid, int expectedBytesCount, Func<byte[], T> formula) where T : struct
            {
                throw new NotImplementedException();
            }

            public string GetTroubleCodes()
            {
                return "0133D0160000"; 
            }

            private byte[] HexToBytes(string str)
            {
                str = str.Replace(" ", string.Empty);

                var ret = new byte[str.Length / 2];

                for (int i = 0; i < str.Length; i += 2)
                {
                    ret[i / 2] = Convert.ToByte(str.Substring(i, 2), 16);
                }

                return ret;
            }
        }

        [TestMethod]
        public void TestMethod1()
        {
            var processor = new OBD.OBDProcessor(new MockElm327());

            var tc = processor.GetTroubleCodes().ToArray();
        }
    }
}
