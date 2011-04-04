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
    public class IPv4Frame : Frame
    {
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
                    bOptionData[iC1] = bRaw[iC1 +  20];
                }
                this.ipoOptions = new IPv4Options(bOptionData);
            }
            else
            {
                this.ipoOptions = new IPv4Options() ;
            }

            byte[] bData = new byte[iTotalLength - iHeaderLength];
            for (int iC1 = iHeaderLength; iC1 < iTotalLength; iC1++)
            {
                bData[iC1 - iHeaderLength] = bRaw[iC1];
            }
            if (this.iProtocol == IPProtocol.TCP)
            {
                this.fEncapsulatedFrame = new TCPFrame(bData);
            }
            else if (this.iProtocol == IPProtocol.UDP)
            {
                this.fEncapsulatedFrame = new UDPFrame(bData);
            }
            else if (this.iProtocol == IPProtocol.ICMP)
            {
                this.fEncapsulatedFrame = new ICMPv4Frame(bData);
            }
            else if (this.iProtocol == IPProtocol.OSPF)
            {
                this.fEncapsulatedFrame = new OSPFCommonHeader(bData);
            }
            else
            {
                this.fEncapsulatedFrame = new RawDataFrame(bData);
            }

            this.clCalc = new ChecksumCalculator();
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
            this.iTimeToLive = 128;
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
        public byte[] GetPseudoHeader()
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

        /// <summary>
        /// Gets or sets the fragment offset
        /// </summary>
        public ushort FragmentOffset
        {
            get { return iFragmentOffset; }
            set { iFragmentOffset = value; }
        }

        /// <summary>
        /// Returns FrameType.IP
        /// </summary>
        public override FrameType FrameType
        {
            get { return FrameType.IP; }
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
        /// Gets the IPv4 options of this frame
        /// </summary>
        public IPv4Options Options
        {
            get { return ipoOptions; }
        }

        /// <summary>
        /// Gets the destination IP-address of this frame
        /// </summary>
        public IPAddress DestinationAddress
        {
            get { return ipaDestination; }
            set { ipaDestination = value;  }
        }
	
        /// <summary>
        /// Gets the source IP-address of this frame
        /// </summary>
        public IPAddress SourceAddress
        {
            get { return ipaSource; }
            set { ipaSource = value; }
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
        public IPProtocol Protocol
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
        public int TimeToLive
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
	    /// Gets the total packet length in bytes
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
        /// Gets or sets the IP version (4 for this frame)
        /// </summary>
        public int Version
        {
            get { return iVersion; }
            set { iVersion = value; }
        }

        /// <summary>
        /// Gets the length of this frame and its encapsulated frame in bytes
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
        /// Returns the string representation of this frame
        /// </summary>
        /// <returns>The string representation of this frame</returns>
        public override string ToString()
        {
            string strDescription = this.FrameType.ToString() + ":\n";
            strDescription += "Source: " + this.SourceAddress.ToString() + "\n";
            strDescription += "Destination: " + this.DestinationAddress.ToString() + "\n";
            strDescription += "Protocol: " + this.Protocol + "\n";
            strDescription += this.ifFlags.ToString();
            strDescription += "Identification: " + this.Identification + "\n";
            strDescription += "IHL: " + this.InternetHeaderLength + "\n";
            strDescription += "Header checksum: " + BitConverter.ToInt16(this.HeaderChecksum, 0).ToString() + "\n";
            strDescription += "TTL: " + this.TimeToLive + "\n";
            strDescription += "Total length: " + this.TotalPacketLength + "\n";
            strDescription += this.TypeOfService.ToString();
            strDescription += "Version: " + this.Version + "\n";
            strDescription += "Fragment offset: " + this.FragmentOffset + "\n";
            strDescription += this.Options.ToString();
            return strDescription;
        }

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

    #region Enums

    /// <summary>
    /// An enumeration for IP protocols
    /// </summary>
    public enum IPProtocol
    {
        /// <summary>
        /// IPv6 Hop-by-Hop Option
        /// </summary>
        HOPOPT = 0,
        /// <summary>
        /// Internet Control Message Protocol
        /// </summary>
        ICMP = 1,
        /// <summary>
        /// Internet Group Management Protocol
        /// </summary>
        IGMP = 2,
        /// <summary>
        /// Gateway-To-Gateway
        /// </summary>
        GGP = 3,
        /// <summary>
        /// IP in IP encapsulation
        /// </summary>
        IP = 4,
        /// <summary>
        /// Stream
        /// </summary>
        Stream = 5,
        /// <summary>
        /// Transmission control protocol
        /// </summary>
        TCP = 6,
        /// <summary>
        /// Core based trees
        /// </summary>
        CBT = 7,
        /// <summary>
        /// Exterior Gateway Protocol
        /// </summary>
        EGP = 8,
        /// <summary>
        /// Any Interior Gateway Protocol
        /// </summary>
        IGP = 9,
        /// <summary>
        /// DDN RCC Monitoring
        /// </summary>
        BBN_RCC_MON = 10,
        /// <summary>
        /// Network Voice Protocol
        /// </summary>
        NVP_II = 11,
        /// <summary>
        /// PARC Universal Protocol
        /// </summary>
        PUP = 12,
        /// <summary>
        /// ARGUS Protocol
        /// </summary>
        ARGUS = 13,
        /// <summary>
        /// Emission Control Protocol
        /// </summary>
        EMCON = 14,
        /// <summary>
        /// Cross Net Debugger
        /// </summary>
        XNET = 15,
        /// <summary>
        /// CHAOS Protocol
        /// </summary>
        CHAOS = 16,
        /// <summary>
        /// User Datagram Protocol
        /// </summary>
        UDP = 17,
        /// <summary>
        /// Multiplexing Protocol
        /// </summary>
        MUX = 18,
        /// <summary>
        /// DCN Measurement Subsystems Protocol
        /// </summary>
        DCN_MEAS = 19,
        /// <summary>
        /// Host Monitoring Protocol
        /// </summary>
        HMP = 20,
        /// <summary>
        /// Packet Radio Measurement
        /// </summary>
        PRM = 21,
        /// <summary>
        /// Xerox NS IDP
        /// </summary>
        XNS_IDP = 22,
        /// <summary>
        /// Trunk-1 Protocol
        /// </summary>
        TRUNK_1 = 23,
        /// <summary>
        /// Trunk-2 Protocol
        /// </summary>
        TRUNK_2 = 24,
        /// <summary>
        /// Leaf-1 Protocol
        /// </summary>
        LEAF_1 = 25,
        /// <summary>
        /// Leaf-2 Protocol
        /// </summary>
        LEAF_2 = 26,
        /// <summary>
        /// Reliable Data Protocol
        /// </summary>
        RDP = 27,
        /// <summary>
        /// Internet Reliable Transaction Protocol
        /// </summary>
        IRTP = 28,
        /// <summary>
        /// ISO Transport Protocol Class 4
        /// </summary>
        ISO_TP4 = 29,
        /// <summary>
        /// Bulk Data Transfer Protocol
        /// </summary>
        NETBLT = 30,
        /// <summary>
        /// MFE Network Services Protocol
        /// </summary>
        MFE_NSP = 31,
        /// <summary>
        /// MERIT Internodal Protocol
        /// </summary>
        MERIT_INP = 32,
        /// <summary>
        /// Datagram Congestion Control Protocol
        /// </summary>
        DCCP = 33,
        /// <summary>
        /// Third Party Connect Protocol
        /// </summary>
        _3PC = 34,
        /// <summary>
        /// Inter-Domain Policy Routing Protocol
        /// </summary>
        IDPR = 35,
        /// <summary>
        /// XTP Protocol
        /// </summary>
        XTP = 36,
        /// <summary>
        /// Datagram Delivery Protocol
        /// </summary>
        DDP = 37,
        /// <summary>
        /// IDPR Control Message Transport Protocol
        /// </summary>
        IDPR_CMTP = 38,
        /// <summary>
        /// TP++ Transport Protocol
        /// </summary>
        TP_PLUSPLUS = 39,
        /// <summary>
        /// IL Transport Protocol
        /// </summary>
        IL = 40, 
        /// <summary>
        /// IPv6 in IP encapsulation
        /// </summary>
        IPv6 = 41,
        /// <summary>
        /// Source Demand Routing Protocol
        /// </summary>
        SDRP = 42,
        /// <summary>
        /// Routing Header for IPv6
        /// </summary>
        IPv6_Route = 43,
        /// <summary>
        /// Fragment Header for IPv6
        /// </summary>
        IPv6_Frag = 44,
        /// <summary>
        /// Inter-Domain Routing Protocol
        /// </summary>
        IDRP = 45,
        /// <summary>
        /// Reservation Protocol
        /// </summary>
        RSVP = 46, 
        /// <summary>
        /// Generic Routing Encapsulation
        /// </summary>
        GRE = 47, 
        /// <summary>
        /// Mobile Host Routing Protocol
        /// </summary>
        MHRP = 48, 
        /// <summary>
        /// BNA Protocol
        /// </summary>
        BNA = 49,
        /// <summary>
        /// Encap Security Payload
        /// </summary>
        ESP = 50,
        /// <summary>
        /// Authentication Header
        /// </summary>
        AH = 51,
        /// <summary>
        /// Integrated Net Layer Security TUBA
        /// </summary>
        I_NLSP = 52,
        /// <summary>
        /// IP with Encryption
        /// </summary>
        SWIPE = 53,
        /// <summary>
        /// NBMA Address Resolution Protocol
        /// </summary>
        NARP = 54,
        /// <summary>
        /// IP Mobility
        /// </summary>
        MOBILE = 55,
        /// <summary>
        /// Transport Layer Security Protocol
        /// </summary>
        TLSP = 56,
        /// <summary>
        /// SKIP Protocol
        /// </summary>
        SKIP = 57,
        /// <summary>
        /// ICMP for IPv6
        /// </summary>
        IPv6_ICMP = 58,
        /// <summary>
        /// No next header for IPv6
        /// </summary>
        IPv6_NoNxt = 59,
        /// <summary>
        /// Destination Options for IPv6
        /// </summary>
        IPv6_Opts = 60,
        /// <summary>
        /// Every host internal protocol
        /// </summary>
        AnyHostInternalProtocol = 61,
        /// <summary>
        /// CFTP Protocol
        /// </summary>
        CFTP = 62,
        /// <summary>
        /// Any local network
        /// </summary>
        AnyLocalNetwork = 63,
        /// <summary>
        /// SATNET and Backroom EXPAK
        /// </summary>
        SAT_EXPAK = 64,
        /// <summary>
        /// KRYPTOLAN Protocol
        /// </summary>
        KRYPTOPLAN = 65,
        /// <summary>
        /// MIT Remote Virtual Disk Protocol
        /// </summary>
        RVD = 66,
        /// <summary>
        /// Internet Pluribus Packet Core
        /// </summary>
        IPPC = 67,
        /// <summary>
        /// Any distributed file system
        /// </summary>
        AnyDistributedFileSystem = 68,
        /// <summary>
        /// SATNET Monitoring
        /// </summary>
        SAT_MON = 69,
        /// <summary>
        /// VISA Protocol
        /// </summary>
        VISA = 70,
        /// <summary>
        /// Internet Packet Core Utility
        /// </summary>
        IPCV = 71,
        /// <summary>
        /// Computer Protocol Network Executive
        /// </summary>
        CPNX = 72,
        /// <summary>
        /// Computer Protocol Heart Beat
        /// </summary>
        CPHB = 73,
        /// <summary>
        /// Wang Span Network
        /// </summary>
        WSN = 74,
        /// <summary>
        /// Packet Video Protocol
        /// </summary>
        PVP = 75,
        /// <summary>
        /// Backroom SATNET Monitoring
        /// </summary>
        BR_SAT_MON = 76,
        /// <summary>
        /// SUN ND PROTOCOL-Temporary
        /// </summary>
        SUN_ND = 77,
        /// <summary>
        /// WIDEBAND Monitoring
        /// </summary>
        WB_MON = 78,
        /// <summary>
        /// WIDEBAND EXPAK
        /// </summary>
        WB_EXPAK = 79,
        /// <summary>
        /// ISO Internet Protocol
        /// </summary>
        ISO_IP = 80,
        /// <summary>
        /// Versatile Message Transaction Protocol
        /// </summary>
        VMTP = 81,
        /// <summary>
        /// Secure Versatile Message Transaction Protocol
        /// </summary>
        SECURE_VMTP = 82,
        /// <summary>
        /// VINES Protocol
        /// </summary>
        VINES = 83,
        /// <summary>
        /// Time Triggered Protocol
        /// </summary>
        TTP = 84,
        /// <summary>
        /// NSFNET Interior Gateway Protocol
        /// </summary>
        NSFNET_IGP = 85, 
        /// <summary>
        /// Dissimilar Gateway Protocol
        /// </summary>
        DGP = 86,
        /// <summary>
        /// TCF Protocol
        /// </summary>
        TCF = 87,
        /// <summary>
        /// Enhanced Interior Gateway Routing Protocol
        /// </summary>
        EIGRP = 88,
        /// <summary>
        /// Open shortest path first
        /// </summary>
        OSPF = 89,
        /// <summary>
        /// Sprite RPC Protocol
        /// </summary>
        Sprite_RPC = 90,
        /// <summary>
        /// Locus Address Resolution Protocol
        /// </summary>
        LARP = 91,
        /// <summary>
        /// Multicast Transport Protocol
        /// </summary>
        MTP = 92, 
        /// <summary>
        /// AX.25 Frames
        /// </summary>
        AX_25 = 93,
        /// <summary>
        /// IP-within-IP Encapsulation Protocol
        /// </summary>
        IPIP = 94,
        /// <summary>
        /// Mobile Internetworking Control Pro
        /// </summary>
        MICP = 95,
        /// <summary>
        /// Semaphore Communications Secure Protocol
        /// </summary>
        SSC_SP = 96,
        /// <summary>
        /// Ethernet-within-IP Encapsulation
        /// </summary>
        ETHERIP = 97,
        /// <summary>
        /// Encapsulation Header
        /// </summary>
        ENCAP = 98,
        /// <summary>
        /// Any private encryption scheme
        /// </summary>
        AnyPrivateEncryptionScheme = 99,
        /// <summary>
        /// GMTP Protocol
        /// </summary>
        GMTP = 100,
        /// <summary>
        /// Ipsilon Flow Management Protocol
        /// </summary>
        IFMP = 101,
        /// <summary>
        /// PPNI over IP
        /// </summary>
        PNNI = 102,
        /// <summary>
        /// Protocol Independent Multicast
        /// </summary>
        PIM = 103,
        /// <summary>
        /// ARIS Protocol
        /// </summary>
        ARIS = 104,
        /// <summary>
        /// SCPS Protocol
        /// </summary>
        SCPS = 105,
        /// <summary>
        /// QNX Protocol
        /// </summary>
        QNX = 106,
        /// <summary>
        /// Active Networks
        /// </summary>
        ActiveNetworks = 107,
        /// <summary>
        /// IP Payload Compression Protocol
        /// </summary>
        IPComp = 108,
        /// <summary>
        /// Sitara Networks Protocol
        /// </summary>
        SNP = 109,
        /// <summary>
        /// Compaq Peer Protocol
        /// </summary>
        Compaq_Peer = 110,
        /// <summary>
        /// IPX in IP
        /// </summary>
        IPX_in_IP = 111,
        /// <summary>
        /// Virtual Router Redundancy Protocol
        /// </summary>
        VRRP = 112,
        /// <summary>
        /// PGM Reliable Transport Protocol
        /// </summary>
        PGM = 113,
        /// <summary>
        /// any 0-hop protocol
        /// </summary>
        AnyZeroHopProtocol = 114,
        /// <summary>
        /// Layer Two Tunneling Protocol
        /// </summary>
        L2TP = 115,
        /// <summary>
        /// D-II Data Exchange (DDX
        /// </summary>
        DDX = 116,
        /// <summary>
        /// Interactive Agent Transfer Protocol
        /// </summary>
        IATP = 117,
        /// <summary>
        /// Schedule Transfer Protocol
        /// </summary>
        STP = 118,
        /// <summary>
        /// SpectraLink Radio Protocol
        /// </summary>
        SRP = 119,
        /// <summary>
        /// UTI Protocol
        /// </summary>
        UTI = 120,
        /// <summary>
        /// Simple Message Protocol
        /// </summary>
        SMP = 121,
        /// <summary>
        /// SM Protocol
        /// </summary>
        SM = 122,
        /// <summary>
        /// Performance Transparency Protocol
        /// </summary>
        PTP = 123,
        /// <summary>
        /// ISIS over IPv4
        /// </summary>
        ISIS_over_IPv4 = 124,
        /// <summary>
        /// FIRE Protocol
        /// </summary>
        FIRE = 125,
        /// <summary>
        /// Combat Radio Transport Protocol
        /// </summary>
        CRTP = 126,
        /// <summary>
        /// Combat Radio User Datagram
        /// </summary>
        CRUDP = 127,
        /// <summary>
        /// SSCOPMCE Protocol
        /// </summary>
        SSCOPMCE = 128,
        /// <summary>
        /// IPLT Protocol
        /// </summary>
        IPLT = 129,
        /// <summary>
        /// Secure Packet Shield
        /// </summary>
        SPS = 130,
        /// <summary>
        /// Private IP Encapsulation within IP
        /// </summary>
        PIPE = 131,
        /// <summary>
        /// Stream Control Transmission Protocol
        /// </summary>
        SCTP = 132,
        /// <summary>
        /// Fibre Channel
        /// </summary>
        FC = 133,
        /// <summary>
        /// RSVP-E2E-IGNORE Protocol
        /// </summary>
        RSVP_E2E_IGNORE = 134,
        /// <summary>
        /// Mobility Header
        /// </summary>
        MobilityHeader = 135,
        /// <summary>
        /// UDP Lite
        /// </summary>
        UDPLite = 136,
        /// <summary>
        /// MPLS in IP
        /// </summary>
        MPLS_in_IP = 137,
        /// <summary>
        /// MANET Protocols
        /// </summary>
        manet = 138,
        /// <summary>
        /// Host Identity Protocol
        /// </summary>
        HIP = 139,
        /// <summary>
        /// Shim6 Protocol
        /// </summary>
        Shim6 = 140,
        /// <summary>
        /// Reserved for future use
        /// </summary>
        Reserved = 255,
        /// <summary>
        /// Unknown protocol
        /// </summary>
        Other = -1
    }

    /// <summary>
    /// Enumeration for varios values of IP precedence
    /// </summary>
    public enum IPPrecedence
    {
        /// <summary>
        /// Routine priority
        /// </summary>
        Routine = 0,
        /// <summary>
        /// Priority
        /// </summary>
        Priority = 1,
        /// <summary>
        /// IMmediate
        /// </summary>
        Immediate = 2,
        /// <summary>
        /// Flash
        /// </summary>
        Flash = 3,
        /// <summary>
        /// Flash override
        /// </summary>
        FlashOverride = 4,
        /// <summary>
        /// Critic ECP
        /// </summary>
        CRITIC_ECP = 5,
        /// <summary>
        /// Internetwork Control
        /// </summary>
        InternetworkControl = 6,
        /// <summary>
        /// Network Control
        /// </summary>
        NetworkControl = 7
    }

    /// <summary>
    /// Enumeration for the IP delay
    /// </summary>
    public enum IPDelay
    {
        /// <summary>
        /// Normal
        /// </summary>
        Normal = 0,
        /// <summary>
        /// Low
        /// </summary>
        Low = 1
    }

    /// <summary>
    /// Enumeration for the IP throughput
    /// </summary>
    public enum IPThroughput
    {
        /// <summary>
        /// Normal
        /// </summary>
        Normal = 0,
        /// <summary>
        /// High
        /// </summary>
        High = 1
    }

    /// <summary>
    /// Enumeration for the IP reliability
    /// </summary>
    public enum IPReliability
    {
        /// <summary>
        /// Normal
        /// </summary>
        Normal = 0,
        /// <summary>
        /// High
        /// </summary>
        High = 1
    }

    #endregion
}
