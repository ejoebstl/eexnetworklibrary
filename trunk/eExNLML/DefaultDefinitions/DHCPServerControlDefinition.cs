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
using eExNetworkLibrary.DHCP;
using eExNLML.IO.HandlerConfigurationLoaders;
using eExNLML.IO.HandlerConfigurationWriters;
using eExNLML.Extensibility;
using eExNetworkLibrary;
using eExNLML.IO;
using eExNLML.DefaultControllers;
namespace eExNLML.DefaultDefinitions
{
    public class DHCPServerControlDefinition : HandlerDefinition
    {
        public DHCPServerControlDefinition()
        {
            Name = "DHCP Server";
            Description = "This traffic handler acts like a simple DHCP server. It has a pool of addresses and redistributes it to requesting clients.";
            Author = "Emanuel Jöbstl";
            WebLink = "http://www.eex-dev.net";
            PluginType = PluginTypes.TrafficHandler;
            PluginKey = "eex_dhcp_server";
        }

        public override IHandlerController Create(IEnvironment env)
        {
            return new DHCPServerController(this, env);
        }
    }
}
