using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.TrafficSplitting;
using eExNLML.Extensibility;
using eExNLML.IO;

namespace eExNLML.SubPlugInDefinitions
{
    public class PortRuleDefinition : TrafficSplitterRuleDefinition
    {
        public PortRuleDefinition() : base()
        {
            Name = "Port Rule";
            PluginType = PluginTypes.SplitterRule;
            Description = "This rule is capable of splitting traffic according to TCP or UCP ports.";
            Author = "Emanuel Jöbstl";
            WebLink = "http://www.eex-dev.net";
            PluginKey = "eex_split_rule_port";
        }

        public override TrafficSplitterRule Create()
        {
            return new PortRule();
        }

        public override string ToString()
        {
            return Name;
        }

        public override TrafficSplitterRule Create(NameValueItem nviConfigurationRoot)
        {
            PortRule prRule = new PortRule();

            prRule.Action = ConvertToAction(nviConfigurationRoot["action"][0]);

            prRule.DestinationPort = Int32.Parse(nviConfigurationRoot["destinationPort"][0].Value);
            prRule.Port = Int32.Parse(nviConfigurationRoot["port"][0].Value);
            prRule.SourcePort = Int32.Parse(nviConfigurationRoot["sourcePort"][0].Value);
            string strProtocol = nviConfigurationRoot["protocol"][0].Value;

            if (strProtocol == TransportProtocol.Any.ToString())
            {
                prRule.Protocol = TransportProtocol.Any;
            }
            else if (strProtocol == TransportProtocol.TCP.ToString())
            {
                prRule.Protocol = TransportProtocol.TCP;
            }
            else
            {
                prRule.Protocol = TransportProtocol.UDP;
            }

            return prRule;
        }

        public override NameValueItem[] GetConfiguration(TrafficSplitterRule tsrRule)
        {
            PortRule prRule = (PortRule)tsrRule;

            List<NameValueItem> lNvi = new List<NameValueItem>();

            lNvi.Add(ConvertActionToNameValueItem(prRule.Action));

            lNvi.Add(new NameValueItem("port", prRule.Port.ToString()));
            lNvi.Add(new NameValueItem("destinationPort", prRule.DestinationPort.ToString()));
            lNvi.Add(new NameValueItem("sourcePort", prRule.SourcePort.ToString()));
            lNvi.Add(new NameValueItem("protocol", prRule.Protocol.ToString()));

            return lNvi.ToArray();
        }
    }
}
