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
using eExNetworkLibrary.Routing;
using eExNLML.IO.HandlerConfigurationLoaders;
using eExNLML.IO.HandlerConfigurationWriters;
using eExNetworkLibrary;
using eExNLML.IO;
using eExNLML.Extensibility;
using eExNLML.DefaultControllers;

namespace eExNLML.DefaultDefinitions
{
    public class NATHandlerControlDefinition : HandlerDefinition
    {
        public NATHandlerControlDefinition()
        {
            Name = "NAT Handler";
            Description = "This traffic handler is capable of performing network address translation (NAT) between internal and external address ranges. Be aware - this component does not work with all other components which rely on IP addressing.";
            Author = "Emanuel Jöbstl";
            WebLink = "http://www.eex-dev.net";
            PluginType = PluginTypes.TrafficHandler;
            PluginKey = "eex_nat";
        }

        public override IHandlerController Create(IEnvironment env)
        {
            return new NATHandlerController(this, env);
        }
    }
}
