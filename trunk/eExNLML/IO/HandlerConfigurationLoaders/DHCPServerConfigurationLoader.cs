using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.DHCP;
using eExNetworkLibrary;
using eExNLML.Extensibility;

namespace eExNLML.IO.HandlerConfigurationLoaders
{
    class DHCPServerConfigurationLoader : HandlerConfigurationLoader
    {
        private DHCPServer thHandler;

        public DHCPServerConfigurationLoader(TrafficHandler thHandler)
            : base(thHandler)
        {
            this.thHandler = (DHCPServer)thHandler;
        }
        protected override void ParseConfiguration(Dictionary<string, NameValueItem[]> strNameValues, IEnvironment eEnviornment)
        {
            if (strNameValues.ContainsKey("gateway"))
            {
                thHandler.GatewayAddress = ConvertToIPAddress(strNameValues["gateway"])[0];
            }
            if (strNameValues.ContainsKey("dns"))
            {
                thHandler.DNSAddress = ConvertToIPAddress(strNameValues["dns"])[0];
            }

            thHandler.DHCPInPort = ConvertToInt(strNameValues["inPort"])[0];
            thHandler.DHCPOutPort = ConvertToInt(strNameValues["outPort"])[0];
            thHandler.LeaseDuration = ConvertToInt(strNameValues["leaseDuration"])[0];

            foreach (NameValueItem nviPool in strNameValues["DHCPPool"])
            {
                foreach (NameValueItem nvi in nviPool.GetChildsByName("DHCPItem"))
                {
                    DHCPPoolItem dhItem = new DHCPPoolItem(ConvertToIPAddress(nvi.GetChildsByName("Address"))[0],
                        ConvertToSubnetmask(nvi.GetChildsByName("Netmask"))[0],
                        ConvertToIPAddress(nvi.GetChildsByName("Gateway"))[0],
                        ConvertToIPAddress(nvi.GetChildsByName("DNSServer"))[0]);

                    thHandler.AddToPool(dhItem);
                }
            }
        }
    }
}

