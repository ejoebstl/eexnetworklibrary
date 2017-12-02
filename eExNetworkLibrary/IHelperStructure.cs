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

namespace eExNetworkLibrary
{
    /// <summary>
    /// Represents a small, frame-like helper structure
    /// </summary>
    public abstract class HelperStructure
    {
        /// <summary>
        /// Gets the bytes of this helper structure
        /// </summary>
        public abstract byte[] Bytes { get; }
        /// <summary>
        /// Gets the length of this helper structure
        /// </summary>
        public abstract int Length { get; }
    }
}
