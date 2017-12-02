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
using eExNetworkLibrary.Ethernet;

namespace eExNetworkLibrary.ProtocolParsing.Providers
{
    public class EthernetProtocolProvider : IProtocolProvider
    {
        public string Protocol
        {
            get { return FrameTypes.Ethernet; }
        }

        public string[] KnownPayloads
        {
            get { return new string[] { FrameTypes.IPv4, FrameTypes.IPv6, FrameTypes.ARP,
                                        FrameTypes.RARP, FrameTypes.IPX, FrameTypes.AppleTalk, 
                                        FrameTypes.AARP, FrameTypes.Novell}; }
        }

        public Frame Parse(Frame fFrame)
        {
            if (fFrame.FrameType == this.Protocol)
            {
                return fFrame;
            }

            return new EthernetFrame(fFrame.FrameBytes);
        }

        public string PayloadType(Frame fFrame)
        {
            if (fFrame.FrameType != this.Protocol)
            {
                fFrame = Parse(fFrame);
            }

            switch (((EthernetFrame)fFrame).EtherType)
            {
                case EtherType.IPv4: return FrameTypes.IPv4;
                    break;
                case EtherType.IPv6: return FrameTypes.IPv6;
                    break;
                case EtherType.ARP: return FrameTypes.ARP;
                    break;
                case EtherType.RARP: return FrameTypes.RARP;
                    break;
                case EtherType.IPX: return FrameTypes.IPX;
                    break;
                case EtherType.AppleTalk: return FrameTypes.AppleTalk;
                    break;
                case EtherType.AARP: return FrameTypes.AARP;
                    break;
                case EtherType.Novell: return FrameTypes.Novell;
                    break;
            }

            return "";
        }
    }
}
