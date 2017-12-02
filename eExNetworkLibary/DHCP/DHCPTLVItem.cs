// This source file is part of the eEx Network Library
//
// Author: 	    Emanuel Jöbstl <emi@eex-dev.net>
// Weblink: 	http://network.eex-dev.net
//		        http://eex.codeplex.com
//
// Licensed under the GNU Library General Public License (LGPL) 
//
// (c) eex-dev 2007-2011

using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary.DHCP
{
    /// <summary>
    /// This class represents a DHCP TLV item which is used to carry various parameters and options in DHCP frames
    /// </summary>
    public class DHCPTLVItem : TLVItem
    {
        /// <summary>
        /// Creates a new instance of this class initialized with DHCPOptions.AddressRequest
        /// </summary>
        public DHCPTLVItem() : base()
        { this.DHCPOptionType = DHCPOptions.AddressRequest; }

        /// <summary>
        /// Creates a new instance of this class by parsing the given data
        /// </summary>
        /// <param name="bData">The data to parse</param>
        public DHCPTLVItem(byte[] bData)
            : base(bData)
        { }

        /// <summary>
        /// Creates a new instance of this class by parsing the given data starting at the given index
        /// </summary>
        /// <param name="bData">The data to parse</param>
        /// <param name="iStartIndex">The index at which parsing should begin</param>
        public DHCPTLVItem(byte[] bData, int iStartIndex)
            : base(bData, iStartIndex)
        { }

        /// <summary>
        /// Gets or sets the DHCP option type
        /// </summary>
        public DHCPOptions DHCPOptionType
        {
            get { return (DHCPOptions)base.Type; }
            set { base.Type = (int)value; }
        }
    }

    /// <summary>
    /// An enumeration for various DHCP options. 
    /// See http://www.iana.org/assignments/bootp-dhcp-parameters/ 
    /// and http://www.faqs.org/rfcs/rfc2132.html 
    /// for more information.
    /// </summary>
    public enum DHCPOptions
    {
        /// <summary>
        /// Padding (Does nothing)
        /// </summary>
        Padding = 0,
        /// <summary>
        /// Subent mask value
        /// </summary>
        SubnetMask = 1,
        /// <summary>
        /// Time Offset in Seconds from UTC
        /// </summary>
        TimeOffset = 2,
        /// <summary>
        /// Router address
        /// </summary>
        Router = 3,
        /// <summary>
        /// Time server address
        /// </summary>
        TimeServer = 4,
        /// <summary>
        /// Name server address
        /// </summary>
        NameServer = 5,
        /// <summary>
        /// DNS server address
        /// </summary>
        DomainNameServer = 6,
        /// <summary>
        /// Log server address
        /// </summary>
        LogServer = 7,
        /// <summary>
        /// Quotes server address
        /// </summary>
        QuotesServer = 8,
        /// <summary>
        /// LPR server address
        /// </summary>
        LPRServer = 9,
        /// <summary>
        /// Impress server address
        /// </summary>
        ImpressServer = 10,
        /// <summary>
        /// RLP server address
        /// </summary>
        RLPServer = 11,
        /// <summary>
        /// Hostname as ASCII string
        /// </summary>
        Hostname = 12,
        /// <summary>
        /// Size of boot file in 512 byte chunks
        /// </summary>
        BootFileSize = 13,
        /// <summary>
        /// Client to dump and name of the file to dump the merit dump file to
        /// </summary>
        MeritDumpFile = 14,
        /// <summary>
        /// The DNS domain name of the client  
        /// </summary>
        DomainName = 15,
        /// <summary>
        /// Swap Server address
        /// </summary>
        SwapServer = 16,
        /// <summary>
        /// Path name for root disk 
        /// </summary>
        RootPath = 17,
        /// <summary>
        /// Path name for more BOOTP info
        /// </summary>
        ExtentionFile = 18,
        /// <summary>
        ///  Enable/Disable IP Forwarding 
        /// </summary>
        ForwardOnOff = 19,
        /// <summary>
        /// Enable/Disable Source Routing 
        /// </summary>
        SourceRouteOnOff = 20,
        /// <summary>
        /// Routing Policy Filters
        /// </summary>
        PolicyFilter = 21,
        /// <summary>
        /// Max Datagram Reassembly Size
        /// </summary>
        MaxDatagramReassemblySize = 22,
        /// <summary>
        /// Default IP Time to Live
        /// </summary>
        DefaultIPTTL = 23,
        /// <summary>
        /// Path MTU Aging Timeout
        /// </summary>
        MTUTimeout = 24,
        /// <summary>
        /// Path MTU Plateau Table
        /// </summary>
        MTUPlateau = 25,
        /// <summary>
        /// Interface MTU Size
        /// </summary>
        MTUInterface = 26,
        /// <summary>
        /// All Subnets are Local Option
        /// </summary>
        MTUSubnet = 27,
        /// <summary>
        /// Broadcast Address
        /// </summary>
        BroadcastAddress = 28,
        /// <summary>
        /// Perform Mask Discovery
        /// </summary>
        MaskDiscovery = 29,
        /// <summary>
        /// Mask Supplier
        /// </summary>
        MaskSupplier = 30,
        /// <summary>
        /// Perform Router Discovery 
        /// </summary>
        RouterDiscovery = 31,
        /// <summary>
        /// Router Solicitation Address 
        /// </summary>
        RouterRequest = 32,
        /// <summary>
        /// Static Routing Table
        /// </summary>
        StaticRoute = 33,
        /// <summary>
        /// Trailer Encapsulation
        /// </summary>
        Trailers = 34,
        /// <summary>
        /// ARP Cache Timeout
        /// </summary>
        ARPTimeout = 35,
        /// <summary>
        /// Ethernet Encapsulation
        /// </summary>
        Ethernet = 36,
        /// <summary>
        /// Default TCP Time to Live
        /// </summary>
        DefaultTCPTTL = 37,
        /// <summary>
        /// TCP Keepalive Interval 
        /// </summary>
        KeepaliveTime = 38,
        /// <summary>
        /// TCP Keepalive Garbage
        /// </summary>
        KeepaliveData = 39,
        /// <summary>
        /// NIS Domain Name 
        /// </summary>
        NISDomain = 40,
        /// <summary>
        /// NIS Server Addresses
        /// </summary>
        NISServers = 41,
        /// <summary>
        /// NIS Server Addresses
        /// </summary>
        NTPServers = 42,
        /// <summary>
        /// Vendor Specific Information
        /// </summary>
        VendorSpecific = 43,
        /// <summary>
        /// NETBIOS Name Servers
        /// </summary>
        NETBIOSNameServer = 44,
        /// <summary>
        /// NETBIOS Datagram Distribution
        /// </summary>
        NETBIOSDistServer = 45,
        /// <summary>
        /// NETBIOS Node Type
        /// </summary>
        NETBIOSNodeType = 46,
        /// <summary>
        /// NETBIOS Scope
        /// </summary>
        NETBIOSScope = 47,
        /// <summary>
        /// X Window Font Server
        /// </summary>
        XWindowFontserver = 48,
        /// <summary>
        /// X Window Display Manager
        /// </summary>
        XWindowManager = 49,
        /// <summary>
        /// Requested IP Address
        /// </summary>
        AddressRequest = 50,
        /// <summary>
        /// IP Address Lease Time
        /// </summary>
        LeaseTime = 51,
        /// <summary>
        /// Overload "sname" or "file"
        /// </summary>
        Overload = 52,
        /// <summary>
        /// DHCP Message Type
        /// </summary>
        DHCPMessageType = 53,
        /// <summary>
        /// DHCP Server Identification 
        /// </summary>
        DHCPServerID = 54,
        /// <summary>
        /// Parameter Request List 
        /// </summary>
        ParameterList = 55,
        /// <summary>
        /// DHCP Error Message
        /// </summary>
        DHCPMessage = 56,
        /// <summary>
        /// DHCP Maximum Message Size
        /// </summary>
        DHCPMaxMsgSize = 57,
        /// <summary>
        /// DHCP Renewal (T1) Time
        /// </summary>
        RenewalTime = 58,
        /// <summary>
        /// DHCP Rebinding (T2) Time
        /// </summary>
        RebindingTime = 59,
        /// <summary>
        /// Class Identifier
        /// </summary>
        ClassID = 60,
        /// <summary>
        /// Client Identifier 
        /// </summary>
        ClientID = 61,
        /// <summary>
        /// NetWare/IP Domain Name
        /// </summary>
        NetWareIPDomain = 62,
        /// <summary>
        /// NetWare/IP sub Options
        /// </summary>
        NetWareIPOption = 63,
        /// <summary>
        /// NIS+ v3 Client Domain Name 
        /// </summary>
        NISDOmainName = 64,
        /// <summary>
        /// NIS+ v3 Server Addresses
        /// </summary>
        NISServerAddress = 65,
        /// <summary>
        /// TFTP Server Name 
        /// </summary>
        ServerName = 66,
        /// <summary>
        /// Boot File Name  
        /// </summary>
        BootfileName = 67,
        /// <summary>
        /// Home Agent Addresses
        /// </summary>
        HomeAgendAddress = 68,
        /// <summary>
        /// Simple Mail Server Addresses
        /// </summary>
        SMTPServer = 69,
        /// <summary>
        /// Post Office Server Addresses
        /// </summary>
        POP3Server = 70,
        /// <summary>
        /// Network News Server Addresses
        /// </summary>
        NNTPServer = 71,
        /// <summary>
        /// WWW Server Addresses
        /// </summary>
        WWWServer = 72,
        /// <summary>
        /// Finger Server Addresses
        /// </summary>
        FingerServer = 73,
        /// <summary>
        /// Chat Server Addresses 
        /// </summary>
        IRCServer = 74,
        /// <summary>
        /// StreetTalk Server Addresses
        /// </summary>
        StreetTalkServer = 75,
        /// <summary>
        /// StreetTalk Directory Assistance (STDA) Server 
        /// </summary>
        STDAServer = 76,
        /// <summary>
        /// User Class Information
        /// </summary>
        UserClass = 77,
        /// <summary>
        /// Directory Agent Information
        /// </summary>
        DirectoryAgent = 78,
        /// <summary>
        /// Service Location Agent Scope
        /// </summary>
        ServiceScope = 79,
        /// <summary>
        /// Rapid Commit
        /// </summary>
        RapidCommit = 80,
        /// <summary>
        /// Fully Qualified Domain Name
        /// </summary>
        ClientFQDN = 81,
        /// <summary>
        /// Relay Agent Information
        /// </summary>
        RelayAgendInformation = 82,
        /// <summary>
        /// Internet Storage Name Service
        /// </summary>
        iSNS = 83,
        /// <summary>
        /// Novell Directory Services 
        /// </summary>
        NDSServers = 85,
        /// <summary>
        /// Novell Directory Services 
        /// </summary>
        NDSTreeName = 86,
        /// <summary>
        /// Novell Directory Services 
        /// </summary>
        NDSContext = 87,
        /// <summary>
        /// BCMCS Controller Domain Name list
        /// </summary>
        BCMCSControllerDomainNameList = 88,
        /// <summary>
        /// BCMCS Controller IPv4 address option
        /// </summary>
        BCMCSControllerIPv4AddressOption = 89,
        /// <summary>
        /// Authentication
        /// </summary>
        Authentication = 90,
        /// <summary>
        /// Client Last Transaction Time
        /// </summary>
        ClientLastTransactionTimeOption = 91,
        /// <summary>
        /// Accosiated IP
        /// </summary>
        AssociatedIPOption = 92,
        /// <summary>
        /// Client System Architecture
        /// </summary>
        CientSystem = 93,
        /// <summary>
        /// Client Network Device Interface
        /// </summary>
        ClientNDI = 94,
        /// <summary>
        /// Lightweight Directory Access Protocol
        /// </summary>
        LDAP = 95,
        /// <summary>
        /// UUID/GUID-based Client Identifier
        /// </summary>
        UUID_GUID = 97,
        /// <summary>
        /// Open Group's User Authentication
        /// </summary>
        UserAuth = 98,
        /// <summary>
        /// GEOCONF CIVIC
        /// </summary>
        GEOCONF_CIVIC = 99,
        /// <summary>
        /// IEEE 1003.1 TZ String
        /// </summary>
        PCode = 100,
        /// <summary>
        /// Reference to the TZ Database
        /// </summary>
        TCode = 101,
        /// <summary>
        /// NetInfo Parent Server Address
        /// </summary>
        NetInfoAddress = 112,
        /// <summary>
        /// NetInfo Parent Server Tag
        /// </summary>
        NetInfoTag = 113,
        /// <summary>
        /// URL
        /// </summary>
        URL = 114,
        /// <summary>
        /// DHCP Auto-Configuration
        /// </summary>
        AutoConfig = 116,
        /// <summary>
        /// Name Service Search 
        /// </summary>
        NameServiceSearch = 117,
        /// <summary>
        /// Subnet Selection Option
        /// </summary>
        SubnetSelectionOption = 118,
        /// <summary>
        /// DNS domain search list
        /// </summary>
        DomainSearch = 119,
        /// <summary>
        /// SIP Servers DHCP Option 
        /// </summary>
        SIPServersDHCPOption = 120,
        /// <summary>
        /// Classless Static Route Option
        /// </summary>
        ClasslessStaticRouteOption = 121,
        /// <summary>
        /// CableLabs Client Configuration 
        /// </summary>
        CCC = 112,
        /// <summary>
        /// GeoConf 
        /// </summary>
        GeoConfOption = 123,
        /// <summary>
        /// Vendor-Identifying Vendor Class 
        /// </summary>
        V_I_VendorClass = 124,
        /// <summary>
        /// Vendor-Identifying Vendor-Specific Information
        /// </summary>
        V_I_VendorSpecificInformation = 125,
        /// <summary>
        /// PXE - undefined (vendor specific) or 
        /// Etherboot signature (E4:45:74:68:00:00) or 
        /// DOCSIS "full security" server IP address or
        /// TFTP Server IP address (for IP Phone software load)
        /// </summary>
        PXE_EtherbootSignature_or_TFTPServerIP = 128,
        /// <summary>
        /// PXE - undefined (vendor specific) or
        /// Kernel options. Variable length string or
        /// Call Server IP address 
        /// </summary>
        PXE_KernelOptoins_or_CallServerIP = 129,
        /// <summary>
        /// PXE - undefined (vendor specific) or
        /// Ethernet interface. Variable length string. or
        /// Discrimination string (to identify vendor)
        /// </summary>
        PXE_EthernetInterface_or_DiscriminationString = 130,
        /// <summary>
        /// PXE - undefined (vendor specific) or
        /// Remote statistics server IP address
        /// </summary>
        PXE_RemoteStatistics = 131,
        /// <summary>
        /// PXE - undefined (vendor specific) or
        /// IEEE 802.1Q VLAN ID 
        /// </summary>
        PXE_802dot1Q_VlanID = 132,
        /// <summary>
        /// PXE - undefined (vendor specific) or
        /// IEEE 802.1D/p Layer 2 Priority
        /// </summary>
        PXE_802dot1DP_Layer2Priority = 133,
        /// <summary>
        /// PXE - undefined (vendor specific) or
        /// Diffserv Code Point (DSCP) for VoIP signalling and media streams
        /// </summary>
        PXE_DiffservCodePoint = 134,
        /// <summary>
        /// PXE - undefined (vendor specific) or
        /// HTTP Proxy for phone-specific applications
        /// </summary>
        PXE_HTTPProxy_for_phone = 135,
        /// <summary>
        /// PANA Agent
        /// </summary>
        PANA_AGENT = 136,
        /// <summary>
        /// V4 Lost
        /// </summary>
        V4_LOST = 137,
        /// <summary>
        /// CAPWAP Access Controller addresses
        /// </summary>
        CAPWAP_AC_V4 = 138,
        /// <summary>
        /// A series of suboptions 
        /// </summary>
        IPv4_Address_MoS = 139,
        /// <summary>
        /// A series of suboptions 
        /// </summary>
        IPv4_FQDN_MoS = 140,
        /// <summary>
        /// TFTP server address or
        /// Etherboot or
        /// GRUB configuration path name
        /// </summary>
        TFPTServer_or_Etherboot_or_GrubConfigPath = 150,
        /// <summary>
        /// Etherboot 
        /// </summary>
        Etherboot = 175,
        /// <summary>
        /// IP Telephone
        /// </summary>
        IPTelephone = 176,
        /// <summary>
        /// Etherboot or PacketCable and CableHome 
        /// </summary>
        Etherboot_or_PacketCable = 177,
        /// <summary>
        /// Magic String (F1:00:74:7E)
        /// </summary>
        PXELinuxMagic = 208,
        /// <summary>
        /// Configuration file 
        /// </summary>
        ConfigurationFile = 209,
        /// <summary>
        /// Path Prefix Option 
        /// </summary>
        PathPrefix = 210,
        /// <summary>
        /// Reboot Time
        /// </summary>
        RebootTime = 211,
        /// <summary>
        /// Subnet Allocation Option
        /// </summary>
        SubnetAllocationOption = 220,
        /// <summary>
        /// Virtual Subnet Selection Option 
        /// </summary>
        VirtualSubnetSelectionOption = 221,
        /// <summary>
        /// End of List
        /// </summary>
        End = 255
    }

    /// <summary>
    /// Defines constants for the DHCP message type (Option field value 53)
    /// </summary>
    public enum DHCPMessageType
    {
        /// <summary>
        /// A DHCP discover
        /// </summary>
        Discover = 1,
        /// <summary>
        /// A DHCP offer
        /// </summary>
        Offer = 2,
        /// <summary>
        /// A DHCP request
        /// </summary>
        Request = 3,
        /// <summary>
        /// A DHCP decline
        /// </summary>
        Decline = 4,
        /// <summary>
        /// A DHCP acknowledgement
        /// </summary>
        ACK = 5,
        /// <summary>
        /// A DHCP not acknowledged message
        /// </summary>
        NAK = 6,
        /// <summary>
        /// A DHCP release
        /// </summary>
        Release = 7,
        /// <summary>
        /// A DHCP inform
        /// </summary>
        Inform = 8,
        /// <summary>
        /// A DHCP force renew
        /// </summary>
        ForceRenew = 9,
        /// <summary>
        /// A DHCP lease query
        /// </summary>
        LeaseQuery = 10,
        /// <summary>
        /// A DHCP lease unassigned message
        /// </summary>
        LeaseUnassingned = 11,
        /// <summary>
        /// A DHCP lease unknown message
        /// </summary>
        LeaseUnknown = 12,
        /// <summary>
        /// A DHCP lease active message
        /// </summary>
        LeaseActieve = 13
    }
}
