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
using eExNetworkLibrary.Routing;
using eExNetworkLibrary;

namespace eExNLML.IO.HandlerConfigurationWriters
{
    class NATHandlerConfigurationWriter : HandlerConfigurationWriter
    {
        private NetworkAddressTranslationHandler thHandler;

        public NATHandlerConfigurationWriter(TrafficHandler thToSave)
            : base(thToSave)
        {
            thHandler = (NetworkAddressTranslationHandler)thToSave;
        }

        protected override void AddConfiguration(List<NameValueItem> lNameValueItems, IEnvironment eEnviornment)
        {
            lNameValueItems.AddRange(ConvertToNameValueItems("dropNonNATFrames", thHandler.DropNonNATFrames));
            lNameValueItems.AddRange(ConvertToNameValueItems("NATTimer", thHandler.NATTimer));
            lNameValueItems.AddRange(ConvertToNameValueItems("portRangeStart", thHandler.PortRangeStart));
            lNameValueItems.AddRange(ConvertToNameValueItems("portRangeEnd", thHandler.PortRangeEnd));

            foreach (NATAddressRange narEntry in thHandler.GetExternalRange())
            {
                NameValueItem nviExternalRange = ConvertToNameValueItems("externalRangeItem", narEntry.NetworkAddress)[0];
                nviExternalRange.AddChildRange(ConvertToNameValueItems("subnetMask", narEntry.Subnetmask));
                lNameValueItems.Add(nviExternalRange);
            } 
            
            foreach (NATAddressRange narEntry in thHandler.GetInternalRange())
            {
                NameValueItem nviInternalRange = ConvertToNameValueItems("internalRangeItem", narEntry.NetworkAddress)[0];
                nviInternalRange.AddChildRange(ConvertToNameValueItems("subnetMask", narEntry.Subnetmask));
                lNameValueItems.Add(nviInternalRange);
            }
        }
    }
}
