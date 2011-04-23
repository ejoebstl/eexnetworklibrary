using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.Routing;
using System.Net;
using eExNetworkLibrary;

namespace eExNLML.IO.HandlerConfigurationLoaders
{
    class RouterConfigurationLoader : HandlerConfigurationLoader
    {
        private Router thHandler;

        public RouterConfigurationLoader(TrafficHandler thHandler)
            : base(thHandler)
        {
            this.thHandler = (Router)thHandler;
        }
        protected override void ParseConfiguration(Dictionary<string, NameValueItem[]> strNameValues, IEnvironment eEnviornment)
        {
            if (strNameValues.ContainsKey("routingEntry"))
            {
                foreach (NameValueItem nvi in strNameValues["routingEntry"])
                {
                    IPAddress[] ipaDestination = ConvertToIPAddress(nvi.GetChildsByName("destination"));
                    IPAddress[] ipaNextHop = ConvertToIPAddress(nvi.GetChildsByName("nexthop"));
                    int[] iMetric = ConvertToInt(nvi.GetChildsByName("metric"));
                    Subnetmask[] smMasks = ConvertToSubnetmask(nvi.GetChildsByName("mask"));

                    if (ipaDestination.Length != ipaNextHop.Length || ipaDestination.Length != iMetric.Length || ipaDestination.Length != smMasks.Length)
                    {
                        throw new ArgumentException("Invalid data.");
                    }

                    for (int iC1 = 0; iC1 < ipaDestination.Length; iC1++)
                    {
                        thHandler.RoutingTable.AddRoute(new RoutingEntry(ipaDestination[iC1], ipaNextHop[iC1], iMetric[iC1], smMasks[iC1], RoutingEntryOwner.UserStatic));
                    }
                }
            }
        }
    }
}
