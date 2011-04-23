using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary;
using System.Net;
using eExNetworkLibrary.ARP;

namespace eExNLML.IO.HandlerConfigurationWriters
{
    class EthernetInterfaceConfigurationWriter : HandlerConfigurationWriter
    {
        private EthernetInterface thHandler;

        public EthernetInterfaceConfigurationWriter(TrafficHandler thToSave)
            : base(thToSave)
        {
            thHandler = (EthernetInterface)thToSave;
        }

        protected override void AddConfiguration(List<NameValueItem> lNameValueItems, IEnvironment eEnviornment)
        {
            lNameValueItems.AddRange(ConvertToNameValueItems("excludeOwnTraffic", thHandler.ExcludeOwnTraffic));
            lNameValueItems.AddRange(ConvertToNameValueItems("excludeLocalHostTraffic", thHandler.ExcludeLocalHostTraffic));
            lNameValueItems.AddRange(ConvertToNameValueItems("autoAnswerARP", thHandler.AutoAnswerARPRequests));
            lNameValueItems.AddRange(ConvertToNameValueItems("filterExpression", thHandler.FilterExpression));
            lNameValueItems.AddRange(ConvertToNameValueItems("addressResolutionMethod", (int)thHandler.AddressResolutionMethod));

            IPAddress[] ipa = thHandler.IpAddresses;
            Subnetmask[] smMasks = thHandler.Subnetmasks;

            for (int iC1 = 0; iC1 < ipa.Length; iC1++)
            {
                NameValueItem nviAddress = new NameValueItem("address", "");
                nviAddress.AddChildRange(ConvertToNameValueItems("ipAddress", ipa[iC1]));
                nviAddress.AddChildRange(ConvertToNameValueItems("mask", smMasks[iC1]));
                lNameValueItems.Add(nviAddress);
            }

            ARPHostEntry[] arHosts = thHandler.ARPTable.GetKnownHosts();

            for (int iC1 = 0; iC1 < arHosts.Length; iC1++)
            {
                if (arHosts[iC1].IsStatic)
                {
                    NameValueItem nviAddress = new NameValueItem("staticArpEntry", "");
                    nviAddress.AddChildRange(ConvertToNameValueItems("ipAddress", arHosts[iC1].IP));
                    nviAddress.AddChildRange(ConvertToNameValueItems("macAddress", arHosts[iC1].MAC));
                    lNameValueItems.Add(nviAddress);
                }
            }
        } 
    }
}
