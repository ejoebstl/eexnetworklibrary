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
using eExNetworkLibrary;
using System.IO;
using System.Xml;
using eExNLML.IO;
using eExNetworkLibrary.Monitoring;

namespace eExNLML.Extensibility
{
    /// <summary>
    /// Provides a base for the definition of handlers which enables extensibility. 
    /// </summary>
    public abstract class HandlerDefinition : eExNLML.Extensibility.IHandlerDefinition
    {
        #region Props

        /// <summary>
        /// Gets the name of this plug-in
        /// </summary>
        public string Name { get; protected set; }
        /// <summary>
        /// Gets the type of the plugin
        /// </summary>
        public string PluginType { get; protected set; }
        /// <summary>
        /// Gets a simple description of this plug-in
        /// </summary>
        public string Description { get; protected set; }
        /// <summary>
        /// Gets the author of this plug-in
        /// </summary>
        public string Author { get; protected set; }
        /// <summary>
        /// Gets a web-link for this plug-in
        /// </summary>
        public string WebLink { get; protected set; }
        /// <summary>
        /// Gets an unique key for this plug-in
        /// </summary>
        public string PluginKey { get; protected set; }
        /// <summary>
        /// Gets the version of this plug-in
        /// </summary>
        public Version Version { get; protected set; }

        #endregion

        /// <summary>
        /// Constructor which calls all create methods and initializes various properties.
        /// </summary>
        protected HandlerDefinition()
        {
            Name = "";
            Description = "";
            PluginType = PluginTypes.TrafficHandler;
            Author = "";
            WebLink = "http://www.eex-dev.net";
            PluginKey = "eex_no_key_present";
            Version = new Version(0, 0);
        }

        /// <summary>
        /// This method must create a new HandlerController instance associated with this class.
        /// </summary>
        /// <param name="env">The environment of the controller</param>
        /// <returns>The created HandlerController instance</returns>
        public abstract IHandlerController Create(IEnvironment env);

        public override string ToString()
        {
            return Name;
        }
    }
}
