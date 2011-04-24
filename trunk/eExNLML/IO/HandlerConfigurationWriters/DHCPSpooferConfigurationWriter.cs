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
using eExNetworkLibrary.Attacks.MITM;
using eExNetworkLibrary;
using eExNetworkLibrary.Attacks.Spoofing;
using eExNetworkLibrary.DHCP;

namespace eExNLML.IO.HandlerConfigurationWriters
{
    class DHCPSpooferConfigurationWriter : DHCPServerConfigurationWriter
    {
        DHCPSpoofer thHandler;

        public DHCPSpooferConfigurationWriter(TrafficHandler h)
            : base(h)
        {
            thHandler = (DHCPSpoofer)h;
        }

        protected override void AddConfiguration(List<NameValueItem> lNameValueItems, IEnvironment eEnviornment)
        {
            lNameValueItems.AddRange(ConvertToNameValueItems("stealAddresses", thHandler.StealAdresses));
            lNameValueItems.AddRange(ConvertToNameValueItems("requestIntervael", thHandler.RequestInterval));
            lNameValueItems.AddRange(ConvertToNameValueItems("redirectGateway", thHandler.RedirectGateway));
            lNameValueItems.AddRange(ConvertToNameValueItems("redirectDNS", thHandler.RedirectDNSServer));
            lNameValueItems.AddRange(ConvertToNameValueItems("hostnameToSpoof", thHandler.HostenameToSpoof));
            lNameValueItems.AddRange(ConvertToNameValueItems("answerArpRequests", thHandler.AnswerARPRequests));

            base.AddConfiguration(lNameValueItems, eEnviornment);
        }
    }
}
