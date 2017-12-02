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
using System.Threading;
using eExNetworkLibrary.Utilities;
using eExNetworkLibrary.Ethernet;
using eExNetworkLibrary.ARP;
using eExNetworkLibrary.IP;

namespace eExNetworkLibrary.Attacks.Scanning
{
    /// <summary>
    /// This class represents a scan task for the ARP net scanner
    /// </summary>
    public class ARPScanTask : ScanTask
    {
        private MACAddress macLocal;

        /// <summary>
        /// Creates a new instance of this class with the given params
        /// </summary>
        /// <param name="ipaStart">The start IP address of the range to scan</param>
        /// <param name="ipaEnd">The end IP address of the range to scan</param>
        /// <param name="macLocal">The MAC address to spoof in the ARP frame. This should equal the MAC of the output interface.</param>
        /// <param name="ipLocal">The IP address which should be spoofed during scanning</param>
        /// <param name="thOut">The traffic handler to which the generated ARP frames should be forwarded. It is wise to assign an ARP net scanner here</param>
        public ARPScanTask(IPAddress ipaStart, IPAddress ipaEnd, MACAddress macLocal, IPAddress ipLocal, TrafficHandler thOut) : base(ipaStart, ipaEnd, ipLocal, thOut)
        {
            this.macLocal = macLocal;
        }

        protected override void Scan(IPAddress ipaDestination)
        {
            ARPFrame arpFrame = new ARPFrame();
            arpFrame.DestinationIP = ipaDestination;
            arpFrame.SourceIP = SourceAddress;
            arpFrame.DestinationMAC = MACAddress.Parse("ff:ff:ff:ff:ff:ff");
            arpFrame.SourceMAC = macLocal;
            arpFrame.ProtocolAddressType = EtherType.IPv4;
            arpFrame.HardwareAddressType = HardwareAddressType.Ethernet;
            arpFrame.Operation = ARPOperation.Request;

            OutputHandler.PushTraffic(arpFrame);
        }
    }
}
