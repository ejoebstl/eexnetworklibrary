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

namespace eExNetworkLibrary.Attacks
{
    public interface IAttack
    {      
        /// <summary>
        /// Returns the name of this attack
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Pauses the attack until ResumeAttack() is called.
        /// </summary>
        void PauseAttack();

        /// <summary>
        /// Resumes the attack which was suspended by a previous call to PauseAttack().
        /// </summary>
        void ResumeAttack();
    }
}
