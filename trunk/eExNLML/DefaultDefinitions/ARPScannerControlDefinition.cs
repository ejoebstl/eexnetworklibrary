using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.Attacks.Scanning;
using eExNLML.Extensibility;
using eExNLML.IO;
using eExNLML.DefaultControllers;

namespace eExNLML.DefaultDefinitions
{
    public class ARPScannerControlDefinition : HandlerDefinition
    {
        public ARPScannerControlDefinition()
        {
            Name = "ARP Scanner";
            Description = "This traffic handler implements ARP-Scanning for the local subnet.";
            Author = "Emanuel Jöbstl";
            WebLink = "http://www.eex-dev.net";
            PluginType = PluginTypes.TrafficHandler;
            PluginKey = "eex_arp_scanner";
        }

        public override IHandlerController Create(IEnvironment env)
        {
            return new ARPScannerController(this, env);
        }
    }
}
