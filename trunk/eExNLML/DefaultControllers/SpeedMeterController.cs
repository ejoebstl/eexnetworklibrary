using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.Monitoring;
using eExNLML.IO;
using eExNetworkLibrary;
using eExNLML.Extensibility;

namespace eExNLML.DefaultControllers
{
    public class SpeedMeterController : HandlerController
    {
        public SpeedMeterController(IHandlerDefinition hbDefinition, IEnvironment env)
            : base(hbDefinition, env, null)
        { }

        protected override TrafficHandler Create(object param)
        {
            return new SpeedMeter();
        }

        protected override HandlerConfigurationLoader CreateConfigurationLoader(TrafficHandler h, object param)
        {
            return null;
        }

        protected override HandlerConfigurationWriter CreateConfigurationWriter(TrafficHandler h, object param)
        {
            return null;
        }

        protected override TrafficHandlerPort[] CreateTrafficHandlerPorts(TrafficHandler h, object param)
        {
            return CreateDefaultPorts(h, true, true, false, false, false);
        }
    }
}