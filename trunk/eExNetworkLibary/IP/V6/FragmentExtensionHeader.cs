using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary.IP.V6
{
    public class FragmentExtensionHeader : ExtensionHeader
    {
        public static string DefaultFrameType { get { return FrameTypes.IPv6Frag; } }

        public int FragmentOffset { get; set; }
        public bool MoreFragments { get; set; }
        public uint Identification { get; set; }

        public FragmentExtensionHeader()
            : base(new byte[1])
        {
        }

        public FragmentExtensionHeader(byte[] bData)
            : base(bData)
        {
            FragmentOffset = bData[2] << 5;
            FragmentOffset |= bData[3] >> 3;

            MoreFragments = (bData[3] & 0x1) != 0;
            Identification = (uint)bData[4] << 24;
            Identification |= (uint)bData[5] << 16;
            Identification |= (uint)bData[6] << 8;
            Identification |= (uint)bData[7];

            Encapsulate(bData, 8);
            
        }


        public override byte[] FrameBytes
        {
            get
            {
                byte[] bData = base.FrameBytes;

                bData[2] = (byte)((FragmentOffset >> 5) & 0xFF);
                bData[3] = (byte)((FragmentOffset << 3) & 0xFF);
                bData[3] |= (byte)(MoreFragments ? 0x01 : 0x00);

                bData[4] = (byte)((Identification >> 24) & 0xFF);
                bData[5] = (byte)((Identification >> 16) & 0xFF);
                bData[6] = (byte)((Identification >> 8) & 0xFF);
                bData[7] = (byte)(Identification & 0xFF);

                if (fEncapsulatedFrame != null)
                {
                    fEncapsulatedFrame.FrameBytes.CopyTo(bData, 8);
                }

                return bData;
            }
        }

        public override string FrameType
        {
            get { return DefaultFrameType; }
        }

        public override int Length
        {
            get { return 8 + (fEncapsulatedFrame != null ? fEncapsulatedFrame.Length : 0); }
        }

        public override Frame Clone()
        {
            return new FragmentExtensionHeader(FrameBytes);
        }
    }
}
