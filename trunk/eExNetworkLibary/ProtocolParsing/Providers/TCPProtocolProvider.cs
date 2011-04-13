using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary.ProtocolParsing.Providers
{
    /// <summary>
    /// <remarks>The TCP protocol provider cannot 
    /// provide any payload parsing functionality 
    /// since TCP is stream oriented.</remarks>
    /// </summary>
    class TCPProtocolProvider : IProtocolProvider
    {
        public string Protocol
        {
            get { return FrameTypes.TCP; }
        }

        public string[] KnownPayloads
        {
            get
            {
                return new string[0];
            }
        }

        public Frame Parse(Frame fFrame)
        {
            if (fFrame.FrameType == this.Protocol)
            {
                return fFrame;
            }

            return new TCP.TCPFrame(fFrame.FrameBytes);
        }

        public string PayloadType(Frame fFrame)
        {
            return "";
        }
    }
}
