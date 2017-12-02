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

namespace eExNetworkLibrary.TrafficModifiers
{
    /// <summary>
    /// This class is used als superclass for all Traffic Handlers which modify traffic
    /// </summary>
    public abstract class TrafficModifier : TrafficHandler
    {
        /// <summary>
        /// Receives a frame, calls ModifyTraffic and forwards this frame to the next handler
        /// </summary>
        /// <param name="fInputFrame">The frame to handle</param>
        protected override void HandleTraffic(Frame fInputFrame)
        {
            fInputFrame = ModifyTraffic(fInputFrame);
            if (fInputFrame != null)
            {
                NotifyNext(fInputFrame);
            }
        }

        /// <summary>
        /// A method which is used to modify traffic
        /// </summary>
        /// <param name="fInputFrame">The frame to handle</param>
        /// <returns>A bool indicating if the frame should be further forwarded.</returns>
        protected abstract Frame ModifyTraffic(Frame fInputFrame);

    }
}
