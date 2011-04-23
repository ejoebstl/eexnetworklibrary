using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.Monitoring;
using eExNetworkLibrary;
using eExNLML.Extensibility;

namespace eExNLML.IO.HandlerConfigurationLoaders
{
    class DumperConfigurationLoader : HandlerConfigurationLoader
    {
        private LibPcapDumper thHandler;

        public DumperConfigurationLoader(TrafficHandler thHandler)
            : base(thHandler)
        {
            this.thHandler = (LibPcapDumper)thHandler;
        }
        protected override void ParseConfiguration(Dictionary<string, NameValueItem[]> strNameValues, IEnvironment eEnviornment)
        {
            if (ConvertToBools(strNameValues["dumping"])[0])
            {
                thHandler.StartLogging(ConvertToString(strNameValues["fileName"])[0], true);
            }
        }
    }
}