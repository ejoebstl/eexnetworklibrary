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

namespace eExNetworkLibrary.Simulation
{
    /// <summary>
    /// This class is capable of dropping packets according to a given probability. 
    /// </summary>
    public class PacketDropper : RandomEventTrafficSimulatorItem
    {
        /// <summary>
        /// Drops the frame
        /// </summary>
        /// <param name="f">The frame to drop</param>
        protected override void CaseHappening(Frame f)
        {
            //Do nothing - Drop
        }

        /// <summary>
        /// Forwards the frame
        /// </summary>
        /// <param name="f">The frame to forward</param>
        protected override void CaseNotHappening(Frame f)
        {
            //Forward
            this.Next.Push(f);
        }
    }
}
