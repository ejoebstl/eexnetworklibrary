using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.Attacks.Scanning;
using eExNLML.Extensibility;
using eExNLML.IO;

namespace eExNLML.DefaultControllers
{
    public class ARPScannerController : HandlerController
    {
        public ARPScannerController(IHandlerDefinition hbDefinition, IEnvironment env) : base(hbDefinition, env, null)
        { }

        protected override eExNetworkLibrary.TrafficHandler Create(object param)
        {
            return new ARPNetScan();
        }

        protected override HandlerConfigurationLoader CreateConfigurationLoader(eExNetworkLibrary.TrafficHandler h, object param)
        {
            return null;
        }

        protected override HandlerConfigurationWriter CreateConfigurationWriter(eExNetworkLibrary.TrafficHandler h, object param)
        {
            return null;
        }

        protected override TrafficHandlerPort[] CreateTrafficHandlerPorts(eExNetworkLibrary.TrafficHandler h, object param)
        {
            return CreateDefaultPorts(h, false, false, true, false, false);
        }
    }
}
