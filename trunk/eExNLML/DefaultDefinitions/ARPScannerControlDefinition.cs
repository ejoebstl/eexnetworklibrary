// This source file is part of the eEx Network Library Management Layer (NLML)
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
using eExNetworkLibrary.Attacks.Scanning;
using eExNLML.Extensibility;
using eExNLML.IO;
using eExNLML.DefaultControllers;

namespace eExNLML.DefaultDefinitions
{
    public class ARPScannerControlDefinition : HandlerDefinition
    {
        public ARPScannerControlDefinition()
        {
            Name = "ARP Scanner";
            Description = "This traffic handler implements ARP-Scanning for the local subnet.";
            Author = "Emanuel Jöbstl";
            WebLink = "http://www.eex-dev.net";
            PluginType = PluginTypes.TrafficHandler;
            PluginKey = "eex_arp_scanner";
        }

        public override IHandlerController Create(IEnvironment env)
        {
            return new ARPScannerController(this, env);
        }
    }
}
