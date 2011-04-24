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

namespace eExNetworkLibrary.Attacks
{
    /// <summary>
    /// Provides an interface for all kind of man in the middle attacks.
    /// 
    /// When implementing such an attack, please use this interface for enhanced functionality, such as network map integration. 
    /// </summary>
    public interface IMITMAttack : IAttack
    {
        /// <summary>
        /// Adds a man in the middle attack entry to the victim list of this attack
        /// </summary>
        /// <param name="apreVicim">The man in the middle attack entry to add</param>
        void AddToVictimList(MITMAttackEntry apreVicim);

        /// <summary>
        /// Returns all man in the middle attack entries of this attack's victim list
        /// </summary>
        /// <returns></returns>
        MITMAttackEntry[] GetVictims();

        /// <summary>
        /// Removes a man in the middle attack entry from the victim list of this attack
        /// </summary>
        /// <param name="apreVicim">The man in the middle attack entry to remove</param>
        void RemoveFromVictimList(MITMAttackEntry apreVicim);

        /// <summary>
        /// Checks whether a specific man in the middle attack entry is contained in this attack
        /// </summary>
        /// <param name="apreVicim">A specific man in the middle attack entry</param>
        /// <returns>A bool indicating whether a specific man in the middle attack entry is contained in this attack</returns>
        bool VictimListContains(MITMAttackEntry apreVicim);
    }
}
