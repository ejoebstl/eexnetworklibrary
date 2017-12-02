// This source file is part of the eEx Network Library Management Layer (NLML)
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

namespace eExNLML
{
    /// <summary>
    /// This class defines a link between two traffic handler ports.
    /// </summary>
    public class Link
    {
        /// <summary>
        /// The first port this link is connected to.
        /// </summary>
        public TrafficHandlerPort Alice { get; protected set; }
        /// <summary>
        /// The second port this link is connected to.
        /// </summary>
        public TrafficHandlerPort Bob { get; protected set; }

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="pAlice">The first port this link is connected to</param>
        /// <param name="pBob">The second port this link is connected to</param>
        public Link(TrafficHandlerPort pAlice, TrafficHandlerPort pBob)
        {
            this.Alice = pAlice;
            this.Bob = pBob;
        }
    }
}
