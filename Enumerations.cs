using System;

namespace eExNetworkLibrary
{
    /// <summary>
    /// An enumeration for hardware address types
    /// </summary>
    public enum HardwareAddressType
    {
        /// <summary>
        /// Ethernet (MAC addresses)
        /// </summary>
        Ethernet = 1,
        /// <summary>
        /// Experimental Ethernet
        /// </summary>
        ExperimentalEthernet = 2,
        /// <summary>
        /// Amateur radio AX23
        /// </summary>
        AmateurRadioAX25 = 3,
        /// <summary>
        /// Proteon pro NET token ring
        /// </summary>
        ProteonProNETTokenRing = 4,
        /// <summary>
        /// CHAOS protocol
        /// </summary>
        Chaos = 5,
        /// <summary>
        /// IEEE 802
        /// </summary>
        IEEE_802 = 6,
        /// <summary>
        /// ARCNET
        /// </summary>
        ARCNET = 7,
        /// <summary>
        /// Hyperchannel
        /// </summary>
        Hyperchannel = 8,
        /// <summary>
        /// LAN star
        /// </summary>
        Lanstar = 9,
        /// <summary>
        /// Autonet short address
        /// </summary>
        AutonetShortAddress = 10,
        /// <summary>
        /// Local talk
        /// </summary>
        LocalTalk = 11,
        /// <summary>
        /// Local net
        /// </summary>
        LocalNet = 12,
        /// <summary>
        /// Ultra link
        /// </summary>
        Ultralink = 13,
        /// <summary>
        /// SMDS
        /// </summary>
        SMDS = 14,
        /// <summary>
        /// Frame relay
        /// </summary>
        FrameRelay = 15,
        /// <summary>
        /// Asyncronous transfer mode 1
        /// </summary>
        ATM1 = 16,
        /// <summary>
        /// HDLC
        /// </summary>
        HDLC = 17,
        /// <summary>
        /// Fibre channel
        /// </summary>
        FibreChannel = 18,
        /// <summary>
        /// Asyncronous transfer mode 2
        /// </summary>
        ATM2 = 19,
        /// <summary>
        /// Serial line
        /// </summary>
        SerialLine = 20,
        /// <summary>
        /// Asyncronous transfer mode 3
        /// </summary>
        ATM3 = 21,
        /// <summary>
        /// MIL STD
        /// </summary>
        MIL_STD_188_220 = 22,
        /// <summary>
        /// Metricom
        /// </summary>
        Metricom = 23,
        /// <summary>
        /// IEEE 1394 and 1995
        /// </summary>
        IEEE_13941995 = 24,
        /// <summary>
        /// MAPOS
        /// </summary>
        MAPOS = 25,
        /// <summary>
        /// Twinaxial
        /// </summary>
        Twinaxial = 26,
        /// <summary>
        /// EUI 64
        /// </summary>
        EUI_64 = 27,
        /// <summary>
        /// HIPRAP
        /// </summary>
        HIPARP = 28,
        /// <summary>
        /// IP and ARP over ISO-7816-3
        /// </summary>
        ISO_7816_3 = 29,
        /// <summary>
        /// ARP Secure
        /// </summary>
        ARPSec = 30,
        /// <summary>
        /// IPSec tunnel
        /// </summary>
        IPsec = 31,
        /// <summary>
        /// Infiniband
        /// </summary>
        Infiniband = 32,
        /// <summary>
        /// CAI
        /// </summary>
        CAI = 33,
        /// <summary>
        /// Wiegand interface
        /// </summary>
        WiegandInterface = 34,
        /// <summary>
        /// Pure IP
        /// </summary>
        PureIP = 35
    }

    /// <summary>
    /// An enumeration for some common ether types
    /// </summary>
    public enum EtherType
    {
        /// <summary>
        /// Indictaes that the encasulated frame is an IPv4 frame
        /// </summary>
        IPv4 = 0x0800,
        /// <summary>
        /// Indictaes that the encasulated frame is an ARP frame
        /// </summary>
        ARP = 0x0806,
        /// <summary>
        /// Indictaes that the encasulated frame is a RARP frame
        /// </summary>
        RARP = 0x8035,
        /// <summary>
        /// Indictaes that the encasulated frame is an Apple Talk frame
        /// </summary>
        AppleTalk = 0x809B,
        /// <summary>
        /// Indictaes that the encasulated frame is an AARP frame
        /// </summary>
        AARP = 0x80F3,
        /// <summary>
        /// Indictaes that the frame contains a VLAN tag
        /// </summary>
        VLANTag = 0x8100,
        /// <summary>
        /// Indictaes that the encasulated frame is an IPX frame
        /// </summary>
        IPX = 08137,
        /// <summary>
        /// Indictaes that the encasulated frame is a Novell frame
        /// </summary>
        Novell = 08138,
        /// <summary>
        /// Indictaes that the encasulated frame is a SERCOS III frame
        /// </summary>
        SERCOS_III = 0x88CD,
        /// <summary>
        /// Indictaes that the encasulated frame is an IPv6 frame
        /// </summary>
        IPv6 = 0x86DD
    }
}