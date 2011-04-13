using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using eExNetworkLibrary.Utilities;
using eExNetworkLibrary.UDP;
using eExNetworkLibrary.TCP;
using eExNetworkLibrary.ICMP;
using eExNetworkLibrary.Routing.OSPF;

namespace eExNetworkLibrary.IP
{
    /// <summary>
    /// This class represents an IPv4 frame.
    /// <remarks>This class was one of the first written classes in this library, probably an historic one.</remarks>
    /// </summary>
    public class IPv4Frame : IPFrame
    {
        public static string DefaultFrameType { get { return FrameTypes.IPv4; } }

        private int iVersion;
        private IPTypeOfService tosTypeOfService;
        private uint iIdentification;
        private IPFlags ifFlags;
        private ushort iFragmentOffset;
        private int iTimeToLive;
        private IPProtocol iProtocol;
        private IPAddress ipaSource;
        private IPAddress ipaDestination;
        private IPv4Options ipoOptions;
        private ChecksumCalculator clCalc;
        private int iHeaderLength;


        /// <summary>
        /// Creates a new instance of this class by parsing the given data.
        /// </summary>
        /// <param name="bRaw">The data to parse</param>
        public IPv4Frame(byte[] bRaw)
        {
            this.clCalc = new ChecksumCalculator();

            int iHeaderLength;
            int iTotalLength;

            if (bRaw.Length < 20)
            {
                throw new ArgumentException("Invalid packet");
            }

            this.iVersion = (bRaw[0] & 0xF0) >> 4;
            iHeaderLength = (bRaw[0] & 0x0F) * 4;
            this.iHeaderLength = iHeaderLength;

            if (iHeaderLength < 20)
            {
                throw new ArgumentException("Invalid packet header");
            }

            this.tosTypeOfService = new IPTypeOfService(bRaw[1]);
            iTotalLength = (int)bRaw[2] * 256 + (int)bRaw[3];

            if (iTotalLength > bRaw.Length)
            {
                throw new ArgumentException("Corrupt packet length");
            }

            this.iIdentification = bRaw[4] * (uint)256 + bRaw[5];
            this.ifFlags = new IPFlags(((bRaw[6] & 0x40) >> 6) == 1, ((bRaw[6] & 0x20)) == 1);
            this.iFragmentOffset = 0;
            this.iFragmentOffset += (byte)((bRaw[6] & 0x1F) << 3);
            this.iFragmentOffset = (byte)(bRaw[7] >> 5);
            this.iTimeToLive = bRaw[8];

            if (Enum.IsDefined(typeof(IPProtocol), (int)bRaw[9]))
            {
                this.iProtocol = (IPProtocol)bRaw[9];
            }
            else
            {
                this.iProtocol = IPProtocol.Other;
            }

            ipaSource = new IPAddress(BitConverter.ToUInt32(bRaw, 12));
            ipaDestination = new IPAddress(BitConverter.ToUInt32(bRaw, 16));

            if (iHeaderLength > 20)
            {
                byte[] bOptionData = new byte[iHeaderLength - 20];

                for (int iC1 = 20; iC1 < iHeaderLength - 20; iC1++)
                {
                    bOptionData[iC1] = bRaw[iC1 + 20];
                }
                this.ipoOptions = new IPv4Options(bOptionData);
            }
            else
            {
                this.ipoOptions = new IPv4Options();
            }

            Encapsulate(bRaw, iHeaderLength);
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public IPv4Frame()
        {
            this.iVersion = 4;
            this.tosTypeOfService = new IPTypeOfService();
            this.iIdentification = 0;
            this.ifFlags = new IPFlags(false, false);
            this.iFragmentOffset = 0;
            this.iTimeToLive = 0xFF;
            this.iProtocol = IPProtocol.Other;
            this.ipaSource = IPAddress.Any;
            this.ipaDestination = IPAddress.Any;
            this.ipoOptions = new IPv4Options();

            this.clCalc = new ChecksumCalculator();
        }

        /// <summary>
        /// Returns the pseudo header for this frame.
        /// This header can be used to calculate TCP and UDP checksums.
        /// </summary>
        /// <returns>The IP pseudo header of this instance.</returns>
        public override byte[] GetPseudoHeader()
        {
            byte[] bSoureAddress = this.SourceAddress.GetAddressBytes();
            byte[] bDestinationAdddress = this.DestinationAddress.GetAddressBytes();
            byte[] bPseudoheader = new byte[4 + bSoureAddress.Length + bDestinationAdddress.Length];
            int iEncFrameLength = this.EncapsulatedFrame != null ? this.EncapsulatedFrame.Length : 0;
            bSoureAddress.CopyTo(bPseudoheader, 0);
            bDestinationAdddress.CopyTo(bPseudoheader, 4);
            bPseudoheader[8] = 0;
            bPseudoheader[9] = (byte)((uint)this.Protocol);
            bPseudoheader[10] = (byte)((iEncFrameLength >> 8) & 0xFF);
            bPseudoheader[11] = (byte)((iEncFrameLength) & 0xFF);

            return bPseudoheader;
        }

        #region Props

        public override string FrameType
        {
            get { return IPv4Frame.DefaultFrameType; }
        }

        /// <summary>
        /// Gets or sets the fragment offset
        /// </summary>
        public ushort FragmentOffset
        {
            get { return iFragmentOffset; }
            set { iFragmentOffset = value; }
        }

        /// <summary>
        /// Gets the raw byte representation of this frame
        /// </summary>
        public override byte[] FrameBytes
        {
            get
            {
                int iDataLength = this.Length;
                byte[] bPacket = new byte[this.Length];
                bPacket[0] = (byte)((short)(this.iVersion << 4));
                bPacket[0] |= (byte)((short)(InternetHeaderLength));
                bPacket[1] = this.tosTypeOfService.Raw;
                bPacket[2] = (byte)((iDataLength >> 8) & 0xFF);
                bPacket[3] = (byte)(iDataLength & 0xFF);
                bPacket[4] = (byte)((this.iIdentification >> 8) & 0xFF);
                bPacket[5] = (byte)(this.iIdentification & 0xFF);
                bPacket[6] = (byte)(Convert.ToInt32(this.ifFlags.DontFragment) << 6);
                bPacket[6] |= (byte)(Convert.ToInt32(this.ifFlags.MoreFragments) << 5);
                bPacket[6] |= (byte)((FragmentOffset >> 3) & 0x0F);
                bPacket[7] = (byte)((FragmentOffset << 5) & 0x0F);
                bPacket[8] = (byte)(this.iTimeToLive);
                bPacket[9] = (byte)((int)this.iProtocol);

                byte[] uiChecksum = this.HeaderChecksum;

                bPacket[10] = uiChecksum[0];
                bPacket[11] = uiChecksum[1];
                bPacket[12] = ipaSource.GetAddressBytes()[0];
                bPacket[13] = ipaSource.GetAddressBytes()[1];
                bPacket[14] = ipaSource.GetAddressBytes()[2];
                bPacket[15] = ipaSource.GetAddressBytes()[3];
                bPacket[16] = ipaDestination.GetAddressBytes()[0];
                bPacket[17] = ipaDestination.GetAddressBytes()[1];
                bPacket[18] = ipaDestination.GetAddressBytes()[2];
                bPacket[19] = ipaDestination.GetAddressBytes()[3];

                this.ipoOptions.Raw.CopyTo(bPacket, 20);

                if (fEncapsulatedFrame != null)
                {
                    this.fEncapsulatedFrame.FrameBytes.CopyTo(bPacket, 20 + this.ipoOptions.OptionLength);
                }

                return bPacket;
            }
        }

        /// <summary>
        /// Gets or sets the IPv4 options of this frame
        /// </summary>
        public IPv4Options Options
        {
            get { return ipoOptions; }
        }

        /// <summary>
        /// Gets or sets the destination IP-address of this frame
        /// </summary>
        public override IPAddress DestinationAddress
        {
            get { return ipaDestination; }
            set
            {
                if (value.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    throw new ArgumentException("Only assigning IPv4 addresses to an IPv4 frame is possible.");
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
                if (value.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    throw new ArgumentException("Only assigning IPv4 addresses to an IPv4 frame is possible.");
                }
                ipaSource = value;
            }
        }

        /// <summary>
        /// Gets the calculated header checksum of this frame.
        /// </summary>
        public byte[] HeaderChecksum
        {
            get
            {
                int iDataLength = this.Length;
                byte[] bHeader = new byte[20 + this.ipoOptions.OptionLength];
                bHeader[0] = (byte)((short)(this.iVersion << 4));
                bHeader[0] |= (byte)((short)(InternetHeaderLength));
                bHeader[1] = this.tosTypeOfService.Raw;
                bHeader[2] = (byte)((iDataLength >> 8) & 0xFF);
                bHeader[3] = (byte)(iDataLength & 0xFF);
                bHeader[4] = (byte)((this.iIdentification >> 8) & 0xFF);
                bHeader[5] = (byte)(this.iIdentification & 0xFF);
                bHeader[6] = (byte)(Convert.ToInt32(this.ifFlags.DontFragment) << 6);
                bHeader[6] |= (byte)(Convert.ToInt32(this.ifFlags.MoreFragments) << 5);
                bHeader[6] |= (byte)((FragmentOffset >> 3) & 0x0F);
                bHeader[7] = (byte)((FragmentOffset << 5) & 0x0F);
                bHeader[8] = (byte)(this.iTimeToLive);
                bHeader[9] = (byte)((int)this.iProtocol);
                bHeader[10] = 0;
                bHeader[11] = 0;
                bHeader[12] = ipaSource.GetAddressBytes()[0];
                bHeader[13] = ipaSource.GetAddressBytes()[1];
                bHeader[14] = ipaSource.GetAddressBytes()[2];
                bHeader[15] = ipaSource.GetAddressBytes()[3];
                bHeader[16] = ipaDestination.GetAddressBytes()[0];
                bHeader[17] = ipaDestination.GetAddressBytes()[1];
                bHeader[18] = ipaDestination.GetAddressBytes()[2];
                bHeader[19] = ipaDestination.GetAddressBytes()[3];

                this.ipoOptions.Raw.CopyTo(bHeader, 20);

                return clCalc.CalculateChecksum(bHeader);
            }
        }

        /// <summary>
        /// Gets or sets the protocol of this frame
        /// </summary>
        public override IPProtocol Protocol
        {
            get { return iProtocol; }
            set { iProtocol = value; }
        }

        /// <summary>
        /// Gets or sets the IP packet flags of this frame
        /// </summary>
        public IPFlags PacketFlags
        {
            get { return ifFlags; }
            set { ifFlags = value; }
        }

        /// <summary>
        /// Gets or sets the time to live
        /// </summary>
        public override int TimeToLive
        {
            get { return iTimeToLive; }
            set { iTimeToLive = value; }
        }

        /// <summary>
        /// Gets or sets the identification
        /// </summary>
        public uint Identification
        {
            get { return iIdentification; }
            set { iIdentification = value; }
        }

        /// <summary>
        /// Gets or sets the total packet length in bytes
        /// </summary>
        public int TotalPacketLength
        {
            get { return this.Length; }
        }

        /// <summary>
        /// Gets or sets the IP type of service
        /// </summary>
        public IPTypeOfService TypeOfService
        {
            get { return tosTypeOfService; }
            set { tosTypeOfService = value; }
        }

        /// <summary>
        /// Gets the internet header length in 32 bit words
        /// </summary>
        public short InternetHeaderLength
        {
            get { return (short)((20 + this.ipoOptions.OptionLength) / 4); }
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
        /// Gets or sets the length of this frame and its encapsulated frame in bytes
        /// </summary>
        public override int Length
        {
            get
            {
                int iDataLength = fEncapsulatedFrame != null ? fEncapsulatedFrame.Length : 0;
                return iDataLength + 20 + this.ipoOptions.OptionLength;
            }
        }

        #endregion

        /// <summary>
        /// Returns an identical copy of this frame
        /// </summary>
        /// <returns>An identical copy of this frame</returns>
        public override Frame Clone()
        {
            return new IPv4Frame(this.FrameBytes);
        }
    }

    /// <summary>
    /// This class represents the IP type of service fields
    /// </summary>
    public class IPTypeOfService
    {
        private IPPrecedence iPrecedence;
        private IPDelay iDelay;
        private IPThroughput iThroughput;
        private IPReliability iReliability;

        #region Props

        /// <summary>
        /// Gets the raw byte representation of this structure
        /// </summary>
        public byte Raw
        {
            get
            {
                byte newBytes = Convert.ToByte((int)iReliability << 2);
                newBytes |= Convert.ToByte((int)iPrecedence << 5);
                newBytes |= Convert.ToByte((int)iDelay << 4);
                newBytes |= Convert.ToByte((int)iThroughput << 3);
                return newBytes;
            }
        }

        /// <summary>
        /// Gets or sets the IP packet reliability
        /// </summary>
        public IPReliability PacketReliablility
        {
            get { return iReliability; }
            set
            {
                iReliability = value;
            }
        }

        /// <summary>
        /// Gets or sets the IP packet throughput
        /// </summary>
        public IPThroughput PacketThroughput
        {
            get { return iThroughput; }
            set
            {
                iThroughput = value;
            }
        }

        /// <summary>
        /// Gets or sets the IP packet delay
        /// </summary>
        public IPDelay PacketDelay
        {
            get { return iDelay; }
            set
            {
                iDelay = value;
            }
        }

        /// <summary>
        /// Gets or sets the IP precedence
        /// </summary>
        public IPPrecedence PacketPrecedence
        {
            get { return iPrecedence; }
            set
            {
                iPrecedence = value;
            }
        }

        #endregion

        /// <summary>
        /// Creates a new instance of this class from the given data
        /// </summary>
        /// <param name="bRaw">The data to parse</param>
        public IPTypeOfService(byte bRaw)
        {
            this.iPrecedence = (IPPrecedence)((bRaw & 0xE0) >> 5); // 11100000 ==> Shift 5 ==> 111
            this.iDelay = (IPDelay)((bRaw & 0x10) >> 4); // 0010000 ==> Shift 4 ==> 1
            this.iThroughput = (IPThroughput)((bRaw & 0x8) >> 3); // 00001000 ==> Shift 3 ==> 1
            this.iReliability = (IPReliability)((bRaw & 0x4) >> 2); // 00000100 ==> Shift 2 ==> 1
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public IPTypeOfService()
        {
            this.iPrecedence = IPPrecedence.Routine;
            this.iDelay = IPDelay.Normal;
            this.iThroughput = IPThroughput.Normal;
            this.iReliability = IPReliability.Normal;
        }

        /// <summary>
        /// Returns a string representation of this class.
        /// </summary>
        /// <returns>A string representation of this class</returns>
        public override string ToString()
        {
            string strDescription = "";
            strDescription += "TOS Packet Delay: " + this.PacketDelay + "\n";
            strDescription += "TOS Packet Precedence: " + this.PacketPrecedence + "\n";
            strDescription += "TOS Packet Reliablility: " + this.PacketReliablility + "\n";
            strDescription += "TOS Packet Throughput: " + this.PacketThroughput + "\n";
            return strDescription;
        }
    }

    /// <summary>
    /// This class represents the IP flags of an IP frame
    /// </summary>
    public class IPFlags
    {
        private bool bDontFragment;
        private bool bMoreFragments;

        /// <summary>
        /// Gets or sets a bool indicating whether the more fragments bit is set.
        /// </summary>
        public bool MoreFragments
        {
            get { return bMoreFragments; }
            set { bMoreFragments = value; }
        }

        /// <summary>
        /// Gets or sets a bool indicating whether the don't fragment bit is set.
        /// </summary>
        public bool DontFragment
        {
            get { return bDontFragment; }
            set { bDontFragment = value; }
        }

        /// <summary>
        /// Creates a new instance of this class with the given values.
        /// </summary>
        /// <param name="DontFragment">The value for the don't fragment bit</param>
        /// <param name="MoreFragments">The value for the more fragments bit</param>
        public IPFlags(bool DontFragment, bool MoreFragments)
        {
            bDontFragment = DontFragment;
            bMoreFragments = MoreFragments;
        }

        /// <summary>
        /// Returns a string representation of this structure
        /// </summary>
        /// <returns>A string representation of this structure</returns>
        public override string ToString()
        {
            string strDescription = "";
            strDescription += "IP Flags don't fragment: " + this.DontFragment + "\n";
            strDescription += "IP Flags more fragments: " + this.MoreFragments + "\n";
            return strDescription;
        }
    }
}