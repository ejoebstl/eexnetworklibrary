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
using eExNLML.Extensibility;
using eExNLML.IO.HandlerConfigurationLoaders;
using eExNLML.IO.HandlerConfigurationWriters;
using eExNLML.IO;
using eExNetworkLibrary;
using eExNetworkLibrary.Monitoring;
using eExNLML.DefaultControllers;

namespace eExNLML.DefaultDefinitions
{
    public class RouterControlDefinition : HandlerDefinition
    {
        public RouterControlDefinition()
        {
            Name = "Router";
            Description = "This traffic handler represents a router which is used to route traffic to the correct interfaces";
            Author = "Emanuel Jöbstl";
            WebLink = "http://www.eex-dev.net";
            PluginType = PluginTypes.TrafficHandler;
            PluginKey = "eex_router";
        }

        public override IHandlerController Create(IEnvironment env)
        {
            return new RouterController(this, env);
        }
    }
}
