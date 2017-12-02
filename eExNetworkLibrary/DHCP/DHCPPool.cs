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
using System.Net;
using eExNetworkLibrary.IP;

namespace eExNetworkLibrary.DHCP
{
    /// <summary>
    /// This class represents a pool filled with DHCP leases
    /// </summary>
    public class DHCPPool
    {
        private List<DHCPPoolItem> lDHCPPool;

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public DHCPPool()
        {
            lDHCPPool = new List<DHCPPoolItem>();
        }

        /// <summary>
        /// Creates a DHCP pool and fills it according to the given params
        /// </summary>
        /// <param name="ipaPoolStart">The start IP address of the pool</param>
        /// <param name="ipaPoolEnd">The end IP address of the pool</param>
        /// <param name="ipaStandardgateway">The standardgateway's IP address</param>
        /// <param name="ipaDNSServer">The DNS server's IP address</param>
        /// <param name="smMask">The subnetmask</param>
        public DHCPPool(IPAddress ipaPoolStart, IPAddress ipaPoolEnd, IPAddress ipaStandardgateway, IPAddress ipaDNSServer, Subnetmask smMask) : this()
        {
            IPAddress[] ipRange = IPAddressAnalysis.GetIPRange(ipaPoolStart, ipaPoolEnd);
            foreach (IPAddress ipa in ipRange)
            {
                lDHCPPool.Add(new DHCPPoolItem(ipa, smMask, ipaStandardgateway, ipaDNSServer));
            }
        }

        /// <summary>
        /// Adds a DHCP pool item to this DHCP pool
        /// </summary>
        /// <param name="dhcpItem">The item to add</param>
        public void AddDHCPPoolItem(DHCPPoolItem dhcpItem)
        {
            lock (lDHCPPool)
            {
                lDHCPPool.Add(dhcpItem);
            }
        }

        /// <summary>
        /// Returns the DHCP pool item associated with the given address 
        /// </summary>
        /// <param name="ipa">The IP address to get the pool item for</param>
        /// <returns>The DHCP pool item associated with the given address </returns>
        public DHCPPoolItem GetItemForAddress(IPAddress ipa)
        {
            DHCPPoolItem freeDHCPItem = null;
            lock (lDHCPPool)
            {
                foreach (DHCPPoolItem dhcpItem in lDHCPPool)
                {
                    if (dhcpItem.Address.Equals(ipa))
                    {
                        freeDHCPItem = dhcpItem;
                        break;
                    }
                }
            }
            return freeDHCPItem;
        }

        /// <summary>
        /// Returns the next non-leased pool item from this DHCP pool
        /// </summary>
        /// <returns></returns>
        public DHCPPoolItem GetNextFreeAddress()
        {
            DHCPPoolItem freeDHCPItem = null;
            lock (lDHCPPool)
            {
                foreach (DHCPPoolItem dhcpItem in lDHCPPool)
                {
                    if (dhcpItem.LeasedTo.IsEmpty)
                    {
                        freeDHCPItem = dhcpItem;
                        break;
                    }
                }
            }
            return freeDHCPItem;
        }

        /// <summary>
        /// Returns all items in this pool
        /// </summary>
        public DHCPPoolItem[] Pool
        {
            get { return lDHCPPool.ToArray(); }
        }

        /// <summary>
        /// Removes a given item from this pool
        /// </summary>
        /// <param name="dhcpPoolItem">The item to remove</param>
        public void RemoveFromPool(DHCPPoolItem dhcpPoolItem)
        {
            lDHCPPool.Remove(dhcpPoolItem);
        }

        /// <summary>
        /// Returns a bool indicating whether a specific item is contained in this pool
        /// </summary>
        /// <param name="dhcpPoolItem">The DHCP pool item to search for</param>
        /// <returns>A bool indicating whether a specific item is contained in this pool</returns>
        public bool PoolContains(DHCPPoolItem dhcpPoolItem)
        {
            return lDHCPPool.Contains(dhcpPoolItem);
        }
    }

    /// <summary>
    /// This class represents an item contained in a DHCP pool which holds a IP address, 
    /// settings like gateway and DNS server and if available facts like the mac address 
    /// and hostname of the host which got this address leased from a DHCP server.
    /// </summary>
    public class DHCPPoolItem
    {
        private IPAddress ipaAddress;
        private Subnetmask smMask;
        private IPAddress ipaGateway;
        private IPAddress ipaDNSServer;
        private MACAddress macLeasedTo;
        private String strHostname;
        private TimeSpan tsLeaseDuration;
        private IPAddress ipaServer;
        private MACAddress macServer;

        /// <summary>
        /// The MAC address to which this item was leased
        /// </summary>
        public MACAddress LeasedTo
        {
            get { return macLeasedTo; }
            set { macLeasedTo = value; }
        }

        /// <summary>
        /// The subnetmask of this item
        /// </summary>
        public Subnetmask Netmask
        {
            get { return smMask; }
            set { smMask = value; }
        }

        /// <summary>
        /// The DNS server address to lease
        /// </summary>
        public IPAddress DNSServer
        {
            get { return ipaDNSServer; }
            set { ipaDNSServer = value; }
        }

        /// <summary>
        /// The gateway address to lease
        /// </summary>
        public IPAddress Gateway
        {
            get { return ipaGateway; }
            set { ipaGateway = value; }
        }

        /// <summary>
        /// The IP address to lease
        /// </summary>
        public IPAddress Address
        {
            get { return ipaAddress; }
            set { ipaAddress = value; }
        }

        /// <summary>
        /// The hostname to which this item was leased
        /// </summary>
        public string LeasedToHostname
        {
            get { return strHostname; }
            set { strHostname = value; }
        }

        /// <summary>
        /// The lease duration of this item
        /// </summary>
        public TimeSpan LeaseDuration
        {
            get { return tsLeaseDuration; }
            set { tsLeaseDuration = value; }
        }

        /// <summary>
        /// The DHCP server which leases this item
        /// </summary>
        public IPAddress DHCPServer
        {
            get { return ipaServer; }
            set { ipaServer = value; }
        }

        /// <summary>
        /// The MAC address of the DHCP server
        /// </summary>
        public MACAddress DHCPServerMAC
        {
            get { return macServer; }
            set { macServer = value; }
        }

        /// <summary>
        /// Creates a new instance of this class with the given params
        /// </summary>
        /// <param name="ipaAddress">The IP address</param>
        /// <param name="smMask">The subnetmask</param>
        /// <param name="ipaGateway">The gateway address</param>
        /// <param name="ipaDNSServer">The DNS server address</param>
        public DHCPPoolItem(IPAddress ipaAddress, Subnetmask smMask, IPAddress ipaGateway, IPAddress ipaDNSServer)
        {
            this.ipaAddress = ipaAddress;
            this.smMask = smMask;
            this.ipaGateway = ipaGateway;
            this.ipaDNSServer = ipaDNSServer;
            macLeasedTo = MACAddress.Empty;
            strHostname = "";
            tsLeaseDuration = new TimeSpan(0, 0, 0, 0);
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public DHCPPoolItem()
            : this(IPAddress.Any, new Subnetmask(), IPAddress.Any, IPAddress.Any)
        { }
    }
}
