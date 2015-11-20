using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Interfaces;
using System.Linq;
using System.Collections.Generic;
using Elm327;
using OBD;

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

            public IEnumerable<string> GetTroubleCodeFrames()
            {
                var elmRaw = new string[] { "43 16 12 01 05 15 30 ",
                       "43 04 80 03 04 03 01 ",
                       "43 03 02 03 03 05 05 ",
                       "",
                       ">"};

                return BitHelper.AllHexStrings(elmRaw).Select(l => l.Trim().Replace(" ", string.Empty));

            }
        }

        [TestMethod]
        [DeploymentItem("Data", "Data")]
        public void TestMethod1()
        {
            var processor = new OBD.OBDProcessor(new MockElm327());

            var tc = processor.GetTroubleCodes().ToArray();

            var descriptor = new DTCDescriptor(new string[] { "Data\\mikas103.txt", "Data\\mikas120.txt", "Data\\mikas120.txt" });

            var descriptions = tc.Select(c => string.Format("{0}: {1}", c, descriptor.GetDescription(c))).ToArray();
        }
    }
}
