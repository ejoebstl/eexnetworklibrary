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

namespace eExNetworkLibrary.Routing
{
    /// <summary>
    /// Provides an interface from which all routers must derive.
    /// </summary>
    public interface IRouter
    {
        /// <summary>
        /// Gets the routing table of a router.
        /// </summary>
        RoutingTable RoutingTable { get; }
        /// <summary>
        /// Gets the name of a router.
        /// </summary>
        string Name { get; }
    }
}
