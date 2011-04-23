using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.TrafficSplitting;
using System.Net;
using eExNetworkLibrary;
using eExNLML.IO;

namespace eExNLML.Extensibility
{
    /// <summary>
    /// This provides an abstract base class for traffic splitter rule definitions
    /// </summary>
    public abstract class TrafficSplitterRuleDefinition : ISubPlugInDefinition<TrafficSplitterRule>
    {
        #region Props

        /// <summary>
        /// Returns the name of the traffic splitter rule which is described by this definition. 
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

        protected TrafficSplitterRuleDefinition()
        {
            Name = "";
            PluginType = PluginTypes.SplitterRule;
            Description = "";
            Author = "";
            WebLink = "http://www.eex-dev.net";
            PluginKey = "eex_splitrule_no_key";
            Version = new Version(0, 0);
        }

        /// <summary>
        /// Converts a action item to a name value item
        /// </summary>
        /// <param name="tsAction">The action item to convert</param>
        /// <returns>A name value item representing the given params</returns>
        protected NameValueItem ConvertActionToNameValueItem(TrafficSplitterActions tsAction)
        {
            return new NameValueItem("action", tsAction.ToString());
        }

        /// <summary>
        /// Converts a name value item to a action item
        /// </summary>
        /// <param name="nvi">The name value item to convert</param>
        /// <returns>The action item</returns>
        protected TrafficSplitterActions ConvertToAction(NameValueItem nvi)
        {
            if (nvi.Value == TrafficSplitterActions.SendToA.ToString())
            {
                return TrafficSplitterActions.SendToA;
            }
            else if (nvi.Value == TrafficSplitterActions.SendToB.ToString())
            {
                return TrafficSplitterActions.SendToB;
            }
            else
            {
                return TrafficSplitterActions.Drop;
            }
        }        
       
        /// <summary>
        /// Must create a new traffic splitter rule
        /// </summary>
        /// <returns>A new traffic splitter rule</returns>
        public abstract TrafficSplitterRule Create();
        /// <summary>
        /// Must create a traffic splitter rule according to the given nested configuration
        /// </summary>
        /// <param name="nviConfigurationRoot">The configuration as name value items</param>
        /// <returns>A new traffic splitter rule</returns>
        public abstract TrafficSplitterRule Create(NameValueItem nviConfigurationRoot);
        /// <summary>
        /// Must return the configuration of the given traffic splitter rule
        /// </summary>
        /// <param name="tsrRule">The traffic splitter rule to get the configuration for</param>
        /// <returns>The configuration of the given traffic splitter rule as an array of name value items</returns>
        public abstract NameValueItem[] GetConfiguration(TrafficSplitterRule tsrRule);

        public override string ToString()
        {
            return Name;
        }
    }
}
