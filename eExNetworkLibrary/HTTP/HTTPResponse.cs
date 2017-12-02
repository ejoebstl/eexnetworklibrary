// This source file is part of the eEx Network Library
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
using System.Web;

namespace eExNetworkLibrary.HTTP
{
    /// <summary>
    /// This class represents a HTTP response
    /// </summary>
    public class HTTPResponse : HTTPMessage
    {
        string strReason;
        string strVersion;
        int iCode;

        /// <summary>
        /// Gets or sets the reponse code
        /// </summary>
        public int ResponseCode
        {
            get { return iCode; }
            set { iCode = value; }
        }

        /// <summary>
        /// Gets or sets the response reason. In most cases, this is a string indicating why an error happened
        /// </summary>
        public string ResponseReason
        {
            get { return strReason; }
            set { strReason = value; }
        }

        /// <summary>
        /// Gets or sets the HTTP version
        /// </summary>
        public string Version
        {
            get { return strVersion; }
            set { strVersion = value; }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public HTTPResponse()
        {
            iCode = 414;
            strVersion = "HTTP/1.1";
            strReason = "Client request too long";
        }

        /// <summary>
        /// Creates a new instance of this class by parsing the given data
        /// </summary>
        /// <param name="bData">The data to parse</param>
        public HTTPResponse(byte[] bData) : base(bData) { }

        /// <summary>
        /// Creates a new instance of this class by parsing the given data
        /// </summary>
        /// <param name="bData">The data to parse</param>
        /// <param name="iLength">An integer which is set to the length of this HTTP message in bytes</param>
        public HTTPResponse(byte[] bData, out int iLength) : base(bData, out iLength) { }

        /// <summary>
        /// Creates a new instance of this class by parsing the given data
        /// </summary>
        /// <param name="sStream">The stream to read from</param>
        public HTTPResponse(System.IO.Stream sStream) : base(sStream) { }

        /// <summary>
        /// Creates a new instance of this class by parsing the given data
        /// </summary>
        /// <param name="sStream">The stream to read from</param>
        /// <param name="iLength">An integer which is set to the length of this HTTP message in bytes</param>
        public HTTPResponse(System.IO.Stream sStream, out int iLength) : base(sStream, out iLength) { }
       
        /// <summary>
        /// Parses an HTTP response's status line
        /// </summary>
        /// <param name="strFirstLine">The string to parse</param>
        protected override void ParseStatusLine(string strFirstLine)
        {
            string[] arstrFirstLine = strFirstLine.Split(' ');

            if (arstrFirstLine.Length < 3)
            {
                throw new ArgumentException("Invalid HTTP header supplied: " + strFirstLine);
            }

            strVersion = arstrFirstLine[0];
            iCode = Int32.Parse(arstrFirstLine[1]);
            StringBuilder sb = new StringBuilder();

            for (int iC1 = 2; iC1 < arstrFirstLine.Length; iC1++)
            {
                if (iC1 != arstrFirstLine.Length - 1)
                {
                    sb.Append(arstrFirstLine[iC1] + " ");
                }
                else
                {
                    sb.Append(arstrFirstLine[iC1]);
                }
            }

            strReason = sb.ToString();
        }  
        
        /// <summary>
        /// Generates this HTTP response's status line.
        /// </summary>
        /// <returns>The generated status line.</returns>
        protected override string GenerateStatusLine()
        {
            return strVersion + " " + iCode + " " + strReason;
        }

        /// <summary>
        /// Returns HTTPMessageType.Response
        /// </summary>
        public override HTTPMessageType MessageType
        {
            get { return HTTPMessageType.Response; }
        }
    }
}
