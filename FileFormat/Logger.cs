using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FileFormat
{

    public class Logger
    {
        public string FileName { get; set; }


        public Logger(string fileName)
        {
            this.FileName = fileName;
        }


        public void WriteLine(string line)
        {
            using (StreamWriter writer = new StreamWriter(FileName, true))
            {
                writer.WriteLine(line);
                writer.Close();
            }
        }

    }

     
}
