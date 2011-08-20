// This source file is part of the eEx Network Library
//
// Author: 	    Emanuel Jöbstl <emi@eex-dev.net>
// Weblink: 	http://network.eex-dev.net
//		        http://eex.codeplex.com
//
// Licensed under the GNU Library General Public License (LGPL) 
//
// (c) eex-dev 2007-2011

using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using eExNetworkLibrary.ARP;
using eExNetworkLibrary.Utilities;
using eExNetworkLibrary.IP;

namespace eExNetworkLibrary.Monitoring
{
    /// <summary>
    /// This class provides capability of building network graphs by analyzing traffic
    /// </summary>
    public class NetMap : Monitoring.TrafficAnalyzer
    {
        private List<Host> lHosts;
        private List<Host> lDataLinkDistributors;
        private List<Host> lDataLinkNeighbours;
        private List<Host> lUpperLayerNeighbours;
        private Dictionary<IPAddress, Host> dictIPHost;
        private Dictionary<MACAddress, Host> dictMACHost;
        private Host hLocalhost;
        private NetDiscoveryUtility ndiuUtility;

        /// <summary>
        /// A bool indicating whether host names of found hosts should be resolved actively.
        /// </summary>
        public bool ResolveHostnames { get; set; }

        /// <summary>
        /// This delegate represents the method which is used to handly host changes
        /// </summary>
        /// <param name="args">The arguments</param>
        /// <param name="sender">The class which rised this event</param>
        public delegate void HostChangedEventHandler(HostInformationChangedEventArgs args, object sender);

        /// <summary>
        /// This event is fired when the information about any host in the graph is changed
        /// </summary>
        public event HostChangedEventHandler HostInformationChanged;

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public NetMap()
        {
            ndiuUtility = new NetDiscoveryUtility();
            ndiuUtility.OnResolveFinished += new NetDiscoveryUtility.ResolveCompletedEventHandler(ndiuUtility_OnResolveFinished);
            lHosts = new List<Host>();
            dictIPHost = new Dictionary<IPAddress, Host>();
            dictMACHost = new Dictionary<MACAddress, Host>();
            lDataLinkNeighbours = new List<Host>();
            lUpperLayerNeighbours = new List<Host>();
            lDataLinkDistributors = new List<Host>();
            ResolveHostnames = true;

            hLocalhost = CreateHost(System.Environment.MachineName);
            Host hNetwork;
            IPAddress[] ipa;
            Subnetmask[] smMasks;
            MACAddress mcMac;

            string[] strInterfaces = InterfaceConfiguration.GetAllInterfaceNames();

            for (int iC2 = 0; iC2 < strInterfaces.Length; iC2++)
            {
                if (strInterfaces[iC2] != null)
                {
                    ipa = InterfaceConfiguration.GetIPAddressesForInterface(strInterfaces[iC2]);
                    smMasks = InterfaceConfiguration.GetIPSubnetsForInterface(strInterfaces[iC2]);
                    mcMac = InterfaceConfiguration.GetMacAddressForInterface(strInterfaces[iC2]);
                    if (ipa != null)
                    {
                        for (int iC1 = 0; iC1 < ipa.Length; iC1++)
                        {
                            if (ipa[iC1].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                if (smMasks != null && smMasks.Length > iC1)
                                {
                                    AddIPToHost(hLocalhost, ipa[iC1]);
                                    hNetwork = CreateHost("Network");
                                    SetAsDistributionDevice(hNetwork);
                                    hNetwork.Properties.Add("subnetmask", smMasks[iC1]);
                                    hNetwork.Properties.Add("network", IPAddressAnalysis.GetClasslessNetworkAddress(ipa[iC1], smMasks[iC1]));
                                    AddIPToHost(hNetwork, IPAddressAnalysis.GetClasslessNetworkAddress(ipa[iC1], smMasks[iC1]));
                                    lDataLinkDistributors.Add(hNetwork);
                                    Connect(hNetwork, hLocalhost);
                                }
                            }
                        }
                    }
                    if (mcMac != null)
                    {
                        AddMACToHost(hLocalhost, mcMac);
                    }
                }
            }
        }

        void ndiuUtility_OnResolveFinished(object sender, IPHostEntry args)
        {
            foreach (IPAddress ipa in args.AddressList)
            {
                if (dictIPHost.ContainsKey(ipa))
                {
                    AssociateHostWithName(dictIPHost[ipa], args.HostName);
                }
            }
        }

