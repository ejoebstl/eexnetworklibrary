using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.CommonTrafficAnalysis;
using System.Net;
using eExNetworkLibrary.UDP;
using eExNetworkLibrary.IP;
using eExNetworkLibrary.DNS;

namespace eExNetworkLibrary.Monitoring
{
    /// <summary>
    /// This traffic analyzer is capable of logging diffrent DNS queries and their responses
    /// </summary>
    public class DNSQueryLogger : TrafficAnalyzer 
    {
        private List<DNSItem> lLog;
        private int iDNSPort;

        /// <summary>
        /// Gets or sets the DNS port to use
        /// </summary>
        public int DNSPort
        {
            get { return iDNSPort; }
            set
            {
                iDNSPort = value;
                InvokePropertyChanged();
            }
        }

        /// <summary>
        /// This delegate is used to handle DNS logger events
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="dsEventArgs">The event arguments</param>
        public delegate void DNSLoggerEventHandler(object sender, DNSEventArgs dsEventArgs);
        
        /// <summary>
        /// This event is rised by this class if an item in the current log is added or updated
        /// </summary>
        public event DNSLoggerEventHandler ItemUpdated;

        /// <summary>
        /// Returns the current log
        /// </summary>
        public DNSItem[] Log
        {
            get { return lLog.ToArray(); }
        }

        /// <summary>
        /// Clears the current log
        /// </summary>
        public void ClearLog()
        {
            lLog.Clear();
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public DNSQueryLogger()
        {
            lLog = new List<DNSItem>();
            iDNSPort = 53;
        }

        /// <summary>
        /// Checks whether the input frame contains a DNS component. 
        /// If it contains a DNS frame, the DNS frame will be parsed and logged
        /// </summary>
        /// <param name="fInputFrame">The frame to analyze</param>
        protected override void HandleTraffic(Frame fInputFrame)
        {
            UDPFrame fUDP = GetUDPFrame(fInputFrame);
            IPFrame ipFrame = GetIPFrame(fInputFrame);
            DNSFrame dFrame = (DNSFrame)GetFrameByType(fInputFrame, FrameTypes.DNS);

            if (fUDP != null && ipFrame != null && dFrame != null)
            {
                bool bFound = false;
                foreach (DNSItem di in lLog)
                {
                    foreach (DNSQuestion qs in dFrame.GetQuestions())
                    {
                        if ((di.QueryingHost.Equals(ipFrame.SourceAddress) || di.QueryingHost.Equals(ipFrame.DestinationAddress)) && di.TransactionID == dFrame.Identifier && di.QueryName == qs.Query && !di.TransactionComplete)
                        {
                            bFound = true;
                        }
                    }
                }
                if (!bFound)
                {
                    foreach (DNSQuestion qs in dFrame.GetQuestions())
                    {
                        DNSItem dsItem;
                        if (dFrame.QRFlag)
                        {
                            dsItem = new DNSItem(qs.Query, ipFrame.DestinationAddress, ipFrame.SourceAddress, TimeSpan.Zero, dFrame.Identifier);
                        }
                        else
                        {
                            dsItem = new DNSItem(qs.Query, ipFrame.SourceAddress, ipFrame.DestinationAddress, TimeSpan.Zero, dFrame.Identifier);
                        }
                        AddLogItem(dsItem);
                    }
                }
                if (dFrame.QRFlag)
                {
                    foreach (DNSItem dsItem in lLog)
                    {
                        if (dFrame.Identifier == dsItem.TransactionID && !dsItem.TransactionComplete)
                        {
                            foreach (DNSResourceRecord rr in dFrame.GetAnswers())
                            {
                                if (rr.Type == DNSResourceType.CNAME)
                                {
                                    if (rr.Name == dsItem.QueryName)
                                    {
                                        string strTMPName = ASCIIEncoding.ASCII.GetString(rr.ResourceData);
                                        foreach (DNSResourceRecord rr2 in dFrame.GetAnswers())
                                        {
                                            if (rr2.Type == DNSResourceType.A && rr2.Name == strTMPName)
                                            {
                                                IPAddress ipa = new IPAddress(rr2.ResourceData);
                                                if (!dsItem.ContainsAnswer(ipa))
                                                {
                                                    dsItem.AddAnswer(ipa);
                                                }
                                                dsItem.ChacheTime = new TimeSpan(0, 0, rr2.TTL);
                                                dsItem.TransactionComplete = true;
                                                dsItem.AnsweringServer = ipFrame.SourceAddress;
                                                InvokeUpdated(dsItem);
                                            }
                                        }
                                    }
                                }
                                if (rr.Type == DNSResourceType.A && rr.Name == dsItem.QueryName)
                                {
                                    IPAddress ipa = new IPAddress(rr.ResourceData);
                                    if (!dsItem.ContainsAnswer(ipa))
                                    {
                                        dsItem.AddAnswer(ipa);
                                    }
                                    dsItem.ChacheTime = new TimeSpan(0, 0, rr.TTL);
                                    dsItem.AnsweringServer = ipFrame.SourceAddress;
                                    dsItem.TransactionComplete = true;
                                    InvokeUpdated(dsItem);
                                }
                            }
                        }
                    }
                }
            }
        }
            

        private void AddLogItem(DNSItem dsItem)
        {
            lLog.Add(dsItem);
            InvokeUpdated(dsItem);
        }

        private void InvokeUpdated(DNSItem dsItem)
        {
            InvokeExternalAsync(ItemUpdated, new DNSEventArgs(dsItem));
        }

        /// <summary>
        /// Does nothing
        /// </summary>
        public override void Cleanup()
        {
            //Nothing needed.
        }
    }

