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

namespace eExNLML.Extensibility
{
    /// <summary>
    /// Provides an interface for protocol definitions.
    /// </summary>
    public interface IProtocolDefinition : IPlugin
    {
        /// <summary>
        /// When implemented by a deriven class, must return an instance of the given protocol provider.
        /// This method is called whenever a new traffic handler is instantiated, so that a the provider can be added to the traffic handler. 
        /// </summary>
        /// <returns>A protocol provider</returns>
        eExNetworkLibrary.ProtocolParsing.IProtocolProvider Create();
    }
}
