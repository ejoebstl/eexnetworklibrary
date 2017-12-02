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
using eExNetworkLibrary;
using eExNLML.Extensibility;

namespace eExNLML.IO.HandlerConfigurationLoaders
{
    class DumperConfigurationLoader : HandlerConfigurationLoader
    {
        private LibPcapDumper thHandler;

        public DumperConfigurationLoader(TrafficHandler thHandler)
            : base(thHandler)
        {
            this.thHandler = (LibPcapDumper)thHandler;
        }
        protected override void ParseConfiguration(Dictionary<string, NameValueItem[]> strNameValues, IEnvironment eEnviornment)
        {
            if (ConvertToBools(strNameValues["dumping"])[0])
            {
                thHandler.StartLogging(ConvertToString(strNameValues["fileName"])[0], true);
            }
        }
    }
}