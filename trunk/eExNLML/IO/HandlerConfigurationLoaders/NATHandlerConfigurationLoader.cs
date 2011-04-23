﻿using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.Routing;
using eExNetworkLibrary;

namespace eExNLML.IO.HandlerConfigurationLoaders
{
    class NATHandlerConfigurationLoader : HandlerConfigurationLoader
    {
        private NetworkAddressTranslationHandler thHandler;

        public NATHandlerConfigurationLoader(TrafficHandler thHandler)
            : base(thHandler)
        {
            this.thHandler = (NetworkAddressTranslationHandler)thHandler;
        }
        protected override void ParseConfiguration(Dictionary<string, NameValueItem[]> strNameValues, IEnvironment eEnviornment)
        {
            thHandler.DropNonNATFrames = ConvertToBools(strNameValues["dropNonNATFrames"])[0];
            thHandler.NATTimer = ConvertToInt(strNameValues["NATTimer"])[0];
            thHandler.PortRangeStart = ConvertToInt(strNameValues["portRangeStart"])[0];
            thHandler.PortRangeEnd = ConvertToInt(strNameValues["portRangeEnd"])[0];

            foreach (NameValueItem nvi in strNameValues["externalRangeItem"])
            {
                NATAddressRange narEntry = new NATAddressRange(ConvertToSubnetmask(nvi.GetChildsByName("subnetMask"))[0] , ConvertToIPAddress(new NameValueItem[] { nvi })[0]);
                thHandler.AddToExternalRange(narEntry);
            }

            foreach (NameValueItem nvi in strNameValues["internalRangeItem"])
            {
                NATAddressRange narEntry = new NATAddressRange(ConvertToSubnetmask(nvi.GetChildsByName("subnetMask"))[0], ConvertToIPAddress(new NameValueItem[] { nvi })[0]);
                thHandler.AddToInternalRange(narEntry);
            }
        }
    }
}
