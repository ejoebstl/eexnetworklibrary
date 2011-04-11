using System;

namespace eExNetworkLibrary.ICMP.V6
{
    /// <summary>
    /// An enumeration for ICMPv6 types as defined here http://www.iana.org/assignments/icmpv6-parameters
    /// </summary>
    public enum ICMPv6Type
    {
        /// <summary>
        /// ICMPv6 destination unreachable [RFC4443] 
        /// </summary>
        DestinationUnreachable = 1,
        /// <summary>
        /// ICMPv6 packet too big [RFC4443] 
        /// </summary>
        PacketTooBig = 2,
        /// <summary>
        /// ICMPv6 time exceeded [RFC4443] 
        /// </summary>
        TimeExceeded = 3,
        /// <summary>
        /// ICMPv6 parameter problem [RFC4443] 
        /// </summary>
        ParameterProblem = 4,
        /// <summary>
        /// ICMPv6 Reserved for expansion of ICMPv6 error messages [RFC4443] 
        /// </summary>
        ReservedForErrorExpansion = 127,
        /// <summary>
        /// ICMPv6 Echo Request [RFC4443] 
        /// </summary>
        EchoRequest = 128,
        /// <summary>
        /// ICMPv6 Echo Reply [RFC4443]
        /// </summary>
        EchoReply = 129,
        /// <summary>
        /// ICMPv6 Multicast Listener Query [RFC2710] 
        /// </summary>
        MulticastListenerQuery = 130,
        /// <summary>
        /// ICMPv6 Multicast Listener Report [RFC2710] 
        /// </summary>
        MulticastListenerReport = 131,
        /// <summary>
        /// ICMPv6 Multicast Listener Done [RFC2710] 
        /// </summary>
        MulticastListenerDone = 132,
        /// <summary>
        /// ICMPv6 Router Solicitation [RFC4861] 
        /// </summary>
        RouterSolicitation = 133,
        /// <summary>
        /// ICMPv6 Router Advertisement [RFC4861] 
        /// </summary>
        RouterAdvertisement = 134,
        /// <summary>
        /// ICMPv6 Neighbor Solicitation [RFC4861] 
        /// </summary>
        NeighborSolicitation = 135,
        /// <summary>
        /// ICMPv6 Neighbor Advertisement [RFC4861] 
        /// </summary>
        NeighborAdvertisement = 136,
        /// <summary>
        /// ICMPv6 Redirect Message [RFC4861] 
        /// </summary>
        RedirectMessage = 137,
        /// <summary>
        /// ICMPv6 Router Renumbering [Crawford] 
        /// </summary>
        RouterRenumbering = 138,
        /// <summary>
        /// ICMPv6 Node Information Query [RFC4620] 
        /// </summary>
        NodeInformationQuery = 139,
        /// <summary>
        /// ICMPv6 Node Information Response [RFC4620] 
        /// </summary>
        NodeInformationResponse = 140,
        /// <summary>
        /// ICMPv6 Inverse Neighbor Discovery Solicitation Message [RFC3122] 
        /// </summary>
        InverseNeighborDiscoverySolicitationMessage = 141,
        /// <summary>
        /// ICMPv6 Inverse Neighbor Discovery Advertisement Message [RFC3122] 
        /// </summary>
        InverseNeighborDiscoveryAdvertisementMessage = 142,
        /// <summary>
        /// ICMPv6 Version 2 Multicast Listener Report [RFC3810] 
        /// </summary>
        Version2MulticastListenerReport = 143,
        /// <summary>
        /// ICMPv6 Home Agent Address Discovery Request Message [RFC-ietf-mext-rfc3775bis-13.txt] 
        /// </summary>
        HomeAgentAddressDiscoveryRequestMessage = 144,
        /// <summary>
        /// ICMPv6 Home Agent Address Discovery Reply Message [RFC-ietf-mext-rfc3775bis-13.txt] 
        /// </summary>
        HomeAgentAddressDiscoveryReplyMessage = 145,
        /// <summary>
        /// ICMPv6 Mobile Prefix Solicitation [RFC-ietf-mext-rfc3775bis-13.txt] 
        /// </summary>
        MobilePrefixSolicitation = 146,
        /// <summary>
        /// ICMPv6 Mobile Prefix Advertisement [RFC-ietf-mext-rfc3775bis-13.txt] 
        /// </summary>
        MobilePrefixAdvertisement = 147,
        /// <summary>
        /// ICMPv6 Certification Path Solicitation Message [RFC3971] 
        /// </summary>
        CertificationPathSolicitationMessage = 148,
        /// <summary>
        /// ICMPv6 Certification Path Advertisement Message [RFC3971] 
        /// </summary>
        CertificationPathAdvertisementMessage = 149,
        /// <summary>
        /// ICMP messages utilized by experimental mobility protocols such as Seamoby [RFC4065] 
        /// </summary>
        ExperimentalMobility = 150,
        /// <summary>
        /// ICMPv6 Multicast Router Advertisement [RFC4286] 
        /// </summary>
        MulticastRouterAdvertisement = 151,
        /// <summary>
        /// ICMPv6 Multicast Router Solicitation [RFC4286] 
        /// </summary>
        MulticastRouterSolicitation = 152,
        /// <summary>
        /// ICMPv6 Multicast Router Termination [RFC4286] 
        /// </summary>
        MulticastRouterTermination = 153,
        /// <summary>
        /// FMIPv6 Messages [RFC5568] 
        /// </summary>
        FMIPv6Messages = 154,
        /// <summary>
        /// ICMPv6 RPL Control Message [RFC-ietf-roll-rpl-19.txt] 
        /// </summary>
        RPLControlMessage = 155,
        /// <summary>
        /// ICMPv6 Reserved for expansion of ICMPv6 informational messages [RFC4443] 
        /// </summary>
        ReservedForInformalExpansion = 255
    }
}