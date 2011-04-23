using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.CodeLab;
using eExNLML.Extensibility;
using eExNLML.IO;
using eExNetworkLibrary;

namespace eExNLML.DefaultControllers
{
    public class CodeLabController : HandlerController
    {
        public CodeLabController(IHandlerDefinition hbDefinition, IEnvironment env)
            : base(hbDefinition, env, null)
        { }

        protected override eExNetworkLibrary.TrafficHandler Create(object param)
        {
            return new DynamicFunctionHandler();
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
            return CreateDefaultPorts(h, true, true, false, true, false);
        }
    }
}
