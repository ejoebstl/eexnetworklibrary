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
using eExNetworkLibrary.Routing.RIP;
using eExNetworkLibrary;

namespace eExNLML.IO.HandlerConfigurationWriters
{
    class RIPRouterConfigurationWriter: HandlerConfigurationWriter
    {
        private RIPRouter thHandler;

        public RIPRouterConfigurationWriter(TrafficHandler thToSave)
            : base(thToSave)
        {
            thHandler = (RIPRouter)thToSave;
        }

        protected override void AddConfiguration(List<NameValueItem> lNameValueItems, IEnvironment eEnviornment)
        {
            lNameValueItems.AddRange(ConvertToNameValueItems("ripPort", thHandler.RIPPort));
            lNameValueItems.AddRange(ConvertToNameValueItems("rip2Address", thHandler.RIPv2Address));
            lNameValueItems.AddRange(ConvertToNameValueItems("updatePeriod", thHandler.UpdatePeriod));
            lNameValueItems.AddRange(ConvertToNameValueItems("version", thHandler.Version));
            lNameValueItems.AddRange(ConvertToNameValueItems("redistStatic", thHandler.RedistributeStatic));
            lNameValueItems.AddRange(ConvertToNameValueItems("holdDownTimer", thHandler.HoldDownTimer));
        }
    }
}

