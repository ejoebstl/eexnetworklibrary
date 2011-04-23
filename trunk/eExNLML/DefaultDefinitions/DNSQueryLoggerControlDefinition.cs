using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.DNS;
using eExNetworkLibrary.Monitoring;
using eExNLML.IO;
using eExNetworkLibrary;
using eExNLML.Extensibility;
using eExNLML.DefaultControllers;

namespace eExNLML.DefaultDefinitions
{
    public class DNSQueryLoggerControlDefinition : HandlerDefinition
    {
        public DNSQueryLoggerControlDefinition()
        {
            Name = "DNS Query Logger";
            Description = "This traffic analyzer creates a small log of all catched DNS queries and their responses.";
            Author = "Emanuel Jöbstl";
            WebLink = "http://www.eex-dev.net";
            PluginType = PluginTypes.TrafficHandler;
            PluginKey = "eex_dns_query_log";
        }

        public override IHandlerController Create(IEnvironment env)
        {
            return new DNSQueryLoggerController(this, env);
        }
    }
}
