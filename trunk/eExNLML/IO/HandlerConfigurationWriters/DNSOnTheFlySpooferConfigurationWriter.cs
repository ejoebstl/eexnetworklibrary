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
using eExNetworkLibrary;

namespace eExNLML.IO.HandlerConfigurationWriters
{
    class DNSOnTheFlySpooferConfigurationWriter : HandlerConfigurationWriter
    {
        private DNSOnTheFlySpoofer thHandler;

        public DNSOnTheFlySpooferConfigurationWriter(TrafficHandler thToSave)
            : base(thToSave)
        {
            thHandler = (DNSOnTheFlySpoofer)thToSave;
        }

        protected override void AddConfiguration(List<NameValueItem> lNameValueItems, IEnvironment eEnviornment)
        {
            lNameValueItems.AddRange(ConvertToNameValueItems("dnsPort", thHandler.DNSPort));

            foreach (DNSSpooferEntry dnsSpoof in thHandler.GetDNSSpooferEntries())
            {
                NameValueItem nviSpoofDefinitions = new NameValueItem("spoofDefinition", "");
                nviSpoofDefinitions.AddChildRange(ConvertToNameValueItems("address", dnsSpoof.Address));
                nviSpoofDefinitions.AddChildRange(ConvertToNameValueItems("domainName", dnsSpoof.Name));
                lNameValueItems.Add(nviSpoofDefinitions);
            }

        }
    }
}
