using Interfaces.SerialTransportProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialTransportProtocol
{
    internal class STPDecoder
    {
        private readonly byte[] frameBeginMark;
        private readonly byte[] frameEndMark;
        private readonly STPFrame.Types frameType;

        private const int bufferSize = 1024 * 5;
        private readonly byte[] buffer = new byte[bufferSize];
        private int bufferLen;

        private STPFrame.Types currentFrameType;
        private ushort currentFrameId;
        private byte currentFrameChecksum;

        private enum States { LookingForBegin, LookingForType, LookingForId1, LookingForId2, LookingForChecksum, LookingForEnd };
        private States state = States.LookingForBegin;

        private int matchCount;

        public STPDecoder(byte[] frameBeginMark, byte[] frameEndMark, STPFrame.Types frameType)
        {
            this.frameType = currentFrameType = frameType;
            this.frameBeginMark = frameBeginMark;
            this.frameEndMark = frameEndMark;
        }

        public void Accept(byte[] input, int inputCount, ref List<STPFrame> resultFrames)
        {
            for (int inpI = 0; inpI < inputCount; ++inpI)
            {
                var b = input[inpI];

                switch (state)
                {
                    case States.LookingForBegin:
                        if (frameBeginMark[matchCount] == b)
                        {
                            matchCount++;
                        }
                        else if (matchCount > 0)
                        {
                            matchCount = 0;
                            goto case States.LookingForBegin;
                        }

                        if (matchCount == frameBeginMark.Length)
                        {
                            state = frameType == STPFrame.Types.Undefined ? States.LookingForType : States.LookingForEnd;
                        }
                        break;

                    case States.LookingForType:
                        currentFrameType = (STPFrame.Types)b;
                        state = States.LookingForId1;
                        break;

                    case States.LookingForId1:
                        currentFrameId = (ushort)(b << 8);
                        state = States.LookingForId2;
                        break;

                    case States.LookingForId2:
                        currentFrameId += b;
                        state = States.LookingForChecksum;
                        break;

                    case States.LookingForChecksum:
                        currentFrameChecksum = b;
                        state = States.LookingForEnd;
                        break;

                    case States.LookingForEnd:
                        buffer[bufferLen++] = b;

                        if (bufferLen >= frameEndMark.Length)
                        {
                            matchCount = 0;

                            for (int i = 0; i < frameEndMark.Length; ++i)
                            {
                                if (buffer[bufferLen - frameEndMark.Length + i] == frameEndMark[i])
                                    matchCount++;
                                else
                                    break;
                            }

                            if (matchCount == frameEndMark.Length)
                            {
                                var dataLen = bufferLen - frameEndMark.Length;
                                var data = new byte[dataLen];
                                buffer.CopyTo(data, 0, dataLen);

                                if (currentFrameChecksum == STPEncoder.CalculateChecksum(data))
                                {
                                    if (resultFrames == null)
                                        resultFrames = new List<STPFrame>();

                                    resultFrames.Add(new STPFrame(data, currentFrameType, currentFrameId));
                                }

                                bufferLen = 0;
                                state = States.LookingForBegin;
                                matchCount = 0;
                            }
                        }

                        if (bufferLen == bufferSize)
                        {
                            bufferLen = 0;
                            state = States.LookingForBegin;
                            matchCount = 0;
                        }
                        break;
                }
            }
        }
    }
}
