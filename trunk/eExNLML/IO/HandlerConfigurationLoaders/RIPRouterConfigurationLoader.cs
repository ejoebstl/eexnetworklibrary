using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.Routing.RIP;
using eExNetworkLibrary;

namespace eExNLML.IO.HandlerConfigurationLoaders
{
    class RIPRouterConfigurationLoader : HandlerConfigurationLoader
    {
        private RIPRouter thHandler;

        public RIPRouterConfigurationLoader(TrafficHandler thHandler)
            : base(thHandler)
        {
            this.thHandler = (RIPRouter)thHandler;
        }
        protected override void ParseConfiguration(Dictionary<string, NameValueItem[]> strNameValues, IEnvironment eEnviornment)
        {
            thHandler.RIPPort = ConvertToInt(strNameValues["ripPort"])[0];
            thHandler.UpdatePeriod = ConvertToInt(strNameValues["updatePeriod"])[0];
            thHandler.Version = ConvertToInt(strNameValues["version"])[0];
            thHandler.HoldDownTimer = ConvertToInt(strNameValues["holdDownTimer"])[0];
            thHandler.RIPv2Address = ConvertToIPAddress(strNameValues["rip2Address"])[0];
            thHandler.RedistributeStatic = ConvertToBools(strNameValues["redistStatic"])[0];
        }
    }
}
