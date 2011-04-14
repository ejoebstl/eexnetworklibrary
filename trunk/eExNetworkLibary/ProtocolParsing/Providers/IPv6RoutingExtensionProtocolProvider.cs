using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary.ProtocolParsing.Providers
{
    public class IPv6RoutingExtensionProtocolProvider : IPv4ProtocolProvider
    {
        public override string Protocol
        {
            get
            {
                return FrameTypes.IPv6Route;
            }
        }

        public override Frame Parse(Frame fFrame)
        {
            return new IP.V6.RoutingExtensionHeader(fFrame.FrameBytes);
        }
    }
}