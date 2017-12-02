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
using eExNetworkLibrary.TrafficModifiers.StreamModification.HTTP.Conditions;
using eExNLML.IO;

namespace eExNLML.SubPlugInDefinitions
{
    public class RegexHeaderConditionDefinition : Extensibility.HTTPModifierConditionDefinition
    {
        public RegexHeaderConditionDefinition()
        {
            Name = "Regex Header Condition";
            Description = "Compares values of a given HTTP header against a regular expression.";
            Author = "Emanuel Jöbstl";
            WebLink = "http://www.eex-dev.net";
            PluginKey = "eex_http_header_regex";
            Version = new Version(0, 9);
        }

        public override eExNetworkLibrary.TrafficModifiers.StreamModification.HTTP.HTTPStreamModifierCondition Create()
        {
            return new HeaderCondition();
        }

        public override eExNetworkLibrary.TrafficModifiers.StreamModification.HTTP.HTTPStreamModifierCondition Create(eExNLML.IO.NameValueItem nviConfigurationRoot)
        {
            HeaderCondition hcCondition = (HeaderCondition)Create();
            hcCondition.Pattern = ConfigurationParser.ConvertToString(nviConfigurationRoot["pattern"])[0];
            hcCondition.Header = ConfigurationParser.ConvertToString(nviConfigurationRoot["header"])[0];
            hcCondition.EvaluateRequestForResponse = ConfigurationParser.ConvertToBools(nviConfigurationRoot["evaluateRequestForResponse"])[0];
            return hcCondition;
        }

        public override eExNLML.IO.NameValueItem[] GetConfiguration(eExNetworkLibrary.TrafficModifiers.StreamModification.HTTP.HTTPStreamModifierCondition htCondition)
        {
            List<NameValueItem> lNvi = new List<NameValueItem>();
            HeaderCondition hcCondition = (HeaderCondition)htCondition;
            lNvi.AddRange(ConfigurationParser.ConvertToNameValueItems("header", hcCondition.Header));
            lNvi.AddRange(ConfigurationParser.ConvertToNameValueItems("pattern", hcCondition.Pattern));
            lNvi.AddRange(ConfigurationParser.ConvertToNameValueItems("evaluateRequestForResponse", hcCondition.EvaluateRequestForResponse));
            return lNvi.ToArray();
        }
    }
}
