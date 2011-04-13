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

            return new ICMPv6Frame(fFrame.FrameBytes);
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