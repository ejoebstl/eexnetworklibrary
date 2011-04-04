using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using eExNetworkLibrary.Utilities;

namespace eExNetworkLibrary.HTTP
{
    /// <summary>
    /// This class provides a base implementation of HTTP messages
    /// </summary>
    public abstract class HTTPMessage
    {
        private static int iDummy;

        HTTPHeaderCollection httpHeaders;
        byte[] bPayload;

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        protected HTTPMessage()
        {
            httpHeaders = new HTTPHeaderCollection();
            bPayload = new byte[0];
        }

        /// <summary>
        /// Creates a new instance of this class by reading from the given stream. 
        /// </summary>
        /// <param name="sStream">The stream to read from</param>
        /// 
        protected HTTPMessage(System.IO.Stream sStream) : this(sStream, out iDummy) { }

        /// <summary>
        /// Creates a new instance of this class by reading from the given stream. 
        /// </summary>
        /// <param name="sStream">The stream to read from</param>
        /// <param name="iLength">An integer which is set to the length of this HTTP message in bytes</param>
        protected HTTPMessage(System.IO.Stream sStream, out int iLength)
        {
            MemoryStream msHeader = new MemoryStream();
            byte[] bMessage = null;
            HTTPHeaderCollection htHeaders = new HTTPHeaderCollection();

            int iContentLength = 0;
            int iHeaderLength = 0;

            // Get the header
            if (!CompressionHelper.CopyUntilFound(sStream, msHeader, new byte[] { (byte)'\r', (byte)'\n', (byte)'\r', (byte)'\n' }))
                throw new HTTPParserStreamEndedException("The end of the stream was reached while trying to parse the message.");

            iHeaderLength = (int)msHeader.Length;

            //Parse the header and get the content length

            byte[] bHeader = msHeader.ToArray();
            bool bIsChunked = false;

            string strHeaderFields = Encoding.ASCII.GetString(bHeader);
            string[] arstrHeaders = strHeaderFields.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            for (int iC1 = 1; iC1 < arstrHeaders.Length; iC1++)
            {
                string[] arstrHeader = arstrHeaders[iC1].Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries);

                if(arstrHeader.Length == 0)
                {
                    throw new HTTPParserException("Invalid HTTP header supplied: " + arstrHeaders[iC1]);
                }
                if (arstrHeader[0] == "Content-Length" && arstrHeader.Length >= 2)
                {
                    iContentLength = Int32.Parse(arstrHeader[1]);
                }
                if (arstrHeader[0] == "Transfer-Encoding" && arstrHeader.Length >= 2)
                {
                    bIsChunked = arstrHeader[1].ToLower() == "chunked";
                }
            }

            if (bIsChunked)
            {
                //We have chunked content encoding (We have to wait)
                if (iContentLength != 0)
                {
                    throw new HTTPParserException("The Transfer-Encoding is set to chunked, but " +
                    "also the Content-Length header is set to " + iContentLength + ". This should not happen.");
                }

                MemoryStream msChunkedContent = new MemoryStream();

                if (!CompressionHelper.CopyUntilFound(sStream, msChunkedContent, new byte[] { (byte)'\r', (byte)'\n', (byte)'0', (byte)'\r', (byte)'\n', (byte)'\r', (byte)'\n' }))
                    throw new HTTPParserStreamEndedException("The end of the stream was reached while trying to parse the message.");
                
                iContentLength = (int)msChunkedContent.Length;

                bMessage = new byte[iHeaderLength + iContentLength];
                msHeader.Position = 0;
                msHeader.Read(bMessage, 0, iHeaderLength);
                msChunkedContent.Position = 0;
                msChunkedContent.Read(bMessage, iHeaderLength, iContentLength);
            }
            else
            {

                //We have normal content encoding
                //Copy all data into an array
                bMessage = new byte[iHeaderLength + iContentLength];
                msHeader.Position = 0;
                msHeader.Read(bMessage, 0, iHeaderLength);
                if(sStream.Read(bMessage, iHeaderLength, iContentLength) != iContentLength)
                    throw new HTTPParserStreamEndedException("The end of the stream was reached while trying to parse the message.");
            }

            //Initialize

            iLength = Initialize(bMessage);

            //Cleanup

            msHeader.Dispose();
        }

        /// <summary>
        /// Creates a new instance of this class by parsing the given data
        /// </summary>
        /// <param name="bData">The data to parse</param>
        protected HTTPMessage(byte[] bData) : this(bData, out iDummy) { }

        /// <summary>
        /// Creates a new instance of this class by parsing the given data
        /// </summary>
        /// <param name="bData">The data to parse</param>
        /// <param name="iLength">An integer which is set to the length of this HTTP message in bytes</param>
        protected HTTPMessage(byte[] bData, out int iLength)
        {
            iLength = Initialize(bData);
        }

