// This source file is part of the eEx Network Library Management Layer (NLML)
//
// Author: 	    Emanuel Jöbstl <emi@eex-dev.net>
// Weblink: 	http://network.eex-dev.net
//		        http://eex.codeplex.com
//
// Licensed under the GNU Library General Public License (LGPL) 
//
// (c) eex-dev 2007-2011

using System;
using System.Collections.Generic;
using System.Text;

namespace eExNLML.Repository
{
    public class PlugInFile
    {
        public PlugInFile(string strFileName, byte[] bFileContents, string strMimeType)
        {
            this.FileName = strFileName;
            this.FileContents = bFileContents;
            this.MimeType = strMimeType;
        }

        public string FileName { get; protected set; }
        public byte[] FileContents { get; protected set; }
        public string MimeType { get; protected set; }
    }
}
