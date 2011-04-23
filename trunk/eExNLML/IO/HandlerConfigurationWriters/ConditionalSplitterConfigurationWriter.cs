using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.TrafficSplitting;
using eExNetworkLibrary;
using eExNLML.Extensibility;

namespace eExNLML.IO.HandlerConfigurationWriters
{
    class ConditionalSplitterConfigurationWriter : HandlerConfigurationWriter
    {
        private ConditionalTrafficSplitter thHandler;

        public ConditionalSplitterConfigurationWriter(TrafficHandler thToSave)
            : base(thToSave)
        {
            thHandler = (ConditionalTrafficSplitter)thToSave;
        }

        protected override void AddConfiguration(List<NameValueItem> lNameValueItems, IEnvironment eEnviornment)
        {
            TrafficSplitterRule[] trRules = thHandler.GetRules();

            foreach (TrafficSplitterRule trRule in trRules)
            {
                NameValueItem nviRule = SaveRule(eEnviornment, trRule);

                if (nviRule != null)
                {
                    lNameValueItems.Add(nviRule);
                }
            }
        }

        private NameValueItem SaveRule(IEnvironment eEnviornment, TrafficSplitterRule trRule)
        {
            NameValueItem nviRule = null;
            ISubPlugInDefinition<TrafficSplitterRule> ruleDefinition = GetRuleDefinitionForName(eEnviornment, trRule.Name);
            if (ruleDefinition != null)
            {
                nviRule = new NameValueItem("rule", "");
                nviRule.AddChildItem(new NameValueItem("ruleKey", ruleDefinition.PluginKey));
                nviRule.AddChildRange(ruleDefinition.GetConfiguration(trRule));
                foreach (TrafficSplitterRule tsrChild in trRule.ChildRules)
                {
                    NameValueItem nvi = SaveRule(eEnviornment, tsrChild);
                    if (nvi != null)
                    {
                        nviRule.AddChildItem(nvi);
                    }
                }
            }
            return nviRule;
        }

        private ISubPlugInDefinition<TrafficSplitterRule> GetRuleDefinitionForName(IEnvironment eEnvironment, string strRuleName)
        {
            foreach (ISubPlugInDefinition<TrafficSplitterRule> tsrRule in eEnvironment.GetPluginsByType(PluginTypes.SplitterRule))
            {
                if (tsrRule.Name == strRuleName)
                {
                    return tsrRule;
                }
            }

            return null;
        }
    }
}
