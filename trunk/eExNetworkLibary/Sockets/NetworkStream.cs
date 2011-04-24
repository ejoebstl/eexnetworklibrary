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

namespace eExNetworkLibrary.Sockets
{
    /// <summary>
    /// This class represents the base for a network stream which is capable of saving 
    /// push flags for bytes which are written. 
    /// </summary>
    public abstract class NetworkStream : System.IO.Stream
    {
        /// <summary>
        /// A bool indicating whether the last bytes which were read last were written with a push flag set.
        /// </summary>
        public abstract bool IsPush { get; }

        /// <summary>
        /// Writes the given bytes to the network stream.
        /// </summary>
        /// <param name="buffer">The buffer to write.</param>
        /// <param name="offset">The offset in buffer where writing starts.</param>
        /// <param name="count">The count of bytes to write.</param>
        /// <param name="bPush">A bool indicating whether a push flag should be set for the bites written.</param>
        public abstract void Write(byte[] buffer, int offset, int count, bool bPush);
    }
}
