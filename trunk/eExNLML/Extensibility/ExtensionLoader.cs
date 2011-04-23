using System;
using System.Collections.Generic;
using System.Text;

namespace eExNLML.Extensibility
{
    /// <summary>
    /// This class is capable of loading extensions for the network library management layer. 
    /// </summary>
    public class ExtensionLoader
    {
        /// <summary>
        /// Loads all handler definitions (extensions) from a specified DLL. 
        /// </summary>
        /// <param name="strPath">The path of the DLL to load the extensions from.</param>
        /// <returns>The loaded extensions</returns>
        public static IHandlerDefinition[] LoadExtensions(string strPath)
        {
            List<IHandlerDefinition> lDefinitions = new List<IHandlerDefinition>();
            PluginLoader<IHandlerDefinition> pLoader = new PluginLoader<IHandlerDefinition>();
            lDefinitions.AddRange(pLoader.LoadPlugins(strPath));

            PluginLoader<IInterfaceFactory> lFactories = new PluginLoader<IInterfaceFactory>();
            foreach (IInterfaceFactory eFactory in lFactories.LoadPlugins(strPath))
            {
                lDefinitions.AddRange(eFactory.Create());
            }

            return lDefinitions.ToArray();
        }

        /// <summary>
        /// Loads all handler definitions (extensions) from all DLLs in a specified directory. 
        /// </summary>
        /// <param name="strPath">The path of the directory which contains the DLLs</param>
        /// <returns>The loaded extensions</returns>
        public static IHandlerDefinition[] LoadExtensionsFromDirectory(string strPath)
        {
            List<IHandlerDefinition> lDefinitions = new List<IHandlerDefinition>();
            PluginLoader<IHandlerDefinition> pLoader = new PluginLoader<IHandlerDefinition>();
            lDefinitions.AddRange(pLoader.LoadPluginsFromDirectory(strPath));

            PluginLoader<IInterfaceFactory> lFactories = new PluginLoader<IInterfaceFactory>();
            foreach (IInterfaceFactory eFactory in lFactories.LoadPluginsFromDirectory(strPath))
            {
                lDefinitions.AddRange(eFactory.Create());
            }

            return lDefinitions.ToArray();
        }
    }
}
