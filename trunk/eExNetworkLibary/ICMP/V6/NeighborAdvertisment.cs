using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary.ICMP.V6
{
    public class NeighborAdvertisment : NeighborSolicitation
    {
        public static string DefaultFrameType { get { return "ICMPv6NeighborAdvertisment"; } }

        public bool RouterFlag { get; set; }
        public bool SolicitedFlag { get; set; }
        public bool OverrideFlag { get; set; }


        public NeighborAdvertisment()
            : base()
        { }

        public NeighborAdvertisment(byte[] bData) : base(bData)
        {
            RouterFlag = (bData[0] & 0x80) != 0;
            SolicitedFlag = (bData[0] & 0x40) != 0;
            OverrideFlag = (bData[0] & 0x20) != 0;
        }

        public override byte[] FrameBytes
        {
            get
            {
                byte[] bData = base.FrameBytes;

                bData[0] |= (byte)(RouterFlag ? 0x80 : 0);
                bData[0] |= (byte)(SolicitedFlag ? 0x40 : 0);
                bData[0] |= (byte)(OverrideFlag ? 0x20 : 0);

                return bData;
            }
        }

        public override string FrameType
        {
            get { return NeighborAdvertisment.DefaultFrameType; }
        }

        public override Frame Clone()
        {
            return new NeighborAdvertisment(this.FrameBytes);
        }
    }
}
