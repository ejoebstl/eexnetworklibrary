using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary.ICMP
{
    /// <summary>
    /// Represents an ICMPv4 frame.
    /// </summary>
    public class ICMPv4Frame : ICMPFrame
    {
        public static string DefaultFrameType { get { return FrameTypes.ICMPv4; } }

        /// <summary>
        /// Returns the type of this frame.
        /// </summary>
        public override string FrameType
        {
            get { return ICMPv4Frame.DefaultFrameType; }
        }

        /// <summary>
        /// Gets or sets the type of this ICMP frame
        /// </summary>
        public ICMPv4Type ICMPv4Type
        {
            get { return (ICMPv4Type)icmpType; }
            set { icmpType = (int)value; }
        }

        public ICMPv4Frame(byte[] bData) : base(bData) { }
        public ICMPv4Frame() : base() { }

        /// <summary>
        /// Gets the ICMP parameter problem code for ICMP parameter problem frames.
        /// This operation is only supported if this ICMP frame is a parameter problem frame.
        /// </summary>
        public ICMPv4ParameterProblemCode ICMPParameterProblemCode
        {
            get
            {
                if (this.ICMPv4Type != ICMPv4Type.ParameterProblem) throw new ArgumentException("The ICMPType of this ICMP frame is not " + ICMPv4Type.ParameterProblem.ToString());
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
                if (this.ICMPv4Type != ICMPv4Type.Redirect) throw new ArgumentException("The ICMPType of this ICMP frame is not " + ICMPv4Type.Redirect.ToString());
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
                if (this.ICMPv4Type != ICMPv4Type.TimeExceeded) throw new ArgumentException("The ICMPType of this ICMP frame is not " + ICMPv4Type.TimeExceeded.ToString());
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
                if (this.ICMPv4Type != ICMPv4Type.DestinationUnreachable) throw new ArgumentException("The ICMPType of this ICMP frame is not " + ICMPv4Type.DestinationUnreachable.ToString());
                return (ICMPv4UnreachableCode)icmpCode;
            }
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
}
