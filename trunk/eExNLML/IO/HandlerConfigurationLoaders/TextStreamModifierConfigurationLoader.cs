using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.Attacks.MITM;
using eExNetworkLibrary;
using System.Net;
using eExNetworkLibrary.Attacks.Spoofing;
using eExNetworkLibrary.Attacks;
using eExNLML.Extensibility;
using eExNetworkLibrary.TrafficModifiers;

namespace eExNLML.IO.HandlerConfigurationLoaders
{
    class TextStreamModifierConfigurationLoader : HandlerConfigurationLoader
    {
        private TextStreamModifier thHandler;

        public TextStreamModifierConfigurationLoader(TrafficHandler thHandler)
            : base(thHandler)
        {
            this.thHandler = (TextStreamModifier)thHandler;
        }
        protected override void ParseConfiguration(Dictionary<string, NameValueItem[]> strNameValues, IEnvironment eEnviornment)
        {
            thHandler.Port = ConvertToInt(strNameValues["Port"])[0];
            thHandler.DataToFind = ConvertToString(strNameValues["DataToFind"])[0];
            thHandler.DataToReplace = ConvertToString(strNameValues["DataToReplace"])[0];
        }
    }
}

