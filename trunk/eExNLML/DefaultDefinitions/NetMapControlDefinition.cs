using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.Monitoring;
using eExNLML.IO;
using eExNetworkLibrary;
using eExNLML.Extensibility;
using eExNLML.DefaultControllers;

namespace eExNLML.DefaultDefinitions
{
    public class NetMapControlDefinition : HandlerDefinition
    {
        public NetMapControlDefinition()
        {
            Name = "Network Map";
            Description = "This traffic analyzer parses all incoming traffic and builds a network sketch from it.";
            Author = "Emanuel Jöbstl";
            WebLink = "http://www.eex-dev.net";
            PluginType = PluginTypes.TrafficHandler;
            PluginKey = "eex_net_map";
        }

        public override IHandlerController Create(IEnvironment env)
        {
            return new NetMapController(this, env);
        }
    }
}
