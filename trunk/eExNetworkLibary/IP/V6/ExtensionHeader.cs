using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary.IP.V6
{
    public abstract class ExtensionHeader : Frame, IIPHeader
    {
        public IPProtocol NextHeader { get; set; }
        public IPProtocol Protocol { get { return NextHeader; } set { NextHeader = value; } }

        protected ExtensionHeader(byte[] bData)
        {
            NextHeader = (IPProtocol)bData[0];
        }

        public override byte[] FrameBytes
        {
            get
            {
                byte[] bData = new byte[Length];
                bData[0] = (byte)NextHeader;
                return bData;
            }
        }
    }
}
