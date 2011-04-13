using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary
{
    /// <summary>
    /// This class represents an abstract captured network frame.
    /// </summary>
    public abstract class Frame
    {
        /// <summary>
        /// The frame encapsulated in this frame.
        /// </summary>
        protected Frame fEncapsulatedFrame;

        /// <summary>
        /// Must return the type of this frame as string.
        /// </summary>
        public abstract string FrameType { get; }

        /// <summary>
        /// Must return this frame and its encapsulated frames converted to bytes.
        /// </summary>
        public abstract byte[] FrameBytes { get; }

        /// <summary>
        /// Gets or sets the frame encapsulated in this frame
        /// </summary>
        public Frame EncapsulatedFrame
        {
            get { return fEncapsulatedFrame; }
            set { fEncapsulatedFrame = value; }
        }

        /// <summary>
        /// Copies the given data into a raw data frame and sets it as the encapsulated frame. If the given parameters would result in an empty frame, the encapsulated frame is set to null instead.
        /// </summary>
        /// <param name="bData">The data to copy.</param>
        /// <param name="iStartIndex">The index at which copying begins.</param>
        protected void Encapsulate(byte[] bData, int iStartIndex)
        {
            if (bData.Length - iStartIndex == 0)
            {
                this.fEncapsulatedFrame = null;
            }
            else
            {
                this.fEncapsulatedFrame = new RawDataFrame(bData, iStartIndex, bData.Length - iStartIndex);
            }
        }

        /// <summary>
        /// Must return the length of the bytes contained in this frame and its encapsulated frames
        /// </summary>
        public abstract int Length { get; }

        /// <summary>
        /// Must return an identical copy of this frame.
        /// </summary>
        /// <returns>An identic clone of this frame</returns>
        public abstract Frame Clone();
    }

    /// <summary>
    /// Defines several well-known frame types.
    /// </summary>
    public static class FrameTypes
    {
        public static bool IsIP(Frame fFrame)
        {
            return fFrame.FrameType == FrameTypes.IPv6 || fFrame.FrameType == FrameTypes.IPv4;
        }

        /// <summary>
        /// A traffic description frame
        /// </summary>
        public static string TrafficDescriptionFrame { get { return "TrafficDescriptionFrame"; } }
        /// <summary>
        /// An ethernet frame
        /// </summary>
        public static string Ethernet { get { return "Ethernet"; } }
        /// <summary>
        /// Internet protocol version 4
        /// </summary>
        public static string IPv4 { get { return "IPv4"; } }
        /// <summary>
        /// Internet protocol version 6
        /// </summary>
        public static string IPv6 { get { return "IPv6"; } }
        /// <summary>
        /// User datagram protocol
        /// </summary>
        public static string UDP { get { return "UDP"; } }
        /// <summary>
        /// Transmission control protocol
        /// </summary>
        public static string TCP { get { return "TCP"; } }
        /// <summary>
        /// A raw data (unparsed) frame
        /// </summary>
        public static string Raw { get { return "RAW"; } }
        /// <summary>
        /// An ARP frame
        /// </summary>
        public static string ARP { get { return "ARP"; } }
        /// <summary>
        /// A RIP frame
        /// </summary>
        public static string RIP { get { return "RIP"; } }
        /// <summary>
        /// A DHCP frame
        /// </summary>
        public static string DHCP { get { return "DHCP"; } }
        /// <summary>
        /// Internet Control Message Protocol version 4
        /// </summary>
        public static string ICMPv4{ get { return "ICMPv4"; } }
        /// <summary>
        /// Internet Control Message Protocol version 6
        /// </summary>
        public static string ICMPv6 { get { return "ICMPv6"; } }
        /// <summary>
        /// An OSPF header
        /// </summary>
        public static string OSPF { get { return "OSPF"; } }
        /// <summary>
        /// A DNS frame
        /// </summary>
        public static string DNS { get { return "DNS"; } }

        public static string RARP { get { return "RARP"; } }
        public static string IPX { get { return "IPX"; } }
        public static string AppleTalk { get { return "AppleTalk"; } }
        public static string AARP { get { return "AARP"; } }
        public static string Novell { get { return "Novell"; } }
        public static string EthernetVlanTag { get { return "EthernetVlanTag"; } }

        /// <summary>
        /// IPv6 Hop-by-Hop Option
        /// </summary>
        public static string IPv6HOPOPT { get { return "IPv6HOPOPT"; } }
        /// <summary>
        /// Internet Group Management Protocol version 4
        /// </summary>
        public static string IGMPv4 { get { return "IGMPv4"; } }
        /// <summary>
        /// Gateway-To-Gateway
        /// </summary>
        public static string GGP { get { return "GGP"; } }
        /// <summary>
        /// Core based trees
        /// </summary>
        public static string CBT { get { return "CBT"; } }
        /// <summary>
        /// Exterior Gateway Protocol
        /// </summary>
        public static string EGP { get { return "EGP"; } }
        /// <summary>
        /// Any Interior Gateway Protocol
        /// </summary>
        public static string IGP { get { return "IGP"; } }
        /// <summary>
        /// Network Voice Protocol
        /// </summary>
        public static string NVP_II { get { return "NVP_II"; } }
        /// <summary>
        /// PARC Universal Protocol
        /// </summary>
        public static string PUP { get { return "PUP"; } }
        /// <summary>
        /// ARGUS Protocol
        /// </summary>
        public static string ARGUS { get { return "ARGUS"; } }
        /// <summary>
        /// Emission Control Protocol
        /// </summary>
        public static string EMCON { get { return "EMCON"; } }
        /// <summary>
        /// Cross Net Debugger
        /// </summary>
        public static string XNET { get { return "XNET"; } }
        /// <summary>
        /// CHAOS Protocol
        /// </summary>
        public static string CHAOS { get { return "CHAOS"; } }
        /// <summary>
        /// Multiplexing Protocol
        /// </summary>
        public static string MUX { get { return "MUX"; } }
        /// <summary>
        /// Host Monitoring Protocol
        /// </summary>
        public static string HMP { get { return "HMP"; } }
        /// <summary>
        /// Packet Radio Measurement
        /// </summary>
        public static string PRM { get { return "PRM"; } }
        /// <summary>
        /// Xerox NS IDP
        /// </summary>
        public static string XNS_IDP { get { return "XNS_IDP"; } }
        /// <summary>
        /// Reliable Data Protocol
        /// </summary>
        public static string RDP { get { return "RDP"; } }
        /// <summary>
        /// Internet Reliable Transaction Protocol
        /// </summary>
        public static string IRTP { get { return "IRTP"; } }
        /// <summary>
        /// Bulk Data Transfer Protocol
        /// </summary>
        public static string NETBLT { get { return "NETBLT"; } }
        /// <summary>
        /// MFE Network Services Protocol
        /// </summary>
        public static string MFE_NSP { get { return "MFE_NSP"; } }
        /// <summary>
        /// MERIT Internodal Protocol
        /// </summary>
        public static string MERIT_INP { get { return "MERIT_INP"; } }
        /// <summary>
        /// Datagram Congestion Control Protocol
        /// </summary>
        public static string DCCP { get { return "DCCP"; } }
        /// <summary>
        /// Inter-Domain Policy Routing Protocol
        /// </summary>
        public static string IDPR { get { return "IDPR"; } }
        /// <summary>
        /// XTP Protocol
        /// </summary>
        public static string XTP { get { return "XTP"; } }
        /// <summary>
        /// Datagram Delivery Protocol
        /// </summary>
        public static string DDP { get { return "DDP"; } }
        /// <summary>
        /// IDPR Control Message Transport Protocol
        /// </summary>
        public static string IDPR_CMTP { get { return "IDPR_CMTP"; } }
        /// <summary>
        /// TP++ Transport Protocol
        /// </summary>
        public static string TP_PLUSPLUS { get { return "TP_PLUSPLUS"; } }
        /// <summary>
        /// IL Transport Protocol
        /// </summary>
        public static string IL { get { return "IL"; } }
        /// <summary>
        /// Source Demand Routing Protocol
        /// </summary>
        public static string SDRP { get { return "SDRP"; } }
        /// <summary>
        /// Routing Header for IPv6
        /// </summary>
        public static string IPv6Route { get { return "IPv6Route"; } }
        /// <summary>
        /// Fragment Header for IPv6
        /// </summary>
        public static string IPv6Frag { get { return "IPv6Frag"; } }
        /// <summary>
        /// Inter-Domain Routing Protocol
        /// </summary>
        public static string IDRP { get { return "IDRP"; } }
        /// <summary>
        /// Reservation Protocol
        /// </summary>
        public static string RSVP { get { return "RSVP"; } }
        /// <summary>
        /// Generic Routing Encapsulation
        /// </summary>
        public static string GRE { get { return "GRE"; } }
        /// <summary>
        /// Mobile Host Routing Protocol
        /// </summary>
        public static string MHRP { get { return "MHRP"; } }
        /// <summary>
        /// BNA Protocol
        /// </summary>
        public static string BNA { get { return "BNA"; } }
        /// <summary>
        /// Encapsulated Security Payload
        /// </summary>
        public static string ESP { get { return "ESP"; } }
        /// <summary>
        /// Authentication Header
        /// </summary>
        public static string AH { get { return "AH"; } }
        /// <summary>
        /// IP with Encryption
        /// </summary>
        public static string SWIPE { get { return "SWIPE"; } }
        /// <summary>
        /// NBMA Address Resolution Protocol
        /// </summary>
        public static string NARP { get { return "NARP"; } }
        /// <summary>
        /// IP Mobility
        /// </summary>
        public static string MOBILE { get { return "MOBILE"; } }
        /// <summary>
        /// Transport Layer Security Protocol
        /// </summary>
        public static string TLSP { get { return "TLSP"; } }
        /// <summary>
        /// SKIP Protocol
        /// </summary>
        public static string SKIP { get { return "SKIP"; } }
        /// <summary>
        /// No next header for IPv6
        /// </summary>
        public static string IPv6NoNxt { get { return "IPv6NoNxt"; } }
        /// <summary>
        /// Destination Options for IPv6
        /// </summary>
        public static string IPv6Opts { get { return "IPv6Opts"; } }
        /// <summary>
        /// CFTP Protocol
        /// </summary>
        public static string CFTP { get { return "CFTP"; } }
        /// <summary>
        /// SATNET and Backroom EXPAK
        /// </summary>
        public static string SAT_EXPAK { get { return "SAT_EXPAK"; } }
        /// <summary>
        /// KRYPTOLAN Protocol
        /// </summary>
        public static string KRYPTOPLAN { get { return "KRYPTOPLAN"; } }
        /// <summary>
        /// MIT Remote Virtual Disk Protocol
        /// </summary>
        public static string RVD { get { return "RVD"; } }
        /// <summary>
        /// Internet Pluribus Packet Core
        /// </summary>
        public static string IPPC { get { return "IPPC"; } }
        /// <summary>
        /// SATNET Monitoring
        /// </summary>
        public static string SAT_MON { get { return "SAT_MON"; } }
        /// <summary>
        /// VISA Protocol
        /// </summary>
        public static string VISA { get { return "VISA"; } }
        /// <summary>
        /// Internet Packet Core Utility
        /// </summary>
        public static string IPCV { get { return "IPCV"; } }
        /// <summary>
        /// Computer Protocol Network Executive
        /// </summary>
        public static string CPNX { get { return "CPNX"; } }
        /// <summary>
        /// Computer Protocol Heart Beat
        /// </summary>
        public static string CPHB { get { return "CPHB"; } }
        /// <summary>
        /// Wang Span Network
        /// </summary>
        public static string WSN { get { return "WSN"; } }
        /// <summary>
        /// Packet Video Protocol
        /// </summary>
        public static string PVP { get { return "PVP"; } }
        /// <summary>
        /// Backroom SATNET Monitoring
        /// </summary>
        public static string BR_SAT_MON { get { return "BR_SAT_MON"; } }
        /// <summary>
        /// SUN ND PROTOCOL-Temporary
        /// </summary>
        public static string SUN_ND { get { return "SUN_ND"; } }
        /// <summary>
        /// WIDEBAND Monitoring
        /// </summary>
        public static string WB_MON { get { return "WB_MON"; } }
        /// <summary>
        /// WIDEBAND EXPAK
        /// </summary>
        public static string WB_EXPAK { get { return "WB_EXPAK"; } }
        /// <summary>
        /// ISO Internet Protocol
        /// </summary>
        public static string ISO_IP { get { return "ISO_IP"; } }
        /// <summary>
        /// Versatile Message Transaction Protocol
        /// </summary>
        public static string VMTP { get { return "VMTP"; } }
        /// <summary>
        /// Secure Versatile Message Transaction Protocol
        /// </summary>
        public static string SECURE_VMTP { get { return "SECURE_VMTP"; } }
        /// <summary>
        /// VINES Protocol
        /// </summary>
        public static string VINES { get { return "VINES"; } }
        /// <summary>
        /// Time Triggered Protocol
        /// </summary>
        public static string TTP { get { return "TTP"; } }
        /// <summary>
        /// NSFNET Interior Gateway Protocol
        /// </summary>
        public static string NSFNET_IGP { get { return "NSFNET_IGP"; } }
        /// <summary>
        /// Dissimilar Gateway Protocol
        /// </summary>
        public static string DGP { get { return "DGP"; } }
        /// <summary>
        /// TCF Protocol
        /// </summary>
        public static string TCF { get { return "TCF"; } }
        /// <summary>
        /// Enhanced Interior Gateway Routing Protocol
        /// </summary>
        public static string EIGRP { get { return "EIGRP"; } }
        /// <summary>
        /// Sprite RPC Protocol
        /// </summary>
        public static string Sprite_RPC { get { return "Sprite_RPC"; } }
        /// <summary>
        /// Locus Address Resolution Protocol
        /// </summary>
        public static string LARP { get { return "LARP"; } }
        /// <summary>
        /// Multicast Transport Protocol
        /// </summary>
        public static string MTP { get { return "MTP"; } }
        /// <summary>
        /// AX.25 Frames
        /// </summary>
        public static string AX_25 { get { return "AX_25"; } }
        /// <summary>
        /// IP-within-IP Encapsulation Protocol
        /// </summary>
        public static string IPIP { get { return "IPIP"; } }
        /// <summary>
        /// Mobile Internetworking Control Pro
        /// </summary>
        public static string MICP { get { return "MICP"; } }
        /// <summary>
        /// Semaphore Communications Secure Protocol
        /// </summary>
        public static string SSC_SP { get { return "SSC_SP"; } }
        /// <summary>
        /// Ethernet-within-IP Encapsulation
        /// </summary>
        public static string ETHERIP { get { return "ETHERIP"; } }
        /// <summary>
        /// Encapsulation Header
        /// </summary>
        public static string ENCAP { get { return "ENCAP"; } }
        /// <summary>
        /// GMTP Protocol
        /// </summary>
        public static string GMTP { get { return "GMTP"; } }
        /// <summary>
        /// Ipsilon Flow Management Protocol
        /// </summary>
        public static string IFMP { get { return "IFMP"; } }
        /// <summary>
        /// PPNI over IP
        /// </summary>
        public static string PNNI { get { return "PNNI"; } }
        /// <summary>
        /// Protocol Independent Multicast
        /// </summary>
        public static string PIM { get { return "PIM"; } }
        /// <summary>
        /// ARIS Protocol
        /// </summary>
        public static string ARIS { get { return "ARIS"; } }
        /// <summary>
        /// SCPS Protocol
        /// </summary>
        public static string SCPS { get { return "SCPS"; } }
        /// <summary>
        /// QNX Protocol
        /// </summary>
        public static string QNX { get { return "QNX"; } }
        /// <summary>
        /// Active Networks
        /// </summary>
        public static string ActiveNetworks { get { return "ActiveNetworks"; } }
        /// <summary>
        /// IP Payload Compression Protocol
        /// </summary>
        public static string IPComp { get { return "IPComp"; } }
        /// <summary>
        /// Sitara Networks Protocol
        /// </summary>
        public static string SNP { get { return "SNP"; } }
        /// <summary>
        /// Compaq Peer Protocol
        /// </summary>
        public static string Compaq_Peer { get { return "CompaqPeer"; } }
        /// <summary>
        /// IPX in IP
        /// </summary>
        public static string IPXinIP { get { return "IPXinIP"; } }
        /// <summary>
        /// Virtual Router Redundancy Protocol
        /// </summary>
        public static string VRRP { get { return "VRRP"; } }
        /// <summary>
        /// PGM Reliable Transport Protocol
        /// </summary>
        public static string PGM { get { return "PGM"; } }
        /// <summary>
        /// Layer Two Tunneling Protocol
        /// </summary>
        public static string L2TP { get { return "L2TP"; } }
        /// <summary>
        /// D-II Data Exchange (DDX
        /// </summary>
        public static string DDX { get { return "DDX"; } }
        /// <summary>
        /// Interactive Agent Transfer Protocol
        /// </summary>
        public static string IATP { get { return "IATP"; } }
        /// <summary>
        /// Schedule Transfer Protocol
        /// </summary>
        public static string STP { get { return "STP"; } }
        /// <summary>
        /// SpectraLink Radio Protocol
        /// </summary>
        public static string SRP { get { return "SRP"; } }
        /// <summary>
        /// UTI Protocol
        /// </summary>
        public static string UTI { get { return "UTI"; } }
        /// <summary>
        /// Simple Message Protocol
        /// </summary>
        public static string SMP { get { return "SMP"; } }
        /// <summary>
        /// SM Protocol
        /// </summary>
        public static string SM { get { return "SM"; } }
        /// <summary>
        /// Performance Transparency Protocol
        /// </summary>
        public static string PTP { get { return "PTP"; } }
        /// <summary>
        /// ISIS over IPv4
        /// </summary>
        public static string ISISoverIPv4 { get { return "IPv4ISIS"; } }
        /// <summary>
        /// FIRE Protocol
        /// </summary>
        public static string FIRE { get { return "FIRE"; } }
        /// <summary>
        /// Combat Radio Transport Protocol
        /// </summary>
        public static string CRTP { get { return "CRTP"; } }
        /// <summary>
        /// Combat Radio User Datagram
        /// </summary>
        public static string CRUDP { get { return "CRUDP"; } }
        /// <summary>
        /// SSCOPMCE Protocol
        /// </summary>
        public static string SSCOPMCE { get { return "SSCOPMCE"; } }
        /// <summary>
        /// IPLT Protocol
        /// </summary>
        public static string IPLT { get { return "IPLT"; } }
        /// <summary>
        /// Secure Packet Shield
        /// </summary>
        public static string SPS { get { return "SPS"; } }
        /// <summary>
        /// Private IP Encapsulation within IP
        /// </summary>
        public static string PIPE { get { return "PIPE"; } }
        /// <summary>
        /// Stream Control Transmission Protocol
        /// </summary>
        public static string SCTP { get { return "SCTP"; } }
        /// <summary>
        /// Fibre Channel
        /// </summary>
        public static string FC { get { return "FC"; } }
        /// <summary>
        /// RSVP-E2E-IGNORE Protocol
        /// </summary>
        public static string RSVP_E2E_IGNORE { get { return "RSVP_E2E_IGNORE"; } }
        /// <summary>
        /// Mobility Header
        /// </summary>
        public static string MobilityHeader { get { return "MobilityHeader"; } }
        /// <summary>
        /// UDP Lite
        /// </summary>
        public static string UDPLite { get { return "UDPLite"; } }
        /// <summary>
        /// MPLS in IP
        /// </summary>
        public static string MPLSinIP { get { return "IPMPLS"; } }
        /// <summary>
        /// Host Identity Protocol
        /// </summary>
        public static string HIP { get { return "HIP"; } }
    }
}
