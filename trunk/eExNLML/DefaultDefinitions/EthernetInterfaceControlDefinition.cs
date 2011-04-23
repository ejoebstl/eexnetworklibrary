using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.Utilities;
using eExNetworkLibrary;
using eExNLML.IO;
using eExNLML.IO.HandlerConfigurationWriters;
using eExNLML.IO.HandlerConfigurationLoaders;
using eExNLML.Extensibility;
using eExNLML.DefaultControllers;

namespace eExNLML.DefaultDefinitions
{
    public class EthernetInterfaceControlDefinition : HandlerDefinition, IInterfaceDefinition
    {
        private WinPcapInterface wpcInt;

        public string InterfaceGUID
        {
            get { return wpcInt.Name; }
        }

        public EthernetInterfaceControlDefinition(WinPcapInterface wpcInt)
        {
            this.wpcInt = wpcInt;

            if (InterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Ethernet &&
                InterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Wireless80211)
            {
                throw new ArgumentException("Cannot create an interface with type " + InterfaceType.ToString() + ", since the EthernetInterface only supports ethernet.");
            }

            try
            {
                Name = InterfaceConfiguration.GetFriendlyName(wpcInt.Name);
            }
            catch (Exception ex)
            {
                Name = "[Could not load description: " + ex.Message + "]";
            }

            Description = "This traffic handler represents a WinPcap capable ethernet interface.\n" + this.Name;
            Author = "Emanuel Jöbstl";
            WebLink = "http://www.eex-dev.net";
            PluginType = PluginTypes.Interface;
            PluginKey = "eex_winpcap_ethernet";
        }

        public override IHandlerController Create(IEnvironment env)
        {
            return new EthernetInterfaceController(wpcInt, this, env);
        }

        public System.Net.NetworkInformation.NetworkInterfaceType InterfaceType
        {
            get { return InterfaceConfiguration.GetAdapterTypeForInterface(wpcInt.Name); }
        }
    }
}
