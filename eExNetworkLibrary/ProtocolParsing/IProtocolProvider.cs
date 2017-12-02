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

namespace eExNetworkLibrary.ProtocolParsing
{
    public interface IProtocolProvider
    {
        /// <summary>
        /// Must return the protocol which is associated with this protocol provider.
        /// </summary>
        string Protocol { get; }

        /// <summary>
        /// Must return an array of strings, filled with the names of all payload protocols known to this provider.
        /// </summary>
        string[] KnownPayloads { get; }

        /// <summary>
        /// If the given frame is a raw data frame, this protocol provider has to parse the frame.<br />
        /// </summary>
        /// <param name="fFrame">The frame to parse.</param>
        /// <returns>The parsed frame</returns>
        Frame Parse(Frame fFrame);

        /// <summary>
        /// Must return the payload type of the given frame.
        /// If the frame is a raw data frame, the protocol provider has to throw an exception.
        /// </summary>
        /// <param name="fFrame">The frame to get the payload type for.</param>
        /// <returns>The payload type.</returns>
        string PayloadType(Frame fFrame);

    }
}
