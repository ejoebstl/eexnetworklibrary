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
using eExNetworkLibrary.Attacks.Modification;
using eExNLML.IO.HandlerConfigurationLoaders;
using eExNLML.IO;
using eExNLML.IO.HandlerConfigurationWriters;
using eExNLML.Extensibility;
using eExNetworkLibrary;
using eExNLML.DefaultControllers;

namespace eExNLML.DefaultDefinitions
{
    public class DNSOTFSpooferControlDefinition : HandlerDefinition
    {
        public DNSOTFSpooferControlDefinition()
        {
            Name = "DNS On The Fly Spoofer";
            Description = "This traffic modifier is capable of changing DNS replies on the fly.";
            Author = "Emanuel Jöbstl";
            WebLink = "http://www.eex-dev.net";
            PluginType = PluginTypes.TrafficHandler;
            PluginKey = "eex_dns_otf_spoofer";
        }

        public override IHandlerController Create(IEnvironment env)
        {
            return new DNSOTFSpooferController(this, env);
        }
    }
}
