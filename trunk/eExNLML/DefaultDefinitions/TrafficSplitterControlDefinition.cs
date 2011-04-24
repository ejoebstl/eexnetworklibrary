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
using eExNetworkLibrary;
using eExNetworkLibrary.TrafficSplitting;
using eExNLML.IO;
using eExNLML.Extensibility;
using eExNetworkLibrary.Monitoring;
using eExNLML.DefaultControllers;

namespace eExNLML.DefaultDefinitions
{
    public class TrafficSplitterControlDefinition : HandlerDefinition
    {
        public TrafficSplitterControlDefinition()
        {
            Name = "Traffic Splitter";
            Description = "This traffic handler represents a traffic splitter which clones all incoming frames to a clone port which can be used for analysis.";
            Author = "Emanuel Jöbstl";
            WebLink = "http://www.eex-dev.net";
            PluginType = PluginTypes.TrafficHandler;
            PluginKey = "eex_splitter";
        }

        public override IHandlerController Create(IEnvironment env)
        {
            return new TrafficSplitterController(this, env);
        }
    }
}
