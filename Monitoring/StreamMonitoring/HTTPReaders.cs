using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.HTTP;
using eExNetworkLibrary.Sockets;

namespace eExNetworkLibrary.Monitoring.StreamMonitoring
{
    class HTTPRequestReader : StreamMonitoring.NetworkStreamMonitor
    {
        public override string Description
        {
            get { return "Reads HTTP Requests"; }
        }

        public event EventHandler<HTTPReaderEventArgs> HTTPRequestCaptured;

        public HTTPRequestReader(NetworkStream nsInput) : base(nsInput)
        { }

        protected override void Run()
        {
            try
            {
                while (bSouldRun)
                {
                    InvokeExternal(HTTPRequestCaptured, new HTTPReaderEventArgs(new HTTPRequest(InputStream)));
                }
            }
            catch (HTTP.HTTPParserStreamEndedException ex)
            { }
        }
    }

    class HTTPResponseReader : StreamMonitoring.NetworkStreamMonitor
    {
        public override string Description
        {
            get { return "Reads HTTP Responses"; }
        }

        public event EventHandler<HTTPReaderEventArgs> HTTPResponseCaptured;

        public HTTPResponseReader(NetworkStream nsInput) : base(nsInput)
        { }

        protected override void Run()
        {
            try
            {
                while (bSouldRun)
                {
                    InvokeExternal(HTTPResponseCaptured, new HTTPReaderEventArgs(new HTTPResponse(InputStream)));
                }
            }
            catch (HTTP.HTTPParserStreamEndedException ex)
            { }
        }
    }

    class HTTPReaderEventArgs : EventArgs
    {
        public HTTPMessage HTTPMessage { get; private set; }

        public HTTPReaderEventArgs(HTTPMessage msg)
        {
            HTTPMessage = msg;
        }
    }
}
