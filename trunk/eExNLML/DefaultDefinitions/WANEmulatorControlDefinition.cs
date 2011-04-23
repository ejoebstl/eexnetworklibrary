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
    public class WANEmulatorControlDefinition : HandlerDefinition
    {
        public WANEmulatorControlDefinition()
        {
            Name = "WAN Emulator";
            Description = "This traffic handler is capable of emulating WAN phenomena, like speed limitations and traffic jitters.";
            Author = "Emanuel Jöbstl";
            WebLink = "http://www.eex-dev.net";
            PluginType = PluginTypes.TrafficHandler;
            PluginKey = "eex_wan_emu";
        }

        public override IHandlerController Create(IEnvironment env)
        {
            return new WANEmulatorController(this, env);
        }
    }
}
