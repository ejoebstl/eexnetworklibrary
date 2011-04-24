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
using eExNLML.IO;
using eExNLML.Extensibility;
using eExNLML.DefaultControllers;

namespace eExNLML.DefaultDefinitions
{
    public class DirectInterfaceIOControlDefinition : HandlerDefinition
    {
        public DirectInterfaceIOControlDefinition()
        {
            Name = "Direct Interface I/O";
            Description = "This traffic handler represents a direct link to an IP interface.";
            Author = "Emanuel Jöbstl";
            WebLink = "http://www.eex-dev.net";
            PluginType = PluginTypes.TrafficHandler;
            PluginKey = "eex_direct_int_io";
        }

        public override IHandlerController Create(IEnvironment env)
        {
            return new DirectInterfaceIOController(this, env);
        }
    }
}
