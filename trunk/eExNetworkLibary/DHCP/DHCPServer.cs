using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using eExNetworkLibrary.Ethernet;

namespace eExNetworkLibrary.DHCP
{
    /// <summary>
    /// This class represents a DHCP server which is capable of assigning IPv4 addresses to clients
    /// </summary>
    public class DHCPServer : DirectInterfaceIOHandler
    {
        private Dictionary<IPInterface, DHCPPool> dictInterfacePool;

        /// <summary>
        /// The DHCP in port (UDP)
        /// </summary>
        protected int iDHCPInPort;

        /// <summary>
        /// The DHCP out port (UDP)
        /// </summary>
        protected int iDHCPOutPort;

        /// <summary>
        /// The IP identification counter
        /// </summary>
        protected int iIPIDCounter;
        private List<int> lOpenServerTransactions;
        private int iLeaseDuration;
        private IP.IPAddressAnalysis ipAnalysis;
        private bool bShuttingDown;

        /// <summary>
        /// The gateway IP address. Setting this field to null will cause this DHCP server to announce the outgoing interfac address as gateway.
        /// </summary>
        protected IPAddress ipaGateway;

        /// <summary>
        /// The DNS server IP address. Setting this field to null will cause this DHCP server to announce the outgoing interface address as DNS server.
        /// </summary>
        protected IPAddress ipaDNSServer;

