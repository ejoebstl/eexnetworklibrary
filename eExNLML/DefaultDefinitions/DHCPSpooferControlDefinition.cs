﻿// This source file is part of the eEx Network Library Management Layer (NLML)
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
using eExNetworkLibrary.Attacks.MITM;
using eExNetworkLibrary.Attacks.Spoofing;
using eExNLML.Extensibility;
using eExNLML.IO.HandlerConfigurationLoaders;
using eExNLML.IO.HandlerConfigurationWriters;
using eExNetworkLibrary;
using eExNLML.IO;
using eExNLML.DefaultControllers;

namespace eExNLML.DefaultDefinitions
{
    public class DHCPSpooferControlDefinition : HandlerDefinition
    {
        public DHCPSpooferControlDefinition()
        {
            Name = "DHCP Spoofer";
            Description = "This traffic handler steals IP-Adresses and redistributes them with spoofed gateways. It also includes the functions of a simple DHCP server.";
            Author = "Emanuel Jöbstl";
            WebLink = "http://www.eex-dev.net";
            PluginType = PluginTypes.TrafficHandler;
            PluginKey = "eex_dhcp_spoofer";
        }

        public override IHandlerController Create(IEnvironment env)
        {
            return new DHCPSpooferController(this, env);
        }
    }
}
