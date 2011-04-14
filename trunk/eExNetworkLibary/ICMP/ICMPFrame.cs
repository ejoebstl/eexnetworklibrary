using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.Utilities;
using eExNetworkLibrary.ICMP.V6;

namespace eExNetworkLibrary.ICMP
{
    /// <summary>
    /// Represents an abstract ICMP frame
    /// </summary>
    public abstract class ICMPFrame : Frame
    {
        protected int icmpType;
        protected int icmpCode;
        byte[] bICMPChecksum;

        /// <summary>
        /// Gets or sets the type of this ICMP frame
        /// </summary>
        public int ICMPType
        {
            get { return icmpType; }
            set { icmpType = value; }
        }

        /// <summary>
        /// Gets or sets the code of this ICMP frame
        /// </summary>
        public int ICMPCode
        {
            get { return icmpCode; }
            set { icmpCode = value; }
        }

        /// <summary>
        /// Gets or sets the ICMP checksum, which has to be 2 bytes long. 
        /// Don't forget to set this property to a valid value before sending the frame. 
        /// Also don't forget to set this property to a byte array full of zeros before calculating the cecksum. 
        /// <br />
        /// <br />
        /// For calculating IPv4 checksums use:
        /// <code>
        /// icmpFrame.Checksum = new byte[2];
        /// icmpFrame.Checksum = icmpFrame.CalculateChecksum();
        /// </code>
        /// For calculating IPv6 checksums use:
        /// <code>
        /// icmpFrame.Checksum = new byte[2];
        /// icmpFrame.Checksum = icmpFrame.CalculateChecksum(ipv6Frame.GetPseudoHeader)
        /// </code>
        /// </summary>
        public byte[] Checksum
        {
            get { return bICMPChecksum; }
            set
            {
                if (value.Length != 2)
                {
                    throw new ArgumentException("The ICMP checksum has to be 2 bytes long.");
                }
                bICMPChecksum = value;
            }
        }

        /// <summary>
        /// Calculates an IPv4 ICMP checksum from this frame.
        /// </summary>
        /// <returns>The ICMPv4 checksum.</returns>
        public byte[] CalculateChecksum()
        {
            return CalculateChecksum(new byte[0]);
        }

        /// <summary>
        /// Calculates an IPv6 ICMP checksum from this frame.
        /// </summary>
        /// <param name="bPseudoHeader">The IPv6 pseudo header to use for the calculation.</param>
        /// <returns>The ICMPv6 checksum.</returns>
        public byte[] CalculateChecksum(byte[] bPseudoHeader)
        {
            byte[] bChecksumData = new byte[bPseudoHeader.Length + this.Length];
            Array.Copy(bPseudoHeader, 0, bChecksumData, 0, bPseudoHeader.Length);
            Array.Copy(this.FrameBytes, 0, bChecksumData, bPseudoHeader.Length, this.Length);

            return ChecksumCalculator.CalculateChecksum(bChecksumData);
        }


        /// <summary>
        /// Creates a new instance of this class by parsing the given data
        /// </summary>
        /// <param name="bICMPData">The data to parse</param>
        public ICMPFrame(byte[] bICMPData)
        {
            icmpType = (int)bICMPData[0];
            icmpCode = (int)bICMPData[1];

            bICMPChecksum = new byte[2];

            bICMPChecksum[0] = bICMPData[2];
            bICMPChecksum[1] = bICMPData[3];

            Encapsulate(bICMPData, 4);
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public ICMPFrame()
        {
            bICMPChecksum = new byte[2];
        }

        /// <summary>
        /// Returns the byte representation of this frame
        /// </summary>
        public override byte[] FrameBytes
        {
            get 
            {
                byte[] bICMPData = new byte[Length];
                bICMPData[0] = (byte)icmpType;
                bICMPData[1] = (byte)icmpCode;

                if (fEncapsulatedFrame != null)
                {
                    fEncapsulatedFrame.FrameBytes.CopyTo(bICMPData, 4);
                }

                bICMPData[2] = bICMPChecksum[0];
                bICMPData[3] = bICMPChecksum[1];

                return bICMPData;
            
            }
        }

        /// <summary>
        /// Returns the length of this frame in bytes
        /// </summary>
        public override int Length
        {
            get
            {
                int iDataLen = fEncapsulatedFrame == null ? 0 : fEncapsulatedFrame.Length;
                return (4 + iDataLen) % 2 == 0 ? 4 + iDataLen : 5 + iDataLen;
            }
        }
    }

