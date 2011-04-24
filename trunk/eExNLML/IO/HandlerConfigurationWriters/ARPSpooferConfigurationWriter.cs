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
using eExNetworkLibrary;
using eExNetworkLibrary.Attacks.MITM;
using eExNetworkLibrary.Attacks.Spoofing;
using eExNetworkLibrary.Attacks;
using eExNetworkLibrary.TrafficModifiers;

namespace eExNLML.IO.HandlerConfigurationWriters
{
    class TextStreamModifierConfigurationWriter : HandlerConfigurationWriter
    {
        private TextStreamModifier thHandler;

        public TextStreamModifierConfigurationWriter(TrafficHandler thToSave)
            : base(thToSave)
        {
            thHandler = (TextStreamModifier)thToSave;
        }

        protected override void AddConfiguration(List<NameValueItem> lNameValueItems, IEnvironment eEnviornment)
        {
            lNameValueItems.AddRange(ConvertToNameValueItems("DataToFind", thHandler.DataToFind));
            lNameValueItems.AddRange(ConvertToNameValueItems("DataToReplace", thHandler.DataToReplace));
            lNameValueItems.AddRange(ConvertToNameValueItems("Port", thHandler.Port));
        }
    }
}
