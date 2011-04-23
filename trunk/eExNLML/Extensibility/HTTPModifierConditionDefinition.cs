using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.TrafficModifiers.StreamModification.HTTP;
using eExNLML.IO;

namespace eExNLML.Extensibility
{
    /// <summary>
    /// This provides an abstract base class for all HTTP modifier conditions
    /// </summary>
    public abstract class HTTPModifierConditionDefinition : ISubPlugInDefinition<HTTPStreamModifierCondition>
    {
        #region Props
        
        /// <summary>
        /// Returns the name of this modifier condition
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
        protected HTTPModifierConditionDefinition()
        {
            Name = "";
            PluginType = PluginTypes.HTTPModifierCondition;
            Description = "";
            Author = "";
            WebLink = "http://www.eex-dev.net";
            PluginKey = "eex_http_condition_no_key";
            Version = new Version(0, 0);
        }

        /// <summary>
        /// Must create a new HTTP modifier condition
        /// </summary>
        /// <returns>A new HTTP modifier condition</returns>
        public abstract HTTPStreamModifierCondition Create();
        /// <summary>
        /// Must create a HTTP modifier condition according to the given nested configuration
        /// </summary>
        /// <param name="nviConfigurationRoot">The configuration as name value items</param>
        /// <returns>A new HTTP modifier condition</returns>
        public abstract HTTPStreamModifierCondition Create(NameValueItem nviConfigurationRoot);
        /// <summary>
        /// Must return the configuration of the given HTTP modifier condition
        /// </summary>
        /// <param name="htCondition">The HTTP modifier condition to get the configuration for</param>
        /// <returns>The configuration of the given HTTP modifier condition as an array of name value items</returns>
        public abstract NameValueItem[] GetConfiguration(HTTPStreamModifierCondition htCondition);

        public override string ToString()
        {
            return Name;
        }
    }
}
