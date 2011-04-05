using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Timers;

namespace eExNetworkLibrary.Routing
{
    /// <summary>
    /// This class represents a traffic handler which is capable of performing network address translation (NAT)
    /// </summary>
    public class NetworkAddressTranslationHandler : TrafficHandler
    {
        private List<NATEntry> lNATDatabase;
        private List<NATAddressRange> lExternalRange;
        private List<NATAddressRange> lInternalRange;
        private List<IPAddress> lExternalAddressPool;

        private int iRangeStartPort;
        private int iRangeEndPort;
        private bool bDropNonNATFrames;
        private bool bThrowOnNonNATFrame;

        private int iNATTimer;

        private Timer tTTLTimer;

        private IP.IPAddressAnalysis ipv4Analysis;

        /// <summary>
        /// This delegate represents the method which is used to handle NAT events
        /// </summary>
        /// <param name="sender">The class which rised the event</param>
        /// <param name="args">The arguments of the event</param>
        public delegate void NATEventHandler(object sender, NATEventArgs args);

        /// <summary>
        /// This event is fired when a NAT entry is newly created
        /// </summary>
        public event NATEventHandler NATEntryCreated;
        /// <summary>
        /// This event is fired when a NAT entry is removed
        /// </summary>
        public event NATEventHandler NATEntryRemoved;

