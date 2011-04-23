using System;
using System.Collections.Generic;
using System.Text;

namespace eExNLML.Extensibility
{
    /// <summary>
    /// Provides a base class for protocol definitions.
    /// </summary>
    public abstract class ProtocolDefinition
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
        protected ProtocolDefinition()
        {
            Name = "";
            Description = "";
            PluginType = PluginTypes.ProtocolProvider;
            Author = "";
            WebLink = "http://www.eex-dev.net";
            PluginKey = "eex_no_key_present";
            Version = new Version(0, 0);
        }

        /// <summary>
        /// When implemented by a deriven class, must return an instance of the given protocol provider.
        /// This method is called whenever a new traffic handler is instantiated, so that a the provider can be added to the traffic handler. 
        /// </summary>
        /// <returns>A protocol provider</returns>
        public abstract eExNetworkLibrary.ProtocolParsing.IProtocolProvider Create();
    }
}
