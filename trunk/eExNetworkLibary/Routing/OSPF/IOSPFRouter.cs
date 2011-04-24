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
namespace eExNetworkLibrary.Routing.OSPF
{
    [Obsolete("This class is marked depreceated in cause of a sloppy implementation and only limited compatibility..", false)]
    interface IOSPFRouter
    {
        int DeadTimer { get; set; }
        uint ID { get; set; }
        int Priority { get; set; }
        System.Net.IPAddress IPAddress { get; set; }
        DirectAttachedNetwork AttachedNetwork { get; set; }
        OSPFState State { get; set; }
        MACAddress MACAddress { get; set; }
        OSPFOptionsField Options { get; set; }
        bool IsMaster { get; set; }
    }
}