        /// <summary>
        /// Creates a pool filled with addresses from the given start to the given end IP address and associates the pool to the according interfaces.
        /// Items which cannot be asooicated with an interface are ignored.
        /// </summary>
        /// <param name="ipaStart">The start address of the pool range</param>
        /// <param name="ipaEnd">The end address of the pool range</param>
        public void CreatePool(IPAddress ipaStart, IPAddress ipaEnd)
        {
            if (lInterfaces.Count < 1)
            {
                throw new Exception("There are currently no interfaces present.");
            }
            if (ipaStart.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork || ipaEnd.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
            {
                throw new ArgumentException("Only IPv4 is supported at the moment");
            }
            foreach (IPInterface ipi in lInterfaces)
            {
                for (int iC1 = 0; iC1 < ipi.IpAddresses.Length && iC1 < ipi.Subnetmasks.Length; iC1++)
                {
                    if (ipi.IpAddresses[iC1].AddressFamily == ipaStart.AddressFamily && ipi.IpAddresses[iC1].AddressFamily == ipaEnd.AddressFamily && 
                        (ipAnalysis.GetClasslessNetworkAddress(ipi.IpAddresses[iC1], ipi.Subnetmasks[iC1]).Equals(ipAnalysis.GetClasslessNetworkAddress(ipaStart, ipi.Subnetmasks[iC1])) ||
                        ipAnalysis.GetClasslessNetworkAddress(ipi.IpAddresses[iC1], ipi.Subnetmasks[iC1]).Equals(ipAnalysis.GetClasslessNetworkAddress(ipaEnd, ipi.Subnetmasks[iC1]))))
                    {
                        this.CreatePool(ipaStart, ipaEnd, ipi, iC1);
                    }
                }
            }
        }

        /// <summary>
        /// Adds the specified items.
        /// If the items can be associated with more than one connected interfaces, multiple pools are created - one for each interface.
        /// Items which cannot be associated with an interface are ignored.
        /// </summary>
        /// <param name="dhItems">The items to add.</param>
        public void AddToPool(DHCPPoolItem[] dhItems)
        {
            foreach (DHCPPoolItem dhItem in dhItems)
            {
                AddToPool(dhItem);
            }
        }

        /// <summary>
        /// Adds the specified item.
        /// An item which cannot be associated with an interface will be ignored.
        /// </summary>
        /// <param name="dhItem">The item to add.</param>
        public void AddToPool(DHCPPoolItem dhItem)
        {
            if (dhItem.Address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
            {
                throw new ArgumentException("Only IPv4 is supported at the moment");
            }
            foreach (IPInterface ipi in lInterfaces)
            {
                for (int iC1 = 0; iC1 < ipi.IpAddresses.Length && iC1 < ipi.Subnetmasks.Length; iC1++)
                {
                    if (ipi.IpAddresses[iC1].AddressFamily == dhItem.Address.AddressFamily 
                        && ipAnalysis.GetClasslessNetworkAddress(ipi.IpAddresses[iC1], ipi.Subnetmasks[iC1]).Equals(ipAnalysis.GetClasslessNetworkAddress(dhItem.Address, ipi.Subnetmasks[iC1])))
                    {
                        DHCPPool dhPool = GetPoolForInterface(ipi);
                        DHCPPoolItem dhPoolItem = new DHCPPoolItem(dhItem.Address, dhItem.Netmask, dhItem.Gateway, dhItem.DNSServer);
                        AddPoolItem(dhPoolItem, dhPool, ipi);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a pool from the given parameters
        /// </summary>
        /// <param name="ipaStart">The start address of the pool range</param>
        /// <param name="ipaEnd">The end address of the pool range</param>
        /// <param name="ipi">The IP idnterface to which this pool should be associated</param>
        /// <param name="iAddrCounter">The index of the address of the interface to use if the interface has multiple IP addresses assigned</param>
        protected virtual void CreatePool(IPAddress ipaStart, IPAddress ipaEnd, IPInterface ipi, int iAddrCounter)
        {
            if (ipaStart.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork || ipaEnd.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
            {
                throw new ArgumentException("Only IPv4 is supported at the moment");
            }

            DHCPPool dhPool = GetPoolForInterface(ipi);

            IPAddress[] ipRange = ipAnalysis.GetIPRange(ipaStart, ipaEnd);

            foreach (IPAddress ipa in ipRange)
            {
                IPInterface ipiInterface = ipi;
                IPAddress ipAddress = ipa;
                IPAddress ipGateway;
                Subnetmask smMask = ipi.Subnetmasks[iAddrCounter];
                IPAddress ipDnsServer = IPAddress.Any;


                if (ipaGateway == null)
                {
                    ipGateway = ipi.IpAddresses[iAddrCounter];
                }
                else
                {
                    ipGateway = ipaGateway;
                }
                if (ipaDNSServer == null)
                {
                    ipDnsServer = ipi.IpAddresses[iAddrCounter];
                }
                else
                {
                    ipDnsServer = ipaDNSServer;
                }

                if (dhPool.GetItemForAddress(ipAddress) == null)
                {
                    DHCPPoolItem dhPoolItem = new DHCPPoolItem(ipAddress, smMask, ipGateway, ipDnsServer);
                    AddPoolItem(dhPoolItem, dhPool, ipi);
                }
            }
        }

        /// <summary>
        /// Adds a pool item to a pool of an interface
        /// </summary>
        /// <param name="dhPoolItem">The pool item to add</param>
        /// <param name="dhPool">The DHCP pool to which this item should be added</param>
        /// <param name="ipi">The interface to which this pool item is associated (or null if it is unknown)</param>
        protected void AddPoolItem(DHCPPoolItem dhPoolItem, DHCPPool dhPool, IPInterface ipi)
        {
                dhPool.AddDHCPPoolItem(dhPoolItem);
                InvokeAddressCreated(new DHCPServerEventArgs(dhPool, dhPoolItem, ipi));
        }

        /// <summary>
        /// Removes a pool item from a pool of an interface
        /// </summary>
        /// <param name="dhPoolItem">The pool item to remove</param>
        /// <param name="dhPool">The DHCP pool from which this item should be removed</param>
        /// <param name="ipi">The interface to which this pool item is associated (or null if it is unknown)</param>
        protected void RemovePoolItem(DHCPPoolItem dhPoolItem, DHCPPool dhPool, IPInterface ipi)
        {
            dhPool.RemoveFromPool(dhPoolItem);
            InvokeAddressRemoved(new DHCPServerEventArgs(dhPool, dhPoolItem, ipi));
        }

        /// <summary>
        /// Removes a pool item.
        /// </summary>
        /// <param name="dhItemToRemove">The item to remove.</param>
        public void RemovePoolItem(DHCPPoolItem dhItemToRemove)
        {
            foreach (IPInterface ipi in dictInterfacePool.Keys)
            {
                if (dictInterfacePool[ipi].PoolContains(dhItemToRemove))
                {
                    RemovePoolItem(dhItemToRemove, dictInterfacePool[ipi], ipi);
                }
            }
        }

        /// <summary>
        /// Removes a range of pool items.
        /// </summary>
        /// <param name="dhItemsToRemove">The items to remove.</param>
        public void RemovePoolItems(DHCPPoolItem[] dhItemsToRemove)
        {
            foreach (DHCPPoolItem dhItem in dhItemsToRemove)
            {
                RemovePoolItem(dhItem);
            }
        }

        /// <summary>
        /// Starts the cleanup process and stops the leasing of new addresses
        /// </summary>
        public override void Cleanup()
        {
            bShuttingDown = true;
        }

        /// <summary>
        /// Represents the method which is used to handle DHCP server events
        /// </summary>
        /// <param name="sender">The object which rised the event</param>
        /// <param name="args">The arguments of the event</param>
        public delegate void DHCPServerEventHandler(object sender, DHCPServerEventArgs args);

        /// <summary>
        /// This event is fired if an DHCP pool item is created
        /// </summary>
        public event DHCPServerEventHandler AddressCreated;
        /// <summary>
        /// This event is fired if an address is leased
        /// </summary>
        public event DHCPServerEventHandler AddressLeased;
        /// <summary>
        /// This event is fired if an DHCP pool item is removed
        /// </summary>
        public event DHCPServerEventHandler AddressRemoved;

        #region Props

        /// <summary>
        /// Returns all DHCP pools of this DHCP server
        /// </summary>
        public DHCPPool[] DHCPPools
        {
            get
            {
                DHCPPool[] dhcPools = new DHCPPool[dictInterfacePool.Values.Count];
                dictInterfacePool.Values.CopyTo(dhcPools, 0);
                return dhcPools;
            }
        }

        /// <summary>
        /// Gets or sets the DHCP out port
        /// </summary>
        public int DHCPOutPort
        {
            get { return iDHCPOutPort; }
            set
            {
                iDHCPOutPort = value;
                InvokePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the DHCP in Port
        /// </summary>
        public int DHCPInPort
        {
            get { return iDHCPInPort; }
            set
            {
                iDHCPInPort = value;
                InvokePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the DHCP lease duration in seconds
        /// </summary>
        public int LeaseDuration
        {
            get { return iLeaseDuration; }
            set
            {
                iLeaseDuration = value;
                InvokePropertyChanged();
            }
        }

        /// <summary>
        /// The gateway IP address. Setting this property to null will cause this DHCP server to announce the outgoing interface address as gateway.<br />
        /// The assigned gateway address is set for all DHCP pool entries which are newly created, not for existing ones. 
        /// </summary>
        public IPAddress GatewayAddress
        {
            get { return ipaGateway; }
            set
            {
                ipaGateway = value;
                InvokePropertyChanged();
            }
        }

        /// <summary>
        /// The DNS server IP address. Setting this property to null will cause this DHCP server to announce the outgoing interface address as DNS server.
        /// The assigned DNS address is set for all DHCP pool entries which are newly created, not for existing ones. 
        /// </summary>
        public IPAddress DNSAddress
        {
            get { return ipaDNSServer; }
            set
            {
                ipaDNSServer = value;
                InvokePropertyChanged();
            }
        }


        #endregion

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public DHCPServer()
        {
            ipAnalysis = new eExNetworkLibrary.IP.IPAddressAnalysis();
            lOpenServerTransactions = new List<int>();
            iDHCPInPort = 68;
            iDHCPOutPort = 67;
            dictInterfacePool = new Dictionary<IPInterface, DHCPPool>();
            iIPIDCounter = 1;
            iLeaseDuration = 86400;
            this.thNextHandler = this;
        }

        /// <summary>
        /// Gets the address pool associated with an given interface
        /// </summary>
        /// <param name="ipi">The interface for which the address pool should be returned</param>
        /// <returns>The address pool associated with the given interface</returns>
        public DHCPPool GetPoolForInterface(IPInterface ipi)
        {
            if (!dictInterfacePool.ContainsKey(ipi))
            {
                dictInterfacePool.Add(ipi, new DHCPPool());
            }

            return dictInterfacePool[ipi];
        }

        /// <summary>
        /// This method is used internally to increment the IP identification counter.
        /// It increments the IP ID counter and returns the value
        /// </summary>
        /// <returns>The current value of the IP ID counter</returns>
        protected int IncrementIPIDCounter()
        {
            iIPIDCounter++;
            if (iIPIDCounter > 65535)
            {
                iIPIDCounter = 1;
            }
            return iIPIDCounter;
        }

        /// <summary>
        /// Adds an interface to this DHCP server
        /// </summary>
        /// <param name="ipInterface">The IP interface to add</param>
        public override void AddInterface(IPInterface ipInterface)
        {
            if (!dictInterfacePool.ContainsKey(ipInterface))
            {
                dictInterfacePool.Add(ipInterface, new DHCPPool());
            }
            base.AddInterface(ipInterface);
        }

        /// <summary>
        /// Removes an interface from this DHCP server
        /// </summary>
        /// <param name="ipInterface">The IP interface to remove</param>
        public override void RemoveInterface(IPInterface ipInterface)
        {
            if (dictInterfacePool.ContainsKey(ipInterface))
            {
                dictInterfacePool.Remove(ipInterface);
            }
            base.RemoveInterface(ipInterface);
        }

        /// <summary>
        /// Tries to extract a DHCP frame from this frame and forwards it to the HandleDHCPFrame method
        /// </summary>
        /// <param name="fInputFrame">The frame to handle</param>
        protected override void HandleTraffic(Frame fInputFrame)
        {
            Ethernet.EthernetFrame ethFrame = GetEthernetFrame(fInputFrame);
            if (ethFrame == null)
            {
                return; //invalid.
            }
            if (bShuttingDown)
            {
                return; //Shutdown pending.
            }
            UDP.UDPFrame udpFrame = GetUDPFrame(fInputFrame);
            TrafficDescriptionFrame tdf = (TrafficDescriptionFrame)GetFrameByType(fInputFrame, FrameTypes.TrafficDescriptionFrame);    
            IP.IPFrame ipv4Frame = GetIPv4Frame(fInputFrame);

            if (udpFrame != null && ((udpFrame.DestinationPort == iDHCPInPort && udpFrame.SourcePort == iDHCPOutPort) || (udpFrame.DestinationPort == iDHCPOutPort && udpFrame.SourcePort == iDHCPInPort)))
            {
                DHCP.DHCPFrame dhcFrame = new DHCPFrame(udpFrame.EncapsulatedFrame.FrameBytes); // parse DHCP frame

                HandleDHCPFrame(dhcFrame, ethFrame, udpFrame, ipv4Frame, tdf);

            }
        }

        /// <summary>
        /// Handles a DHCP frame and sends responses or leases addresses according to its contents
        /// </summary>
        /// <param name="dhcFrame">The DHCP frame to handle</param>
        /// <param name="ethFrame">The according ethernet frame</param>
        /// <param name="udpFrame">The according UDP frame</param>
        /// <param name="ipv4Frame">The according IPv4 frame</param>
        /// <param name="tdf">The according traffic description frame</param>
        protected virtual void HandleDHCPFrame(DHCPFrame dhcFrame, eExNetworkLibrary.Ethernet.EthernetFrame ethFrame, eExNetworkLibrary.UDP.UDPFrame udpFrame, eExNetworkLibrary.IP.IPFrame ipv4Frame, TrafficDescriptionFrame tdf)
        {
            bool bIsRequest = false;
            bool bIsDiscover = false;

            foreach (DHCPTLVItem tlvItem in dhcFrame.GetDHCPTLVItems())
            {
                if (tlvItem.DHCPOptionType == DHCPOptions.DHCPMessageType)
                {
                    if (dhcFrame.MessageType == DHCPType.BootRequest && (DHCPMessageType)tlvItem.Data[0] == DHCPMessageType.Discover)
                    {
                        bIsDiscover = true;
                        break;
                    }
                    if (dhcFrame.MessageType == DHCPType.BootRequest && (DHCPMessageType)tlvItem.Data[0] == DHCPMessageType.Request && lOpenServerTransactions.Contains(dhcFrame.TransactionID))
                    {
                        bIsRequest = true;
                        break;
                    }
                }
            }

            if (bIsRequest)
            {
                #region Server Process request

                MACAddress mClientID = dhcFrame.ClientMac;

                if (tdf != null && tdf.SourceInterface != null)
                {
                    if (dictInterfacePool.ContainsKey(tdf.SourceInterface))
                    {
                        IPAddress ipaAddressRequestet = IPAddress.Any;
                        string strHostname = "";

                        foreach (DHCPTLVItem tlvItemSearch in dhcFrame.GetDHCPTLVItems())
                        {
                            if (tlvItemSearch.DHCPOptionType == DHCPOptions.AddressRequest)
                            {
                                ipaAddressRequestet = new IPAddress(tlvItemSearch.Data);
                            }
                            if (tlvItemSearch.DHCPOptionType == DHCPOptions.Hostname)
                            {
                                strHostname = ASCIIEncoding.ASCII.GetString(tlvItemSearch.Data);
                            }
                        }

                        DHCPPool dhPool = dictInterfacePool[tdf.SourceInterface];
                        DHCPPoolItem dhItem = dhPool.GetItemForAddress(ipaAddressRequestet);

                        if (dhItem != null)
                        {
                            IPAddress ipaServer = tdf.SourceInterface.IpAddresses[0];
                            IPAddress offeredAddress = dhItem.Address;

                            DHCPFrame newDHCPFrame = new DHCPFrame();
                            newDHCPFrame.ClientAddress = IPAddress.Any;
                            newDHCPFrame.ClientMac = mClientID;
                            newDHCPFrame.Hardwarelen = 6;
                            newDHCPFrame.HardwareType = eExNetworkLibrary.HardwareAddressType.Ethernet;
                            newDHCPFrame.Hops = 0;
                            newDHCPFrame.MessageType = DHCPType.BootReply;
                            newDHCPFrame.OfferedAddress = offeredAddress;
                            newDHCPFrame.RelayAddress = IPAddress.Any;
                            newDHCPFrame.RequestedFile = "";
                            newDHCPFrame.RequestedServerName = "";
                            newDHCPFrame.Secs = dhcFrame.Secs + 1;
                            newDHCPFrame.ServerAddress = ipaServer;
                            newDHCPFrame.ValidIPFlag = true;
                            newDHCPFrame.TransactionID = dhcFrame.TransactionID;

                            DHCPTLVItem tlvItem = new DHCPTLVItem();
                            tlvItem.DHCPOptionType = DHCPOptions.DHCPMessageType;
                            tlvItem.Data = new byte[] { (byte)DHCPMessageType.ACK };

                            newDHCPFrame.AddDHCPTLVItem(tlvItem);

                            tlvItem = new DHCPTLVItem();
                            tlvItem.DHCPOptionType = DHCPOptions.ClientID;
                            byte[] bIDData = new byte[7];
                            bIDData[0] = (byte)HardwareAddressType.Ethernet;
                            mClientID.AddressBytes.CopyTo(bIDData, 1);
                            tlvItem.Data = bIDData;

                            newDHCPFrame.AddDHCPTLVItem(tlvItem);

                            tlvItem = new DHCPTLVItem();
                            tlvItem.DHCPOptionType = DHCPOptions.SubnetMask;
                            tlvItem.Data = dhItem.Netmask.MaskBytes;

                            newDHCPFrame.AddDHCPTLVItem(tlvItem);

                            tlvItem = new DHCPTLVItem();
                            tlvItem.DHCPOptionType = DHCPOptions.Router;
                            tlvItem.Data = dhItem.Gateway.GetAddressBytes();

                            newDHCPFrame.AddDHCPTLVItem(tlvItem);

                            tlvItem = new DHCPTLVItem();
                            tlvItem.DHCPOptionType = DHCPOptions.DomainNameServer;
                            tlvItem.Data = dhItem.DNSServer.GetAddressBytes();

                            newDHCPFrame.AddDHCPTLVItem(tlvItem);

                            tlvItem = new DHCPTLVItem();
                            tlvItem.DHCPOptionType = DHCPOptions.LeaseTime;
                            tlvItem.Data = BitConverter.GetBytes(iLeaseDuration);

                            newDHCPFrame.AddDHCPTLVItem(tlvItem);

                            tlvItem = new DHCPTLVItem();
                            tlvItem.DHCPOptionType = DHCPOptions.DHCPServerID;
                            tlvItem.Data = ipaServer.GetAddressBytes();

                            newDHCPFrame.AddDHCPTLVItem(tlvItem);

                            UDP.UDPFrame newUDPFrame = new eExNetworkLibrary.UDP.UDPFrame();
                            newUDPFrame.DestinationPort = iDHCPInPort;
                            newUDPFrame.SourcePort = iDHCPOutPort;
                            newUDPFrame.EncapsulatedFrame = newDHCPFrame;

                            IP.IPv4Frame newIPv4Frame = new eExNetworkLibrary.IP.IPv4Frame();
                            newIPv4Frame.Version = 4;
                            newIPv4Frame.DestinationAddress = IPAddress.Broadcast;
                            newIPv4Frame.SourceAddress = ipaServer;
                            newIPv4Frame.Protocol = eExNetworkLibrary.IP.IPProtocol.UDP;
                            newIPv4Frame.EncapsulatedFrame = newUDPFrame;
                            newIPv4Frame.Identification = (uint)IncrementIPIDCounter();
                            newIPv4Frame.TimeToLive = 128;

                            TrafficDescriptionFrame tdFrame = new TrafficDescriptionFrame(null, DateTime.Now);
                            tdFrame.EncapsulatedFrame = newIPv4Frame;

                            tdf.SourceInterface.Send(tdFrame, IPAddress.Broadcast);
                            dhItem.LeasedTo = mClientID;
                            dhItem.LeasedToHostname = strHostname;
                            dhItem.LeaseDuration = new TimeSpan(0, 0, 0, iLeaseDuration, 0);
                            lOpenServerTransactions.Remove(newDHCPFrame.TransactionID);
                            InvokeAddressLeased(new DHCPServerEventArgs(dhPool, dhItem, tdf.SourceInterface));
                        }
                    }
                }

                #endregion
            }
            else if (bIsDiscover)
            {
                #region Server Process discover

                MACAddress mClientID = dhcFrame.ClientMac;

                if (tdf != null && tdf.SourceInterface != null)
                {
                    if (dictInterfacePool.ContainsKey(tdf.SourceInterface))
                    {
                        DHCPPool dhPool = dictInterfacePool[tdf.SourceInterface];
                        DHCPPoolItem dhItem = dhPool.GetNextFreeAddress();

                        if (dhItem != null)
                        {
                            IPAddress ipaServer = tdf.SourceInterface.IpAddresses[0];
                            IPAddress offeredAddress = dhItem.Address;

                            DHCPFrame newDHCPFrame = new DHCPFrame();
                            newDHCPFrame.ClientAddress = IPAddress.Any;
                            newDHCPFrame.ClientMac = mClientID;
                            newDHCPFrame.Hardwarelen = 6;
                            newDHCPFrame.HardwareType = eExNetworkLibrary.HardwareAddressType.Ethernet;
                            newDHCPFrame.Hops = 0;
                            newDHCPFrame.MessageType = DHCPType.BootReply;
                            newDHCPFrame.OfferedAddress = offeredAddress;
                            newDHCPFrame.RelayAddress = IPAddress.Any;
                            newDHCPFrame.RequestedFile = "";
                            newDHCPFrame.RequestedServerName = "";
                            newDHCPFrame.Secs = dhcFrame.Secs + 1;
                            newDHCPFrame.ServerAddress = ipaServer;
                            newDHCPFrame.ValidIPFlag = true;
                            newDHCPFrame.TransactionID = dhcFrame.TransactionID;

                            DHCPTLVItem tlvItem = new DHCPTLVItem();
                            tlvItem.DHCPOptionType = DHCPOptions.DHCPMessageType;
                            tlvItem.Data = new byte[] { (byte)DHCPMessageType.Offer };

                            newDHCPFrame.AddDHCPTLVItem(tlvItem);

                            tlvItem = new DHCPTLVItem();
                            tlvItem.DHCPOptionType = DHCPOptions.ClientID;
                            byte[] bIDData = new byte[7];
                            bIDData[0] = (byte)HardwareAddressType.Ethernet;
                            mClientID.AddressBytes.CopyTo(bIDData, 1);
                            tlvItem.Data = bIDData;

                            newDHCPFrame.AddDHCPTLVItem(tlvItem);

                            tlvItem = new DHCPTLVItem();
                            tlvItem.DHCPOptionType = DHCPOptions.SubnetMask;
                            tlvItem.Data = dhItem.Netmask.MaskBytes;

                            newDHCPFrame.AddDHCPTLVItem(tlvItem);

                            tlvItem = new DHCPTLVItem();
                            tlvItem.DHCPOptionType = DHCPOptions.Router;
                            tlvItem.Data = dhItem.Gateway.GetAddressBytes();

                            newDHCPFrame.AddDHCPTLVItem(tlvItem);

                            tlvItem = new DHCPTLVItem();
                            tlvItem.DHCPOptionType = DHCPOptions.DomainNameServer;
                            tlvItem.Data = dhItem.DNSServer.GetAddressBytes();

                            newDHCPFrame.AddDHCPTLVItem(tlvItem);

                            tlvItem = new DHCPTLVItem();
                            tlvItem.DHCPOptionType = DHCPOptions.LeaseTime;
                            tlvItem.Data = BitConverter.GetBytes(86400);

                            newDHCPFrame.AddDHCPTLVItem(tlvItem);

                            tlvItem = new DHCPTLVItem();
                            tlvItem.DHCPOptionType = DHCPOptions.DHCPServerID;
                            tlvItem.Data = ipaServer.GetAddressBytes();

                            newDHCPFrame.AddDHCPTLVItem(tlvItem);

                            UDP.UDPFrame newUDPFrame = new eExNetworkLibrary.UDP.UDPFrame();
                            newUDPFrame.DestinationPort = iDHCPInPort;
                            newUDPFrame.SourcePort = iDHCPOutPort;
                            newUDPFrame.EncapsulatedFrame = newDHCPFrame;

                            IP.IPv4Frame newIPv4Frame = new eExNetworkLibrary.IP.IPv4Frame();
                            newIPv4Frame.Version = 4;
                            newIPv4Frame.DestinationAddress = IPAddress.Broadcast;
                            newIPv4Frame.SourceAddress = ipaServer;
                            newIPv4Frame.Protocol = eExNetworkLibrary.IP.IPProtocol.UDP;
                            newIPv4Frame.EncapsulatedFrame = newUDPFrame;
                            newIPv4Frame.Identification = (uint)IncrementIPIDCounter();
                            newIPv4Frame.TimeToLive = 128;

                            TrafficDescriptionFrame tdFrame = new TrafficDescriptionFrame(null, DateTime.Now);
                            tdFrame.EncapsulatedFrame = newIPv4Frame;

                            tdf.SourceInterface.Send(tdFrame, IPAddress.Broadcast);
                            lOpenServerTransactions.Add(newDHCPFrame.TransactionID);
                        }
                    }
                }

                #endregion
            }
        }

        /// <summary>
        /// Invokes the AddressLeased events
        /// </summary>
        /// <param name="args">The event args</param>
        protected void InvokeAddressLeased(DHCPServerEventArgs args)
        {
            InvokeExternalAsync(AddressLeased, args);
        }

        /// <summary>
        /// Invokes the AddressCreated events
        /// </summary>
        /// <param name="args">The event args</param>
        protected void InvokeAddressCreated(DHCPServerEventArgs args)
        {
            InvokeExternalAsync(AddressCreated, args);
        }

        /// <summary>
        /// Invokes the AddressRemoved events
        /// </summary>
        /// <param name="args">The event args</param>
        protected void InvokeAddressRemoved(DHCPServerEventArgs args)
        {
            InvokeExternalAsync(AddressRemoved, args);
        }

        /// <summary>
        /// Setting output handlers is not supported by DHCP servers
        /// </summary>
        public override TrafficHandler  OutputHandler
        {
            get { return this; }
            set { throw new InvalidOperationException("Traffic analyzers must not have any output"); }
        }
    }
    
    /// <summary>
    /// This class contains event data for DHCP server event args
    /// </summary>
    public class DHCPServerEventArgs : EventArgs
    {
        private DHCPPool pPool;
        private DHCPPoolItem dhcpItem;
        private IPInterface ipiInterface;

        /// <summary>
        /// The pool item associated with the event
        /// </summary>
        public DHCPPoolItem PoolItem
        {
            get { return dhcpItem;  }
        }

        /// <summary>
        /// The DHCP pool associated with the event
        /// </summary>
        public DHCPPool Pool
        {
            get { return pPool; }
        }

        /// <summary>
        /// The IP interface associated with the event
        /// </summary>
        public IPInterface Interface
        {
            get { return ipiInterface; }
        }

        /// <summary>
        /// Creates a new instance of this class with the given parameters
        /// </summary>
        /// <param name="pPool">The pool item associated with the event</param>
        /// <param name="dhcpItem">The DHCP pool associated with the event</param>
        /// <param name="ipiInterface">The IP interface associated with the event</param>
        public DHCPServerEventArgs(DHCPPool pPool, DHCPPoolItem dhcpItem, IPInterface ipiInterface)
        {
            this.pPool = pPool;
            this.dhcpItem = dhcpItem;
            this.ipiInterface = ipiInterface;
        }
    }
}
