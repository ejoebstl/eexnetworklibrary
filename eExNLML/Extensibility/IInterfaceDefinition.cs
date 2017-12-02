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
using System.Net.NetworkInformation;
namespace eExNLML.Extensibility
{
    /// <summary>
    /// This interface defines an network interface.
    /// </summary>
    public interface IInterfaceDefinition : IHandlerDefinition
    {
        /// <summary>
        /// Gets the unique GUID for this interface, which is diffrent for each NIC on each host. 
        /// </summary>
        string InterfaceGUID { get; }

        /// <summary>
        /// Gets the interface type for this interface.
        /// </summary>
        NetworkInterfaceType InterfaceType { get; }
    }
}
