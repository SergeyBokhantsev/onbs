using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogLib
{
    public abstract class FileLogWriter
    {
        private string filePath;
        private List<byte[]> buffer = new List<byte[]>();
        private readonly int autoflushSize;

        protected FileLogWriter(int autoflushSize)
        {
            this.autoflushSize = autoflushSize;
            ObtainFileName();
        }

        protected abstract string CreateFilePath(int index);

        private bool ObtainFileName()
        {
            for (int i=0; i<1000; ++i)
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
            buffer.Add(data);

            if (buffer.Count > autoflushSize)
            {
                Save();
            }
        }

        protected void Save()
        {
            var dataCount = buffer.Count;

            if (dataCount == 0)
                return;

            try
            {
                using (var stream = File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.Read))
                {
                    for (int i = 0; i < dataCount; ++i)
                    {
                        var data = buffer[0];
                        stream.Write(data, 0, data.Length);
                        buffer.RemoveAt(0);
                    }
                }
            }
            catch (Exception ex)
            {
                buffer.Add(Encoding.Default.GetBytes(string.Concat(Environment.NewLine, "ERROR WRITING LOG FILE:", Environment.NewLine, ex.Message, Environment.NewLine)));
            }
        }
    }
}
