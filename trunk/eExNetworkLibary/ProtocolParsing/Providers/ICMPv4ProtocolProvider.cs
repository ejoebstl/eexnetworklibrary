using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary.ProtocolParsing.Providers
{
    class ICMPv4ProtocolProvider : IProtocolProvider
    {
        public string Protocol
        {
            get { return FrameTypes.ICMPv4; }
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

            return new ICMP.ICMPv4Frame(fFrame.FrameBytes);
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