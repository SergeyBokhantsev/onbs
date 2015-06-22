using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavitConfigGenerator
{
    public static class ConfigGenerator
    {
       // private const string 

        public bool Generate(string templateFileName, string outFileName, NavitConfiguration config)
        {
            if (!File.Exists(templateFileName))
                throw new Exception(string.Format("Navit Template file is not exists: '{0}'", templateFileName));

            if (config == null)
                throw new Exception("NavitConfiguration not provided");
        }
    }
}
