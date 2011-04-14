using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary.ProtocolParsing.Providers
{
    class DNSProtocolProvider : IProtocolProvider
    {
        public string Protocol
        {
            get { return FrameTypes.DNS; }
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

            return new DNS.DNSFrame(fFrame.FrameBytes);
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