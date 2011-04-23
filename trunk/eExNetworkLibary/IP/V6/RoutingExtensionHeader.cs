using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace eExNetworkLibrary.IP.V6
{
    /// <summary>
    /// This class represents an IPv6 routing extension header.
    /// </summary>
    public class RoutingExtensionHeader : ExtensionHeader
    {
        /// <summary>
        /// Returns FrameTypes.IPv6Route
        /// </summary>
        public static string DefaultFrameType { get { return FrameTypes.IPv6Route; } }

        private List<IPAddress> lAddresses;

        /// <summary>
        /// Gets or sets the value of the RoutingType field
        /// </summary>
        public byte RoutingType { get; set; }
        /// <summary>
        /// Gets or sets the value of the SegmentsLeft field
        /// </summary>
        public byte SegmentsLeft { get; set; }

        /// <summary>
        /// Creates a new, empty instance of this class
        /// </summary>
        public RoutingExtensionHeader() 
            : base(new byte[1])
        {
            RoutingType = 0;
            SegmentsLeft = 0;
            lAddresses = new List<IPAddress>();
        }

        /// <summary>
        /// Creates a new instance of this class from the given bytes.
        /// </summary>
        /// <param name="bData">The byte data to parse.</param>
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

            //Counting in 16 bit hops
            for (int iC1 = 0; iC1 < iLen; iC1 = iC1 + 2)
            {
                Array.Copy(bData, 8 + (8 * iC1), bAddressData, 0, 16);
                lAddresses.Add(new IPAddress(bAddressData));
            }

            Encapsulate(bData, Length);
        }

        /// <summary>
        /// Adds an address to this routing extension header.
        /// </summary>
        /// <param name="ipaAddress">The address to add.</param>
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

        /// <summary>
        /// Gets the count of addresses, currently contained by this routing extension header.
        /// </summary>
        public int AddressCount { get { return lAddresses.Count; } }

        public void RemoveAddress(IPAddress ipa)
        {
            lAddresses.Remove(ipa);
        }

        /// <summary>
        /// Gets a bool indicating whether a specific address is contained in this routing extension header.
        /// </summary>
        /// <param name="ipa">The address to check for.</param>
        /// <returns>A bool indicating whether a specific address is contained in this routing extension header.</returns>
        public bool ContainsAddress(IPAddress ipa)
        {
            return lAddresses.Contains(ipa);
        }

        /// <summary>
        /// Gets all addresses contained in this routing extension header.
        /// </summary>
        /// <returns>All addresses contained in this routing extension header.</returns>
        public IPAddress[] GetAddresses()
        {
            return lAddresses.ToArray();
        }

        /// <summary>
        /// Returns FrameTypes.IPv6Route
        /// </summary>
        public override string FrameType
        {
            get { return DefaultFrameType; }
        }

        /// <summary>
        /// Returns this frame and it's encapsulated data, converted to raw bytes
        /// </summary>
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

        /// <summary>
        /// Gets the length of this frame and the encapsulated frame.
        /// </summary>
        public override int Length
        {
            get { return 8 + (lAddresses.Count * 16) + (fEncapsulatedFrame != null ? fEncapsulatedFrame.Length : 0); }
        }

        /// <summary>
        /// Returns a copy of this frame.
        /// </summary>
        /// <returns>A copy of this frame</returns>
        public override Frame Clone()
        {
            return new RoutingExtensionHeader(this.FrameBytes);
        }
    }
}
