using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.Simulation;
using eExNLML.Extensibility;
using eExNLML.IO;
using eExNLML.IO.HandlerConfigurationLoaders;
using eExNLML.IO.HandlerConfigurationWriters;
using eExNetworkLibrary;
using eExNetworkLibrary.TrafficModifiers;
using eExNetworkLibrary.TrafficModifiers.StreamModification;

namespace eExNLML.DefaultControllers
{
    public class HTTPStreamModifierController : HandlerController
    {
        public HTTPStreamModifierController(IHandlerDefinition hbDefinition, IEnvironment env)
            : base(hbDefinition, env, null)
        { }

        protected override eExNetworkLibrary.TrafficHandler Create(object param)
        {
            return new HTTPStreamModifier();
        }

        protected override HandlerConfigurationLoader CreateConfigurationLoader(TrafficHandler h, object param)
        {
            return new HTTPModifierConfigurationLoader((HTTPStreamModifier)h);
        }

        protected override HandlerConfigurationWriter CreateConfigurationWriter(TrafficHandler h, object param)
        {
            return new HTTPModifierConfigurationWriter((HTTPStreamModifier)h);
        }

        protected override TrafficHandlerPort[] CreateTrafficHandlerPorts(TrafficHandler h, object param)
        {
            return CreateDefaultPorts(h, true, true, false, false, false);
        }
    }
}
