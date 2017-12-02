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
using eExNetworkLibrary;
using eExNLML.Extensibility;

namespace eExNLML.IO.HandlerConfigurationLoaders
{
    class ConditionalSplitterConfigurationLoader : HandlerConfigurationLoader
    {
        private ConditionalTrafficSplitter thHandler;

        public ConditionalSplitterConfigurationLoader(TrafficHandler thHandler)
            : base(thHandler)
        {
            this.thHandler = (ConditionalTrafficSplitter)thHandler;
        }
        protected override void ParseConfiguration(Dictionary<string, NameValueItem[]> strNameValues, IEnvironment eEnviornment)
        {
            if (strNameValues.ContainsKey("rule"))
            {
                foreach (NameValueItem nviRuleItem in strNameValues["rule"])
                {
                    TrafficSplitterRule tsrRule = LoadRule(nviRuleItem, eEnviornment);
                    if (tsrRule != null)
                    {
                        thHandler.AddRule(tsrRule);
                    }
                }
            }
        }

        private TrafficSplitterRule LoadRule(NameValueItem nviItem, IEnvironment eEnviornment)
        {
            string strKey = nviItem["ruleKey"][0].Value;
            ISubPlugInDefinition<TrafficSplitterRule> ruleDefinition = (ISubPlugInDefinition<TrafficSplitterRule>)eEnviornment.GetPlugInByKey(strKey);

            if (ruleDefinition != null)
            {
                TrafficSplitterRule tsrRule = ruleDefinition.Create(nviItem);
                foreach (NameValueItem nviChild in nviItem["rule"])
                {
                    tsrRule.AddChildRule(LoadRule(nviChild, eEnviornment));
                }
                return tsrRule;
            }

            return null;
        }
    }
}
