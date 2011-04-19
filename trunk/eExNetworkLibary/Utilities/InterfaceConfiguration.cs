using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace eExNetworkLibrary.Utilities
{
    /// <summary>
    /// This class is capable of fetching and caching the interface configuration of IPInterfaces from the operating system
    /// </summary>
    public static class InterfaceConfiguration
    {
        private static string[] arInterfaceNames;
        private static IPAddress[][] arIPAddresses;
        private static IPAddress[] arLocalIPAddresses;
        private static MACAddress[] arMACAddresses;
        private static Subnetmask[][] arSubnetmasks;
        private static IPAddress[][] arStandardgateways;
        private static bool[] arIPEnabled;
        private static NetworkInterfaceType[] arAdapterType;
        private static string[] arDescription;
        private static string[] arEasyName;
        private static Dictionary<string, int> dictNameIndex;
        private static Dictionary<int, int> dictIntIndexStoreIndex;
        private static int[] arMTU;

        private static bool bChacheLoaded;

        /// <summary>
        /// Loads the interface configuration into a cache for quick access
        /// </summary>
        public static void LoadCache()
        {
            NetworkInterface[] arnic = NetworkInterface.GetAllNetworkInterfaces();
            bChacheLoaded = true;
            List<IPAddress[]> lipaAddresses = new List<IPAddress[]>();
            List<IPAddress> lLocalIPAddresses = new List<IPAddress>();
            List<string> lstrInterfaceNames = new List<string>();
            List<MACAddress> lmcMacAddress = new List<MACAddress>();
            List<Subnetmask[]> lsmSubnetmask = new List<Subnetmask[]>();
            List<IPAddress[]> lipaStandardgateway = new List<IPAddress[]>();
            List<bool> lbIPEnabled = new List<bool>();
            List<string> lsterDNSHostname = new List<string>();
            List<NetworkInterfaceType> lNicType = new List<NetworkInterfaceType>();
            List<string> lEasyName = new List<string>();
            List<string> lDescription = new List<string>();
            List<int> lMTU = new List<int>();

            dictNameIndex = new Dictionary<string, int>();
            dictIntIndexStoreIndex = new Dictionary<int,int>();

            int iStoreIndex = 0;

            foreach (NetworkInterface nic in arnic)
            {
                List<IPAddress> lipAddress = new List<IPAddress>();
                List<Subnetmask> lsmMask = new List<Subnetmask>();

                IPInterfaceProperties ipProps = nic.GetIPProperties();
                IPv4InterfaceProperties ipv4Props = null;
                IPv6InterfaceProperties ipv6Props = null;

                try { ipv4Props = ipProps.GetIPv4Properties(); }
                catch { /*Black hole, since there is an exception if the protocol is not supported by the OS.*/ }

                try { ipv6Props = ipProps.GetIPv6Properties(); }
                catch { /*Black hole, since there is an exception if the protocol is not supported by the OS.*/ }

                //Skip interface if not IP ready
                if (ipProps != null && (ipv4Props != null || ipv6Props != null))
                {
                    int iIndex;

                    if (ipv4Props != null)
                    {
                        iIndex = ipv4Props.Index;
                    }
                    else
                    {
                        iIndex = ipv6Props.Index;
                    }

                    dictIntIndexStoreIndex.Add(iIndex, iStoreIndex);

                    //Get Name
                    string strIntName = nic.Id;
                    lstrInterfaceNames.Add(strIntName);
                    dictNameIndex.Add(strIntName, iIndex);

                    //Get IPS & Subnetmasks
                    UnicastIPAddressInformationCollection aripTemp = ipProps.UnicastAddresses;

                    for (int iC1 = 0; iC1 < aripTemp.Count; iC1++)
                    {
                        if (aripTemp[iC1].Address.AddressFamily == AddressFamily.InterNetwork ||
                            aripTemp[iC1].Address.AddressFamily == AddressFamily.InterNetworkV6)
                        {
                            if (aripTemp[iC1].IPv4Mask != null && aripTemp[iC1].Address != null && aripTemp[iC1].Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                lipAddress.Add(aripTemp[iC1].Address);
                                lsmMask.Add(new Subnetmask(aripTemp[iC1].IPv4Mask.GetAddressBytes()));
                            }
                            else if (aripTemp[iC1].Address != null && aripTemp[iC1].Address.AddressFamily == AddressFamily.InterNetworkV6)
                            {
                                lipAddress.Add(aripTemp[iC1].Address);
                                lsmMask.Add(Subnetmask.IPv6Default);
                            }
                        }
                    }

                    lipaAddresses.Add(lipAddress.ToArray());
                    lLocalIPAddresses.AddRange(lipAddress);
                    lsmSubnetmask.Add(lsmMask.ToArray());
                    if (ipv4Props != null)
                    {
                        lMTU.Add(ipv4Props.Mtu);
                    }
                    else
                    {
                        lMTU.Add(ipv6Props.Mtu);
                    }

                    lipAddress.Clear();

                    lsmMask.Clear();

                    try
                    {
                        lmcMacAddress.Add(new MACAddress(nic.GetPhysicalAddress().GetAddressBytes()));
                    }
                    catch
                    {
                        //Interface is not ethernet - we have no MAC
                        lmcMacAddress.Add(MACAddress.Empty);
                    }


                    //Get Standardgateways
                    GatewayIPAddressInformationCollection arGateways = ipProps.GatewayAddresses;

                    for (int iC1 = 0; iC1 < arGateways.Count; iC1++)
                    {
                        IPAddress ipa = arGateways[iC1].Address;
                        if (ipa.AddressFamily == AddressFamily.InterNetwork || ipa.AddressFamily == AddressFamily.InterNetworkV6)
                        {
                            lipAddress.Add(ipa);
                        }
                    }

                    lipaStandardgateway.Add(lipAddress.ToArray());

                    lDescription.Add(nic.Description);
                    lEasyName.Add(nic.Name);
                    lNicType.Add(nic.NetworkInterfaceType);

                    iStoreIndex++;
                }
            }

         

            arInterfaceNames = lstrInterfaceNames.ToArray();
            arIPAddresses = lipaAddresses.ToArray();
            arMACAddresses = lmcMacAddress.ToArray();
            arSubnetmasks = lsmSubnetmask.ToArray();
            arStandardgateways = lipaStandardgateway.ToArray();
            arIPEnabled = lbIPEnabled.ToArray();

            arLocalIPAddresses = lLocalIPAddresses.ToArray();

            arAdapterType = lNicType.ToArray();
            arDescription = lDescription.ToArray();
            arEasyName = lEasyName.ToArray();
            arMTU = lMTU.ToArray();
        }

        /// <summary>
        /// Gets the adapter type for a specific interface
        /// </summary>
        /// <param name="strName">The interface name</param>
        /// <returns>The adapter type</returns>
        public static NetworkInterfaceType GetAdapterTypeForInterface(string strName)
        {
            CheckChache();
            return arAdapterType[GetStoreIndexForName(strName)];
        }

        private static void CheckChache()
        {
            if (!bChacheLoaded)
                LoadCache();
        }

        private static int GetStoreIndexForName(string strName)
        {
            CheckChache();
            string strPrepared = PrepareString(strName);
            if (dictNameIndex.ContainsKey(strPrepared))
            {
                return dictIntIndexStoreIndex[dictNameIndex[strPrepared]];
            }

            throw new NotImplementedException("The submitted interface could not be found");
        }

        /// <summary>
        /// Gets the interface index for a specific interface
        /// </summary>
        /// <param name="strName">The interface name</param>
        /// <returns>The interface index</returns>
        public static int GetIndexForName(string strName)
        {
            CheckChache();
            string strPrepared = PrepareString(strName);
            if (dictNameIndex.ContainsKey(strPrepared))
            {
                return dictNameIndex[strPrepared];
            }

            throw new NotImplementedException("The submitted interface name could not be found");
        }

        /// <summary>
        /// Gets the Maximum Transmission Unit for a specific interface
        /// </summary>
        /// <param name="strName">The interface name</param>
        /// <returns>The Maximum Transmission Unit of the interface.</returns>
        public static int GetMtuForInterface(string strName)
        {
            CheckChache();
            return arMTU[GetStoreIndexForName(strName)];
        }


        /// <summary>
        /// Gets the MACAddress for a specific interface
        /// </summary>
        /// <param name="strName">The interface name</param>
        /// <returns>The MACAddress</returns>
        public static MACAddress GetMacAddressForInterface(string strName)
        {
            CheckChache();
            return arMACAddresses[GetStoreIndexForName(strName)];
        }

        /// <summary>
        /// Gets a bool indicating whether IP is enabled for a specific interface
        /// </summary>
        /// <param name="strName">The interface name</param>
        /// <returns>A bool indicating whether IP is enabled</returns>
        public static bool GetIsIPEnabled(string strName)
        {
            CheckChache();
            return arIPEnabled[GetStoreIndexForName(strName)];
        }

        /// <summary>
        /// Gets the easy name for a specific interface
        /// </summary>
        /// <param name="strName">The interface name</param>
        /// <returns>The easy name</returns>
        public static string GetEasyName(string strName)
        {
            CheckChache();
            return arEasyName[GetStoreIndexForName(strName)];
        }

        /// <summary>
        /// Returns the local addresses allocated by the operating system.
        /// </summary>
        public static IPAddress[] LocalAddresses
        {
            get { return (IPAddress[])arLocalIPAddresses.Clone(); }
        }

        /// <summary>
        /// Returns a bool inidcating whether the given IP address is an address allocated by the operating system.
        /// </summary>
        /// <param name="ipa">The IP address to check for</param>
        /// <returns>A bool inidcating whether the given IP address is an address allocated by the operating system.</returns>
        public static bool IsLocalAddress(IPAddress ipa)
        {
            foreach (IPAddress ipaLocal in arLocalIPAddresses)
            {
                if (ipa.Equals(ipaLocal))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the description for a specific interface
        /// </summary>
        /// <param name="strName">The interface name</param>
        /// <returns>The description</returns>
        public static string GetFriendlyName(string strName)
        {
            CheckChache();
            return arDescription[GetStoreIndexForName(strName)];
        }


        /// <summary>
        /// Gets the standard gateways for a specific interface
        /// </summary>
        /// <param name="strName">The interface name</param>
        /// <returns>The standard gateways</returns>
        public static IPAddress[] GetIPStandardGatewaysForInterface(string strName)
        {
            CheckChache();
            return arStandardgateways[GetStoreIndexForName(strName)];
        }

        /// <summary>
        /// Gets the IPAddresses for a specific interface
        /// </summary>
        /// <param name="strName">The interface name</param>
        /// <returns>The IPAddresses</returns>
        public static IPAddress[] GetIPAddressesForInterface(string strName)
        {
            CheckChache();
            return arIPAddresses[GetStoreIndexForName(strName)];
        }

        /// <summary>
        /// Gets the subnetmasks for a specific interface
        /// </summary>
        /// <param name="strName">The interface name</param>
        /// <returns>The subnetmasks</returns>
        public static Subnetmask[] GetIPSubnetsForInterface(string strName)
        {
            CheckChache();
            return arSubnetmasks[GetStoreIndexForName(strName)];
        }

        /// <summary>
        /// Gets all known interface names
        /// </summary>
        /// <returns>All known interface names</returns>
        public static string[] GetAllInterfaceNames()
        {
            CheckChache();
            return arInterfaceNames;
        }

        private static string PrepareString(string strInterfaceName)
        {
            string strDevice = "" + strInterfaceName;
            if (strDevice.StartsWith("\\Device\\NPF_"))
            {
                strDevice = strDevice.Substring(12);
            }
            return strDevice;
        }
    }
}
