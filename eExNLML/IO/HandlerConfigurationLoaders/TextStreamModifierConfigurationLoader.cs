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

