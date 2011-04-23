using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.TrafficModifiers.StreamModification.HTTP;
using eExNLML.IO;

namespace eExNLML.Extensibility
{
    /// <summary>
    /// This provides an abstract base class for all HTTP modifier action conditions
    /// </summary>
    public abstract class HTTPModifierActionDefinition : ISubPlugInDefinition<HTTPStreamModifierAction>
    {
        #region Props

        /// <summary>
        /// Returns the name of this modifier action
        /// </summary>
        public string Name { get; protected set; }
        /// <summary>
        /// Gets the type of the plug-in
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
        /// Creates a new instance of this class.
        /// </summary>
        protected HTTPModifierActionDefinition()
        {
            Name = "";
            PluginType = PluginTypes.HTTPModifierAction;
            Description = "";
            Author = "";
            WebLink = "http://www.eex-dev.net";
            PluginKey = "eex_http_action_no_key";
            Version = new Version(0, 0);
        }

        /// <summary>
        /// Must create a new HTTP modifier action
        /// </summary>
        /// <returns>A new HTTP modifier action</returns>
        public abstract HTTPStreamModifierAction Create();
        /// <summary>
        /// Must create a HTTP modifier action according to the given nested configuration
        /// </summary>
        /// <param name="nviConfigurationRoot">The configuration as name value items</param>
        /// <returns>A new HTTP modifier action</returns>
        public abstract HTTPStreamModifierAction Create(NameValueItem nviConfigurationRoot);
        /// <summary>
        /// Must return the configuration of the given HTTP modifier action
        /// </summary>
        /// <param name="htCondition">The HTTP modifier action to get the configuration for</param>
        /// <returns>The configuration of the given HTTP modifier action as an array of name value items</returns>
        public abstract NameValueItem[] GetConfiguration(HTTPStreamModifierAction htCondition);

        public override string ToString()
        {
            return Name;
        }
    }
}