        /// <summary>
        /// Initializes this instance with the given data.
        /// </summary>
        /// <param name="bData">The data to create this HTTP message from</param>
        /// <returns>The length of the data which belongs to this HTTP message</returns>
        private int Initialize(byte[] bData)
        {
            int iLength;
            byte[] bHeader = null;
            byte[] bContent = null;
            httpHeaders = new HTTPHeaderCollection();
            bPayload = new byte[0];

            for (int iC1 = 0; iC1 < bData.Length - 3; iC1++)
            {
                if (bData[iC1] == '\r' && bData[iC1 + 1] == '\n' && bData[iC1 + 2] == '\r' && bData[iC1 + 3] == '\n')
                {
                    bHeader = new byte[iC1 + 4];
                    for (int iC2 = 0; iC2 <= iC1 + 3; iC2++)
                    {
                        bHeader[iC2] = bData[iC2];
                    }
                    break;

                    //bContent = new byte[bData.Length - iC1 - 4];
                    //for (int iC2 = 0; iC2 < bData.Length - iC1 - 4; iC2++)
                    //{
                    //    bHeader[iC2] = bData[iC2 + iC1 + 4];
                    //}
                }
            }

            if (bHeader == null)
            {
                bHeader = bData;
            }

            string strHeaderFields = Encoding.ASCII.GetString(bHeader);
            string[] arstrHeaders = strHeaderFields.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            ParseStatusLine(arstrHeaders[0]);

            for (int iC1 = 1; iC1 < arstrHeaders.Length; iC1++)
            {
                int iFirstIndexOf = arstrHeaders[iC1].IndexOf(": ");

                if(iFirstIndexOf == -1)
                {
                    throw new HTTPParserException("Invalid HTTP header supplied: " + arstrHeaders[iC1]);
                }

                string strHeader = arstrHeaders[iC1].Substring(0, iFirstIndexOf);
                string strValue = arstrHeaders[iC1].Substring(iFirstIndexOf + 2);

                httpHeaders.Add(new HTTPHeader(strHeader, strValue));
            }

            if (httpHeaders.Contains("Content-Length"))
            {
                int iContentLength = Int32.Parse(httpHeaders["Content-Length"][0].Value);

                if (iContentLength > bData.Length - bHeader.Length)
                {
                    throw new HTTPParserException("Payload length given in HTTP-header is longer then length of HTTP-conversation.");
                }

                bContent = new byte[iContentLength];

                for (int iC1 = 0; iC1 < iContentLength; iC1++)
                {
                    bContent[iC1] = bData[iC1 + bHeader.Length];
                }
            }
            else
            {
                if (httpHeaders.Contains("Transfer-Encoding") && httpHeaders["Transfer-Encoding"][0].Value.ToLower() == "chunked")
                {
                    //We've got chunked encoding
                    for (int iC1 = bHeader.Length; iC1 < bData.Length - 4; iC1++)
                    {
                        if (bData[iC1] == '\r' && bData[iC1 + 1] == '\n' && bData[iC1 + 2] == '0' && bData[iC1 + 3] == '\r' && bData[iC1 + 4] == '\n')
                        {
                            bContent = new byte[iC1 + 5 - bHeader.Length];
                            for (int iC2 = bHeader.Length; iC2 <= iC1 + 4; iC2++)
                            {
                                bContent[iC2 - bHeader.Length] = bData[iC2];
                            }
                            break;
                        }
                    }
                }
                else
                {
                    bContent = new byte[0];
                }
            }

            this.bPayload = bContent;

            iLength = bHeader.Length + bContent.Length;
            return iLength;
        }

        /// <summary>
        /// This method must be capable of parsing the HTTP status line when overloaded
        /// </summary>
        /// <param name="strFirstLine">The status line to parse</param>
        protected abstract void ParseStatusLine(string strFirstLine);


        /// <summary>
        /// This method must be capable of generating the HTTP status line when overloaded
        /// </summary>
        /// <returns>Must return the first line of the HTTP message</returns>
        protected abstract string GenerateStatusLine();

        /// <summary>
        /// Gets or sets the payload of this message
        /// </summary>
        public byte[] Payload
        {
            get { return bPayload; }
            set { bPayload = value; }
        }

        /// <summary>
        /// Gets the HTTP headers of this message
        /// </summary>
        public HTTPHeaderCollection Headers
        {
            get { return httpHeaders; }
        }

        /// <summary>
        /// Gets the type of this HTTP message
        /// </summary>
        public abstract HTTPMessageType MessageType { get; }
        
        /// <summary>
        /// Gets the byte representation of this HTTP message 
        /// </summary>
        public byte[] RawBytes
        {
            get 
            {
                byte[] bData = new byte[Length];

                //Headers 
                StringBuilder strbHeaders = new StringBuilder();

                strbHeaders.AppendLine(GenerateStatusLine());
                
                foreach (HTTPHeader hHeader in httpHeaders.AllHeaders)
                {
                    strbHeaders.Append(hHeader.Name);
                    strbHeaders.Append(": ");
                    strbHeaders.AppendLine(hHeader.Value);
                }

                strbHeaders.AppendLine();

                byte[] bHeaders = ASCIIEncoding.ASCII.GetBytes(strbHeaders.ToString());

                bHeaders.CopyTo(bData, 0);

                //Data
                bPayload.CopyTo(bData, bHeaders.Length);

                return bData;

            }
        }

        /// <summary>
        /// Gets the length of this HTTP message
        /// </summary>
        public int Length
        {
            get 
            {
                //Status line + new line
                int iLen = GenerateStatusLine().Length + 2;
                
                //Headers + new line
                foreach (HTTPHeader hHeader in httpHeaders.AllHeaders)
                {
                    iLen += hHeader.Name.Length + hHeader.Value.Length + 2 + Environment.NewLine.Length;
                }

                //End of header (new line)
                iLen += 2;

                //Payload
                iLen += bPayload.Length;

                return iLen;
            }
        }
    }

    public class HTTPParserException : Exception
    {
        public HTTPParserException(string strMessage) : base(strMessage) { }
    }

    public class HTTPParserStreamEndedException : HTTPParserException
    {
        public HTTPParserStreamEndedException(string strMessage) : base(strMessage) { }
    }

    /// <summary>
    /// Represents the HTTP message type
    /// </summary>
    public enum HTTPMessageType
    {
        /// <summary>
        /// A HTTP response
        /// </summary>
        Response = 1,
        /// <summary>
        /// A HTTP request
        /// </summary>
        Request = 0
    }
}
