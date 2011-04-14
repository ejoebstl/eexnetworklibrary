using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace eExNetworkLibrary.IP.V6
{
    public class RoutingExtensionHeader : ExtensionHeader
    {
        public static string DefaultFrameType { get { return FrameTypes.IPv6Route; } }

        private List<IPAddress> lAddresses;

        public byte RoutingType { get; set; }
        public byte SegmentsLeft { get; set; }


        public RoutingExtensionHeader() 
            : base(new byte[1])
        {
            RoutingType = 0;
            SegmentsLeft = 0;
            lAddresses = new List<IPAddress>();
        }

        public RoutingExtensionHeader(byte[] bData) 
            : base(bData)
        {
            //Len in units of 8 bytes, excluding the first 8 bytes
            int iLen = bData[1];
            RoutingType = bData[2];
            SegmentsLeft = bData[3];

            if(RoutingType != 0)
            {
                throw new ArgumentException("Routing types other than zero are not supported, since their is no specification for them.");
            }

            byte[] bAddressData = new byte[16];
            lAddresses = new List<IPAddress>();

            //Counting in 16 bin hops
            for (int iC1 = 0; iC1 < iLen; iC1 = iC1 + 2)
            {
                Array.Copy(bData, 8 + (8 * iC1), bAddressData, 0, 16);
                lAddresses.Add(new IPAddress(bAddressData));
            }

            Encapsulate(bData, Length);
        }

        public void AddAddress(IPAddress ipaAddress)
        {
            if (ipaAddress.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                throw new ArgumentException("Only IPv6 addresses can be used in an IPv6 routing extension header");
            }
            if (lAddresses.Count >= 128)
            {
                throw new InvalidOperationException("Only a maximum of 128 addresses is supported.");
            }

            lAddresses.Add(ipaAddress);
        }

        public int AddressCount { get { return lAddresses.Count; } }

        public void RemoveAddress(IPAddress ipa)
        {
            lAddresses.Remove(ipa);
        }

        public bool ContainsAddress(IPAddress ipa)
        {
            return lAddresses.Contains(ipa);
        }

        public IPAddress[] GetAddresses()
        {
            return lAddresses.ToArray();
        }

        public override string FrameType
        {
            get { return DefaultFrameType; }
        }

        public override byte[] FrameBytes
        {
            get
            {
                byte[] bData = base.FrameBytes;
                bData[1] = (byte)(lAddresses.Count * 2);
                bData[2] = RoutingType;
                bData[3] = SegmentsLeft;

                int iC1 = 8;

                foreach (IPAddress ipa in lAddresses)
                {
                    ipa.GetAddressBytes().CopyTo(bData, iC1);
                    iC1 += 16;
                }

                if (fEncapsulatedFrame != null)
                {
                    fEncapsulatedFrame.FrameBytes.CopyTo(bData, iC1);
                }

                return bData;
            }
        }

        public override int Length
        {
            get { return 8 + (lAddresses.Count * 16) + (fEncapsulatedFrame != null ? fEncapsulatedFrame.Length : 0); }
        }


        public override Frame Clone()
        {
            return new RoutingExtensionHeader(this.FrameBytes);
        }
    }
}
