using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogLib
{
    public abstract class FileLogWriter
    {
        private string filePath;
        private ConcurrentQueue<byte[]> buffer = new ConcurrentQueue<byte[]>();
        private readonly int autoflushSize;
        private readonly object locker = new object();

        protected FileLogWriter(int autoflushSize)
        {
            this.autoflushSize = autoflushSize;
        }

        protected abstract string CreateFilePath(int index);

        private bool ObtainFileName()
        {
            for (int i=0; i<100000; ++i)
            {
                filePath = CreateFilePath(i);

                if (!File.Exists(filePath))
                    break;
            }

            try
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch
            {
                throw new Exception("Unable to obtain log file name.");
            }

            return true;
        }

        protected void Add(string content)
        {
            var data = Encoding.Default.GetBytes(content);
            buffer.Enqueue(data);

            if (buffer.Count > autoflushSize && !Monitor.IsEntered(locker))
            {
                Save();
            }
        }

        protected void Save()
        {
            if (!buffer.Any())
                return;

            try
            {
                lock (locker)
                {
                    if (filePath == null)
                        ObtainFileName();

                    using (var stream = File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.Read))
                    {
                        while (buffer.Any())
                        {
                            byte[] data;

                            if (buffer.TryDequeue(out data))
                            {
                                stream.Write(data, 0, data.Length);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                buffer.Enqueue(Encoding.Default.GetBytes(string.Concat(Environment.NewLine, "ERROR WRITING LOG FILE:", Environment.NewLine, ex.Message, Environment.NewLine)));
            }
        }
    }
}
