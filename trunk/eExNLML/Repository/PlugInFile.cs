using System;
using System.Collections.Generic;
using System.Text;

namespace eExNLML.Repository
{
    public class PlugInFile
    {
        public PlugInFile(string strFileName, string strFileContents)
        {
            this.FileName = strFileName;
            this.FileContents = strFileContents;
        }

        public string FileName { get; protected set; }
        public string FileContents { get; protected set; }
    }
}