        private void AssociateHostWithName(Host h, string strName)
        {
            h.Name = strName;
            InvokeHostStatusChanged(new HostInformationChangedEventArgs(h));
        }

        #region helpers

        private Host CreateHost(string strName)
        {
            Host h = new Host(strName);
            lHosts.Add(h);
            InvokeHostStatusChanged(new HostInformationChangedEventArgs(h));
            return h;
        }

        private void AddIPToHost(Host h, IPAddress ipa)
        {
            if (!h.IPAddresses.Contains(ipa))
            {
                h.IPAddresses.Add(ipa);
                if (dictIPHost.ContainsKey(ipa))
                {
                    dictIPHost[ipa].IPAddresses.Remove(ipa);
                    dictIPHost[ipa].Name = "Unknown";
                    InvokeHostStatusChanged(new HostInformationChangedEventArgs(dictIPHost[ipa]));
                    dictIPHost.Remove(ipa);
                }
                dictIPHost.Add(ipa, h);

                Host hNet = GetNetworkForAddress(ipa);
                if (hNet != null)
                {
                    Connect(h, hNet);
                    if (hNet.Type == HostType.Network)
                    {
                        SetAsPhysicalNeighbour(h);
                    }
                }
                else
                {
                    if (h.Type != HostType.Network)
                    {
                        SetAsUpperLayerNeighbour(h);
                    }
                }
                if ((h.Type == HostType.PhysicalNeigbour || h.Type == HostType.UpperLayerNeigbour) && h != hLocalhost && ResolveHostnames)
                {
                    ndiuUtility.ResolveHostnameAsnc(ipa);
                }
            }
        }

        private void AddMACToHost(Host h, MACAddress mac)
        {
            if (!h.MACAddresses.Contains(mac))
            {
                h.MACAddresses.Add(mac);
                if (dictMACHost.ContainsKey(mac))
                {
                    dictMACHost[mac].MACAddresses.Remove(mac);
                    InvokeHostStatusChanged(new HostInformationChangedEventArgs(dictMACHost[mac]));
                    dictMACHost.Remove(mac);
                }
                dictMACHost.Add(mac, h);
                InvokeHostStatusChanged(new HostInformationChangedEventArgs(h));
            }
        }

        private void SetAsPhysicalNeighbour(Host h)
        {
            if (h.Type != HostType.PhysicalNeigbour)
            {
                h.Type = HostType.PhysicalNeigbour;
                InvokeHostStatusChanged(new HostInformationChangedEventArgs(h));
            }
        }

        private void SetAsDistributionDevice(Host h)
        {
            if (h.Type != HostType.Network)
            {
                h.Type = HostType.Network;
                InvokeHostStatusChanged(new HostInformationChangedEventArgs(h));
            }
        }

        private void SetAsUpperLayerNeighbour(Host h)
        {
            if (h.Type != HostType.UpperLayerNeigbour)
            {
                h.Type = HostType.UpperLayerNeigbour;
                InvokeHostStatusChanged(new HostInformationChangedEventArgs(h));
            }
        }

        private void Connect(Host h, Host h2)
        {
            if (!h.ConnectedTo.Contains(h2) && !h2.ConnectedTo.Contains(h))
            {
                h.ConnectedTo.Add(h2);
                h2.ConnectedTo.Add(h);
                InvokeHostStatusChanged(new HostInformationChangedEventArgs(h));
                InvokeHostStatusChanged(new HostInformationChangedEventArgs(h2));
            }
        }

        private void Disconnect(Host h, Host h2)
        {
            if (h.ConnectedTo.Contains(h2) && h2.ConnectedTo.Contains(h))
            {
                h.ConnectedTo.Remove(h2);
                h2.ConnectedTo.Remove(h);
                InvokeHostStatusChanged(new HostInformationChangedEventArgs(h));
                InvokeHostStatusChanged(new HostInformationChangedEventArgs(h2));
            }
        }

