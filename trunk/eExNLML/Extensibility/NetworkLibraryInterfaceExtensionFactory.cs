using System;
using System.Collections.Generic;
using System.Text;
using eExNLML.Extensibility;
using eExNetworkLibrary.Utilities;
using eExNetworkLibrary;
using eExNLML.DefaultDefinitions;

namespace eExNLML.Extensibility
{
    /// <summary>
    /// This is the default extension factory of the Network Library Management Layer.
    /// </summary>
    public class NetworkLibraryInterfaceExtensionFactory : IInterfaceFactory
    {
        /// <summary>
        /// Returns all interface extensions known by the Network Library Management Layer by default. This normally includes all Ethernet interfaces of the computer.
        /// </summary>
        /// <returns>All interface extensions known by the Network Library Management Layer by default</returns>
        public IInterfaceDefinition[] Create()
        {
            List<IInterfaceDefinition> lDefinitions = new List<IInterfaceDefinition>();

            foreach (WinPcapInterface wpc in EthernetInterface.GetAllPcapInterfaces())
            {
                if (InterfaceConfiguration.GetAdapterTypeForInterface(wpc.Name) == System.Net.NetworkInformation.NetworkInterfaceType.Ethernet ||
                    InterfaceConfiguration.GetAdapterTypeForInterface(wpc.Name) == System.Net.NetworkInformation.NetworkInterfaceType.Wireless80211)
                {
                    lDefinitions.Add(new EthernetInterfaceControlDefinition(wpc));
                }
            }

            return lDefinitions.ToArray();
        }

        public string Author
        {
            get { return "Emanuel Jöbstl"; }
        }

        public string Description
        {
            get { return "This plugin is capable of dynamically creating interface definitions."; }
        }

        public string Name
        {
            get { return "Network Library Interface Factory"; }
        }

        public string PluginKey
        {
            get { return "eex_network_library_interface_factory"; }
        }

        public string PluginType
        {
            get { return PluginTypes.InterfaceFactory; }
        }

        public string WebLink
        {
            get { return "http://www.eex-dev.net"; }
        }

        public Version Version
        {
            get { return new Version(1, 0); }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