    /// <summary>
    /// A simple class which derives from event args and is used to notify about log updates
    /// </summary>
    public class DNSEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the DNS item which was updated
        /// </summary>
        public DNSItem DNSItem
        {
            get { return dnsItem; }
        }

        private DNSItem dnsItem;

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="itemChanged">The DNS item which changed</param>
        public DNSEventArgs(DNSItem itemChanged)
        {
            this.dnsItem = itemChanged;
        }
    }

    /// <summary>
    /// Represents a DNS item in the log
    /// </summary>
    public class DNSItem
    {
        private string strQueryName;
        private IPAddress ipaQueryingHost;
        private IPAddress ipaAnsweringServer;
        private List<IPAddress> ipaAnswer;
        private TimeSpan tsChacheTime;
        private int iTransactionID;
        private bool bTransactionComplete;

        /// <summary>
        /// Removes all answers
        /// </summary>
        public void ClearAnswers()
        {
            ipaAnswer.Clear();
        }

        /// <summary>
        /// Adds a answer
        /// </summary>
        /// <param name="ipa">The answer to add</param>
        public void AddAnswer(IPAddress ipa)
        {
            ipaAnswer.Add(ipa);
        }

        /// <summary>
        /// Gets all answers
        /// </summary>
        /// <returns>All associated answers</returns>
        public IPAddress[] GetAnswers()
        {
            return ipaAnswer.ToArray();
        }

        /// <summary>
        /// Gets or sets a bool indicating whether the transaction is complete
        /// </summary>
        public bool TransactionComplete
        {
            get { return bTransactionComplete; }
            set { bTransactionComplete = value; }
        }

        /// <summary>
        /// Returns a bool indicating whether a answer is contained in this frame
        /// </summary>
        /// <param name="ipa">The answer to search for</param>
        /// <returns>A bool indicating whether a answer is contained in this frame</returns>
        public bool ContainsAnswer(IPAddress ipa)
        {
            return ipaAnswer.Contains(ipa);
        }

        /// <summary>
        /// Removes a specific answer from this frame
        /// </summary>
        /// <param name="ipa">The answer to remove</param>
        public void RemoveAnswer(IPAddress ipa)
        {
            ipaAnswer.Remove(ipa);
        }

        /// <summary>
        /// Gets or sets the transaction ID
        /// </summary>
        public int TransactionID
        {
            get { return iTransactionID; }
            set { iTransactionID = value; }
        }

        /// <summary>
        /// Gets or sets the query name
        /// </summary>
        public string QueryName
        {
            get { return strQueryName; }
            set { strQueryName = value; }
        }

        /// <summary>
        /// Gets or sets the querying host
        /// </summary>
        public IPAddress QueryingHost
        {
            get { return ipaQueryingHost; }
            set { ipaQueryingHost = value; }
        }

        /// <summary>
        /// Gets or sets the answering server
        /// </summary>
        public IPAddress AnsweringServer
        {
            get { return ipaAnsweringServer; }
            set { ipaAnsweringServer = value; }
        }

        /// <summary>
        /// Gets or sets the cache time
        /// </summary>
        public TimeSpan ChacheTime
        {
            get { return tsChacheTime; }
            set { tsChacheTime = value; }
        }

        /// <summary>
        /// Creates a new instance of this class with the given properties
        /// </summary>
        /// <param name="strQueryName">The query name</param>
        /// <param name="ipaQueryingHost">The querying host</param>
        /// <param name="ipaAnsweringServer">The answering server</param>
        /// <param name="tsChacheTime">The cache time</param>
        /// <param name="iTransactionID">The transaction ID</param>
        public DNSItem(string strQueryName, IPAddress ipaQueryingHost, IPAddress ipaAnsweringServer, TimeSpan tsChacheTime, int iTransactionID)
        {
            this.strQueryName = strQueryName;
            this.ipaQueryingHost = ipaQueryingHost;
            this.ipaAnsweringServer = ipaAnsweringServer;
            this.tsChacheTime = tsChacheTime;
            this.ipaAnswer = new List<IPAddress>();
            this.iTransactionID = iTransactionID;
            bTransactionComplete = false;
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public DNSItem()
        {
            strQueryName = "";
            ipaAnsweringServer = IPAddress.Any;
            ipaQueryingHost = IPAddress.Any;
            ipaAnswer = new List<IPAddress>();
            tsChacheTime = new TimeSpan();
            bTransactionComplete = false;
        }
    }
}
