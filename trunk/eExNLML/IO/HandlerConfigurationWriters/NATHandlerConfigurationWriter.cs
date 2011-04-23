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
