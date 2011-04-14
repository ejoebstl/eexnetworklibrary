using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary.ProtocolParsing.Providers
{
    class DHCPProtocolProvider : IProtocolProvider
    {
        public string Protocol
        {
            get { return FrameTypes.DHCP; }
        }

        public string[] KnownPayloads
        {
            get { return new string[0]; }
        }

        public Frame Parse(Frame fFrame)
        {
            if (fFrame.FrameType == this.Protocol)
            {
                return fFrame;
            }

            return new DHCP.DHCPFrame(fFrame.FrameBytes);
        }

        public string PayloadType(Frame fFrame)
        {
            if (fFrame.FrameType != this.Protocol)
            {
                fFrame = Parse(fFrame);
            }

            return "";
        }
    }
}