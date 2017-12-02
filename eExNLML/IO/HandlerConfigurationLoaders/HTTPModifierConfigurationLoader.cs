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
using eExNetworkLibrary.TrafficModifiers;
using eExNetworkLibrary;
using eExNetworkLibrary.TrafficModifiers.StreamModification.HTTP;
using eExNLML.Extensibility;

namespace eExNLML.IO.HandlerConfigurationLoaders
{
    class HTTPModifierConfigurationLoader : HandlerConfigurationLoader
    {
        private HTTPStreamModifier thHandler;

        public HTTPModifierConfigurationLoader(TrafficHandler thToSave)
            : base(thToSave)
        {
            thHandler = (HTTPStreamModifier)thToSave;
        }

        protected override void ParseConfiguration(Dictionary<string, NameValueItem[]> strNameValues, IEnvironment eEnviornment)
        {
            if (strNameValues.ContainsKey("action"))
            {
                foreach (NameValueItem nviActionItem in strNameValues["action"])
                {
                    HTTPStreamModifierAction htAction = LoadAction(nviActionItem, eEnviornment);
                    if (htAction != null)
                    {
                        thHandler.AddAction(htAction);
                    }
                }
            }
        }

        private HTTPStreamModifierAction LoadAction(NameValueItem nviActionItem, IEnvironment eEnviornment)
        {
            string strKey = nviActionItem["actionKey"][0].Value;
            ISubPlugInDefinition<HTTPStreamModifierAction> actionDefinition = (ISubPlugInDefinition<HTTPStreamModifierAction>)eEnviornment.GetPlugInByKey(strKey);

            if (actionDefinition != null)
            {
                HTTPStreamModifierAction htAction = actionDefinition.Create(nviActionItem);
                foreach (NameValueItem nviChild in nviActionItem["condition"])
                {
                    htAction.AddChildRule(LoadCondition(nviChild, eEnviornment));
                }
                return htAction;
            }

            return null;
        }

        private HTTPStreamModifierCondition LoadCondition(NameValueItem nviConditionItem, IEnvironment eEnviornment)
        {
            string strKey = nviConditionItem["conditionKey"][0].Value;
            ISubPlugInDefinition<HTTPStreamModifierCondition> conditionDefinition = (ISubPlugInDefinition<HTTPStreamModifierCondition>)eEnviornment.GetPlugInByKey(strKey);

            if (conditionDefinition != null)
            {
                HTTPStreamModifierCondition htCondition = conditionDefinition.Create(nviConditionItem);
                foreach (NameValueItem nviChild in nviConditionItem["condition"])
                {
                    htCondition.AddChildRule(LoadCondition(nviChild, eEnviornment));
                }
                return htCondition;
            }

            return null;
        }
    }
}
