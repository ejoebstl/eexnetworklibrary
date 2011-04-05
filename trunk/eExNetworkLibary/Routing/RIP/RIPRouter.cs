using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Timers;

namespace eExNetworkLibrary.Routing.RIP
{
    /// <summary>
    /// This class represents a RIP version 1 or 2 routing process.
    /// This Router will announce all direct connected interfaces by default. 
    /// </summary>
    public class RIPRouter : RoutingProcess
    {
        private List<IPInterface> ipiPassiveInterfaces;
        private int iRIPPort;
        private int iVersion;
        private bool bRedistributeStatic;
        private IPAddress ipaRIPv2Address;
        private IP.IPAddressAnalysis ipv4Analysis;
        private bool bShutdownPending;
        private Timer tPeriodicUpdate;
        private int iHoldDownTimer;
        private Timer tHoldDownTicker;
        private List<RIPHoldownItem> lHolddownItems;

        /// <summary>
        /// Gets or sets the RIP holddown timer in secods
        /// </summary>
        public int HoldDownTimer
        {
            get { return iHoldDownTimer; }
            set
            {
                iHoldDownTimer = value;
                InvokePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the RIP update period in millisecods
        /// </summary>
        public int UpdatePeriod
        {
            get { return (int)tPeriodicUpdate.Interval; }
            set
            {
                tPeriodicUpdate.Interval = (double)value;
                InvokePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the Port used by RIP
        /// </summary>
        public int RIPPort
        {
            get { return iRIPPort; }
            set
            {
                iRIPPort = value;
                InvokePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the RIP version of this instance (1 or 2)
        /// </summary>
        public int Version
        {
            get { return iVersion; }
            set
            {
                if (value != 1 && value != 2)
                {
                    throw new ArgumentException("Only RIP version 1 or 2 is supported");
                }
                iVersion = value;
                InvokePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the multicast address used by RIPv2
        /// </summary>
        public IPAddress RIPv2Address
        {
            get { return ipaRIPv2Address; }
            set
            {
                ipaRIPv2Address = value;
                InvokePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a bool indicating whether static routes should also be forwarded.
        /// </summary>
        public bool RedistributeStatic
        {
            get { return bRedistributeStatic; }
            set
            {
                bRedistributeStatic = value;
                InvokePropertyChanged();
            }
        }

        /// <summary>
        /// Adds an interface to this RIP routers passive interface list.
        /// </summary>
        /// <param name="ipi">The IPInterface to mark as passive</param>
        public void AddPassiveInterface(IPInterface ipi)
        {
            lock (ipiPassiveInterfaces)
            {
                ipiPassiveInterfaces.Add(ipi);
            }
        }

        /// <summary>
        /// Removes an interface from this RIP routers passive interface list. 
        /// </summary>
        /// <param name="ipi">The IPInterface to remove</param>
        public void RemovePassiveInterface(IPInterface ipi)
        {
            lock (ipiPassiveInterfaces)
            {
                ipiPassiveInterfaces.Remove(ipi);
            }
        }

        /// <summary>
        /// Check whether an interface is contained in this RIP routers passive interface list. 
        /// </summary>
        /// <param name="ipi">Thie IPInterface to search for</param>
        /// <returns>A boolean indicating whether an interface is contained in this RIP routers passive interface list. </returns>
        public bool ContainsPassiveInterface(IPInterface ipi)
        {
            lock (ipiPassiveInterfaces)
            {
                return ipiPassiveInterfaces.Contains(ipi);
            }
        }

        /// <summary>
        /// Returns all passive interfaces of this instance.
        /// </summary>
        /// <returns>All passive interfaces of this instance</returns>
        public IPInterface[] GetPassiveInterfaces()
        {
            lock (ipiPassiveInterfaces)
            {
                return ipiPassiveInterfaces.ToArray();
            }
        }

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        public RIPRouter()
        {
            this.thNextHandler = this;

            ipaRIPv2Address = RIPFrame.RIPv2MulticastAddress;
            iRIPPort = 520;
            iVersion = 2;

            iHoldDownTimer = 30;

            tHoldDownTicker = new Timer();
            tHoldDownTicker.AutoReset = true;
            tHoldDownTicker.Interval = 1000;
            tHoldDownTicker.Elapsed += new ElapsedEventHandler(tHoldDownTicker_Elapsed);

            lHolddownItems = new List<RIPHoldownItem>();

            ipv4Analysis = new eExNetworkLibrary.IP.IPAddressAnalysis();

            ipiPassiveInterfaces = new List<IPInterface>();


            bShutdownPending = false;

            tPeriodicUpdate = new Timer();
            tPeriodicUpdate.Elapsed += new ElapsedEventHandler(tPeriodicUpdate_Elapsed);
            tPeriodicUpdate.AutoReset = true;
            tPeriodicUpdate.Interval = 30000;
        }

        void tHoldDownTicker_Elapsed(object sender, ElapsedEventArgs e)
        {
            RIPHoldownItem[] arRhi = lHolddownItems.ToArray();
            foreach (RIPHoldownItem rhi in arRhi)
            {
                rhi.Timer--;
                if (rhi.Timer >= 0)
                {
                    lHolddownItems.Remove(rhi);
                }
            }
        }

        private void Holddown(IPAddress ipaNetwork)
        {
            lHolddownItems.Add(new RIPHoldownItem(iHoldDownTimer, ipaNetwork));
        }

        private bool IsHoldDown(IPAddress ipaNetwork)
        {
            bool bFound = false;

            foreach (RIPHoldownItem rhi in lHolddownItems)
            {
                if (rhi.Network.Equals(ipaNetwork))
                {
                    bFound = true;
                    break;
                }
            }

            return bFound;
        }

        void tPeriodicUpdate_Elapsed(object sender, ElapsedEventArgs e)
        {
            DistributeUpdate();
        }

        /// <summary>
        /// Forces this instance to distribute updates immideately.
        /// </summary>
        public void DistributeUpdate()
        {
            DistributeUpdate(null);
        }

        /// <summary>
        /// Forces this instance to distribute traffic immidiately, with exluding the specified interface from forwarding operations.
        /// </summary>
        /// <param name="ipiExcludeInterface">The interface to exclude from forwarding operations or null, if no interface should be excluded.</param>
        public void DistributeUpdate(IPInterface ipiExcludeInterface)
        {
            IRouter rtRouter = this.RouterToManage;

            if (rtRouter != null)
            {
                RIPFrame rf = new RIPFrame();
                rf.Version = (uint)this.iVersion;
                rf.Command = RipCommand.RIPResponse;

                RoutingEntry[] arre = rtRouter.RoutingTable.GetRoutes();

                foreach (RoutingEntry re in arre)
                {
                    if (re.Owner == RoutingEntryOwner.RIP || re.Owner == RoutingEntryOwner.Interface)
                    {
                        if (this.iVersion == 1)
                        {
                            rf.AddUpdate(new RIPUpdate(RIPEntryAddressFamily.IPv4, new byte[2], new Subnetmask(), IPAddress.Any, re.Destination, (uint)((re.Metric + 1) > 16 ? 16 : (re.Metric + 1))));
                        }
                        else if (this.iVersion == 2)
                        {
                            rf.AddUpdate(new RIPUpdate(RIPEntryAddressFamily.IPv4, new byte[2], re.Subnetmask, IPAddress.Any, re.Destination, (uint)((re.Metric + 1) > 16 ? 16 : (re.Metric + 1))));
                        }
                    }
                    else if ((re.Owner == RoutingEntryOwner.UserStatic || re.Owner == RoutingEntryOwner.System) && this.bRedistributeStatic)
                    {
                        if (this.iVersion == 1)
                        {
                            rf.AddUpdate(new RIPUpdate(RIPEntryAddressFamily.IPv4, new byte[2], new Subnetmask(), IPAddress.Any, re.Destination, 1));
                        }
                        else if (this.iVersion == 2)
                        {
                            rf.AddUpdate(new RIPUpdate(RIPEntryAddressFamily.IPv4, new byte[2], re.Subnetmask, IPAddress.Any, re.Destination, 1));
                        }
                    }
                }

                UDP.UDPFrame uf = new UDP.UDPFrame();
                uf.DestinationPort = iRIPPort;
                uf.SourcePort = iRIPPort;
                uf.EncapsulatedFrame = rf;

                foreach (IPInterface ipi in lInterfaces)
                {
                    if (ipi != ipiExcludeInterface && ipi.IpAddresses.Length > 0 && !ipiPassiveInterfaces.Contains(ipi))
                    {
                        IP.IPv4Frame ipf = new IP.IPv4Frame();
                        ipf.SourceAddress = ipi.IpAddresses[0];
                        if (iVersion == 2)
                        {
                            ipf.DestinationAddress = ipaRIPv2Address;
                        }
                        else if (iVersion == 1)
                        {
                            ipf.DestinationAddress = IPAddress.Broadcast;
                        }
                        ipf.Protocol = eExNetworkLibrary.IP.IPProtocol.UDP;
                        ipf.Version = 4;
                        ipf.EncapsulatedFrame = uf;

                        TrafficDescriptionFrame tdf = new TrafficDescriptionFrame(null, DateTime.Now);
                        tdf.EncapsulatedFrame = ipf;

                        ipi.Send(tdf, IPAddress.Broadcast);
                    }
                }
            }
        }

        /// <summary>
        /// Checks the incoming traffic for RIP updates.
        /// </summary>
        /// <param name="fInputFrame">The frame to handle.</param>
        protected override void HandleTraffic(Frame fInputFrame)
        {
            IRouter rtRouterToManage = this.RouterToManage;

            TrafficDescriptionFrame tdf = (TrafficDescriptionFrame)GetFrameByType(fInputFrame, FrameType.TrafficDescriptionFrame);
            if (!bShutdownPending)
            {
                UDP.UDPFrame udpFrame = GetUDPFrame(fInputFrame);
                IP.IPFrame ipFrame = GetIPv4Frame(fInputFrame);

                if (udpFrame != null && ipFrame != null && rtRouterToManage != null && udpFrame.EncapsulatedFrame != null)
                {
                    if (iVersion == 1)
                    {
                        if (HandleRIPV1(udpFrame, ipFrame))
                        {
                            DistributeUpdate(tdf.SourceInterface);
                        }
                    }
                    else if (iVersion == 2)
                    {
                        if (HandleRIPV2(udpFrame, ipFrame))
                        {
                            DistributeUpdate(tdf.SourceInterface);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles RIPv1 updates
        /// </summary>
        /// <param name="udpFrame"></param>
        /// <param name="ipFrame"></param>
        /// <returns>Bool indicating if something changed</returns>
        private bool HandleRIPV1(UDP.UDPFrame udpFrame, IP.IPFrame ipFrame)
        {
            RIPFrame fRIPFrame;
            bool bChanged = false;

            if (ipFrame.SourceAddress.Equals(IPAddress.Broadcast)) // Check Addr
            {
                if (udpFrame.DestinationPort == iRIPPort) // Check Port
                {
                    fRIPFrame = new RIPFrame(udpFrame.EncapsulatedFrame.FrameBytes);
                    if (fRIPFrame.Version == 1)
                    {
                        foreach (RIPUpdate ru in fRIPFrame.GetUpdates())
                        {
                            if (ru.AddressFamilyIdentifier == RIPEntryAddressFamily.IPv4)
                            {
                                bChanged |= UpdateEntry(ipFrame.SourceAddress, ru.Address, ipv4Analysis.GetClassfullSubnetMask(ru.Address), (int)ru.Metric);
                            }
                        }
                    }
                }
            }

            return bChanged;
            
        }

        /// <summary>
        /// Updates an RIP Entry
        /// </summary>
        /// <param name="ipaNextHop"></param>
        /// <param name="ipaDestinatoin"></param>
        /// <param name="smMask"></param>
        /// <param name="iMetric"></param>
        /// <returns>Bool indicating if something changed</returns>
        private bool UpdateEntry(IPAddress ipaNextHop, IPAddress ipaDestinatoin, Subnetmask smMask, int iMetric)
        {
            bool bFound = false;
            bool bChanged = true;

            foreach (RoutingEntry re in RoutingEntries)
            {
                if (re.Destination.Equals(ipaDestinatoin) && re.Subnetmask.Equals(smMask))
                {
                    if (iMetric != re.Metric || !re.NextHop.Equals(ipaNextHop))
                    {
                        bChanged = true;
                        if (iMetric < 16)
                        {
                            re.Metric = iMetric;
                        }
                        else
                        {
                            re.Metric = 65535;
                            Holddown(ipaDestinatoin);
                        }
                        re.NextHop = ipaNextHop; 
                        InvokeEntryUpdated(re);
                    }
                    bFound = true;
                }
            }

            if (!bFound)
            {
                bChanged = true;
                RoutingEntry re = new RoutingEntry(ipaDestinatoin, ipaNextHop, iMetric, smMask, RoutingEntryOwner.RIP);
                AddRoutingEntry(re);
            }

            return bChanged;
        }

        private void RemoveEntry(IPAddress ipaNextHop, IPAddress ipaDestinatoin, Subnetmask smMask, int iMetric)
        {
            RoutingEntry reFound = null;

            foreach (RoutingEntry re in RoutingEntries)
            {
                if (re.NextHop.Equals(ipaNextHop) && re.Destination.Equals(ipaDestinatoin) && re.Subnetmask.Equals(smMask))
                {
                    reFound = re;
                }
            }
            if (reFound != null)
            {
                RemoveEntry(reFound);
            }
        }

        /// <summary>
        /// Handles RIPv2 Frames
        /// </summary>
        /// <param name="udpFrame"></param>
        /// <param name="ipFrame"></param>
        /// <returns>Bool indicating if something changed</returns>
        private bool HandleRIPV2(UDP.UDPFrame udpFrame, IP.IPFrame ipFrame)
        {
            RIPFrame fRIPFrame;
            bool bChanged = false;

            if (ipFrame.SourceAddress.Equals(IPAddress.Broadcast))
            {
                HandleRIPV1(udpFrame, ipFrame); // RIPv1? Fallback!
            }
            else if(ipFrame.DestinationAddress.Equals(ipaRIPv2Address)) // Check Addr
            {
                if (udpFrame.DestinationPort == iRIPPort) //Check Port
                {
                    fRIPFrame = new RIPFrame(udpFrame.EncapsulatedFrame.FrameBytes);

                    if (fRIPFrame.Version == 2)
                    {
                        foreach (RIPUpdate ru in fRIPFrame.GetUpdates())
                        {
                            if (ru.AddressFamilyIdentifier == RIPEntryAddressFamily.IPv4)
                            {
                                if (!IsHoldDown(ru.Address))
                                {
                                    bChanged |= UpdateEntry(ipFrame.SourceAddress, ru.Address, ru.Ripv2SubnetMask, (int)ru.Metric);
                                }
                            }
                        }
                        
                    }
                    else
                    {
                        bChanged |= HandleRIPV1(udpFrame, ipFrame);
                    }
                }
            }

            return bChanged;
        }

        /// <summary>
        /// Clears the associated router's Routingtable and stops all timers and working threads.
        /// Also after calling this method, this instance will not be receiving or sending RIP updates any more.
        /// </summary>
        public override void Cleanup()
        {
            bShutdownPending = true;
            tPeriodicUpdate.Stop();
            tHoldDownTicker.Stop();
            IRouter rtRouterToManage = RouterToManage;
            if (rtRouterToManage != null)
            {
                foreach (RoutingEntry re in RoutingEntries)
                {
                    rtRouterToManage.RoutingTable.RemoveRoute(re);
                }
            }
            base.Cleanup();
        }

        /// <summary>
        /// Starts this RIP router.
        /// </summary>
        public override void Start()
        {
            tHoldDownTicker.Start();
            tPeriodicUpdate.Start();
            base.Start();
        }

        /// <summary>
        /// Stops this RIP router.
        /// </summary>
        public override void Stop()
        {
            base.Stop();
        }

        private class RIPHoldownItem
        {
            private IPAddress ipaNetwork;
            private int iTimer;

            public int Timer
            {
                get { return iTimer; }
                set { iTimer = value; }
            }

            public IPAddress Network
            {
                get { return ipaNetwork; }
                set { ipaNetwork = value; }
            }

            public RIPHoldownItem(int iTimer, IPAddress ipaNetwork)
            {
                this.iTimer = iTimer;
                this.ipaNetwork = ipaNetwork;
            }

            public RIPHoldownItem()
            {
                iTimer = 0;
                ipaNetwork = IPAddress.Any;
            }
        }
    }
}
