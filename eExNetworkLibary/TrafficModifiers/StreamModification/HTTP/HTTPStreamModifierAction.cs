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
using eExNetworkLibrary.HTTP;

namespace eExNetworkLibrary.TrafficModifiers.StreamModification.HTTP
{
    public abstract class HTTPStreamModifierAction : HTTPStreamModifierCondition
    {
        /// <summary>
        /// Applys an actions to this HTTP Message
        /// </summary>
        /// <param name="httpMessage">The HTTP message to edit</param>
        /// <returns>The edited HTTP message</returns>
        public abstract HTTPMessage ApplyAction(HTTPMessage httpMessage);
    }
}
