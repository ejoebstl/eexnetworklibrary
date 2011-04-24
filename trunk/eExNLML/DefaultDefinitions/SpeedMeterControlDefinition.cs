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
    public class SpeedMeterControlDefinition : HandlerDefinition
    {
        public SpeedMeterControlDefinition()
        {
            Name = "Speed Meter";
            Description = "This traffic handler is capable of measuring the throughput datarate. It counts all data as it is on the medium (except ethernet preamble and checksum), not only layer 3 or 4 data.";
            Author = "Emanuel Jöbstl";
            WebLink = "http://www.eex-dev.net";
            PluginType = PluginTypes.TrafficHandler;
            PluginKey = "eex_speed_meter";
        }

        public override IHandlerController Create(IEnvironment env)
        {
            return new SpeedMeterController(this, env);
        }
    }
}