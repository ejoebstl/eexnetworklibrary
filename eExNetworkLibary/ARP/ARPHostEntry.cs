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

namespace eExNetworkLibrary.ARP
{
    /// <summary>
    /// This class represents an entry in an ARP host table
    /// </summary>
    public class ARPHostEntry
    {
        private MACAddress macAddress;
        private IPAddress ipAddress;
        private bool bIsStatic;
        private DateTime dtValidUtil;

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="macAddress">The IP address</param>
        /// <param name="ipAddress">The MAC address associated with the IP address</param>
        public ARPHostEntry(MACAddress macAddress, IPAddress ipAddress) : this(macAddress, ipAddress, false)
        {

        }
        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="macAddress">The IP address</param>
        /// <param name="ipAddress">The MAC address associated with the IP address</param>
        /// <param name="bStatic">A bool indicating whether this address entry is static</param>
        public ARPHostEntry(MACAddress macAddress, IPAddress ipAddress, bool bStatic)
            : this(macAddress, ipAddress, bStatic, bStatic ? new DateTime(0) : DateTime.Now.AddMinutes(1))
        {

        }


        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="macAddress">The IP address</param>
        /// <param name="ipAddress">The MAC address associated with the IP address</param>
        /// <param name="bStatic">A bool indicating whether this address entry is static</param>
        public ARPHostEntry(MACAddress macAddress, IPAddress ipAddress, bool bStatic, DateTime dtValidUtil)
        {
            this.macAddress = macAddress;
            this.ipAddress = ipAddress;
            this.bIsStatic = bStatic;
            this.dtValidUtil = dtValidUtil;
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

        /// <summary>
        /// Gets a DateTime object which indicates how long this ARP entry is valid. 
        /// </summary>
        public DateTime ValidUtil
        {
            get { return dtValidUtil; }
        }

        /// <summary>
        /// Gets a bool indicating whether this entry is static
        /// </summary>
        public bool IsStatic
        {
            get { return bIsStatic; }
        }
    }
}
