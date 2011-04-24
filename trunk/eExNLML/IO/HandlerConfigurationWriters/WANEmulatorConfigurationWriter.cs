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
using eExNetworkLibrary.Simulation;
using eExNetworkLibrary;

namespace eExNLML.IO.HandlerConfigurationWriters
{
    class WANEmulatorConfigurationWriter : HandlerConfigurationWriter
    {
        private WANEmulator thHandler;

        public WANEmulatorConfigurationWriter(TrafficHandler thToSave)
            : base(thToSave)
        {
            thHandler = (WANEmulator)thToSave;
        }

        protected override void AddConfiguration(List<NameValueItem> lNameValueItems, IEnvironment eEnviornment)
        {
            lNameValueItems.AddRange(ConvertToNameValueItems("byteFlipperMaxErrorCount", thHandler.ByteFlipper.MaxErrorCount));
            lNameValueItems.AddRange(ConvertToNameValueItems("byteFlipperMinErrorCount", thHandler.ByteFlipper.MinErrorCount));
            lNameValueItems.AddRange(ConvertToNameValueItems("byteFlipperProbability", thHandler.ByteFlipper.Probability));
            lNameValueItems.AddRange(ConvertToNameValueItems("delayJitterMaxDelay", thHandler.DelayJitter.MaxDelay));
            lNameValueItems.AddRange(ConvertToNameValueItems("delayJitterMinDelay", thHandler.DelayJitter.MinDelay));
            lNameValueItems.AddRange(ConvertToNameValueItems("packetDropperProbability", thHandler.PacketDropper.Probability));
            lNameValueItems.AddRange(ConvertToNameValueItems("packetDuplicatorProbability", thHandler.PacketDuplicator.Probability));
            lNameValueItems.AddRange(ConvertToNameValueItems("packetReordererAccumulationTime", thHandler.PacketReorderer.AccumulationTime));
            lNameValueItems.AddRange(ConvertToNameValueItems("speedConstrainerSpeed", thHandler.SpeedConstrainer.Speed));
        }
    }
}