        /// <summary>
        /// Gets or sets the NAT range start port, inclusive this port. NAT table entries which use this port will not be deleted when changing this value.
        /// </summary>
        public int PortRangeStart
        {
            get { return iRangeStartPort; }
            set
            {
                if (iRangeStartPort != value)
                {
                    iRangeStartPort = value;
                    InvokePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets a bool indicating whether a frame which does neither belong to the internal nor to the external range should be dropped.
        /// </summary>
        public bool DropNonNATFrames
        {
            get { return bDropNonNATFrames; }
            set
            {
                if (bDropNonNATFrames != value)
                {
                    bDropNonNATFrames = value;
                    InvokePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets a bool indicating whether an exception should be thrown when a external or unknown frame is discarded. 
        /// </summary>
        public bool ThrowOnNonNatFrames
        {
            get { return bThrowOnNonNATFrame; }
            set
            {
                if (bThrowOnNonNATFrame != value)
                {
                    bThrowOnNonNATFrame = value;
                    InvokePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the NAT timeout timer. This timer describes after how many seconds entries should removed from the NAT database when they are not accessed any more.
        /// </summary>
        public int NATTimer
        {
            get { return iNATTimer; }
            set
            {
                if (iNATTimer != value)
                {
                    iNATTimer = value;
                    InvokePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the NAT range end port, exclusive this port. NAT table entries which use this port will not be deleted when changing this value.
        /// </summary>
        public int PortRangeEnd
        {
            get { return iRangeEndPort; }
            set
            {
                if (iRangeEndPort != value)
                {
                    iRangeEndPort = value;
                    InvokePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Creates a new NAT handler with the given port range for NAT translation
        /// </summary>
        public NetworkAddressTranslationHandler()
        {
            this.iRangeStartPort = 4000;
            this.iRangeEndPort = 8000;
            ipv4Analysis = new eExNetworkLibrary.IP.IPAddressAnalysis();
            lNATDatabase = new List<NATEntry>();
            lExternalRange = new List<NATAddressRange>();
            lExternalAddressPool = new List<IPAddress>();
            lInternalRange = new List<NATAddressRange>();
            iNATTimer = 60;

            tTTLTimer = new Timer(1000);
            tTTLTimer.AutoReset = true;
            tTTLTimer.Elapsed += new ElapsedEventHandler(tTTLTimer_Elapsed);
        }

        void tTTLTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (lNATDatabase)
            {
                foreach (NATEntry eNatEntry in lNATDatabase.ToArray())
                {
                    eNatEntry.TTL--;
                    if (eNatEntry.TTL <= 0)
                    {
                        lNATDatabase.Remove(eNatEntry);
                        InvokeExternalAsync(NATEntryRemoved, new NATEventArgs(eNatEntry));
                    }
                }
            }
        }

        

        /// <summary>
        /// Adds the given NAT address range to the external range
        /// </summary>
        /// <param name="toAdd"></param>
        public void AddToExternalRange(NATAddressRange toAdd)
        {
            lock (lExternalRange)
            {
                lExternalAddressPool.AddRange(ipv4Analysis.GetIPRange(
                    ipv4Analysis.GetClasslessNetworkAddress(toAdd.NetworkAddress, toAdd.Subnetmask),
                    ipv4Analysis.GetClasslessBroadcastAddress(toAdd.NetworkAddress, toAdd.Subnetmask))
                    );
                lExternalRange.Add(toAdd);
            }
            InvokePropertyChanged();
        }

        /// <summary>
        /// Adds the given NAT address range to the internal range
        /// </summary>
        /// <param name="toAdd"></param>
        public void AddToInternalRange(NATAddressRange toAdd)
        {
            lock (lInternalRange)
            {
                lInternalRange.Add(toAdd);
            }
            InvokePropertyChanged();
        }

        /// <summary>
        /// Removes the given NAT address range from the external range. Open connections will not be interrupted.
        /// </summary>
        /// <param name="toRemove">The address range to remove</param>
        public void RemoveFromExternalRange(NATAddressRange toRemove)
        {
            lock (lExternalRange)
            {
                foreach (IPAddress ipa in ipv4Analysis.GetIPRange(
                    ipv4Analysis.GetClasslessNetworkAddress(toRemove.NetworkAddress, toRemove.Subnetmask),
                    ipv4Analysis.GetClasslessBroadcastAddress(toRemove.NetworkAddress, toRemove.Subnetmask))
                    )
                {
                    lExternalAddressPool.Remove(ipa);
                }
                lExternalRange.Remove(toRemove);
            }
            InvokePropertyChanged();
        }

        /// <summary>
        /// Removes the given NAT address range from the internal range. Open connections will not be interrupted.
        /// </summary>
        /// <param name="toRemove">The address range to remove</param>
        public void RemoveFromInternalRange(NATAddressRange toRemove)
        {
            lock (lInternalRange)
            {
                lInternalRange.Remove(toRemove);
            }
            InvokePropertyChanged();
        }

        /// <summary>
        /// Gets the internal NAT address range
        /// </summary>
        public NATAddressRange[] GetInternalRange()
        {
            lock (lInternalRange)
            {
                return lInternalRange.ToArray();
            }
        }

        /// <summary>
        /// Gets the external NAT address range
        /// </summary>
        public NATAddressRange[] GetExternalRange()
        {
            lock (lExternalRange)
            {
                return lExternalRange.ToArray();
            }
        }

        /// <summary>
        /// Does nothing
        /// </summary>
        public override void Cleanup()
        {
            //Do nothing
        }

        /// <summary>
        /// Extracts a IP frame and does some NAT
        /// </summary>
        /// <param name="fInputFrame">The frame to handle</param>
        protected override void HandleTraffic(Frame fInputFrame)
        {
            int iSourcePort = 0;
            int iDestinationPort = 0;

            IP.IPFrame ipFrame = GetIPv4Frame(fInputFrame);
            TCP.TCPFrame tcpFrame = GetTCPFrame(fInputFrame);
            UDP.UDPFrame udpFrame = null;
            if (tcpFrame != null)
            {
                iSourcePort = tcpFrame.SourcePort;
                iDestinationPort = tcpFrame.DestinationPort;
            }
            else
            {
                udpFrame = GetUDPFrame(fInputFrame);
                if (udpFrame != null)
                {
                    iSourcePort = udpFrame.SourcePort;
                    iDestinationPort = udpFrame.DestinationPort;
                }
            }

            if (ipFrame != null)
            {
                //In to out
                if (IsInternalRange(ipFrame.SourceAddress))
                {
                    NATEntry neEntry = GetTranslationEntry(ipFrame.SourceAddress, ipFrame.DestinationAddress, iSourcePort, iDestinationPort, ipFrame.Protocol);
                    if (neEntry == null)
                    {
                        neEntry = CreateTranslationEntry(ipFrame.SourceAddress, ipFrame.DestinationAddress, iSourcePort, iDestinationPort, ipFrame.Protocol);
                    }

                    ipFrame.SourceAddress = neEntry.TranslatedSourceAddress;
                    if (tcpFrame != null)
                    {
                        tcpFrame.SourcePort = neEntry.TranslatedSourcePort;
                        tcpFrame.Checksum = tcpFrame.CalculateChecksum(ipFrame.GetPseudoHeader());

                        CheckForTCPFinish(tcpFrame, neEntry);
                         
                    }
                    else if (udpFrame != null)
                    {
                        udpFrame.SourcePort = neEntry.TranslatedSourcePort;
                        udpFrame.Checksum = new byte[2];
                    }

                    NotifyNext(fInputFrame);
                }
                else if (IsExternalRange(ipFrame.DestinationAddress)) //Out to in
                {
                      NATEntry neEntry = GetReTranslationEntry(ipFrame.DestinationAddress, ipFrame.SourceAddress, iDestinationPort, iSourcePort, ipFrame.Protocol);
                      if (neEntry != null)
                      {
                          ipFrame.DestinationAddress = neEntry.OriginalSourceAddress;
                          if (tcpFrame != null)
                          {
                              tcpFrame.DestinationPort = neEntry.OriginalSourcePort;
                              tcpFrame.Checksum = tcpFrame.CalculateChecksum(ipFrame.GetPseudoHeader());

                              CheckForTCPFinish(tcpFrame, neEntry);
                          }
                          else if (udpFrame != null)
                          {
                              udpFrame.DestinationPort = neEntry.OriginalSourcePort;
                              udpFrame.Checksum = new byte[2];
                          }

                          NotifyNext(fInputFrame);
                      }
                      else
                      {
                          PushDroppedFrame(fInputFrame);
                          if (bThrowOnNonNATFrame)
                          {
                              throw new Exception("The external frame was discarded because there was no corresponding translation entry in the database. (Source: " + ipFrame.SourceAddress + "/" + iSourcePort + ", Destination: " + ipFrame.DestinationAddress + "/" + iDestinationPort + ", Protocol: " + ipFrame.Protocol.ToString());
                          }
                      }
                }
                else if (bDropNonNATFrames)
                {
                    PushDroppedFrame(fInputFrame);
                    if (bThrowOnNonNATFrame)
                    {
                        throw new Exception("The frame's source address was neither known in the external ranges nor in the internal ranges. (Source: " + ipFrame.SourceAddress + "/" + iSourcePort + ", Destination: " + ipFrame.DestinationAddress + "/" + iDestinationPort + ", Protocol: " + ipFrame.Protocol.ToString());
                    }
                }
                else
                {
                    NotifyNext(fInputFrame);
                }
            }
        }

        private void CheckForTCPFinish(TCP.TCPFrame tcpFrame, NATEntry neEntry)
        {
            if (tcpFrame.FinishFlagSet) //TCP finish flag checking
            {
                if (neEntry.IsTCPTeardown)
                {
                    neEntry.IsTCPFinished = true;
                }
                else
                {
                    neEntry.IsTCPTeardown = true;
                }
            }
            if (neEntry.IsTCPFinished && tcpFrame.AcknowledgementFlagSet) // Deactivate when finished
            {
                lock (lNATDatabase)
                {
                    lNATDatabase.Remove(neEntry);
                    InvokeExternalAsync(NATEntryRemoved, new NATEventArgs(neEntry));
                }
            }
        }

        /// <summary>
        /// Returns the NAT translation database
        /// </summary>
        public NATEntry[] NATTable
        {
            get
            {
                lock (lNATDatabase)
                {
                    return lNATDatabase.ToArray();
                }
            }
        }

        /// <summary>
        /// Starts this traffic handler
        /// </summary>
        public override void Start()
        {
            base.Start();
            tTTLTimer.Start();
        }

        /// <summary>
        /// Stops this traffic handler
        /// </summary>
        public override void Stop()
        {
            tTTLTimer.Stop();
            base.Stop();
        }

        //Seen from inside
        private NATEntry GetTranslationEntry(IPAddress ipaSource, IPAddress ipaDestination, int iSourcePort, int iDestinationPort, IP.IPProtocol iIPProtocol)
        {
            lock (lNATDatabase)
            {
                foreach (NATEntry ne in lNATDatabase)
                {
                    if (ne.OriginalSourceAddress.Equals(ipaSource) &&
                        ne.DestinationAddress.Equals(ipaDestination) &&
                        ne.OriginalSourcePort.Equals(iSourcePort) &&
                        ne.DestinationPort.Equals(iDestinationPort) &&
                        ne.IPProtocol.Equals(iIPProtocol))
                    {
                        ne.TTL = iNATTimer;
                        return ne;
                    }
                }
            }
            return null;
        }

        //Seen from inside
        private NATEntry GetReTranslationEntry(IPAddress ipaTranslatedSource, IPAddress ipaDestination, int iTranslatedSourcePort, int iDestinationPort, IP.IPProtocol iProtocol)
        {
            lock (lNATDatabase)
            {
                foreach (NATEntry ne in lNATDatabase)
                {
                    if (ne.TranslatedSourceAddress.Equals(ipaTranslatedSource) &&
                        ne.DestinationAddress.Equals(ipaDestination) &&
                        ne.TranslatedSourcePort.Equals(iTranslatedSourcePort) &&
                        ne.DestinationPort.Equals(iDestinationPort) &&
                        ne.IPProtocol.Equals(iProtocol))
                    {
                        ne.TTL = iNATTimer;
                        return ne;
                    }
                }
            }
            return null;
        }

        // Seen from inside
        private NATEntry CreateTranslationEntry(IPAddress ipaSource, IPAddress ipaDestination, int iSourcePort, int iDestinationPort, IP.IPProtocol iIPProtocol)
        {
            int iTranslatedSourcePort = 0;

            NATEntry eNatEntry;

            lock (lExternalRange)
            {
                foreach (IPAddress ipa in lExternalAddressPool)
                {
                    if (iIPProtocol == IP.IPProtocol.UDP || iIPProtocol == IP.IPProtocol.TCP)
                    {
                        iTranslatedSourcePort = iRangeStartPort;
                    }
                    do
                    {
                        if (GetReTranslationEntry(ipa, ipaDestination, iTranslatedSourcePort, iDestinationPort, iIPProtocol) == null)
                        {
                            eNatEntry = new NATEntry(iIPProtocol, ipaSource, ipa, ipaDestination, iSourcePort, iTranslatedSourcePort, iDestinationPort);
                            eNatEntry.TTL = iNATTimer;
                            lock (lNATDatabase)
                            {
                                lNATDatabase.Add(eNatEntry);
                            }
                            InvokeExternalAsync(NATEntryCreated, new NATEventArgs(eNatEntry));
                            return eNatEntry;
                        }
                        iTranslatedSourcePort++;
                    }
                    while ((iIPProtocol == IP.IPProtocol.UDP || iIPProtocol == IP.IPProtocol.TCP) && iRangeStartPort < iRangeEndPort);
                }
            }

            throw new Exception("A NAT entry could not be created cause the pools are exhausted. (Source: " + ipaSource + "/" + iSourcePort + ", Destination: " + ipaDestination + "/" + iDestinationPort + ", Protocol: " + iIPProtocol.ToString());
        }

        private bool IsExternalRange(IPAddress ipaAddress)
        {
            lock (lExternalRange)
            {
                foreach (NATAddressRange ipa in lExternalRange)
                {
                    if (ipv4Analysis.GetClasslessNetworkAddress(ipa.NetworkAddress, ipa.Subnetmask).Equals(ipv4Analysis.GetClasslessNetworkAddress(ipaAddress, ipa.Subnetmask)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsInternalRange(IPAddress ipaAddress)
        {
            lock (lInternalRange)
            {
                foreach (NATAddressRange ipa in lInternalRange)
                {
                    if (ipv4Analysis.GetClasslessNetworkAddress(ipa.NetworkAddress, ipa.Subnetmask).Equals(ipv4Analysis.GetClasslessNetworkAddress(ipaAddress, ipa.Subnetmask)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    /// <summary>
    /// This class represents a simple NAT entry
    /// </summary>
    public class NATEntry
    {
        private IP.IPProtocol ipProtocol;
        private IPAddress ipaOriginalSource;
        private IPAddress ipaTranslatedSource;
        private IPAddress ipaDestination;

        private int iOriginalSourcePort;
        private int iTranslatedSourcePort;
        private int iDestinationPort;

        private int iTTL;
        private bool bTCPTeardown;
        private bool bIsTCPFinished;

        /// <summary>
        /// Indicates whether this TCP connection is finished
        /// </summary>
        internal bool IsTCPFinished
        {
            get { return bIsTCPFinished; }
            set { bIsTCPFinished = value; }
        }

        /// <summary>
        /// Indicates whether the TCP connection is in a teardown process
        /// </summary>
        internal bool IsTCPTeardown
        {
            get { return bTCPTeardown; }
            set { bTCPTeardown = value; }
        }

        /// <summary>
        /// Gets the value of the IP protocol field of the frame to be translated
        /// </summary>
        public IP.IPProtocol IPProtocol
        {
            get { return ipProtocol; }
        }

        /// <summary>
        /// Gets the value of the original source address (address of internal host) 
        /// </summary>
        public IPAddress OriginalSourceAddress
        {
            get { return ipaOriginalSource; }
        }

        /// <summary>
        /// Gets the value of the source address (address of internal host) after the translation
        /// </summary>
        public IPAddress TranslatedSourceAddress
        {
            get { return ipaTranslatedSource; }
        }

        /// <summary>
        /// Gets the value of the destination address (address of the external host)
        /// </summary>
        public IPAddress DestinationAddress
        {
            get { return ipaDestination; }
        }

        /// <summary>
        /// Gets the value of the original source port (port of internal host) 
        /// </summary>
        public int OriginalSourcePort
        {
            get { return iOriginalSourcePort; }
        }

        /// <summary>
        /// Gets the value of the original source port (port of internal host) after the translation
        /// </summary>
        public int TranslatedSourcePort
        {
            get { return iTranslatedSourcePort; }
        }

        /// <summary>
        /// Gets the  value of the destination port (port of the external host)
        /// </summary>
        public int DestinationPort
        {
            get { return iDestinationPort; }
        }

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="ipProtocol">The IP portocol</param>
        /// <param name="ipaSource">The original source address</param>
        /// <param name="ipaTranslatedSource">The translated source address</param>
        /// <param name="ipaDestination">The destination address</param>
        /// <param name="iSourcePort">The original source port</param>
        /// <param name="iTranslatedSourcePort">The translated source port</param>
        /// <param name="iDestinationPort">The destination port</param>
        public NATEntry(IP.IPProtocol ipProtocol, IPAddress ipaSource, IPAddress ipaTranslatedSource, IPAddress ipaDestination, int iSourcePort, int iTranslatedSourcePort, int iDestinationPort)
        {
            this.ipProtocol = ipProtocol;
            this.ipaOriginalSource = ipaSource;
            this.ipaTranslatedSource = ipaTranslatedSource;
            this.ipaDestination = ipaDestination;
            this.iOriginalSourcePort = iSourcePort;
            this.iTranslatedSourcePort = iTranslatedSourcePort;
            this.iDestinationPort = iDestinationPort;
        }

        /// <summary>
        /// Gets or sets the TTL of this entry
        /// </summary>
        internal int TTL
        {
            get { return iTTL; }
            set { iTTL = value; }
        }

        /// <summary>
        /// Compares an object to this object
        /// </summary>
        /// <param name="obj">The object to compare to this object</param>
        /// <returns>A bool indicating whether this object equals the given object</returns>
        public override bool Equals(object obj)
        {
            if (obj is NATEntry)
            {
                NATEntry e = (NATEntry)obj;
                return e.iDestinationPort == this.iDestinationPort &&
                    e.iOriginalSourcePort == this.iOriginalSourcePort &&
                    e.ipaDestination == this.ipaDestination &&
                    e.ipaOriginalSource == this.ipaOriginalSource &&
                    e.ipaTranslatedSource == this.ipaTranslatedSource &&
                    e.ipProtocol == this.ipProtocol &&
                    e.iTranslatedSourcePort == this.iTranslatedSourcePort;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the hash code of this object, based on the destination address
        /// </summary>
        /// <returns>The hash code of this object, based on the destination address</returns>
        public override int GetHashCode()
        {
            return ipaDestination.GetHashCode();
        }
    }

    /// <summary>
    /// This class represents a network range
    /// </summary>
    public class NATAddressRange
    {
        IPAddress ipaNetworkAddress;
        Subnetmask smSubnetmask;

        /// <summary>
        /// Gets the subnetmask of the network to represent
        /// </summary>
        public Subnetmask Subnetmask
        {
            get { return smSubnetmask; }
        }

        /// <summary>
        /// Gets the network address of the network to represent
        /// </summary>
        public IPAddress NetworkAddress
        {
            get { return ipaNetworkAddress; }
        }

        /// <summary>
        /// Creates a new instance of this class with the given params
        /// </summary>
        /// <param name="smMask">The subnetmask of the network to represent</param>
        /// <param name="ipaNetworkAddress">The IP address of the network to represent</param>
        public NATAddressRange(Subnetmask smMask, IPAddress ipaNetworkAddress)
        {
            this.smSubnetmask = smMask;
            this.ipaNetworkAddress = ipaNetworkAddress;
        }
    }

    /// <summary>
    /// This class represents the data which is associated with NAT events
    /// </summary>
    public class NATEventArgs : EventArgs
    {
        private NATEntry neEntry;

        /// <summary>
        /// Gets the NAT entry associated with this event.
        /// </summary>
        public NATEntry Entry
        {
            get { return neEntry; }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="neEntry">The entry to associate with this event</param>
        public NATEventArgs(NATEntry neEntry)
        {
            this.neEntry = neEntry;
        }
    }
}
