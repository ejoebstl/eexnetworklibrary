using System;
using System.Collections.Generic;
using System.Text;

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
            return new IP.V6.IPv6Frame(fFrame.FrameBytes);
        }
    }
}
