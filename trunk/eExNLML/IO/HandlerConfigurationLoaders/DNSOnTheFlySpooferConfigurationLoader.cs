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
using System.Net;
using eExNLML.Extensibility;

namespace eExNLML.IO.HandlerConfigurationLoaders
{
    class DNSOnTheFlySpooferConfigurationLoader : HandlerConfigurationLoader
    {
        private DNSOnTheFlySpoofer thHandler;

        public DNSOnTheFlySpooferConfigurationLoader(TrafficHandler thHandler)
            : base(thHandler)
        {
            this.thHandler = (DNSOnTheFlySpoofer)thHandler;
        }
        protected override void ParseConfiguration(Dictionary<string, NameValueItem[]> strNameValues, IEnvironment eEnviornment)
        {
            thHandler.DNSPort = ConvertToInt(strNameValues["dnsPort"])[0];

            if (strNameValues.ContainsKey("spoofDefinition"))
            {
                foreach (NameValueItem nvi in strNameValues["spoofDefinition"])
                {
                    IPAddress[] ipaData = ConvertToIPAddress(nvi.GetChildsByName("address"));
                    string[] strName = ConvertToString(nvi.GetChildsByName("domainName"));

                    if (ipaData.Length != strName.Length)
                    {
                        throw new ArgumentException("Invaild data");
                    }

                    for (int iC1 = 0; iC1 < ipaData.Length; iC1++)
                    {
                        thHandler.AddDNSSpooferEntry(new DNSSpooferEntry(strName[iC1], ipaData[iC1]));
                    }
                }
            }
        }
    }
}
