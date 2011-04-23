using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.Simulation;
using eExNLML.Extensibility;
using eExNLML.IO;
using eExNLML.IO.HandlerConfigurationLoaders;
using eExNLML.IO.HandlerConfigurationWriters;
using eExNetworkLibrary;
using eExNLML.DefaultControllers;

namespace eExNLML.DefaultDefinitions
{
    public class HTTPStreamModifierControlDefinition : HandlerDefinition
    {
        public HTTPStreamModifierControlDefinition()
        {
            Name = "HTTP Stream Modifier";
            Description = "This traffic handler is capable of changing HTTP content on the fly.";
            Author = "Emanuel Jöbstl";
            WebLink = "http://www.eex-dev.net";
            PluginType = PluginTypes.TrafficHandler;
            PluginKey = "eex_tcp_http_mod";
        }

        public override IHandlerController Create(IEnvironment env)
        {
            return new HTTPStreamModifierController(this, env);
        }
    }
}
