using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.Monitoring;
using eExNLML.IO;
using eExNetworkLibrary;
using eExNLML.IO.HandlerConfigurationWriters;
using eExNLML.IO.HandlerConfigurationLoaders;
using eExNLML.Extensibility;

namespace eExNLML.DefaultControllers
{
    public class TrafficDumperController : HandlerController
    {
        public TrafficDumperController(IHandlerDefinition hbDefinition, IEnvironment env) : base(hbDefinition, env, null)
        { }

        protected override TrafficHandler Create(object param)
        {
            return new LibPcapDumper();
        }

        protected override HandlerConfigurationLoader CreateConfigurationLoader(TrafficHandler h, object param)
        {
            return new DumperConfigurationLoader(h);
        }

        protected override HandlerConfigurationWriter CreateConfigurationWriter(TrafficHandler h, object param)
        {
            return new DumperConfigurationWriter(h);
        }

        protected override TrafficHandlerPort[] CreateTrafficHandlerPorts(TrafficHandler h, object param)
        {
            return CreateDefaultPorts(h, true, false, false, false, false);
        }
    }
}
