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
using eExNetworkLibrary.Monitoring;
using eExNLML.IO;
using eExNetworkLibrary;
using eExNLML.Extensibility;
using eExNLML.DefaultControllers;

namespace eExNLML.DefaultDefinitions
{
    public class HTTPMonitorControlDefinition : HandlerDefinition
    {
        public HTTPMonitorControlDefinition()
        {
            Name = "HTTP Monitor";
            Description = "This Traffic Analyzier is capable of intercepting HTTP-conversations and displaying intercepted site content (This plug-in is experimental).";
            Author = "Emanuel Jöbstl";
            WebLink = "http://www.eex-dev.net";
            PluginType = PluginTypes.TrafficHandler;
            PluginKey = "eex_http_monitor";
        }

        public override IHandlerController Create(IEnvironment env)
        {
            return new HTTPMonitorController(this, env);
        }
    }
}
