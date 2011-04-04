using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.CommonTrafficAnalysis;
using System.Net;
using System.IO;
using eExNetworkLibrary.HTTP;
using eExNetworkLibrary.Monitoring.StreamMonitoring;

namespace eExNetworkLibrary.Monitoring
{
    /// <summary>
    /// This traffic analyzer provides the capability to intercept and isolate HTTP traffic.
    /// </summary>
    [Obsolete("This class is marked as an experimental preview and not fully functional at the moment", false)]
    public class HTTPMonitor : TCPStreamMonitor
    {
        Dictionary<TCPStreamMonitorStack, HTTPConversation> dictConversations;
        private int iHTTPPort;
        Dictionary<TCPStreamMonitorStack, Queue<HTTPResponse>> dictResponses;

        /// <summary>
        /// This delegate is used to handle surfer monitor events.
        /// </summary>
        /// <param name="sender">The object which rised the event</param>
        /// <param name="args">The event arguments</param>
        public delegate void HTTPMonitorEventHandler(object sender, HTTPMonitorEventArgs args);

        /// <summary>
        /// This event is fired when a session is completely intercepted
        /// </summary>
        public event HTTPMonitorEventHandler HTTPSessionMonitored;

        /// <summary>
        /// This event is fired when a session starts and monitoring begins.
        /// </summary>
        public event HTTPMonitorEventHandler HTTPSessionStarted;

        /// <summary>
        /// This event is fired when information related to a HTTP sesion changes. 
        /// </summary>
        public event HTTPMonitorEventHandler HTTPSessionInformationChanged;

        /// <summary>
        /// Gets or sets the HTTP port
        /// </summary>
        public int HTTPPort
        {
            get { return iHTTPPort; }
            set { iHTTPPort = value; }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public HTTPMonitor()
        {
            iHTTPPort = 80;
            dictConversations = new Dictionary<TCPStreamMonitorStack, HTTPConversation>();
            dictResponses = new Dictionary<TCPStreamMonitorStack, Queue<HTTPResponse>>();
            this.StackCreated += new EventHandler<TCPStreamMonitorEventArgs>(HTTPMonitor_StackCreated);
            this.StackDestroyed += new EventHandler<TCPStreamMonitorEventArgs>(HTTPMonitor_StackDestroyed);
        }

        void HTTPMonitor_StackDestroyed(object sender, TCPStreamMonitorEventArgs e)
        {
            InvokeExternalAsync(HTTPSessionMonitored, new HTTPMonitorEventArgs(dictConversations[e.Stack]));
            if (dictResponses[e.Stack].Count > 0)
            {
                InvokeExceptionThrown(new InvalidOperationException(this.Name + " was notified that a stack is being destroyed, but there are still requests missing in the HTTP conversation."));
            }
            dictResponses.Remove(e.Stack);
            dictConversations.Remove(e.Stack);
        }

        void HTTPMonitor_StackCreated(object sender, TCPStreamMonitorEventArgs e)
        {
            dictConversations.Add(e.Stack, new HTTPConversation(e.Stack.StackAlice.IPSocket.LocalBinding, e.Stack.StackBob.IPSocket.LocalBinding, e.Stack.StackAlice.TCPSocket.LocalBinding));
            dictResponses.Add(e.Stack, new Queue<HTTPResponse>());
            InvokeExternalAsync(HTTPSessionStarted, new HTTPMonitorEventArgs(dictConversations[e.Stack]));
        }

        protected override NetworkStreamMonitor[] CreateAndLinkStreamMonitors(eExNetworkLibrary.Sockets.NetworkStream nsAlice, eExNetworkLibrary.Sockets.NetworkStream nsBob)
        {
            HTTPRequestReader reqReader = new HTTPRequestReader(nsBob);
            HTTPResponseReader respReader = new HTTPResponseReader(nsAlice);

            reqReader.HTTPRequestCaptured += new EventHandler<HTTPReaderEventArgs>(reqReader_HTTPRequestCaptured);
            respReader.HTTPResponseCaptured += new EventHandler<HTTPReaderEventArgs>(respReader_HTTPResponseCaptured);

            return new NetworkStreamMonitor[] { reqReader, respReader };
        }

        void respReader_HTTPResponseCaptured(object sender, HTTPReaderEventArgs e)
        {
            TCPStreamMonitorStack tsStack = GetStackForMonitor((NetworkStreamMonitor)sender);
            if (tsStack == null)
            {
                InvokeExceptionThrown(new InvalidOperationException(this.Name + "captured a HTTP response from a stack which was already destroyed."));
                return;
            }
            HTTPConversation hcConversation = dictConversations[tsStack];

            HTTPRequest htqr = GetNextRequest(hcConversation);

            if (htqr == null)
            {
                //Request missing for response
                dictResponses[tsStack].Enqueue((HTTPResponse)e.HTTPMessage);
            }
            else
            {
                htqr.Response = (HTTPResponse)e.HTTPMessage;
            }

            InvokeExternalAsync(HTTPSessionInformationChanged, new HTTPMonitorEventArgs(hcConversation));
        }

        HTTPRequest GetNextRequest(HTTPConversation httpConversation)
        {
            foreach (HTTPRequest htrq in httpConversation.Requests)
            {
                if (htrq.Response == null)
                {
                    return htrq;
                }
            }

            return null;
        }

        void reqReader_HTTPRequestCaptured(object sender, HTTPReaderEventArgs e)
        {
            TCPStreamMonitorStack tsStack = GetStackForMonitor((NetworkStreamMonitor)sender);

            if (tsStack == null)
            {
                InvokeExceptionThrown(new InvalidOperationException("HTTP monitor captured a HTTP response from a stack which was already destroyed."));
                return;
            }

            HTTPConversation hcConversation = dictConversations[tsStack];

            hcConversation.AddRequest((HTTPRequest)e.HTTPMessage);

            if (dictResponses[tsStack].Count > 0)
            {
                HTTPRequest htqr = GetNextRequest(hcConversation);

                if (htqr != null)
                    htqr.Response = dictResponses[tsStack].Dequeue();
            }

            InvokeExternalAsync(HTTPSessionInformationChanged, new HTTPMonitorEventArgs(hcConversation));
        }

        protected override bool ShouldIntercept(IPAddress ipaSource, IPAddress ipaDestination, int iSourcePort, int iDestinationPort)
        {
            return iSourcePort == iHTTPPort || iDestinationPort == iHTTPPort;
        }
    }

