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

namespace eExNetworkLibrary.Attacks
{
    /// <summary>
    /// Provides an interface for all kinds of attacks against single hosts.
    /// 
    /// When implementing such an attack, please use this interface for enhanced functionality, such as network map integration. 
    /// </summary>
    public interface ISingleHostAttack : IAttack
    {
        /// <summary>
        /// Adds the given IPAddress to the attacks victim list, so it will be attacked.
        /// </summary>
        /// <param name="ipaVictim">The IPAddress to attack</param>
        void Attack(IPAddress ipaVictim);

        /// <summary>
        /// Removes the given IPAddress from the victim list, so attacking will be stopped immediately. 
        /// </summary>
        /// <param name="ipaVictim">The IPAddress to remove from the victim list</param>
        void StopAttack(IPAddress ipaVictim);
    }
}
