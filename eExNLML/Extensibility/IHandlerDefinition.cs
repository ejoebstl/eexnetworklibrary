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
namespace eExNLML.Extensibility
{
    /// <summary>
    /// This interface defines a plug-in
    /// </summary>
    public interface IHandlerDefinition : IPlugin
    {  
        /// <summary>
        /// This method must create a new HandlerController instance associated with this class.
        /// </summary>
        /// <returns>The created HandlerController instance</returns>
        IHandlerController Create(IEnvironment env);
        /// <summary>
        /// Gets the author of this plug-in
        /// </summary>
        string Author { get; }      
        /// <summary>
        /// Gets a simple description of this plug-in
        /// </summary>
        string Description { get; }
        /// <summary>
        /// Gets the name of this plug-in
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Gets an unique key for this plug-in
        /// </summary>
        string PluginKey { get; }
        /// <summary>
        /// Gets the type of the plugin
        /// </summary>
        string PluginType { get; }
        /// <summary>
        /// Gets a web-link for this plug-in
        /// </summary>
        string WebLink { get; } 
    }
}
