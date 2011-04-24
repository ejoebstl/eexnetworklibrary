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

namespace eExNLML.IO.HandlerConfigurationLoaders
{
    class WANEmulatorConfigurationLoader : HandlerConfigurationLoader
    {
        private WANEmulator thHandler;

        public WANEmulatorConfigurationLoader(TrafficHandler thHandler)
            : base(thHandler)
        {
            this.thHandler = (WANEmulator)thHandler;
        }
        protected override void ParseConfiguration(Dictionary<string, NameValueItem[]> strNameValues, IEnvironment eEnviornment)
        {
            thHandler.ByteFlipper.MaxErrorCount = ConvertToInt(strNameValues["byteFlipperMaxErrorCount"])[0];
            thHandler.ByteFlipper.MinErrorCount = ConvertToInt(strNameValues["byteFlipperMinErrorCount"])[0];
            thHandler.ByteFlipper.Probability = ConvertToInt(strNameValues["byteFlipperProbability"])[0];
            thHandler.DelayJitter.MaxDelay = ConvertToInt(strNameValues["delayJitterMaxDelay"])[0];
            thHandler.DelayJitter.MinDelay = ConvertToInt(strNameValues["delayJitterMinDelay"])[0];
            thHandler.PacketDropper.Probability = ConvertToInt(strNameValues["packetDropperProbability"])[0];
            thHandler.PacketDuplicator.Probability = ConvertToInt(strNameValues["packetDuplicatorProbability"])[0];
            thHandler.PacketReorderer.AccumulationTime = ConvertToInt(strNameValues["packetReordererAccumulationTime"])[0];
            thHandler.SpeedConstrainer.Speed = ConvertToInt(strNameValues["speedConstrainerSpeed"])[0];
        }
    }
}
