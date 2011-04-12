using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace eExNetworkLibrary.ICMP.V6
{
    public class NeighborSolicitation : Frame
    {
        private IPAddress ipaTargetAddress;

        public IPAddress TargetAddress
        {
            get
            {
                return ipaTargetAddress;
            }
            set
            {
                if (value.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
                    throw new ArgumentException("Only IPv6 addresses are supported by the ICMPv6NeighborAdvertisment.");
                ipaTargetAddress = value;
            }
        }

        public NeighborSolicitation()
        {
            ipaTargetAddress = IPAddress.IPv6Any;
        }

        public NeighborSolicitation(byte[] bData)
        {
            byte[] bAddressBytes = new byte[16];

            Array.Copy(bData, 4, bAddressBytes, 0, 16);

            this.ipaTargetAddress = new IPAddress(bAddressBytes);

            Encapsulate(bData, 20);
        }

        public override FrameType FrameType
        {
            get { return FrameType.ICMP; }
        }

        public override byte[] FrameBytes
        {
            get 
            {
                byte[] bData = new byte[Length];

                Array.Copy(ipaTargetAddress.GetAddressBytes(), 0, bData, 4, 16);

                if (fEncapsulatedFrame != null)
                {
                    Array.Copy(fEncapsulatedFrame.FrameBytes, 0, bData, 20, fEncapsulatedFrame.Length);
                }

                return bData;
            }
        }

        public override int Length
        {
            get { return 20 + (fEncapsulatedFrame != null ? fEncapsulatedFrame.Length : 0); }
        }

        public override Frame Clone()
        {
            return new NeighborSolicitation(this.FrameBytes);
        }
    }
}
