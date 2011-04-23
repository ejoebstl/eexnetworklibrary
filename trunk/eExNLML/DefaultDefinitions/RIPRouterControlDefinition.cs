using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.Routing.RIP;
using eExNLML.IO;
using eExNetworkLibrary;
using eExNLML.Extensibility;
using eExNLML.DefaultControllers;

namespace eExNLML.DefaultDefinitions
{
    public class RIPRouterControlDefinition : HandlerDefinition
    {
        public RIPRouterControlDefinition()
        {
            Name = "RIP Router";
            Description = "This traffic handler represents a simple RIP routing process.";
            Author = "Emanuel Jöbstl";
            WebLink = "http://www.eex-dev.net";
            PluginType = PluginTypes.TrafficHandler;
            PluginKey = "eex_rip_router";
        }

        public override IHandlerController Create(IEnvironment env)
        {
            return new RIPRouterController(this, env);
        }
    }
}
