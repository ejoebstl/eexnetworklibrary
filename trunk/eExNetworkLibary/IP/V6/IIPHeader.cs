using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary.IP.V6
{
    /// <summary>
    /// Provides an interface for the smallest common part of all IP headers (including IPv6 special headers): The Payload Protocol (Or Next Header in IPv6) field.
    /// </summary>
    public interface IIPHeader
    {
        /// <summary>
        /// Gets or sets the payload protocol of this IP header. This field corresponds to the NextHeader field of the IPv6 frame.
        /// </summary>
         IPProtocol Protocol { get; set; }
    }
}
