using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary;
using eExNetworkLibrary.Routing;

namespace eExNLML.IO.HandlerConfigurationWriters
{
    class RouterConfigurationWriter : HandlerConfigurationWriter
    {
        private Router thHandler;

        public RouterConfigurationWriter(TrafficHandler thToSave) : base(thToSave)
        {
            thHandler = (Router)thToSave;
        }

        protected override void AddConfiguration(List<NameValueItem> lNameValueItems, IEnvironment eEnviornment)
        {
            foreach(RoutingEntry re in thHandler.RoutingTable.GetRoutes())
            {
                if(re.Owner == RoutingEntryOwner.UserStatic)
                {
                    NameValueItem nviRoutingEntry = new NameValueItem("routingEntry","");
                    nviRoutingEntry.AddChildRange(ConvertToNameValueItems("destination", re.Destination));
                    nviRoutingEntry.AddChildRange(ConvertToNameValueItems("mask", re.Subnetmask));
                    nviRoutingEntry.AddChildRange(ConvertToNameValueItems("metric", re.Metric));
                    nviRoutingEntry.AddChildRange(ConvertToNameValueItems("nexthop", re.NextHop));
                    lNameValueItems.Add(nviRoutingEntry);
                }
            }
        }
    }
}
