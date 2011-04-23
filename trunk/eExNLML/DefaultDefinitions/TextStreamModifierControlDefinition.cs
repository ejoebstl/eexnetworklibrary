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
    public class TextStreamModifierControlDefinition : HandlerDefinition
    {
        public TextStreamModifierControlDefinition()
        {
            Name = "Text Stream Modifier";
            Description = "This traffic handler is capable of changing ASCII streams on the fly.";
            Author = "Emanuel Jöbstl";
            WebLink = "http://www.eex-dev.net";
            PluginType = PluginTypes.TrafficHandler;
            PluginKey = "eex_tcp_text_mod";
        }

        public override IHandlerController Create(IEnvironment env)
        {
            return new TextStreamModifierController(this, env);
        }
    }
}
