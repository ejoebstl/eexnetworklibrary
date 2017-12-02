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
    /// This interface defines an interface extension factory which is capable of creating and returning more than one extensions at once.
    /// This interface makes it possible to create any number of network interface definitions dynamically, for example one for each interface of the computer. 
    /// </summary>
    public interface IInterfaceFactory : IPlugin
    {
        /// <summary>
        /// Must create and return any number of interface extensions (interface handler definitions).
        /// This interface makes it possible to create any number of interface definitions, for example one for each interface of the computer. 
        /// </summary>
        /// <returns>Some interface extensions</returns>
        IInterfaceDefinition[] Create();
    }
}
