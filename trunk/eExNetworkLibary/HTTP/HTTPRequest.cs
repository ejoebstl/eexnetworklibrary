using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace eExNetworkLibrary.HTTP
{
    /// <summary>
    /// This class represents an HTTP request
    /// </summary>
    public class HTTPRequest : HTTPMessage
    {
        string strTarget;
        string strVersion;
        HTTPMethod httpMethod;
        HTTPResponse rResponse;

        /// <summary>
        /// Gets or sets the HTTP method
        /// </summary>
        public HTTPMethod Method
        {
            get { return httpMethod; }
            set { httpMethod = value; }
        }

        /// <summary>
        /// Gets or sets the HTTP response associated with this HTTP request
        /// </summary>
        public HTTPResponse Response
        {
            get { return rResponse; }
            set { rResponse = value; }
        }

        /// <summary>
        /// Gets or sets the request's targt
        /// </summary>
        public string Target
        {
            get { return strTarget; }
            set { strTarget = value; }
        }

        /// <summary>
        /// Gets or sets the request's version
        /// </summary>
        public string Version
        {
            get { return strVersion; }
            set { strVersion = value; }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public HTTPRequest()
        {
            strTarget = "/";
            strVersion = "HTTP/1.1";
            httpMethod = HTTPMethod.Get;
        }

        /// <summary>
        /// Creates a new instance of this class by parsing the given data
        /// </summary>
        /// <param name="bData">The data to parse</param>
        public HTTPRequest(byte[] bData) : base(bData) { }

        /// <summary>
        /// Creates a new instance of this class by parsing the given data
        /// </summary>
        /// <param name="bData">The data to parse</param>
        /// <param name="iLength">An integer which is set to the length of this HTTP message in bytes</param>
        public HTTPRequest(byte[] bData, out int iLength) : base(bData, out iLength) { }

        /// <summary>
        /// Creates a new instance of this class by parsing the given data
        /// </summary>
        /// <param name="sStream">The stream to read from</param>
        public HTTPRequest(System.IO.Stream sStream) : base(sStream) { }

        /// <summary>
        /// Creates a new instance of this class by parsing the given data
        /// </summary>
        /// <param name="sStream">The stream to read from</param>
        /// <param name="iLength">An integer which is set to the length of this HTTP message in bytes</param>
        public HTTPRequest(System.IO.Stream sStream, out int iLength) : base(sStream, out iLength) { }

        /// <summary>
        /// Parses an HTTP request's status line
        /// </summary>
        /// <param name="strFirstLine">The string to parse</param>
        protected override void ParseStatusLine(string strFirstLine)
        {
            string[] arstrFirstLine = strFirstLine.Split(' ');

            if (arstrFirstLine.Length < 3)
            {
                throw new ArgumentException("Invalid HTTP header supplied: " + strFirstLine);
            }

            //Parse Method
            switch (arstrFirstLine[0].ToUpper())
            {
                case "GET": this.httpMethod = HTTPMethod.Get;
                    break;
                case "POST": this.httpMethod = HTTPMethod.Post;
                    break;
                case "PUT": this.httpMethod = HTTPMethod.Put;
                    break;
                case "TRACE": this.httpMethod = HTTPMethod.Trace;
                    break;
                case "DELETE": this.httpMethod = HTTPMethod.Delete;
                    break;
                case "UNLINK": this.httpMethod = HTTPMethod.Unlink;
                    break;
                case "HEAD": this.httpMethod = HTTPMethod.Head;
                    break;
                case "LINK": this.httpMethod = HTTPMethod.Link;
                    break;
                default: throw new ArgumentException("Invaild HTTP-method: " + arstrFirstLine[0]);
            }

            //Set Target
            strTarget = arstrFirstLine[1];

            //Set Version
            strVersion = arstrFirstLine[2];
        }

        /// <summary>
        /// Returns HTTPMessageType.Request
        /// </summary>
        public override HTTPMessageType MessageType
        {
            get { return HTTPMessageType.Request; }
        }

        /// <summary>
        /// Generates this HTTP request's status line.
        /// </summary>
        /// <returns>The generated status line.</returns>
        protected override string GenerateStatusLine()
        {
            string strMethod = "";

            switch (httpMethod)
            {
                case HTTPMethod.Get: strMethod = "GET";
                    break;
                case HTTPMethod.Post: strMethod = "POST";
                    break;
                case HTTPMethod.Put: strMethod = "PUT";
                    break;
                case HTTPMethod.Trace: strMethod = "TRACE";
                    break;
                case HTTPMethod.Delete: strMethod = "DELETE";
                    break;
                case HTTPMethod.Unlink: strMethod = "UNLINK";
                    break;
                case HTTPMethod.Head: strMethod = "HEAD";
                    break;
                case HTTPMethod.Link: strMethod = "LINK";
                    break;
                default: throw new InvalidOperationException("Unknown HTTP method set. This should never happen.");
            }

            return strMethod + " " + strTarget + " " + strVersion;
        }
    }

    /// <summary>
    /// An enumeration representing various HTTP methods
    /// </summary>
    public enum HTTPMethod
    {
        /// <summary>
        /// The GET method
        /// </summary>
        Get = 1,
        /// <summary>
        /// The HEAD method
        /// </summary>
        Head = 2,
        /// <summary>
        /// The POST method
        /// </summary>
        Post = 3,
        /// <summary>
        /// The PUT method
        /// </summary>
        Put = 4,
        /// <summary>
        /// The DELETE method
        /// </summary>
        Delete = 5,
        /// <summary>
        /// The LINK method
        /// </summary>
        Link = 6,
        /// <summary>
        /// The UNLINK method
        /// </summary>
        Unlink = 7,
        /// <summary>
        /// The TRACE method
        /// </summary>
        Trace = 8
    }
}
