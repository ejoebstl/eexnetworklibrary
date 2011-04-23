using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using eExNetworkLibrary.TrafficSplitting;
using eExNLML.IO;

namespace eExNLML.Extensibility
{
    /// <summary>
    /// Provides an interface for traffic splitter rule definitions
    /// </summary>
    public interface ISubPlugInDefinition<T> : IPlugin
    {
        /// <summary>
        /// Must create a new instance of this sub-plugin
        /// </summary>
        /// <returns>A new instance of this sub-plugin</returns>
        T Create();
        /// <summary>
        /// Must create a new instance of this sub-plugin according to the given nested configuration
        /// </summary>
        /// <param name="nviConfigurationRoot">The configuration as name value items</param>
        /// <returns>A new instance of this sub-plugin</returns>
        T Create(NameValueItem nviConfigurationRoot);
        /// <summary>
        /// Returns the name of the sub-plugin which is described by this definition. The name has to be exactly the same as in the name-property of the corresponding sub-plugin. 
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Must return the configuration of the given sub-plugin
        /// </summary>
        /// <param name="tsrRule">The sub-plugin rule to get the configuration for</param>
        /// <returns>The configuration of the given sub-plugin as an array of name value items</returns>
        NameValueItem[] GetConfiguration(T tsrRule);
    }
}
