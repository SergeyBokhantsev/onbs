using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBD
{
    public class DTCDescriptor
    {
        private readonly Dictionary<string, string> descriptions = new Dictionary<string,string>();

        /// <summary>
        /// New descriptor
        /// </summary>
        /// <param name="dtcFiles">Description CSV files (first is prior)</param>
        public DTCDescriptor(string[] dtcFiles)
        {
            LoadDescriptors(dtcFiles);
        }

        private void LoadDescriptors(string[] dtcFiles)
        {
            foreach (var file in dtcFiles)
            {
                if (File.Exists(file))
                {
                    var lines = File.ReadAllLines(file);
                    foreach(var line in lines)
                    {
                        var items = line.Split(new char[] { (char)9 }, StringSplitOptions.RemoveEmptyEntries);

                        if (items.Length == 2 && !string.IsNullOrWhiteSpace(items[0]) && !string.IsNullOrWhiteSpace(items[1]) && !descriptions.ContainsKey(items[0]))
                        {
                            descriptions.Add(items[0], items[1]);
                        }
                    }
                }
                else
                {
                    throw new Exception(string.Format("DTC description file {0} is not exist", file));
                }
            }
        }

        public string GetDescription(string code)
        {
            if (descriptions.ContainsKey(code))
                return descriptions[code];
            else
                return null;
        }
    }
}
