﻿// This source file is part of the eEx Network Library
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

namespace eExNetworkLibrary.ProtocolParsing.Providers
{
    public class TrafficDescriptionFrameProtocolProvider : IProtocolProvider
    {
        public string Protocol
        {
            get { return FrameTypes.TrafficDescriptionFrame; }
        }

        public string[] KnownPayloads
        {
            get { return new string[]{FrameTypes.Ethernet}; }
        }

        public Frame Parse(Frame fFrame)
        {
            throw new InvalidOperationException("A traffic description frame cannot be parsed.");
        }

        public string PayloadType(Frame fFrame)
        {
            if (fFrame.FrameType == this.Protocol
                && ((TrafficDescriptionFrame)fFrame).SourceInterface.AdapterType == System.Net.NetworkInformation.NetworkInterfaceType.Ethernet
                && ((TrafficDescriptionFrame)fFrame).SourceInterface.AdapterType == System.Net.NetworkInformation.NetworkInterfaceType.Ethernet3Megabit
                && ((TrafficDescriptionFrame)fFrame).SourceInterface.AdapterType == System.Net.NetworkInformation.NetworkInterfaceType.FastEthernetT
                && ((TrafficDescriptionFrame)fFrame).SourceInterface.AdapterType == System.Net.NetworkInformation.NetworkInterfaceType.GigabitEthernet
                && ((TrafficDescriptionFrame)fFrame).SourceInterface.AdapterType == System.Net.NetworkInformation.NetworkInterfaceType.Wireless80211
                && ((TrafficDescriptionFrame)fFrame).SourceInterface.AdapterType == System.Net.NetworkInformation.NetworkInterfaceType.FastEthernetFx)
            {
                return FrameTypes.Ethernet;
            }

            return null;
        }
    }
}
