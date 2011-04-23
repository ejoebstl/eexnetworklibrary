using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary;
using eExNetworkLibrary.DHCP;

namespace eExNLML.IO.HandlerConfigurationWriters
{
    class DHCPServerConfigurationWriter: HandlerConfigurationWriter
    {
        private DHCPServer thHandler;

        public DHCPServerConfigurationWriter(TrafficHandler thToSave) : base(thToSave)
        {
            thHandler = (DHCPServer)thToSave;
        }

        protected override void AddConfiguration(List<NameValueItem> lNameValueItems, IEnvironment eEnviornment)
        {
            if (thHandler.GatewayAddress != null)
            {
                lNameValueItems.AddRange(ConvertToNameValueItems("gateway", thHandler.GatewayAddress));
            }
            if (thHandler.DNSAddress != null)
            {
                lNameValueItems.AddRange(ConvertToNameValueItems("dns", thHandler.DNSAddress));
            }
            lNameValueItems.AddRange(ConvertToNameValueItems("inPort", thHandler.DHCPInPort));
            lNameValueItems.AddRange(ConvertToNameValueItems("outPort", thHandler.DHCPOutPort));
            lNameValueItems.AddRange(ConvertToNameValueItems("leaseDuration", thHandler.LeaseDuration));

            foreach (DHCPPool dhPool in thHandler.DHCPPools)
            {
                NameValueItem nviPool = new NameValueItem("DHCPPool", "");
                foreach (DHCPPoolItem dhItem in dhPool.Pool)
                {
                    NameValueItem nvi = new NameValueItem("DHCPItem", "");
                    nvi.AddChildRange(ConvertToNameValueItems("Address", dhItem.Address));
                    nvi.AddChildRange(ConvertToNameValueItems("Gateway", dhItem.Gateway));
                    nvi.AddChildRange(ConvertToNameValueItems("DNSServer", dhItem.DNSServer));
                    nvi.AddChildRange(ConvertToNameValueItems("Netmask", dhItem.Netmask));
                    nviPool.AddChildItem(nvi);
                }
                lNameValueItems.Add(nviPool);
            }
        }
    }
}
