using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.Attacks.MITM;
using eExNetworkLibrary;
using eExNetworkLibrary.Attacks.Spoofing;
using eExNetworkLibrary.DHCP;

namespace eExNLML.IO.HandlerConfigurationWriters
{
    class DHCPSpooferConfigurationWriter : DHCPServerConfigurationWriter
    {
        DHCPSpoofer thHandler;

        public DHCPSpooferConfigurationWriter(TrafficHandler h)
            : base(h)
        {
            thHandler = (DHCPSpoofer)h;
        }

        protected override void AddConfiguration(List<NameValueItem> lNameValueItems, IEnvironment eEnviornment)
        {
            lNameValueItems.AddRange(ConvertToNameValueItems("stealAddresses", thHandler.StealAdresses));
            lNameValueItems.AddRange(ConvertToNameValueItems("requestIntervael", thHandler.RequestInterval));
            lNameValueItems.AddRange(ConvertToNameValueItems("redirectGateway", thHandler.RedirectGateway));
            lNameValueItems.AddRange(ConvertToNameValueItems("redirectDNS", thHandler.RedirectDNSServer));
            lNameValueItems.AddRange(ConvertToNameValueItems("hostnameToSpoof", thHandler.HostenameToSpoof));
            lNameValueItems.AddRange(ConvertToNameValueItems("answerArpRequests", thHandler.AnswerARPRequests));

            base.AddConfiguration(lNameValueItems, eEnviornment);
        }
    }
}
