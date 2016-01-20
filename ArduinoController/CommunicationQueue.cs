using Interfaces.SerialTransportProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArduinoController
{
    internal class CommunicationQueue : IDisposable
    {
        private const int confirmationTimeout = 200; //ms
        private const int retryCount = 3;

        private readonly Queue<STPFrame> frames = new Queue<STPFrame>();
        private readonly ManualResetEventSlim queueSync = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim confirmationSync = new ManualResetEventSlim(false);

        private STPFrame currentFrame;
        private readonly object currentFrameLocker = new object();

        private bool disposed;

        public event Action<STPFrame> SendFrame;

        public CommunicationQueue()
        {
            var t = new Thread(WorkLoop);
            t.Name = "communication queue";
            t.Priority = ThreadPriority.AboveNormal;
            t.IsBackground = true;
            t.Start();
        }

        public void Enqueue(STPFrame frame)
        {
            if (!disposed)
            {
                lock (frames)
                {
                    frames.Enqueue(frame);
                }
                queueSync.Set();
            }
        }

        public void ConfirmFrame(ushort frameId)
        {
            lock (currentFrameLocker)
            {
                if (currentFrame != null)
                {
                    if (currentFrame.Id == frameId)
                    {
                        currentFrame = null;
                        confirmationSync.Set();
                    }
                }
            }
        }

        private void WorkLoop()
        {
            while (!disposed)
            {
                queueSync.Wait();

                bool sended = false;

                lock (currentFrameLocker)
                {
                    if (currentFrame == null)
                    {
                        sended = Send();
                    }
                }

                if (sended)
                    WaitConfirmation();
            }
        }

        private void WaitConfirmation()
        {
            int retry = 0;

            while (!disposed && retry++ < retryCount)
            {
                confirmationSync.Wait(200);

                lock (currentFrameLocker)
                {
                    if (currentFrame == null)
                    {
                        return;
                    }
                    else
                    {
                        OnSend(currentFrame);
                    }
                }
            }

            lock (currentFrameLocker)
            {
                currentFrame = null;
            }
        }

        private bool Send()
        {
            lock (frames)
            {
                if (frames.Any())
                {
                    lock (currentFrameLocker)
                    {
                        if (currentFrame != null)
                            throw new InvalidOperationException("currentFrame is not null!");

                        currentFrame = frames.Dequeue();
                        OnSend(currentFrame);
                        confirmationSync.Reset();

                        if (!frames.Any())
                            queueSync.Reset();

                        return true;
                    }
                }
                else
                {
                    queueSync.Reset();
                }
            }

            return false;
        }

        private void OnSend(STPFrame frame)
        {
            var handler = SendFrame;
            if (!disposed && handler != null)
                handler(frame);
        }

        public void Dispose()
        {
            disposed = true;
        }
    }
}
