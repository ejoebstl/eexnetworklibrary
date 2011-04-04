using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace eExNetworkLibrary.ARP
{
    /// <summary>
    /// This class represents an entry in an ARP host table
    /// </summary>
    public class ARPHostEntry
    {
        private MACAddress macAddress;
        private IPAddress ipAddress;

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="macAddress">The IP address</param>
        /// <param name="ipAddress">The MAC address associated with the IP address</param>
        public ARPHostEntry(MACAddress macAddress, IPAddress ipAddress)
        {
            this.macAddress = macAddress;
            this.ipAddress = ipAddress;
        }

        /// <summary>
        /// The MAC address associated with the IP address
        /// </summary>
        public MACAddress MAC
        {
            get { return macAddress; }
        }

        /// <summary>
        /// The IP address
        /// </summary>
        public IPAddress IP
        {
            get { return ipAddress; }
        }
    }
}
