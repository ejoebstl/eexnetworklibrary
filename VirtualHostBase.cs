using System;
using System.Collections.Generic;
using System.Text;
using System.Net.NetworkInformation;
using System.Net;

namespace eExNetworkLibrary
{
    /// <summary>
    /// This class represents the definition of a basic host in a network for representation in a graph
    /// </summary>
    public class VirtualHostBase
    {
        private IPAddress ipaHostAddress;

        /// <summary>
        /// The IP-Address of this host
        /// </summary>
        public IPAddress HostAddress
        {
            get { return ipaHostAddress; }
            set { ipaHostAddress = value; }
        }

        private List<IPAddress> ipaNextHops;

        /// <summary>
        /// All next hops connected to this host
        /// </summary>
        public List<IPAddress> NextHops
        {
            get { return ipaNextHops; }
            set { ipaNextHops = value; }
        }

        private List<IPAddress> ipaPreviousHops;

        /// <summary>
        /// All previous hops connected to this hosts
        /// </summary>
        public List<IPAddress> PreviousHops
        {
            get { return ipaPreviousHops; }
            set { ipaPreviousHops = value; }
        }

        private double dRoundtripTime;

        /// <summary>
        /// The estimated roundtrip time of a ping to this host
        /// </summary>
        public double RoundtripTime
        {
            get { return dRoundtripTime; }
            set { dRoundtripTime = value; }
        }

        private string strName;

        /// <summary>
        /// The name of this host
        /// </summary>
        public string Name
        {
            get { return strName; }
            set { strName = value; }
        }
	

        /// <summary>
        /// Creates a new instance of this class with the given IP-Address
        /// </summary>
        /// <param name="ipaHostAddress">The IP-Address to assign</param>
        public VirtualHostBase(IPAddress ipaHostAddress)
        {
            this.ipaHostAddress = ipaHostAddress;
            ipaNextHops = new List<IPAddress>();
            ipaPreviousHops = new List<IPAddress>();
            dRoundtripTime = double.NaN;
            strName = "<unknown>";
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public VirtualHostBase() : this(IPAddress.Any)
        {
        }
    }
}
