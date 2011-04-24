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
    /// This intrerface defines a base for all TrafficHandler controller classes, which define TrafficHandler ports, handle linking and the loading and saving of configuration.
    /// </summary>
    public interface IHandlerController
    {
        /// <summary>
        /// Gets all traffic handler ports which are attachable at the moment
        /// </summary>
        eExNLML.TrafficHandlerPort[] AvailableTrafficHandlerPorts { get; } 
        
        /// <summary>
        /// Gets the definition for this handler.
        /// </summary>
        IHandlerDefinition BaseDefinition { get; }   
    
        /// <summary>
        /// Gets the configuration loader instance created by this definition.
        /// </summary>
        eExNLML.IO.HandlerConfigurationLoader ConfigurationLoader { get; }

        /// <summary>
        /// Gets the configuration writer instance created by this definition.
        /// </summary>
        eExNLML.IO.HandlerConfigurationWriter ConfigurationWriter { get; }        
        
        /// <summary>
        /// Gets the environment associated with this controller.
        /// </summary>
        eExNLML.IEnvironment Environment { get; }

        /// <summary>
        /// Gets the port which is associated with the given name.
        /// </summary>
        /// <param name="strName">The name to get the port for.</param>
        /// <returns>The found port or null if not found.</returns>
        eExNLML.TrafficHandlerPort GetPortForName(string strName);

        /// <summary>
        /// Reads the configuration of this handler from the given XmlWriter.
        /// </summary>
        /// <param name="xmlReader">The XmlWriter to read the configuration from.</param>
        void LoadConfiguration(System.Xml.XmlReader xmlReader);

        /// <summary>
        /// Gets or sets this controllers unique name. 
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// This event is fired whenever the name is changing and allows to cancle the name change.
        /// </summary>
        event HandlerController.NameChangingEventHandler NameChanging;

        /// <summary>
        /// Gets a dictionary where custom named properties of this controller can be stored.
        /// <remarks>The types which also support saving and loading are: int, string, float, double, IPAddress, Subnetmask, MACAddress. All other types will simply be converted to a string via the ToString() method before saving.</remarks>
        /// </summary>
        System.Collections.Generic.Dictionary<string, object> Properties { get; }

        /// <summary>
        /// Writes the configuration of this handler in the given environment to the given XmlWriter.
        /// </summary>
        /// <param name="xmlWriter">The XmlWriter to writes the configuration to.</param>
        void SaveConfiguration(System.Xml.XmlWriter xmlWriter);

        /// <summary>
        /// Gets the traffic handler instance created by this definition.
        /// </summary>
        eExNetworkLibrary.TrafficHandler TrafficHandler { get; }

        /// <summary>
        /// Gets the traffic handler ports owned by this definition.
        /// </summary>
        eExNLML.TrafficHandlerPort[] TrafficHandlerPorts { get; }
    }
}
