// This source file is part of the eEx Network Library
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
using eExNetworkLibrary.ICMP.V6;

namespace eExNetworkLibrary.ProtocolParsing.Providers
{
    class ICMPv6ProtocolProvider : IProtocolProvider
    {
        public string Protocol
        {
            get { return FrameTypes.ICMPv6; }
        }

        public string[] KnownPayloads
        {
            get
            {
                return new string[]{
                NeighborAdvertisment.DefaultFrameType, 
                NeighborSolicitation.DefaultFrameType};
            }
        }

        public Frame Parse(Frame fFrame)
        {
            if (fFrame.FrameType == this.Protocol)
            {
                return fFrame;
            } 
            
            ICMPv6Frame icmpFrame = new ICMPv6Frame(fFrame.FrameBytes);

            switch (icmpFrame.ICMPv6Type)
            {
                case ICMPv6Type.NeighborAdvertisement:
                    icmpFrame.EncapsulatedFrame = new NeighborAdvertisment(icmpFrame.EncapsulatedFrame.FrameBytes);
                    break;
                case ICMPv6Type.NeighborSolicitation:
                    icmpFrame.EncapsulatedFrame = new NeighborSolicitation(icmpFrame.EncapsulatedFrame.FrameBytes);
                    break;
            }

            return icmpFrame;
        }

        public string PayloadType(Frame fFrame)
        {
            if (fFrame.FrameType != this.Protocol)
            {
                fFrame = Parse(fFrame);
            }

            switch (((ICMPv6Frame)fFrame).ICMPv6Type)
            {
                case ICMPv6Type.NeighborAdvertisement: return NeighborAdvertisment.DefaultFrameType;
                    break;
                case ICMPv6Type.NeighborSolicitation: return NeighborSolicitation.DefaultFrameType;
                    break;
            }

            return "";
        }
    }
}