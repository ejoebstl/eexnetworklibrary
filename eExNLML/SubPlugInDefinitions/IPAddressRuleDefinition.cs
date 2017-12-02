// This source file is part of the eEx Network Library Management Layer (NLML)
//
// Author: 	    Emanuel Jöbstl <emi@eex-dev.net>
// Weblink: 	http://network.eex-dev.net
//		        http://eex.codeplex.com
//
// Licensed under the GNU Library General Public License (LGPL) 
//
// (c) eex-dev 2007-2011

using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.TrafficSplitting;
using System.Net;
using eExNetworkLibrary;
using eExNLML.Extensibility;
using eExNLML.IO;

namespace eExNLML.SubPlugInDefinitions
{
    public class IPAddressRuleDefinition : TrafficSplitterRuleDefinition
    {
        public IPAddressRuleDefinition()
            : base()
        {
            Name = "IP Address Rule";
            PluginType = PluginTypes.SplitterRule;
            Description = "This rule is capable of splitting traffic according IP addresses.";
            Author = "Emanuel Jöbstl";
            WebLink = "http://www.eex-dev.net";
            PluginKey = "eex_split_rule_ipaddress";
        }

        public override TrafficSplitterRule Create()
        {
            return new IPAddressRule();
        }

        public override string ToString()
        {
            return Name;
        }

        public override TrafficSplitterRule Create(NameValueItem nviConfigurationRoot)
        {
            IPAddressRule ipaRule = new IPAddressRule();

            ipaRule.Action = ConvertToAction(nviConfigurationRoot.GetChildsByName("action")[0]);

            if (nviConfigurationRoot.ContainsChildItem("address"))
                ipaRule.Address = IPAddress.Parse(nviConfigurationRoot["address"][0].Value);
            if (nviConfigurationRoot.ContainsChildItem("wildcard"))
                ipaRule.Wildcard = Subnetmask.Parse(nviConfigurationRoot["wildcard"][0].Value);

            if (nviConfigurationRoot.ContainsChildItem("destinationAddress"))
                ipaRule.Destination = IPAddress.Parse(nviConfigurationRoot["destinationAddress"][0].Value);
            if (nviConfigurationRoot.ContainsChildItem("sourceAddress"))
                ipaRule.Source = IPAddress.Parse(nviConfigurationRoot["sourceAddress"][0].Value);

            if (nviConfigurationRoot.ContainsChildItem("destinationWildcard"))
                ipaRule.DestinationWildcard = Subnetmask.Parse(nviConfigurationRoot["destinationWildcard"][0].Value);
            if (nviConfigurationRoot.ContainsChildItem("sourceWildcard"))
                ipaRule.SourceWildcard = Subnetmask.Parse(nviConfigurationRoot["sourceWildcard"][0].Value);

            return ipaRule;
        }

        public override NameValueItem[] GetConfiguration(TrafficSplitterRule tsrRule)
        {
            IPAddressRule ipaRule = (IPAddressRule)tsrRule;

            List<NameValueItem> lNvi = new List<NameValueItem>();

            lNvi.Add(ConvertActionToNameValueItem(ipaRule.Action));

            if(ipaRule.Address != null)
                lNvi.Add(new NameValueItem("address", ipaRule.Address.ToString()));

            if (ipaRule.Wildcard != null)
                lNvi.Add(new NameValueItem("wildcard", ipaRule.Wildcard.ToString()));

            if (ipaRule.Destination != null)
                lNvi.Add(new NameValueItem("destinationAddress", ipaRule.Destination.ToString()));

            if (ipaRule.Source != null)
                lNvi.Add(new NameValueItem("sourceAddress", ipaRule.Source.ToString()));

            if (ipaRule.DestinationWildcard != null)
                lNvi.Add(new NameValueItem("destinationWildcard", ipaRule.DestinationWildcard.ToString()));

            if (ipaRule.SourceWildcard != null)
                lNvi.Add(new NameValueItem("sourceWildcard", ipaRule.SourceWildcard.ToString()));


            return lNvi.ToArray();
        }
    }
}