        private void InvokeHostStatusChanged(HostInformationChangedEventArgs args)
        {
            if (HostInformationChanged != null)
            {
                foreach (Delegate dDelgate in HostInformationChanged.GetInvocationList())
                {
                    if (dDelgate.Target != null && dDelgate.Target is System.ComponentModel.ISynchronizeInvoke
                        && ((System.ComponentModel.ISynchronizeInvoke)(HostInformationChanged.Target)).InvokeRequired)
                    {
                        ((System.ComponentModel.ISynchronizeInvoke)(HostInformationChanged.Target)).Invoke(HostInformationChanged, new object[] { args, this });
                    }
                    else
                    {
                        HostInformationChanged.DynamicInvoke(args, this);
                    }
                }
            }
        }

        private void ProcessAdresses(IPAddress ipa, MACAddress mca)
        {
            if (mca != null)
            {
                if (!hLocalhost.MACAddresses.Contains(mca))
                {
                    if (!dictMACHost.ContainsKey(mca) && !dictIPHost.ContainsKey(ipa))
                    {
                        Host h = CreateHost("Unknown");
                        AddMACToHost(h, mca);
                        AddIPToHost(dictMACHost[mca], ipa);
                    }
                    if (!dictIPHost.ContainsKey(ipa))
                    {
                        AddIPToHost(dictMACHost[mca], ipa);
                    }
                    if (!dictMACHost.ContainsKey(mca))
                    {
                        AddMACToHost(dictIPHost[ipa], mca);
                    }
                }
            }
            else
            {
                if (!hLocalhost.IPAddresses.Contains(ipa))
                {
                    if (!dictIPHost.ContainsKey(ipa))
                    {
                        Host h = CreateHost("Unknown");
                        AddIPToHost(h, ipa);
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Analyzes the input frame for new information.
        /// </summary>
        /// <param name="fInputFrame">The frame to analyze</param>
        protected override void HandleTraffic(Frame fInputFrame)
        {
            ARPFrame arpFrame = GetARPFrame(fInputFrame);
            Ethernet.EthernetFrame ethFrame = GetEthernetFrame(fInputFrame);
            IP.IPFrame ipFrame = GetIPv4Frame(fInputFrame);

            if (arpFrame != null)
            {
                if (arpFrame.Operation == ARPOperation.Request)
                {
                    ProcessAdresses(arpFrame.SourceIP, arpFrame.SourceMAC);
                }
                if (arpFrame.Operation == ARPOperation.Reply)
                {
                    ProcessAdresses(arpFrame.SourceIP, arpFrame.SourceMAC);
                }
            }

            if (ethFrame != null && ipFrame != null)
            {
                ProcessAdresses(ipFrame.SourceAddress, null);
                ProcessAdresses(ipFrame.DestinationAddress, null);
            }
        }

        /// <summary>
        /// Stops all pending name resolves and all worker threads
        /// </summary>
        public override void Stop()
        {
            ndiuUtility.CancelAll();
            base.Stop();
        }

        private Host GetNetworkForAddress(IPAddress ipa)
        {
            int iMask;
            int iMaskFav = -1;
            Host hFavourite = null;
            foreach (Host h in lDataLinkDistributors)
            {
                if (((IPAddress)h.Properties["network"]).Equals(IPAddressAnalysis.GetClasslessNetworkAddress(ipa, (Subnetmask)h.Properties["subnetmask"])))
                {
                    iMask = SubnetmaskToInt((Subnetmask)h.Properties["subnetmask"]);

                    if (iMask > iMaskFav)
                    {
                        iMaskFav = SubnetmaskToInt((Subnetmask)h.Properties["subnetmask"]);
                        hFavourite = h;
                    }
                }
            }

            return hFavourite;
        }

        private int SubnetmaskToInt(Subnetmask seMask)
        {
            byte[] bMaskBytes = seMask.MaskBytes;

            return BitConverter.ToInt32(bMaskBytes, 0);
        }

        /// <summary>
        /// Returns all known hosts
        /// </summary>
        public Host[] Hosts
        {
            get { return lHosts.ToArray(); }
        }

        /// <summary>
        /// Returns the localhost
        /// </summary>
        public Host Localhost
        {
            get { return hLocalhost; }
        }

        /// <summary>
        /// Returns all data link distrubutors (switches, hubs etc.) around the local host
        /// </summary>
        public Host[] DataLinkDistributors
        {
            get { return lDataLinkDistributors.ToArray(); }
        }

        /// <summary>
        /// Returns all data link neighbour hosts around the local host
        /// </summary>
        public Host[] DataLinkNeighbours
        {
            get { return lDataLinkNeighbours.ToArray(); }
        }

        /// <summary>
        /// Returns all known upper layer neighbours around the local host
        /// </summary>
        public Host[] UpperLayerNeighbours
        {
            get { return lUpperLayerNeighbours.ToArray(); }
        }

        /// <summary>
        /// Checks whether the given IP address is associated with a host
        /// </summary>
        /// <param name="ipa">The IP address to search for</param>
        /// <returns>A bool indicating whether the given IP address is associated with a host</returns>
        public bool ContainsHostForIP(IPAddress ipa)
        {
            return dictIPHost.ContainsKey(ipa);
        }

        /// <summary>
        /// Checks whether the given MAC address is associated with a host
        /// </summary>
        /// <param name="mac">The MAC address to search for</param>
        /// <returns>A bool indicating whether the given MAC address is associated with a host</returns>
        public bool ContainsHostForMAC(MACAddress mac)
        {
            return dictMACHost.ContainsKey(mac);
        }

        /// <summary>
        /// Returns the host associated with a given IP address
        /// </summary>
        /// <param name="ipa">The IP address to search for</param>
        /// <returns>The host associated with a given IP address</returns>
        public Host GetHostForIP(IPAddress ipa)
        {
            return dictIPHost[ipa];
        }

        /// <summary>
        /// Returns the host associated with a given MAC address
        /// </summary>
        /// <param name="mac">The MAC address to search for</param>
        /// <returns>The host associated with a given MAC address</returns>
        public Host GetHostForMAC(MACAddress mac)
        {
            return dictMACHost[mac];
        }

        /// <summary>
        /// Does nothing
        /// </summary>
        public override void Cleanup()
        {
            //Don't need to do anything on init shutdown. 
        }
    }

    /// <summary>
    /// This class represents an IP host which can be used for building host graphes
    /// </summary>
    public class Host
    {
        private string strName;
        private List<IPAddress> ipaAddresses;
        private List<MACAddress> maMacAddresses;
        private Dictionary<string, object> dictProperties;
        private List<Host> lConnectedTo;
        private HostType tType;

        /// <summary>
        /// Gets the property dictionary associated with this host
        /// </summary>
        public Dictionary<string, object> Properties
        {
            get { return dictProperties; }
        }

        /// <summary>
        /// Gets a list containing all connected hosts
        /// </summary>
        public List<Host> ConnectedTo
        {
            get { return lConnectedTo; }
        }

        /// <summary>
        /// Gets a list containing all IP addresses 
        /// </summary>
        public List<IPAddress> IPAddresses
        {
            get { return ipaAddresses; }
        }

        /// <summary>
        /// Gets a list containing all MAC addresses 
        /// </summary>
        public List<MACAddress> MACAddresses
        {
            get { return maMacAddresses; }
        }

        /// <summary>
        /// Gets a list containing the Name of this host
        /// </summary>
        public string Name
        {
            get { return strName; }
            set { strName = value; }
        }

        /// <summary>
        /// Gets a list containing the type of this host
        /// </summary>
        public HostType Type
        {
            get { return tType; }
            set { tType = value; }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="strName">The hostname</param>
        public Host(string strName)
        {
            this.strName = strName;
            this.ipaAddresses = new List<IPAddress>();
            this.maMacAddresses = new List<MACAddress>();
            this.dictProperties = new Dictionary<string, object>();
            this.lConnectedTo = new List<Host>();
            tType = HostType.Unknown;
        }
    }

    /// <summary>
    /// An enumeration for host types
    /// </summary>
    public enum HostType
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = -1,
        /// <summary>
        /// Physical neighbour (same subnet)
        /// </summary>
        PhysicalNeigbour = 0,
        /// <summary>
        /// Upper layer neighbour (internet etc.)
        /// </summary>
        UpperLayerNeigbour = 1,
        /// <summary>
        /// Network (switch, hub etc.)
        /// </summary>
        Network = 2
    }

    /// <summary>
    /// This class carries information about host changed event args
    /// </summary>
    public class HostInformationChangedEventArgs : EventArgs
    {
        private Host hHost;

        /// <summary>
        /// The host which changed
        /// </summary>
        public Host Host
        {
            get { return hHost; }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="hHost">The host which changed</param>
        public HostInformationChangedEventArgs(Host hHost)
        {
            this.hHost = hHost;
        }
    }
}