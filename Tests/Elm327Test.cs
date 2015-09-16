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

        //[TestMethod]
        //public void TestElmClient()
        //{
        //    //INIT
        //    var client = new Elm327.Lanos(ElmPortName, new Mocks.Logger());

        //    var reset = client.Reset();

        //    while(true)
        //    {
        //        var speed = client.GetSpeed();

        //        System.Diagnostics.Debug.Print(DateTime.Now.Millisecond.ToString() + " - " + (speed.HasValue ? speed.Value.ToString() : "--"));
                

        //        Thread.Sleep(50);
        //    }
        //}

        [TestMethod]
        public void TestGetSupportedPids()
        {
            var result = new List<byte>();

            uint group = 0;

            while (true)
            {
                var pidFourBytes = new byte[] { 0xBE, 0x1F, 0xA8, 0x13 };

                for (int b = 0; b < 4; ++b)
                {
                    byte mask = 128;

                    for (int bit = 0; bit < 8; ++bit)
                    {
                        if ((mask & pidFourBytes[b]) == mask)
                        {
                            result.Add((byte)(group + (b*8) + bit + 1));
                        }

                        mask = (byte)(mask >> 1);
                    }
                }

                group += 0x20;

                if (result.Last() != (byte)(group))
                    break;
            }

        }

        //[TestMethod]
        //public void Elm327Requests()
        //{
        //    //INIT
        //    List<IElm327Response> responses = new List<IElm327Response>();
        //    Action<IElm327Response> responseHandler = resp => { if (resp.Type == Elm327FunctionTypes.EngineRPM) responses.Add(resp); };

        //    var elm = new Elm327Client(ElmPortName, new Mocks.Logger());
        //    elm.ResponceReseived += responseHandler;

        //    //ACT
        //    elm.Request(Elm327FunctionTypes.EngineRPM);

        //    Thread.Sleep(2000);

        //    //ASSERT
        //    Assert.AreEqual(1, responses.Count);
        //}

        //[TestMethod]
        //public void Elm327Modes01()
        //{
        //    //INIT
        //    List<IElm327Response> responses = new List<IElm327Response>();
        //    Action<IElm327Response> responseHandler = resp => responses.Add(resp);

        //    var elm = new Elm327Client(ElmPortName, new Mocks.Logger());
        //    elm.ResponceReseived += responseHandler;

        //    //ACT
        //    elm.Request(Elm327FunctionTypes.MonitorStatus);
        //    Thread.Sleep(100);
        //    elm.Request(Elm327FunctionTypes.FuelSystemStatus);
        //    Thread.Sleep(100);
        //    elm.Request(Elm327FunctionTypes.EngineRPM);
        //    Thread.Sleep(100);
        //    elm.Request(Elm327FunctionTypes.Speed);
        //    Thread.Sleep(100);

        //    Thread.Sleep(2000);

        //    //ASSERT
        //    Assert.IsTrue(responses.Count > 0);
        //}
    }
}
