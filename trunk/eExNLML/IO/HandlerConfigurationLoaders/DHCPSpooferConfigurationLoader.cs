﻿using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.Attacks.MITM;
using eExNetworkLibrary;
using eExNetworkLibrary.Attacks.Spoofing;
using eExNLML.Extensibility;
using eExNetworkLibrary.DHCP;

namespace eExNLML.IO.HandlerConfigurationLoaders
{
    class DHCPSpooferConfigurationLoader : DHCPServerConfigurationLoader
    {
        private DHCPSpoofer thHandler;

        public DHCPSpooferConfigurationLoader(TrafficHandler h)
            : base(h)
        {
            thHandler = (DHCPSpoofer)h;
        }

        protected override void ParseConfiguration(Dictionary<string, NameValueItem[]> strNameValues, IEnvironment eEnviornment)
        {
            thHandler.StealAdresses = ConvertToBools(strNameValues["stealAddresses"])[0];
            thHandler.RedirectDNSServer = ConvertToBools(strNameValues["redirectDNS"])[0];
            thHandler.RedirectGateway = ConvertToBools(strNameValues["redirectGateway"])[0];
            thHandler.AnswerARPRequests = ConvertToBools(strNameValues["answerArpRequests"])[0];
            thHandler.HostenameToSpoof = ConvertToString(strNameValues["hostnameToSpoof"])[0];
            thHandler.RequestInterval = ConvertToInt(strNameValues["requestInterval"])[0];

            base.ParseConfiguration(strNameValues, eEnviornment);
        }
    }
}
