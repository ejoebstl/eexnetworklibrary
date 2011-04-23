using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.TrafficModifiers;
using eExNetworkLibrary.TrafficModifiers.StreamModification.HTTP;
using eExNLML.Extensibility;
using eExNetworkLibrary;

namespace eExNLML.IO.HandlerConfigurationWriters
{
    class HTTPModifierConfigurationWriter : HandlerConfigurationWriter
    {
        private HTTPStreamModifier thHandler;

        public HTTPModifierConfigurationWriter(TrafficHandler thToSave)
            : base(thToSave)
        {
            thHandler = (HTTPStreamModifier)thToSave;
        }

        protected override void AddConfiguration(List<NameValueItem> lNameValueItems, IEnvironment eEnviornment)
        {
            HTTPStreamModifierAction[] arActions = thHandler.Actions;

            foreach (HTTPStreamModifierAction htAction in arActions)
            {
                NameValueItem nviRule = SaveAction(eEnviornment, htAction);

                if (nviRule != null)
                {
                    lNameValueItems.Add(nviRule);
                }
            }
        }
        private NameValueItem SaveAction(IEnvironment eEnviornment, HTTPStreamModifierAction trAction)
        {
            NameValueItem nviRule = null;
            ISubPlugInDefinition<HTTPStreamModifierAction> ispActionDefinition = null;
            ispActionDefinition = GetActionDefinitionForName(eEnviornment, trAction.Name);
            
            if (ispActionDefinition != null)
            {
                nviRule = new NameValueItem("action", "");
                nviRule.AddChildItem(new NameValueItem("actionKey", ispActionDefinition.PluginKey));
                nviRule.AddChildRange(ispActionDefinition.GetConfiguration(trAction));
                foreach (HTTPStreamModifierCondition cChild in trAction.ChildRules)
                {
                    NameValueItem nvi = SaveCondition(eEnviornment, cChild);
                    if (nvi != null)
                    {
                        nviRule.AddChildItem(nvi);
                    }
                }
            }
            return nviRule;
        }

        private NameValueItem SaveCondition(IEnvironment eEnviornment, HTTPStreamModifierCondition cCondition)
        {
            NameValueItem nviRule = null;
            ISubPlugInDefinition<HTTPStreamModifierCondition> ispConditionDefinition = null;
            ispConditionDefinition = GetConditionefinitionForName(eEnviornment, cCondition.Name);

            if (ispConditionDefinition != null)
            {
                nviRule = new NameValueItem("condition", "");
                nviRule.AddChildItem(new NameValueItem("conditionKey", ispConditionDefinition.PluginKey));
                nviRule.AddChildRange(ispConditionDefinition.GetConfiguration(cCondition));
                foreach (HTTPStreamModifierCondition cChild in cCondition.ChildRules)
                {
                    NameValueItem nvi = SaveCondition(eEnviornment, cChild);
                    if (nvi != null)
                    {
                        nviRule.AddChildItem(nvi);
                    }
                }
            }
            return nviRule;
        }

        private ISubPlugInDefinition<HTTPStreamModifierAction> GetActionDefinitionForName(IEnvironment eEnvironment, string strActionName)
        {
            foreach (ISubPlugInDefinition<HTTPStreamModifierAction> htAction in eEnvironment.GetPluginsByType(PluginTypes.HTTPModifierAction))
            {
                if (htAction.Name == strActionName)
                {
                    return htAction;
                }
            }

            return null;
        }

        private ISubPlugInDefinition<HTTPStreamModifierCondition> GetConditionefinitionForName(IEnvironment eEnvironment, string strConditionName)
        {
            foreach (ISubPlugInDefinition<HTTPStreamModifierCondition> hcCondition in eEnvironment.GetPluginsByType(PluginTypes.HTTPModifierCondition))
            {
                if (hcCondition.Name == strConditionName)
                {
                    return hcCondition;
                }
            }

            return null;
        }
    }
}
