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
    public interface IPlugin
    {       
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
        /// <summary>
        /// Gets the version of this plug-in
        /// </summary>
        Version Version { get; }
    }

    public static class PluginTypes
    {
        public static string TrafficHandler { get { return "TrafficHandler"; } }
        public static string InterfaceFactory { get { return "InterfaceFactory"; } }
        public static string Interface { get { return  "Interface"; } }
        public static string SplitterRule { get { return "SplitterRule"; } }
        public static string HTTPModifierCondition { get { return "HTTPModifierCondition"; } }
        public static string HTTPModifierAction { get { return "HTTPModifierAction"; } }
        public static string ProtocolProvider { get { return "ProtocolProvider"; } }    
    }
}