    /// <summary>
    /// An enumeration for ICMPv4 types
    /// </summary>
    public enum ICMPv4Type
    {
        /// <summary>
        /// Unknown - do not try to send a frame with this type set
        /// </summary>
        Unknown = -1,
        /// <summary>
        /// Echo (ping) reply
        /// </summary>
        EchoReply = 0,
        /// <summary>
        /// Destination unreachable
        /// </summary>
        DestinationUnreachable = 3,
        /// <summary>
        /// Source quench
        /// </summary>
        SourceQuench = 4,
        /// <summary>
        /// Redirect
        /// </summary>
        Redirect = 5,
        /// <summary>
        /// Alternate host address
        /// </summary>
        AlternateHostAddress = 6,
        /// <summary>
        /// Echo request
        /// </summary>
        EchoRequest = 8,
        /// <summary>
        /// Router advertisment
        /// </summary>
        RouterAdvertisment = 9,
        /// <summary>
        /// Router solication
        /// </summary>
        RouterSolication = 10,
        /// <summary>
        /// Time exceeded
        /// </summary>
        TimeExceeded = 11,
        /// <summary>
        /// Parameter problem
        /// </summary>
        ParameterProblem = 12,
        /// <summary>
        /// Timestamp request
        /// </summary>
        TimestampRequest = 13,
        /// <summary>
        /// Timestamp reply
        /// </summary>
        TimestampReply = 14,
        /// <summary>
        /// Information request
        /// </summary>
        InformationRequest = 15,
        /// <summary>
        /// Information reply
        /// </summary>
        InformationReply = 16,
        /// <summary>
        /// Address mask request
        /// </summary>
        AddressMaskRequest = 17,
        /// <summary>
        /// Address mask reply
        /// </summary>
        AddressMaskReply = 18,
        /// <summary>
        /// Traceroute
        /// </summary>
        Traceroute = 30,
        /// <summary>
        /// Datagram conversion error
        /// </summary>
        DatagramConversionError = 31,
        /// <summary>
        /// Mobile host redirect
        /// </summary>
        MobileHostRedirect = 32,
        /// <summary>
        /// Mobile registration request
        /// </summary>
        MobileRegistrationRequest = 35,
        /// <summary>
        /// Mobile registration reply
        /// </summary>
        MobileRegistrationReply = 36,
        /// <summary>
        /// Domain name request
        /// </summary>
        DomainNameRequest = 37,
        /// <summary>
        /// Domain name reply
        /// </summary>
        DomainNameReply = 36,
        /// <summary>
        /// SKIP
        /// </summary>
        SKIP = 39,
        /// <summary>
        /// Photuris
        /// </summary>
        Photuris = 40,
    }

    /// <summary>
    /// An enumeration for ICMP unreachable codes. These codes can be get or set if the ICMP type of the corresponding frame is ICMPv4Type.DestinationUnreachable
    /// </summary>
    public enum ICMPv4UnreachableCode
    {
        /// <summary>
        /// Unknown - do not try to send a frame with this type set
        /// </summary>
        Unknown = -1,
        /// <summary>
        /// The destination network is unreachable
        /// </summary>
        DestinationNetworkUnreachable = 0,
        /// <summary>
        /// The destination host is unreachable
        /// </summary>
        DestinationHostUnreachable = 1,
        /// <summary>
        /// The destination protocol is unreachable
        /// </summary>
        DestinationProtocolUnreachable = 2,
        /// <summary>
        /// The destination port is unreachable
        /// </summary>
        DestinationPortUnreachable = 3,
        /// <summary>
        /// The destination network is not known
        /// </summary>
        DestinationNetworkUnknown = 6,
        /// <summary>
        /// The destination host is not known
        /// </summary>
        DestinationHostUnknown = 7,
        /// <summary>
        /// The source host is not known
        /// </summary>
        SourceHostUnknown = 8,
        /// <summary>
        /// The communication with the destination network is administratively prohibited
        /// </summary>
        NetworkProhibited = 9,
        /// <summary>
        /// The communication with the destination host is administratively prohibited
        /// </summary>
        HostProhibited = 10,
        /// <summary>
        /// The destination network is unreachable for this type of service
        /// </summary>
        NetworkUnreachableForTOS = 11,
        /// <summary>
        /// The destination host is unreachable for this type of service
        /// </summary>
        HostUnreachableForTOS = 12,
        /// <summary>
        /// The communication is administratively prohibited
        /// </summary>
        AdministrativelyProhibited = 13,
        /// <summary>
        /// There is a host precedence violation
        /// </summary>
        HostPrecedenceViolation = 14,
        /// <summary>
        /// The precedence of the datagram was below the minimum required level
        /// </summary>
        PrecedenceCutoffInEffect = 15
    }

    /// <summary>
    /// An enumeration for ICMPv4 redirect codes. These codes can be get or set if the ICMP type of the corresponding frame is ICMPv4Type.Redirect
    /// </summary>
    public enum ICMPv4RedirectCode
    {
        /// <summary>
        /// Forces the source to redirect all datagrams for the corresponding network. 
        /// </summary>
        RedirectDatagramForNetwork = 0,
        /// <summary>
        /// Forces the source to redirect all datagrams for the corresponding host
        /// </summary>
        RedirectDatagramForHost = 1,
        /// <summary>
        /// Forces the source to redirect all datagrams for the corresponding network and TOS
        /// </summary>
        RedirectDatagramForTypeOfServiceAndNetwork = 2,
        /// <summary>
        /// Forces the source to redirect all datagrams for the corresponding host and TOS
        /// </summary>
        RedirectDatagramForTypeOfServiceAndHost = 3
    }

    /// <summary>
    /// An enumeration for ICMP time exceeded codes. These codes can be get or set if the ICMP type of the corresponding frame is ICMPv4Type.TimeExceeded
    /// </summary>
    public enum ICMPv4TimeExceededCode
    {
        /// <summary>
        /// The TTL (Time to Live) exceeded during transmit
        /// </summary>
        TTLExceeded = 0,
        /// <summary>
        /// The FRT (fragment reassembly time) exceeded
        /// </summary>
        FRTExceeded = 1
    }

    /// <summary>
    /// An enumeration for ICMP parameter problem codes. These codes can be get or set if the ICMP type of the corresponding frame is ICMPv4Type.ParameterProblem
    /// </summary>
    public enum ICMPv4ParameterProblemCode
    {
        /// <summary>
        /// A pointer indicates an error
        /// </summary>
        PointerIndicatesAnError = 0,
        /// <summary>
        /// A required option is missing
        /// </summary>
        MissingARequiredOption = 1,
        /// <summary>
        /// The frame has got a bad length
        /// </summary>
        BadLength = 2
    }
}
