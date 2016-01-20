using Interfaces;
using Interfaces.SerialTransportProtocol;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SerialTransportProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    internal class STPTestPort : IPort
    {
        public event System.IO.Ports.SerialDataReceivedEventHandler DataReceived;

        private byte[] data;

        public long OverallReadedBytes
        {
            get { throw new NotImplementedException(); }
        }

        public STPTestPort(byte[] data)
        {
            this.data = data;
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            if (data == null)
                return 0;

            if (count < data.Length)
                throw new NotSupportedException();

            Array.Copy(data, 0, buffer, offset, data.Length);

            var result = data.Length;

            data = null;

            return result;
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }

    [TestClass]
    public class STPTests
    {
        [TestMethod]
        public void SerializeDeserializeTest()
        {
            //INIT
            var initialData = Guid.NewGuid().ToByteArray();
            var frame = new STPFrame(initialData, STPFrame.Types.ArduCommand);
            var fbm = new byte[] { 1,2,3 };
            var fem = new byte[] { 4,5,6 };

            //ACT
            var codec = new STPCodec(fbm, fem);
            var serializedFrame = codec.Encode(frame);
            var deserializedFrames = codec.Decode(new STPTestPort(serializedFrame));

            //ASSERT
            Assert.IsNotNull(deserializedFrames);
            Assert.AreEqual(1, deserializedFrames.Count);
            Assert.AreEqual(STPFrame.Types.ArduCommand, deserializedFrames.First().Type);
            Assert.IsTrue(initialData.SequenceEqual(deserializedFrames.First().Data));
            Assert.AreEqual(frame.Id, deserializedFrames.First().Id);
        }

        [TestMethod]
        public void STPFrameIdsCoDec()
        {
            var initialData = Guid.NewGuid().ToByteArray();
            var frame0 = new STPFrame(initialData, STPFrame.Types.ArduCommand, ushort.MaxValue);
            var frame1 = new STPFrame(initialData, STPFrame.Types.ArduCommand, ushort.MinValue);
            var frame2 = new STPFrame(initialData, STPFrame.Types.ArduCommand, ushort.MaxValue / 2);
            var fbm = new byte[] { 1, 2, 3 };
            var fem = new byte[] { 4, 5, 6 };

            //ACT
            var codec = new STPCodec(fbm, fem);
            var serializedData = new List<byte>();
            serializedData.AddRange(codec.Encode(frame0));
            serializedData.AddRange(codec.Encode(frame1));
            serializedData.AddRange(codec.Encode(frame2));
            var deserializedFrames = codec.Decode(new STPTestPort(serializedData.ToArray()));

            //ASSERT
            Assert.IsNotNull(deserializedFrames);
            Assert.AreEqual(3, deserializedFrames.Count);
            Assert.IsTrue(deserializedFrames.All(f => f.Type == STPFrame.Types.ArduCommand));
            Assert.IsTrue(deserializedFrames.All(f => initialData.SequenceEqual(f.Data)));
            
            Assert.AreEqual(frame0.Id, deserializedFrames[0].Id);
            Assert.AreEqual(frame1.Id, deserializedFrames[1].Id);
            Assert.AreEqual(frame2.Id, deserializedFrames[2].Id);
        }
    }
}
