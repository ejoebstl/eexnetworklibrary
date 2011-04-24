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
using eExNetworkLibrary.IP.V6;
using eExNetworkLibrary.IP;

namespace eExNetworkLibrary.ProtocolParsing.Providers
{
    public class IPv6ProtocolProvider : IPv4ProtocolProvider
    {
        public override string Protocol
        {
            get
            {
                return FrameTypes.IPv6;
            }
        }

        public override Frame Parse(Frame fFrame)
        {
            IPv6Frame ipFrame = new IPv6Frame(fFrame.FrameBytes);
            
            //Automatically parse IPv6 headers

            Frame fLastFrame = ipFrame;

            while (fLastFrame.FrameType != RawDataFrame.DefaultFrameType)
            {
                byte[] bPayload = fLastFrame.EncapsulatedFrame.FrameBytes;

                switch (((IIPHeader)fLastFrame).Protocol)
                {
                    case IPProtocol.IPv6_Frag:
                        fLastFrame.EncapsulatedFrame = new FragmentExtensionHeader(bPayload);
                        break;
                    case IPProtocol.IPv6_Route:
                        fLastFrame.EncapsulatedFrame = new RoutingExtensionHeader(bPayload);
                        break;
                    default:
                        fLastFrame.EncapsulatedFrame = new RawDataFrame(bPayload);
                        break;
                }

                fLastFrame = fLastFrame.EncapsulatedFrame;
            }

            return ipFrame;
        }
    }
}