    /// <summary>
    /// Represents the arguments of the HTTP monitor
    /// </summary>
    public class HTTPMonitorEventArgs
    {
        HTTPConversation siItem;

        /// <summary>
        /// The information associated with the event
        /// </summary>
        public HTTPConversation HTTPConversationInformation
        {
            get { return siItem; }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="siItem">The information associated with the event</param>
        public HTTPMonitorEventArgs(HTTPConversation siItem)
        {
            this.siItem = siItem;
        }
    }
    
    /// <summary>
    /// This class represents a intercepted HTTP conversation
    /// </summary>
    public class HTTPConversation
    {
        private IPAddress ipaServer;
        private IPAddress ipaClient;
        private int iSourcePort;
        private List<HTTPRequest> lRequests;

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="ipaClient">The IP of the client</param>
        /// <param name="ipaServer">The IP of the server</param>
        /// <param name="iSourcePort">The requesting client's source port</param>
        public HTTPConversation(IPAddress ipaClient, IPAddress ipaServer, int iSourcePort)
        {
            this.ipaClient = ipaClient;
            this.ipaServer = ipaServer;
            this.iSourcePort = iSourcePort;
            lRequests = new List<HTTPRequest>();
        }

        /// <summary>
        /// Returns all HTTP Requests associated with this communication
        /// </summary>
        public HTTPRequest[] Requests
        {
            get { return lRequests.ToArray(); }
        }

        /// <summary>
        /// Adds a HTTP request to this conversation.
        /// </summary>
        /// <param name="req">The request to add</param>
        internal void AddRequest(HTTPRequest req)
        {
            lRequests.Add(req);
        }

        /// <summary>
        /// Gets the IP-Address of the server
        /// </summary>
        public IPAddress Server
        {
            get { return ipaServer; }
        }

        /// <summary>
        /// Gets the IP-Address of the client
        /// </summary>
        public IPAddress Client
        {
            get { return ipaClient; }
        }

        /// <summary>
        /// Gets the requesting client's source port
        /// </summary>
        public int SourcePort
        {
            get { return iSourcePort; }
        }
    }
}
