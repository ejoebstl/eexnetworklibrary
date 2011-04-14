using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using eExNetworkLibrary.Sockets;
using eExNetworkLibrary.IP.V6;

namespace eExNetworkLibrary.IP
{
    public abstract class IPFrame : Frame, IIPHeader
    {
        /// <summary>
        /// Gets or sets the destination IP-address of this frame.
        /// </summary>
        public abstract IPAddress DestinationAddress { get; set; }

        /// <summary>
        /// Gets or sets the source IP-address of this frame.
        /// </summary>
        public abstract IPAddress SourceAddress { get; set; }

        
        /// <summary>
        /// Gets or sets the IP version of this frame.
        /// </summary>
        public abstract int Version { get; set; }

        /// <summary>
        /// Gets or sets the payload protocol of this IP frame. This field corresponds to the NextHeader field of the IPv6 frame.
        /// </summary>
        public abstract IPProtocol Protocol { get; set; }

        /// <summary>
        /// Returns the pseudo header for this frame.
        /// This header can be used to calculate TCP and UDP checksums.
        /// </summary>
        /// <returns>The IP pseudo header of this instance.</returns>
        public abstract byte[] GetPseudoHeader();

        /// <summary>
        /// Gets or sets the TTL of this IP frame. This field corresponds to the HopLimit field of the IPv6 frame.
        /// </summary>
        public abstract int TimeToLive { get; set; }

        /// <summary>
        /// Returns the string representation of this frame
        /// </summary>
        /// <returns>The string representation of this frame</returns>
        public override string ToString()
        {
            return "IPFrame [version: "+ Version.ToString() + ", source: " + SourceAddress.ToString() + ", destination: " + DestinationAddress.ToString() + "]";
        }

        public static IPFrame Create(byte[] bData)
        {
            int iVersion = (bData[0] & 0xF0) >> 4;

            if (iVersion == 4)
            {
                return new IPv4Frame(bData);
            }
            else if (iVersion == 6)
            {
                return new IPv6Frame(bData);
            }

            throw new ArgumentException("The bytes submitted indicate no valid IP version (6 or 4, submitted data indicates " + iVersion + ").");
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
        /// Internet Control Message Protocol version 4
        /// </summary>
        ICMP = 1,
        /// <summary>
        /// Internet Group Management Protocol version 4
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

