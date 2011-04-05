using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.Utilities;

namespace eExNetworkLibrary.ICMP
{
    /// <summary>
    /// Represents a ICMPv4 frame
    /// </summary>
    public class ICMPv4Frame : Frame
    {
        byte[] bData;
        ICMPv4Type icmpType;
        int icmpCode;
        ChecksumCalculator cCalc;

        /// <summary>
        /// Gets or sets the type of this ICMP frame
        /// </summary>
        public ICMPv4Type ICMPType
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
        /// Gets the ICMP parameter problem code for ICMP parameter problem frames.
        /// This operation is only supported if this ICMP frame is a parameter problem frame.
        /// </summary>
        public ICMPv4ParameterProblemCode ICMPParameterProblemCode
        {
            get
            {
                if (this.icmpType != ICMPv4Type.ParameterProblem) throw new ArgumentException("The ICMPType of this ICMP frame is not " + ICMPv4Type.ParameterProblem.ToString());
                return (ICMPv4ParameterProblemCode)icmpCode;
            }
        }

        /// <summary>
        /// Gets the ICMP redirect code for ICMP redirect frames.
        /// This operation is only supported if this ICMP frame is a redirect frame.
        /// </summary>
        public ICMPv4RedirectCode ICMPRedirectCode
        {
            get
            {
                if (this.icmpType != ICMPv4Type.Redirect) throw new ArgumentException("The ICMPType of this ICMP frame is not " + ICMPv4Type.Redirect.ToString());
                return (ICMPv4RedirectCode)icmpCode;
            }
        }

        /// <summary>
        /// Gets the ICMP time exceeded code for ICMP time exceeded frames.
        /// This operation is only supported if this ICMP frame is a time exceeded frame.
        /// </summary>
        public ICMPv4TimeExceededCode ICMPTimeExceededCode
        {
            get
            {
                if (this.icmpType != ICMPv4Type.TimeExceeded) throw new ArgumentException("The ICMPType of this ICMP frame is not " + ICMPv4Type.TimeExceeded.ToString()); 
                return (ICMPv4TimeExceededCode)icmpCode;
            }
        }

        /// <summary>
        /// Gets the ICMP unreachable code for ICMP unreachable frames.
        /// This operation is only supported if this ICMP frame is a unreachable frame.
        /// </summary>
        public ICMPv4UnreachableCode UnreachableCode
        {
            get
            {
                if (this.icmpType != ICMPv4Type.DestinationUnreachable) throw new ArgumentException("The ICMPType of this ICMP frame is not " + ICMPv4Type.DestinationUnreachable.ToString()); 
                return (ICMPv4UnreachableCode)icmpCode;
            }
        }

        /// <summary>
        /// Creates a new instance of this class by parsing the given data
        /// </summary>
        /// <param name="bICMPData">The data to parse</param>
        public ICMPv4Frame(byte[] bICMPData)
        {
            cCalc = new ChecksumCalculator();
            icmpType = (ICMPv4Type)bICMPData[0];
            icmpCode = (int)bICMPData[1];
            //2 bytes checksum
            bData = new byte[bICMPData.Length - 4];
            for (int iC1 = 4; iC1 < bICMPData.Length; iC1++)
            {
                bData[iC1 - 4] = bICMPData[iC1];
            }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public ICMPv4Frame()
        {
            cCalc = new ChecksumCalculator();
            bData = new byte[0];
        }

        /// <summary>
        /// Returns FrameType.ICMP
        /// </summary>
        public override FrameType FrameType
        {
            get { return FrameType.ICMP; }
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

                bData.CopyTo(bICMPData, 4);

                byte[] bChecksum = cCalc.CalculateChecksum(bICMPData);

                bICMPData[2] = bChecksum[0];
                bICMPData[3] = bChecksum[1];

                return bICMPData;
            
            }
        }

        /// <summary>
        /// Returns the length of this frame in bytes
        /// </summary>
        public override int Length
        {
            get { return (4 + bData.Length) % 2 == 0 ? 4 + bData.Length : 5 + bData.Length; }
        }

        /// <summary>
        /// Creates a new identical instance of this class
        /// </summary>
        /// <returns>A new identical instance of this class</returns>
        public override Frame Clone()
        {
            return new ICMPv4Frame(this.FrameBytes);
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
        Photuris = 40
    }

    /// <summary>
    /// An enumeration for ICMPv4 unreachable codes. These codes can be get or set if the ICMP type of the corresponding frame is ICMPv4Type.DestinationUnreachable
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
    /// An enumeration for ICMPv4 time exceeded codes. These codes can be get or set if the ICMP type of the corresponding frame is ICMPv4Type.TimeExceeded
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
    /// An enumeration for ICMPv4 parameter problem codes. These codes can be get or set if the ICMP type of the corresponding frame is ICMPv4Type.ParameterProblem
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
