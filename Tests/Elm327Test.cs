using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Elm327;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class Elm327Test
    {
        private const string ElmPortName = "COM19";

        [TestMethod]
        public void Elm327Requests()
        {
            //INIT
            List<IElm327Response> responses = new List<IElm327Response>();
            Action<IElm327Response> responseHandler = resp => { if (resp.Type == Elm327FunctionTypes.EngineRPM) responses.Add(resp); };

            var elm = new Elm327Client(ElmPortName, new Mocks.Logger());
            elm.ResponceReseived += responseHandler;

            //ACT
            elm.Request(Elm327FunctionTypes.EngineRPM);

            Thread.Sleep(2000);

            //ASSERT
            Assert.AreEqual(1, responses.Count);
        }

        [TestMethod]
        public void Elm327Modes01()
        {
            //INIT
            List<IElm327Response> responses = new List<IElm327Response>();
            Action<IElm327Response> responseHandler = resp => responses.Add(resp);

            var elm = new Elm327Client(ElmPortName, new Mocks.Logger());
            elm.ResponceReseived += responseHandler;

            //ACT
            elm.Request(Elm327FunctionTypes.MonitorStatus);
            Thread.Sleep(100);
            elm.Request(Elm327FunctionTypes.FuelSystemStatus);
            Thread.Sleep(100);
            elm.Request(Elm327FunctionTypes.EngineRPM);
            Thread.Sleep(100);
            elm.Request(Elm327FunctionTypes.Speed);
            Thread.Sleep(100);

            Thread.Sleep(2000);

            //ASSERT
            Assert.IsTrue(responses.Count > 0);
        }
    }
}
