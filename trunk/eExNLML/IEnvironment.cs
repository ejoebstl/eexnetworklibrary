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
using eExNetworkLibrary.Attacks.Scanning;
using eExNetworkLibrary.Attacks.MITM;
using eExNetworkLibrary.Routing;
using eExNetworkLibrary.TrafficSplitting;
using eExNetworkLibrary;
using eExNetworkLibrary.Attacks;
using eExNLML.Extensibility;

namespace eExNLML
{
    /// <summary>
    /// Provides an interface for the management layer environments
    /// </summary>
    public interface IEnvironment
    {
        /// <summary>
        /// Returns all scanners known in the current environment
        /// </summary>
        /// <returns>All scanners known in the current environment</returns>
        IScanner[] GetScanners();
        /// <summary>
        /// Returns all IP-based MITM-attacks known in the current environment
        /// </summary>
        /// <returns>All IP-based MITM-attacks known in the current environment</returns>
        IMITMAttack[] GetMITMAttacks();
        /// <summary>
        /// Returns all IP-based attacks against single hosts in the current environment
        /// </summary>
        /// <returns>All IP-based attacks against single hosts in the current environment</returns>
        ISingleHostAttack[] GetAttacks();
        /// <summary>
        /// Returns all routers known in the current environment
        /// </summary>
        /// <returns>All routers known in the current environment</returns>
        IRouter[] GetRouters();

        /// <summary>
        /// Returns all traffic handlers in the current environment.
        /// </summary>
        TrafficHandler[] ActiveHandlers { get; }
        /// <summary>
        /// Returns all controllers in the current environment.
        /// </summary>
        IHandlerController[] Controllers { get; }
        /// <summary>
        /// Gets the controller for the given name from the current environment.
        /// </summary>
        /// <param name="strName">The name to get the controller for.</param>
        /// <returns>The controller with the given name.</returns>
        IHandlerController GetControllerForName(string strName);
        /// <summary>
        /// Gets the TrafficHandler for the given name from the current environment.
        /// </summary>
        /// <param name="strName">The name to get the TrafficHandler for.</param>
        /// <returns>The TrafficHandler with the given name.</returns>
        TrafficHandler GetHandlerForName(string strName);
        /// <summary>
        /// Gets the controller for the given TrafficHandler
        /// </summary>
        /// <param name="thHandler">The TrafficHandler to get the controller for</param>
        /// <returns>The controller for the given TrafficHandler</returns>
        IHandlerController GetControllerForHandler(TrafficHandler thHandler);        
        
        /// <summary>
        /// Returns all known plug-ins with the given type.
        /// </summary>
        /// <returns>All known plug-ins with the given type.</returns>
        IPlugin[] GetPluginsByType(string strType);

        /// <summary>
        /// Gets all plug-ins known by this environment.
        /// </summary>
        /// <returns>All plug-ins known by this environment.</returns>
        IPlugin[] GetPlugins();

        /// <summary>
        /// Returns the plug-in with the given plug-in key.
        /// </summary>
        /// <returns>The plug-in with the given plug-in key or null, if no plug-in was found.</returns>
        IPlugin GetPlugInByKey(string strPluginKey);
    }
}
