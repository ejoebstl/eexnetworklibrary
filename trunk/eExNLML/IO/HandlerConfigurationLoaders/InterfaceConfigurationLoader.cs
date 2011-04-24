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
using System.Net;
using eExNetworkLibrary.ARP;

namespace eExNLML.IO.HandlerConfigurationLoaders
{
    class EthernetInterfaceConfigurationLoader : HandlerConfigurationLoader
    {
        private EthernetInterface thHandler;

        public EthernetInterfaceConfigurationLoader(TrafficHandler thHandler)
            : base(thHandler)
        {
            this.thHandler = (EthernetInterface)thHandler;
        }
        protected override void ParseConfiguration(Dictionary<string, NameValueItem[]> strNameValues, IEnvironment eEnviornment)
        {
            if (strNameValues.ContainsKey("autoAnswerARP"))
                thHandler.AutoAnswerARPRequests = ConvertToBools(strNameValues["autoAnswerARP"])[0];

            if (strNameValues.ContainsKey("excludeOwnTraffic"))
                thHandler.ExcludeOwnTraffic = ConvertToBools(strNameValues["excludeOwnTraffic"])[0];

            if (strNameValues.ContainsKey("excludeLocalHostTraffic"))
                thHandler.ExcludeLocalHostTraffic = ConvertToBools(strNameValues["excludeLocalHostTraffic"])[0];

            if (strNameValues.ContainsKey("filterExpression"))
                thHandler.FilterExpression = ConvertToString(strNameValues["filterExpression"])[0];

            if (strNameValues.ContainsKey("addressResolutionMethod"))
                thHandler.AddressResolutionMethod = (AddressResolution)ConvertToInt(strNameValues["addressResolutionMethod"])[0];


            foreach (IPAddress ipa in thHandler.IpAddresses)
            {
                thHandler.RemoveAddress(ipa);
            }

            if (strNameValues.ContainsKey("address"))
            {
                foreach (NameValueItem nvi in strNameValues["address"])
                {
                    IPAddress[] aripa = ConvertToIPAddress(nvi.GetChildsByName("ipAddress"));
                    Subnetmask[] sm = ConvertToSubnetmask(nvi.GetChildsByName("mask"));

                    if (aripa.Length != sm.Length)
                    {
                        throw new ArgumentException("Invalid Data");
                    }

                    for (int iC1 = 0; iC1 < aripa.Length; iC1++)
                    {
                        thHandler.AddAddress(aripa[iC1], sm[iC1]);
                    }
                }
            }

            if (strNameValues.ContainsKey("staticArpEntry"))
            {
                foreach (NameValueItem nvi in strNameValues["staticArpEntry"])
                {
                    MACAddress[] armac = ConvertToMACAddress(nvi.GetChildsByName("macAddress"));
                    IPAddress[] aripa = ConvertToIPAddress(nvi.GetChildsByName("ipAddress"));

                    if (aripa.Length != armac.Length)
                    {
                        throw new ArgumentException("Invalid Data");
                    }

                    for (int iC1 = 0; iC1 < aripa.Length; iC1++)
                    {
                        thHandler.ARPTable.AddHost(new ARPHostEntry(armac[iC1], aripa[iC1], true, new DateTime(0)));
                    }
                }
            }
        }
    }
}
