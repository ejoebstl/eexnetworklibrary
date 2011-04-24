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

namespace eExNetworkLibrary.ICMP.V6
{
    /// <summary>
    /// Provides a NeighborDiscoveryOption Frame for ICMPv6 ND
    /// </summary>
    public class NeighborDiscoveryOption : Frame
    {
        public NeighborDiscoveryOptionType OptionType { get; set; }
        public byte[] OptionData { get; set; }

        /// <summary>
        /// Returns ICMPv6NeighborDiscoveryOption
        /// </summary>
        public static string DefaultFrameType { get { return "ICMPv6NeighborDiscoveryOption"; } }

        /// <summary>
        /// Creates a new, empty instance of this class.
        /// </summary>
        public NeighborDiscoveryOption()
        {
            OptionType = NeighborDiscoveryOptionType.TargetLinkLayerAddress;
            OptionData = new byte[0];
        }

        /// <summary>
        /// Creates a new instance of this class from the given bytes.
        /// </summary>
        /// <param name="bData">The bytes to create the data from.</param>
        public NeighborDiscoveryOption(byte[] bData)
        {
            int iOptionType = bData[0];
            int iOptionLength = bData[1];

            iOptionLength = (iOptionLength * 8) - 2;

            OptionType = (NeighborDiscoveryOptionType)iOptionType;

            OptionData = new byte[iOptionLength];

            Array.Copy(bData, 2, OptionData, 0, iOptionLength);

            byte[] bPayload = new byte[bData.Length - (2 + iOptionLength)];
            Array.Copy(bData, 2 + iOptionLength, bPayload, 0, bPayload.Length);

            if (bPayload.Length > 0)
            {
                this.fEncapsulatedFrame = new NeighborDiscoveryOption(bPayload);
            }
 
        }

        /// <summary>
        /// Returns ICMPv6NeighborDiscoveryOption
        /// </summary>
        public override string FrameType
        {
            get { return NeighborDiscoveryOption.DefaultFrameType; }
        }

        /// <summary>
        /// Returns this neighbor discovery option and its encapsulated frame, converted to raw bytes.
        /// </summary>
        public override byte[] FrameBytes
        {
            get 
            {
                byte[] bData = new byte[Length];

                bData[0] = (byte)(((int)OptionType) & 0xFF);
                bData[1] = (byte)(((OptionData.Length + 2) / 8) & 0xFF);

                Array.Copy(OptionData, 0, bData, 2, OptionData.Length);

                if (fEncapsulatedFrame != null)
                {
                    Array.Copy(fEncapsulatedFrame.FrameBytes, 0, bData, 2 + OptionData.Length, fEncapsulatedFrame.Length);
                }

                return bData;
            }
        }

        /// <summary>
        /// Gets the length of this neighbor discovery option and the encapsulated frame.
        /// </summary>
        public override int Length
        {
            get { return 2 + OptionData.Length + (fEncapsulatedFrame != null ? fEncapsulatedFrame.Length : 0); }
        }

        /// <summary>
        /// Returns a copy of this frame.
        /// </summary>
        /// <returns></returns>
        public override Frame Clone()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Provides an enumeration of ICMPv6 neighbor discovery option types as defined here: http://www.iana.org/assignments/icmpv6-parameters
    /// </summary>
    public enum NeighborDiscoveryOptionType
    {
        /// <summary>
        /// Source Link-layer Address [RFC4861]
        /// </summary>
        SourceLinkLayerAddress = 1,
        /// <summary>
        /// Target Link-layer Address [RFC4861]
        /// </summary>
        TargetLinkLayerAddress = 2,
        /// <summary>
        /// Prefix Information [RFC4861]
        /// </summary>
        PrefixInformation = 3,
        /// <summary>
        /// Redirected Header [RFC4861]
        /// </summary>
        RedirectedHeader = 4,
        /// <summary>
        /// MTU [RFC4861]
        /// </summary>
        MTU = 5,
        /// <summary>
        /// NBMA Shortcut Limit Option [RFC2491]
        /// </summary>
        NBMAShortcutLimitOption = 6,
        /// <summary>
        /// Advertisement Interval Option [RFC-ietf-mext-rfc3775bis-13.txt]
        /// </summary>
        AdvertisementIntervalOption = 7,
        /// <summary>
        /// Home Agent Information Option [RFC-ietf-mext-rfc3775bis-13.txt]
        /// </summary>
        HomeAgentInformationOption = 8,
        /// <summary>
        /// Source Address List [RFC3122]
        /// </summary>
        SourceAddressList = 9,
        /// <summary>
        /// Target Address List [RFC3122]
        /// </summary>
        TargetAddressList = 10,
        /// <summary>
        /// CGA option [RFC3971]
        /// </summary>
        CGAOption = 11,
        /// <summary>
        /// RSA Signature option [RFC3971]
        /// </summary>
        RSASignatureOption = 12,
        /// <summary>
        /// Timestamp option [RFC3971]
        /// </summary>
        TimestampOption = 13,
        /// <summary>
        /// Nonce option [RFC3971]
        /// </summary>
        NonceOption = 14,
        /// <summary>
        /// Trust Anchor option [RFC3971]
        /// </summary>
        TrustAnchorOption = 15,
        /// <summary>
        /// Certificate option [RFC3971]
        /// </summary>
        CertificateOption = 16,
        /// <summary>
        /// IP Address/Prefix Option [RFC5568]
        /// </summary>
        IPAddressPrefixOption = 17,
        /// <summary>
        /// New Router Prefix Information Option [RFC4068]
        /// </summary>
        NewRouterPrefixInformationOption = 18,
        /// <summary>
        /// Link-layer Address Option [RFC5568]
        /// </summary>
        LinkLayerAddressOption = 19,
        /// <summary>
        /// Neighbor Advertisement Acknowledgment Option [RFC5568]
        /// </summary>
        NeighborAdvertisementAcknowledgmentOption = 20,
        /// <summary>
        /// MAP Option [RFC4140]
        /// </summary>
        MAPOption = 23,
        /// <summary>
        /// Route Information Option [RFC4191]
        /// </summary>
        RouteInformationOption = 24,
        /// <summary>
        /// Recursive DNS Server Option [RFC5006][RFC6106]
        /// </summary>
        RecursiveDNSServerOption = 25,
        /// <summary>
        /// RA Flags Extension Option [RFC5175]
        /// </summary>
        RAFlagsExtensionOption = 26,
        /// <summary>
        /// Handover Key Request Option [RFC5269]
        /// </summary>
        HandoverKeyRequestOption = 27,
        /// <summary>
        /// Handover Key Reply Option [RFC5269]
        /// </summary>
        HandoverKeyReplyOption = 28,
        /// <summary>
        /// Handover Assist Information Option [RFC5271]
        /// </summary>
        HandoverAssistInformationOption = 29,
        /// <summary>
        /// Mobile Node Identifier Option [RFC5271]
        /// </summary>
        MobileNodeIdentifierOption = 30,
        /// <summary>
        /// DNS Search List Option [RFC6106]
        /// </summary>
        DNSSearchListOption = 31,
        /// <summary>
        /// Proxy Signature (PS) [RFC-ietf-csi-proxy-send-05.txt]
        /// </summary>
        ProxySignature = 32,
        /// <summary>
        /// CARD Request option [RFC4065]
        /// </summary>
        CARDRequestOption = 138,
        /// <summary>
        /// CARD Reply option [RFC4065]
        /// </summary>
        CARDReplyOption = 139	
    }
}
