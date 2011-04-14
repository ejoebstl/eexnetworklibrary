using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace eExNetworkLibrary.IP.V6
{
    public class IPv6Frame : IPFrame
    {
        public static string DefaultFrameType { get { return FrameTypes.IPv6; } }

        private int iVersion;
        private ushort sTrafficClass;
        private uint iFlowLabel;
        //private ushort iPayloadLength;
        private byte bNextHeader;
        private byte bHopLimit;
        private IPAddress ipaSource;
        private IPAddress ipaDestination;

        public IPv6Frame()
        {
            ipaSource = IPAddress.IPv6Any;
            ipaDestination = IPAddress.IPv6Any;
            iVersion = 6;
            sTrafficClass = 0;
            iFlowLabel = 0;
            bNextHeader = 0;
            bHopLimit = (byte)0xFF;
        }

        public override string FrameType
        {
            get { return IPv6Frame.DefaultFrameType; }
        }

        public IPv6Frame(byte[] bRaw)
        {
            if (bRaw.Length < 40)
            {
                throw new ArgumentException("Invalid raw data. An IPv6 header has at least 40 bytes of data, but the raw data array contains only " + bRaw.Length + ".");
            }

            ushort sPayloadLength = 0;

            this.iVersion = (bRaw[0] & 0xF0) >> 4;
            this.sTrafficClass = (byte)((bRaw[0] & 0x0F) << 4);
            this.sTrafficClass |= (byte)((bRaw[1] & 0xF0) >> 4);
            this.iFlowLabel = (uint)((bRaw[1] & 0x0F) << 16);
            this.iFlowLabel |= (uint)((bRaw[2]) << 8);
            this.iFlowLabel |= (uint)(bRaw[3]);

            sPayloadLength |= (ushort)(bRaw[4] << 8);
            sPayloadLength = bRaw[5];
 
            bNextHeader = bRaw[6];
            bHopLimit = bRaw[7];

            byte[] bAddress = new byte[16];
            Array.Copy(bRaw, 8, bAddress, 0, 16);
            ipaSource = new IPAddress(bAddress);
            Array.Copy(bRaw, 24, bAddress, 0, 16);
            ipaDestination = new IPAddress(bAddress);

            //Automatically parse IPv6 headers

            byte[] bPayload = new byte[sPayloadLength];
            Array.Copy(bRaw, 40, bPayload, 0, sPayloadLength);

            switch (NextHeader)
            {
                case IPProtocol.IPv6_Frag:
                    this.fEncapsulatedFrame = new FragmentExtensionHeader(bPayload);
                    break;
                case IPProtocol.IPv6_Route:
                    this.fEncapsulatedFrame = new RoutingExtensionHeader(bPayload);
                    break;
                default:
                    this.fEncapsulatedFrame = new RawDataFrame(bPayload);
                    break;
            }

        }

        /// <summary>
        /// Gets or sets the destination IP-address of this frame
        /// </summary>
        public override IPAddress DestinationAddress
        {
            get { return ipaDestination; }
            set
            {
                if (value.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    throw new ArgumentException("Only assigning IPv6 addresses to an IPv6 frame is possible.");
                }
                ipaDestination = value;
            }
        }

        /// <sumary>
        /// Gets  or sets the source IP-address of this frame
        /// </summary>
        public override IPAddress SourceAddress
        {
            get { return ipaSource; }
            set
            {
                if (value.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    throw new ArgumentException("Only assigning IPv6 addresses to an IPv6 frame is possible.");
                }
                ipaSource = value;
            }
        }

        /// <summary>
        /// Gets or sets the IP version of this frame, where 4 is default for IPv4.
        /// </summary>
        public override int Version
        {
            get { return iVersion; }
            set
            {
                if (iVersion > 0x0F)
                {
                    throw new ArgumentException("An IP version greater then " + 0x0F + " is not possible.");
                }
                iVersion = value;
            }
        }
        
        /// <summary>
        /// Returns the IPv6 pseudo header for this frame.<br />
        /// This pseudo header can be used to calculate TCP, ICMP and UDP checksums.
        /// </summary>
        /// <returns>The IPv6 pseudo header as byte[].</returns>
        public override byte[] GetPseudoHeader()
        {
            return GetPseudoHeader(SourceAddress, DestinationAddress, fEncapsulatedFrame != null ? fEncapsulatedFrame.Length : 0, bNextHeader);
        }

        /// <summary>
        /// Returns the IPv6 pseudo header from the given params.<br />
        /// This pseudo header can be used to calculate TCP, ICMP and UDP checksums.
        /// </summary>
        /// <param name="bNextHeader">A byte defining the type of the payload protocol.</param>
        /// <param name="ipaDestination">The destination address to use in the checksum calculation.</param>
        /// <param name="ipaSource">The source address to use in the checksum calculation.</param>
        /// <param name="iPayloadLen">The payload len to use in the checksum calculation.</param>
        /// <returns>The IPv6 pseudo header as byte[].</returns>
        static byte[] GetPseudoHeader(IPAddress ipaSource, IPAddress ipaDestination, int iPayloadLen, byte bNextHeader)
        {
            byte[] bPseudoHeader = new byte[40];

            Array.Copy(ipaSource.GetAddressBytes(), 0, bPseudoHeader, 0, 16);
            Array.Copy(ipaDestination.GetAddressBytes(), 0, bPseudoHeader, 16, 16);
            
            bPseudoHeader[32] = (byte)((iPayloadLen >> 24) & 0xFF);
            bPseudoHeader[33] = (byte)((iPayloadLen >> 16) & 0xFF);
            bPseudoHeader[34] = (byte)((iPayloadLen >> 8) & 0xFF);
            bPseudoHeader[35] = (byte)((iPayloadLen) & 0xFF);
            bPseudoHeader[39] = (byte)bNextHeader;

            return bPseudoHeader;
        }

        public override byte[] FrameBytes
        {
            get 
            {
                byte[] bRaw = new byte[this.Length];

                ushort sPayloadLength = (ushort)(fEncapsulatedFrame != null ? fEncapsulatedFrame.Length : 0);

                bRaw[0] = (byte)((iVersion & 0x0F) << 4);
                bRaw[0] |= (byte)((sTrafficClass & 0xF0) >> 4);
                bRaw[1] |= (byte)((sTrafficClass & 0x0F) << 4);
                bRaw[1] = (byte)((iFlowLabel & 0xF0000) >> 16);
                bRaw[2] = (byte)((iFlowLabel & 0xFF00) >> 8);
                bRaw[3] = (byte)(iFlowLabel & 0xFF);
                
                bRaw[4] |= (byte)((sPayloadLength >> 8) & 0xFF);
                bRaw[5] = (byte)(sPayloadLength & 0xFF);

                bRaw[6] = bNextHeader;
                bRaw[7] = bHopLimit;

                Array.Copy(ipaSource.GetAddressBytes(), 0, bRaw, 8, 16);
                Array.Copy(ipaDestination.GetAddressBytes(), 0, bRaw, 24, 16);

                if (fEncapsulatedFrame != null)
                {
                    Array.Copy(fEncapsulatedFrame.FrameBytes, 0, bRaw, 40, sPayloadLength);
                }

                return bRaw;
            }
        }

        /// <summary>
        /// Gets or sets the protocol of this frame. This value is exactly the same as the value encapsulated by the NextHeader property.
        /// </summary>
        public override IPProtocol Protocol
        {
            get { return (IPProtocol)bNextHeader; }
            set { bNextHeader = (byte)value; }
        }

        /// <summary>
        /// Gets or sets the next header of this frame.
        /// </summary>
        public IPProtocol NextHeader
        {
            get { return (IPProtocol)bNextHeader; }
            set { bNextHeader = (byte)value; }
        }

        /// <summary>
        /// Gets or sets the time to live. This value is exactly the same as the value encapsulated by the HopLimit property.
        /// </summary>
        public override int TimeToLive
        {
            get { return (int)bHopLimit; }
            set { bHopLimit = (byte)value; }
        }

        /// <summary>
        /// Gets or sets the hop limit.
        /// </summary>
        public byte HopLimit
        {
            get { return bHopLimit; }
            set { bHopLimit = value; }
        }


        public override int Length
        {
            get { return 40 + (fEncapsulatedFrame != null ? fEncapsulatedFrame.Length : 0); }
        }

        public override Frame Clone()
        {
            return new IPv6Frame(this.FrameBytes);
        }
    }
}
