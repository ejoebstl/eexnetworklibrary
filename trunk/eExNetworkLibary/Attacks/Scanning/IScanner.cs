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
using System.Net;

namespace eExNetworkLibrary.Attacks.Scanning
{
    /// <summary>
    /// This interface represents a scanner which scans various IP ranges.
    /// 
    /// When implementing such a scanner, please use this interface for enhanced functionality, such as network map integration.
    /// </summary>
    public interface IScanner
    {
        /// <summary>
        /// Starts the scan from the given start address to the given end address, including the start and the end address.
        /// </summary>
        /// <param name="ipaScanStart">The address at which scanning starts</param>
        /// <param name="ipaScanEnd">The address at which scanning ends</param>
        void Scan(IPAddress ipaScanStart, IPAddress ipaScanEnd);

        /// <summary>
        /// Gets the name of the scanner
        /// </summary>
        string Name { get; }
    }
}
