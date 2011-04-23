using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.Attacks.MITM;
using eExNetworkLibrary.Attacks.Spoofing;
using eExNLML.Extensibility;
using eExNLML.IO;
using eExNLML.IO.HandlerConfigurationLoaders;
using eExNLML.IO.HandlerConfigurationWriters;
using eExNetworkLibrary;
using eExNLML.DefaultControllers;

namespace eExNLML.DefaultDefinitions
{
    public class APRAttackControlDefinition : HandlerDefinition
    {
        public APRAttackControlDefinition() : base()
        {
            Name = "ARP spoofer";
            Description = "This traffic handler is capable of initiating an MITM via APR.";
            Author = "Emanuel Jöbstl";
            WebLink = "http://www.eex-dev.net";
            PluginType = PluginTypes.TrafficHandler;
            PluginKey = "eex_apr_attack";
        }

        public override IHandlerController Create(IEnvironment env)
        {
            return new APRAttackController(this, env);
        }
    }
}
