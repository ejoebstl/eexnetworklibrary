using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.Net;
using eExNetworkLibrary.Utilities;
using eExNetworkLibrary.IP;
using eExNetworkLibrary.Ethernet;
using System.Net.NetworkInformation;

namespace eExNetworkLibrary.Routing.OSPF
{
    /// <summary>
    /// A very, very, very sloppy OSPF implementation. Man, what do 
    /// you estaminate for a planned time of 2 weeks for this task?
    /// If you want do do this task better than I did, refer 
    /// to http://www.freesoft.org/CIE/RFC/1583/ and code 
    /// like the wind. 
    /// </summary>
    [Obsolete("This class is marked depreceated in cause of a sloppy implementation and only limited compatibility..", true)]
    class OSPFRouter : DirectInterfaceIOHandler, IOSPFRouter
    {
        private uint iRouterID;
        //private OSPFArea areaBackbone;
        private OSPFAreaVertex oRoot;
        private Dictionary<uint, OSPFArea> dictIDArea;
        private int iHelloInterval;
        private int iRouterDeadInterval;
        private Timer tTimer;
        private Timer tHello;
        private List<OSPFNeighbour> lNeighbours;
        private List<DirectAttachedNetwork> lNetworks;
        private IPAddressAnalysis ipv4Analysis;
        private int iIPIDCounter;
        private byte bPriority;
        private uint iLSASeqCounter;
        private int iMTU;
        private bool bIsASBorderRouter;
        private bool bIsAreaBorderRouter;
        private bool bIsVirtualEndPoint;
        private Dictionary<OSPFArea, List<LSAHeader>> dictlLocalLSAs;
        private OSPFOptionsField ospfOptions;

        private IPAddress ipaAllOSPFRouters;
        private IPAddress ipaAllOSPFDesignatedRouters;

        public delegate void NetworkEventHanlder(object sender, NetworkEventArgs args);
        public event NetworkEventHanlder NetworkAdded;
        public event NetworkEventHanlder NetworkRemoved;

        public IPAddress AllOSPFDesignatedRouters
        {
            get { return ipaAllOSPFDesignatedRouters; }
        }

        public OSPFOptionsField Options
        {
            get { return ospfOptions; }
            set { ospfOptions = value; }
        }

        public IPAddress AllOSPFRouters
        {
            get { return ipaAllOSPFRouters; }
        }

        public int Priority
        {
            get { return bPriority; }
            set { bPriority = (byte)value; }
        }

        public int HelloInterval
        {
            get { return iHelloInterval; }
            set
            {
                iHelloInterval = value;
                tHello.Interval = iHelloInterval * 1000;
            }
        }

        public int RouterDeadInterval
        {
            get { return iRouterDeadInterval; }
            set { iRouterDeadInterval = value; }
        }

        public NetworkEntry[] Networks
        {
            get 
            {
                List<NetworkEntry> ne = new List<NetworkEntry>();

                DirectAttachedNetwork[] ardan = lNetworks.ToArray();

                foreach (DirectAttachedNetwork dan in ardan)
                {
                    if (dan.AssociatedArea != null)
                    {
                        ne.Add(new NetworkEntry(dan.InterfaceAddress, ToWildcard(dan.SubnetMask), dan.AssociatedArea.AreaID));
                    }
                }

                return ne.ToArray();
            }
        }

        public OSPFRouter()
        {
            iHelloInterval = 10;
            iRouterDeadInterval = 40;
            
            bIsASBorderRouter = false;
            bIsAreaBorderRouter = false;
            bIsVirtualEndPoint = false;
            dictlLocalLSAs = new Dictionary<OSPFArea, List<LSAHeader>>();
            ipaAllOSPFDesignatedRouters = new IPAddress(new byte[] { 224, 0, 0, 6 });
            ipaAllOSPFRouters = new IPAddress(new byte[] { 224, 0, 0, 5 });
            ipv4Analysis = new IPAddressAnalysis();
            lNeighbours = new List<OSPFNeighbour>();
            dictIDArea = new Dictionary<uint, OSPFArea>();
            lNetworks = new List<DirectAttachedNetwork>();
            tTimer = new Timer(1000);
            tTimer.AutoReset = true;
            iMTU = 1500;
            tTimer.Elapsed += new ElapsedEventHandler(tTimer_Elapsed);
            tHello = new Timer(iHelloInterval * 1000);
            tHello.AutoReset = true;
            tHello.Elapsed += new ElapsedEventHandler(tHello_Elapsed);
            this.thNextHandler = this;
            this.iRouterID = 0xDEADBEEF;
            this.bPriority = 1;
            this.ospfOptions = new OSPFOptionsField();
            this.ospfOptions.ContainsLLSData = false;
            this.ospfOptions.DemandCircuitsSupported = false;
            this.ospfOptions.DNBit = false;
            this.ospfOptions.EBit = true;
            this.ospfOptions.MCBit = false;
            this.ospfOptions.OBit = false;
            this.ospfOptions.SupportsNSSA = false;
            this.ospfOptions.TBit = false;
            oRoot = new OSPFAreaVertex(0);
        }

        private void RemoveArea(uint iAreaID)
        {
            lock (dictIDArea)
            {
                if (!dictIDArea.ContainsKey(iAreaID))
                {
                    dictIDArea.Remove(iAreaID);
                }
            }
        }

        public void AddNetwork(NetworkEntry ne)
        {
            AddNetwork(ne.AreaID, ne.Network, ne.Wildcard);
        }

        public void AddNetwork(uint iAreaID, IPAddress ipa, Subnetmask smWildcard)
        {
            DirectAttachedNetwork[] arDan = lNetworks.ToArray();
            OSPFArea ospfArea = GetAreaForID(iAreaID);
            List<LSAHeader> lLSA = new List<LSAHeader>();

            foreach (DirectAttachedNetwork dan in arDan)
            {
                if (MatchWildcard(ipa, dan.InterfaceAddress, smWildcard)) // DO WILDCARD
                {
                    dan.AssociatedArea = ospfArea;
                    InvokeExternalAsync(NetworkAdded, new NetworkEventArgs(new NetworkEntry(dan.InterfaceAddress, smWildcard, iAreaID)));
                }
            }

            UpdateAreaLSA(iAreaID);

            ospfArea.Database.AddLSARange(lLSA.ToArray());
        }

        private LSAHeader CreateRouterLSA(uint iAreaID)
        {
            LSAHeader h = CreateLSAHeader(LSType.Router);
            h.LinkStateID = iRouterID;

            DirectAttachedNetwork[] arDan = lNetworks.ToArray();

            RouterLSA rLSA = new RouterLSA();

            rLSA.IsAreaBorderRouter = bIsAreaBorderRouter;
            rLSA.IsASBoundaryRouter = bIsASBorderRouter;
            rLSA.IsVirtualEndpoint = bIsVirtualEndPoint;

            foreach (DirectAttachedNetwork dan in arDan)
            {
                if (dan.AssociatedArea != null && dan.AssociatedArea.AreaID == iAreaID)
                {
                    switch (dan.NetworkType) //Add link data items
                    {
                        case OSPFNetworkType.PointToPoint: throw new NotImplementedException();
                        case OSPFNetworkType.PointToMultipoint: throw new NotImplementedException();
                        default:
                            RouterLSA.LinkItem rLSALI;
                            rLSALI = new RouterLSA.LinkItem();
                            rLSALI.Type = RouterLSA.LinkType.Transit;
                            IPAddress ipaDRAddress;
                            if (dan.DR != null)
                            {
                                if (dan.DR != this)
                                {
                                    ipaDRAddress = dan.DR.IPAddress;
                                }
                                else
                                {
                                    ipaDRAddress = dan.InterfaceAddress;
                                }
                            }
                            else
                            {
                                ipaDRAddress = dan.InterfaceAddress;
                            }
                            rLSALI.LinkID = ConvertIntIP(ipaDRAddress);
                            rLSALI.LinkData = ConvertIntIP(dan.InterfaceAddress);
                            rLSALI.ZeroTOSMetric = 1;
                            rLSA.AddLinkItem(rLSALI);
                            rLSALI = new RouterLSA.LinkItem();
                            rLSALI.Type = RouterLSA.LinkType.Stub;
                            rLSALI.LinkID = ConvertIntIP(ipv4Analysis.GetClasslessNetworkAddress(dan.InterfaceAddress, dan.SubnetMask));
                            rLSALI.LinkData = dan.SubnetMask.IntNotation;
                            rLSALI.ZeroTOSMetric = 2;
                            rLSA.AddLinkItem(rLSALI);
                            break;
                    }
                }
            }

            h.EncapsulatedFrame = rLSA;

            return h;
        }

        /// <summary>
        /// Updates the information about the local router in an LS database for an area.
        /// </summary>
        /// <param name="iAreaID">The ID of the area to update</param>
        private void UpdateAreaLSA(uint iAreaID)
        {
            UpdateAreaLSA(dictIDArea[iAreaID]);   
        }

        /// <summary>
        /// Updates the information about the local router in an LS database for an area.
        /// </summary>
        /// <param name="aArea">The area to update</param>
        private void UpdateAreaLSA(OSPFArea aArea)
        {
            List<LSAHeader> lLSAHeader = new List<LSAHeader>();
            lLSAHeader.Add(CreateRouterLSA(aArea.AreaID));

            DirectAttachedNetwork[] arDan = lNetworks.ToArray();

            foreach (DirectAttachedNetwork dan in arDan)
            {
                if (dan.AssociatedArea != null && dan.AssociatedArea.AreaID == aArea.AreaID)
                {
                    lLSAHeader.Add(CreateNetworkLSA(dan));
                }
            }

            if (dictlLocalLSAs.ContainsKey(aArea))
            {
                aArea.Database.RemoveLSARange(dictlLocalLSAs[aArea].ToArray());
                dictlLocalLSAs[aArea].Clear();
            }
            else
            {
                dictlLocalLSAs.Add(aArea, new List<LSAHeader>());
            }
            aArea.Database.AddLSARange(lLSAHeader.ToArray());
            dictlLocalLSAs[aArea].AddRange(lLSAHeader);
        }

        private LSAHeader CreateNetworkLSA(DirectAttachedNetwork dan)
        {
            LSAHeader h = CreateLSAHeader(LSType.Network);
            if (dan.DR != this && dan.DR != null)
            {
                h.LinkStateID = ConvertIntIP(dan.DR.IPAddress);
            }
            else
            {
                h.LinkStateID = ConvertIntIP(dan.InterfaceAddress);
            }
            NetworkLSA nLSA = new NetworkLSA();

            nLSA.Netmask = dan.SubnetMask;

            OSPFNeighbour[] arnNeigh = lNeighbours.ToArray();

            foreach (OSPFNeighbour nNeigh in arnNeigh)
            {
                if (nNeigh.AttachedNetwork == dan)
                {
                    NetworkLSA.NetworkLSAItem nLSAItem = new NetworkLSA.NetworkLSAItem();
                    nLSAItem.AttachedRouterID = nNeigh.ID;
                    nLSAItem.Mask = dan.SubnetMask;
                    nLSA.AddNetworkItem(nLSAItem);
                }
            }

            h.EncapsulatedFrame = nLSA;

            return h;
        }

        private LSAHeader CreateLSAHeader(LSType lsType)
        {
            LSAHeader lHeader = new LSAHeader();
            lHeader.AdvertisingRouter = iRouterID;
            lHeader.Options.EBit = true;
            lHeader.LSAge = 0;
            lHeader.LSType = lsType;
            lHeader.OrigalLength = -1;
            lHeader.SequenceNumber = GetLSASeqCounter();

            return lHeader;
        }

        private uint GetLSASeqCounter()
        {
            if (iLSASeqCounter == uint.MaxValue)
            {
                iLSASeqCounter = 0;
            }

            return iLSASeqCounter++;
        }

        public void RemoveNetwork(NetworkEntry ne)
        {
            RemoveNetwork(ne.AreaID, ne.Network, ne.Wildcard);
        }

        public void RemoveNetwork(uint iAreaID, IPAddress ipa, Subnetmask smWildcard)
        {
            //Todo: Remove LSA from Database

            DirectAttachedNetwork[] arDan = lNetworks.ToArray();
            OSPFArea ospfArea = GetAreaForID(iAreaID);

            foreach (DirectAttachedNetwork dan in arDan)
            {
                if (dan.AssociatedArea != null && dan.AssociatedArea.AreaID == iAreaID && MatchWildcard(ipa, dan.InterfaceAddress, smWildcard))
                {
                    dan.AssociatedArea = null;
                    InvokeExternalAsync(NetworkRemoved, new NetworkEventArgs(new NetworkEntry(dan.InterfaceAddress, smWildcard, iAreaID)));
                }
            }
        }

        private bool MatchWildcard(IPAddress ipa1, IPAddress ipa2, Subnetmask sWildcard)
        {
            byte[] bAddress1 = ipa1.GetAddressBytes();
            byte[] bAddress2 = ipa2.GetAddressBytes();
            byte[] bWildcard = sWildcard.MaskBytes;

            bool bMatch = true;

            for (int iC1 = 0; iC1 < 4; iC1++)
            {
                if ((bAddress1[iC1] & (~bWildcard[iC1])) != (bAddress2[iC1] & (~bWildcard[iC1])))
                {
                    bMatch = false;
                }
            }

            return bMatch;
        }

        private bool MatchSubnet(IPAddress ipa1, IPAddress ipa2, Subnetmask sSubnetmask)
        {
            byte[] bAddress1 = ipa1.GetAddressBytes();
            byte[] bAddress2 = ipa2.GetAddressBytes();
            byte[] bSubnet = sSubnetmask.MaskBytes;

            bool bMatch = true;

            for (int iC1 = 0; iC1 < 4; iC1++)
            {
                if ((bAddress1[iC1] & (bSubnet[iC1])) != (bAddress2[iC1] & (bSubnet[iC1])))
                {
                    bMatch = false;
                }
            }

            return bMatch;
        }

        private OSPFArea GetAreaForID(uint iID)
        {
            OSPFArea a;
            lock (dictIDArea)
            {
                if (!dictIDArea.ContainsKey(iID))
                {
                    OSPFArea oArea = new OSPFArea(iID);
                    dictIDArea.Add(iID, oArea);
                }
                a = dictIDArea[iID];
            }

            return a;
        }

        public override void AddInterface(IPInterface ipInterface)
        {
            IPAddress[] arips = ipInterface.IpAddresses;
            Subnetmask[] smMask = ipInterface.Subnetmasks;
            OSPFNetworkType netType = GetTypeForInterface(ipInterface);

            ipInterface.AddressAdded += new IPInterface.AddressEventHandler(ipInterface_AddressAdded);
            ipInterface.AddressRemoved += new IPInterface.AddressEventHandler(ipInterface_AddressRemoved);

            for (int iC1 = 0; iC1 < arips.Length && iC1 < smMask.Length; iC1++)
            {
                DirectAttachedNetwork dan = new DirectAttachedNetwork(ipInterface, netType);
                dan.InterfaceAddress = arips[iC1];
                dan.SubnetMask = smMask[iC1];
                lNetworks.Add(dan);
            }
            base.AddInterface(ipInterface);
        }

        void ipInterface_AddressRemoved(object sender, AddressEventArgs args)
        {
            RemoveInterfaceAddress(args.Interface, args.Netmask, args.IP);
        }

        private void RemoveInterfaceAddress(IPInterface ipi, Subnetmask smMask, IPAddress ipa)
        {
            DirectAttachedNetwork[] nNetworks = lNetworks.ToArray();
            foreach (DirectAttachedNetwork dan in nNetworks)
            {
                if (dan.AttachedInterface == ipi && dan.SubnetMask.Equals(smMask) && dan.InterfaceAddress.Equals(ipa))
                {
                    lNetworks.Remove(dan);
                }
            }
        }

        void ipInterface_AddressAdded(object sender, AddressEventArgs args)
        {
            OSPFNetworkType netType = GetTypeForInterface(args.Interface);
            DirectAttachedNetwork dan = new DirectAttachedNetwork(args.Interface, netType);
            dan.InterfaceAddress = args.IP;
            dan.SubnetMask = args.Netmask;
        }

        private OSPFNetworkType GetTypeForInterface(IPInterface ipInterface)
        {
            if (ipInterface.AdapterType == NetworkInterfaceType.Unknown)
            {
                throw new ArgumentException("An adapter type of 'unknown' is not supported");
            }
            throw new NotImplementedException("Not implemented - have to implement SupportsMulticast or so into the IP interface class");
            //else if (ipInterface.AdapterType == AdapterType.Ethernet ||
            //    ipInterface.AdapterType == AdapterType.EthernetDIXHeader ||
            //    ipInterface.AdapterType == AdapterType.Wireless ||
            //    ipInterface.AdapterType == AdapterType.InfraredWireless ||
            //    ipInterface.AdapterType == AdapterType.IEEE1394 ||
            //    ipInterface.AdapterType == AdapterType.ARCNET ||
            //    ipInterface.AdapterType == AdapterType.ARCNET_878_2)
            //{
            //    return OSPFNetworkType.BroadcastMultiAccess;
            //}
            //else if (ipInterface.AdapterType == AdapterType.TokenRing ||
            //        ipInterface.AdapterType == AdapterType.LocalTalk)
            //{
            //    return OSPFNetworkType.NonBroadcastMultipleAccess;
            //}
            //else if (ipInterface.AdapterType == AdapterType.WAN ||
            //    ipInterface.AdapterType == AdapterType.ATM)
            //{
            //    return OSPFNetworkType.PointToMultipoint;
            //}
            //else
            //{
            //    return OSPFNetworkType.PointToPoint;
            //}
        }

        public override void RemoveInterface(IPInterface ipInterface)
        {
            IPAddress[] arips = ipInterface.IpAddresses;
            Subnetmask[] smMask = ipInterface.Subnetmasks;

            for (int iC1 = 0; iC1 < arips.Length && iC1 < smMask.Length; iC1++)
            {
                RemoveInterfaceAddress(ipInterface, smMask[iC1], arips[iC1]);
            }

            base.RemoveInterface(ipInterface);
        }

        void tHello_Elapsed(object sender, ElapsedEventArgs e)
        {
            DirectAttachedNetwork[] arDan = lNetworks.ToArray();

            foreach (DirectAttachedNetwork dan in arDan)
            {
                if (dan.AssociatedArea != null)
                {
                    IP.IPFrame ipFrame = CreateIPFrame(dan.InterfaceAddress, this.ipaAllOSPFRouters);
                    ipFrame.EncapsulatedFrame = CreateHeader(dan.AssociatedArea.AreaID, OSPFFrameType.Hello);

                    OSPFNeighbour[] arNeigh = lNeighbours.ToArray();

                    List<IPAddress> lipaNeighbours = new List<IPAddress>();
                    foreach (OSPFNeighbour neigh in arNeigh)
                    {
                        lipaNeighbours.Add(ConvertIntIP(neigh.ID));
                    }

                    ipFrame.EncapsulatedFrame.EncapsulatedFrame = CreateHelloMessage(dan.SubnetMask, lipaNeighbours.ToArray(), dan);

                    dan.AttachedInterface.Send(ipFrame, ipaAllOSPFRouters);
                }
            }
        }

        private OSPFCommonHeader CreateHeader(uint iAreaID, OSPFFrameType oType)
        {
            OSPFCommonHeader hHeader = new OSPFCommonHeader();
            hHeader.AreaID = iAreaID;
            hHeader.AuthType = OSPFAuthenticationType.NoAuthentication;
            hHeader.RouterID = this.iRouterID;
            hHeader.Version = 2;
            hHeader.OSPFType = oType;

            return hHeader;
        }

        private EthernetFrame CreateEthFrame()
        {
            return CreateEthFrame(new MACAddress(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }));
        }

        private EthernetFrame CreateEthFrame(MACAddress macDestination)
        {
            EthernetFrame ethFrame = new EthernetFrame();
            ethFrame.Destination = macDestination;
            return ethFrame;
        }

        private OSPFHelloMessage CreateHelloMessage(Subnetmask smSubnetmask, IPAddress[] ipaNeighbours, DirectAttachedNetwork dan)
        {
            OSPFHelloMessage oHello = new OSPFHelloMessage();
            oHello.BackupDesignatedRouter = IPAddress.Any;
            oHello.DeadInterval = this.iRouterDeadInterval;
            oHello.DesignatedRouter = IPAddress.Any;
            oHello.HelloInterval = this.iHelloInterval;
            oHello.Options = new OSPFOptionsField(this.ospfOptions.Data);
            oHello.Netmask = smSubnetmask;
            oHello.Priority = bPriority;
            foreach (IPAddress ipa in ipaNeighbours)
            {
                oHello.AddNeighbour(ipa);
            }

            if(dan.DR != null)
            {
                if (dan.DR != this)
                {
                    oHello.DesignatedRouter = dan.DR.IPAddress;
                }
                else
                {
                    oHello.DesignatedRouter = dan.InterfaceAddress;
                }
            }
            if (dan.BDR != null)
            {
                if (dan.BDR != this)
                {
                    oHello.BackupDesignatedRouter = dan.BDR.IPAddress;
                }
                else
                {
                    oHello.BackupDesignatedRouter = dan.InterfaceAddress;
                }
            }
            return oHello;
        }

        private IPFrame CreateIPFrame(IPAddress ipaSource, IPAddress ipaDestination)
        {
            IPv4Frame ipv4Frame = new IPv4Frame();
            ipv4Frame.DestinationAddress = ipaDestination;
            ipv4Frame.Identification = (uint)iIPIDCounter;
            iIPIDCounter++;
            if (iIPIDCounter > 65535)
            {
                iIPIDCounter = 0;
            }
            ipv4Frame.Protocol = IPProtocol.OSPF;
            ipv4Frame.SourceAddress = ipaSource;
            ipv4Frame.Version = 4;

            return ipv4Frame;
        }

        private Subnetmask ToWildcard(Subnetmask smMask)
        {
            byte[] bSource = smMask.MaskBytes;
            return new Subnetmask(new byte[] { (byte)~bSource[0], (byte)~bSource[1], (byte)~bSource[2], (byte)~bSource[3] });
        }

        void tTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            OSPFNeighbour[] arNeigh = lNeighbours.ToArray();
            foreach (OSPFNeighbour n in arNeigh)
            {
                n.DeadTimer--;
                if (n.DeadTimer < 0)
                {
                    n.AttachedNetwork.InterfaceState = OSPFState.Down;
                    lNeighbours.Remove(n);
                }
            }
        }

        protected override void HandleTraffic(Frame fInputFrame)
        {
            IP.IPFrame ipv4Frame = GetIPv4Frame(fInputFrame);

            if (ipv4Frame != null && ipv4Frame.EncapsulatedFrame != null && ipv4Frame.Protocol == eExNetworkLibrary.IP.IPProtocol.OSPF)
            {
                OSPFCommonHeader ospfHeader = new OSPFCommonHeader(ipv4Frame.EncapsulatedFrame.FrameBytes);
                EthernetFrame ethFrame = GetEthernetFrame(fInputFrame);

                switch (ospfHeader.OSPFType)
                {
                    case OSPFFrameType.DatabaseDescription: HandleDatabaseDescription(ospfHeader, (OSPFDatabaseDescriptionMessage)ospfHeader.EncapsulatedFrame);
                        break;
                    case OSPFFrameType.Hello: HandleHello(ospfHeader, (OSPFHelloMessage)ospfHeader.EncapsulatedFrame, ipv4Frame, ethFrame);
                        break;
                    case OSPFFrameType.LinkStateAcknowledgement: HandleLSACK(ospfHeader, (OSPFLSAAcknowledgementMessage)ospfHeader.EncapsulatedFrame);
                        break;
                    case OSPFFrameType.LinkStateRequest: HandleLSREQ(ospfHeader, (OSPFLSARequestMessage)ospfHeader.EncapsulatedFrame);
                        break;
                    case OSPFFrameType.LinkStateUpdate: HandleLSUPD(ospfHeader, (OSPFLSAUpdateMessage)ospfHeader.EncapsulatedFrame);
                        break;
                }
            }
        }

        private void UpdateDRandBDR(DirectAttachedNetwork dan)
        {

            OSPFNeighbour[] arNeigh = lNeighbours.ToArray();

            IOSPFRouter ospfDR = null;
            IOSPFRouter ospfBDR = null;

            if (dan.DR == null)
            {
                foreach (OSPFNeighbour ospfNeighbour in arNeigh)
                {
                    if (dan == ospfNeighbour.AttachedNetwork)
                    {
                        if (ospfDR == null || ospfNeighbour.Priority > ospfDR.Priority || (ospfNeighbour.Priority == ospfDR.Priority && ospfNeighbour.ID > ospfDR.ID))
                        {
                            ospfDR = ospfNeighbour;
                        }
                    }
                }

                if (ospfDR == null || this.bPriority > ospfDR.Priority || (this.bPriority == ospfDR.Priority & this.iRouterID > ospfDR.ID))
                {
                    ospfDR = this;
                }
            }
            else
            {
                ospfDR = dan.DR;
            }

            if (dan.BDR == null)
            {
                foreach (OSPFNeighbour ospfNeighbour in arNeigh)
                {
                    if (dan == ospfNeighbour.AttachedNetwork)
                    {
                        if (ospfBDR == null || ((ospfNeighbour.Priority > ospfBDR.Priority || (ospfNeighbour.Priority == ospfBDR.Priority && ospfNeighbour.ID > ospfBDR.ID))) && ospfBDR != ospfDR)
                        {
                            ospfBDR = ospfNeighbour;
                        }
                    }
                }

                if (ospfDR != null)
                {
                    if (ospfBDR == null || this.bPriority > ospfBDR.Priority || (this.bPriority == ospfBDR.Priority & this.iRouterID > ospfBDR.ID))
                    {
                        ospfDR = this;
                    }
                }
            }
            else
            {
                ospfBDR = dan.BDR;
            }

            dan.BDR = ospfBDR;
            dan.DR = ospfDR;
        }

        private void HandleDatabaseDescription(OSPFCommonHeader ospfHeader, OSPFDatabaseDescriptionMessage ospfDesc)
        {
            OSPFNeighbour ospfNeigh = GetNeighbourByID(ospfHeader.RouterID);

            if (ospfNeigh != null)
            {
                if (ospfNeigh.State == OSPFState.Init)
                {
                    //Database Description without hello? Neh...   
                }
                else if (ospfNeigh.State == OSPFState.TwoWay)
                {
                    //Database Description without DR or BDR? Neh...   
                }
                else if (ospfNeigh.State == OSPFState.ExStart)
                {
                    if (ospfDesc.IsInit && ospfDesc.IsMaster && ospfDesc.IsMore && ospfDesc.GetItems().Length == 0 && ospfNeigh.ID > this.iRouterID)
                    {
                        ospfNeigh.LSAHeadersToSend.AddRange(ospfNeigh.AttachedNetwork.AssociatedArea.Database.GetLSAs());                   
                        ospfNeigh.MTU = ospfDesc.InterfaceMTU;
                        ospfNeigh.IsMaster = true;
                        ospfNeigh.Options = new OSPFOptionsField(ospfDesc.Options.Data);
                        ospfNeigh.SetDDSeqCounter(ospfDesc.SequenceNumber);
                        SendInitDBDescription(ospfNeigh, false, true);
                        ospfNeigh.State = OSPFState.Exchange;
                        ospfNeigh.AttachedNetwork.InterfaceState = OSPFState.Exchange;
                    }
                    if (!ospfDesc.IsInit && !ospfDesc.IsMaster && ospfDesc.IsMore && ospfNeigh.ID < this.iRouterID)
                    {
                        //I am the Master.
                        ospfNeigh.LSAHeadersToSend.AddRange(ospfNeigh.AttachedNetwork.AssociatedArea.Database.GetLSAs());
                        ospfNeigh.MTU = ospfDesc.InterfaceMTU;
                        ospfNeigh.IsMaster = false;
                        ospfNeigh.Options = new OSPFOptionsField(ospfDesc.Options.Data);
                        SendNextDBDescription(ospfNeigh, true, false, true);
                        ospfNeigh.State = OSPFState.Exchange;
                        ospfNeigh.AttachedNetwork.InterfaceState = OSPFState.Exchange;
                    }
                }
                else if (ospfNeigh.State == OSPFState.Exchange)
                {
                    if (ospfDesc.IsInit)
                    {
                        NeighborError(ospfNeigh);
                    }
                    if (ospfDesc.Options.Data != ospfNeigh.Options.Data)
                    {
                        NeighborError(ospfNeigh);
                    }

                    if (!ospfNeigh.IsMaster)
                    {
                        //I am master
                        if (ospfDesc.SequenceNumber == ospfNeigh.PeekDDSeqCounter() - 1)
                        {
                            if (ospfDesc.IsMore)
                            {
                                ProcessDatabaseDescriptionData(ospfNeigh, ospfHeader, ospfDesc);
                                SendNextDBDescription(ospfNeigh, true, false, true);
                            }
                            else
                            {
                                ospfNeigh.SetDDSeqCounter(ospfDesc.SequenceNumber);
                                SendNextDBDescription(ospfNeigh, true, false, false);
                                ospfNeigh.State = OSPFState.Loading;
                                StartExchange(ospfNeigh);
                            }
                        }
                        else if (ospfDesc.SequenceNumber == ospfNeigh.PeekDDSeqCounter() - 2)
                        {
                            //Duplicate - Simply Discard
                        }
                        else
                        {
                            NeighborError(ospfNeigh);
                        }
                    }
                    else
                    {
                        //I am slave
                        if (ospfDesc.SequenceNumber == ospfNeigh.PeekDDSeqCounter())
                        {
                            if (ospfDesc.IsMore)
                            {
                                ProcessDatabaseDescriptionData(ospfNeigh, ospfHeader, ospfDesc);
                                ospfNeigh.SetDDSeqCounter(ospfDesc.SequenceNumber);
                                SendNextDBDescription(ospfNeigh, false, false, true);
                            }
                            else
                            {
                                ospfNeigh.SetDDSeqCounter(ospfDesc.SequenceNumber);
                                SendNextDBDescription(ospfNeigh, false, false, false);
                                ospfNeigh.State = OSPFState.Loading;
                                StartExchange(ospfNeigh);
                            }
                        }
                        else if (ospfDesc.SequenceNumber == ospfNeigh.PeekDDSeqCounter() - 1)
                        {
                            //Respond with last send DD Description
                            ospfNeigh.AttachedNetwork.AttachedInterface.Send(ospfNeigh.LastSendDDDescription);
                        }
                        else
                        {
                            NeighborError(ospfNeigh);
                        }
                    }
                }
                else if (ospfNeigh.State == OSPFState.Loading || ospfNeigh.State == OSPFState.Full)
                {
                    if (ospfDesc.IsInit)
                    {
                        NeighborError(ospfNeigh);
                    }
                    else if (!ospfNeigh.IsMaster)
                    {
                        if (ospfDesc.SequenceNumber == ospfNeigh.PeekDDSeqCounter() - 1)
                        {
                            //Duplicate - Simply discard
                        }
                        else
                        {
                            NeighborError(ospfNeigh);
                        }
                    }
                    else
                    {
                        if (ospfDesc.SequenceNumber == ospfNeigh.PeekDDSeqCounter())
                        {
                            //Duplicate - Simply discard
                        }
                        else
                        {
                            NeighborError(ospfNeigh);
                        }
                    }
                }

            }

            #region Depracted
            /* Commented out because: Not fully RFC Compliant
             * Currently rewriting this region like described in RFC.
             * 
            if (ospfNeigh != null)
            {
                if (ospfNeigh.State == OSPFState.ExStart)
                {
                    if (ospfNeigh.ID < this.ID)
                    {
                        iSeqCounter = ospfDesc.SequenceNumber;
                        SendFullDBDescr(ospfNeigh, false, false);
                        ospfNeigh.State = OSPFState.Exchange;
                    }
                    else
                    {
                        SendEmptyDBDescription(ospfNeigh, true, true);
                        ospfNeigh.State = OSPFState.Exchange;
                    }
                }
               
                if (ospfNeigh.State == OSPFState.Exchange && !ospfDesc.IsInit)
                {
                    if (ospfNeigh.ID < this.ID)
                    {
                        if (RequestLSAS(ospfNeigh, ospfHeader, ospfDesc))
                        {
                            if (ospfNeigh.State == OSPFState.ExStart)
                            {
                                
                            }
                            ospfNeigh.State = OSPFState.LoadingState;
                        }
                        else
                        {
                            ospfNeigh.State = OSPFState.FullState;
                        }
                    }
                    else
                    {
                        SendFullDBDescr(ospfNeigh, true, false);
                        if (RequestLSAS(ospfNeigh, ospfHeader, ospfDesc))
                        {
                            ospfNeigh.State = OSPFState.LoadingState;
                        }
                        else
                        {
                            ospfNeigh.State = OSPFState.FullState;
                        }
                    }
                }
            }
             * */
            #endregion
        }

        private void ProcessDatabaseDescriptionData(OSPFNeighbour ospfNeigh, OSPFCommonHeader ospfHeader, OSPFDatabaseDescriptionMessage ospfDesc)
        {
            ospfNeigh.LSAHeadersReceived.AddRange(ospfDesc.GetItems());
        }

        private void StartExchange(OSPFNeighbour ospfNeigh)
        {

        }

        /// <summary>
        /// On Neighbour error - Simply kill.
        /// </summary>
        /// <param name="ospfNeigh">The neighbour to remove</param>
        private void NeighborError(IOSPFRouter ospfNeigh)
        {
            ospfNeigh.State = OSPFState.Down;
            ospfNeigh.DeadTimer = int.MaxValue;
            throw new Exception("Neighbour Error");
        }

        private void SendNextDBDescription(OSPFNeighbour nNeigh, bool bSetMasterBit, bool bSetInit, bool bSetMore)
        {
            EthernetFrame ethFrame = CreateEthFrame(nNeigh.MACAddress);
            ethFrame.EncapsulatedFrame = CreateIPFrame(nNeigh.AttachedNetwork.InterfaceAddress, nNeigh.IPAddress);
            ethFrame.EncapsulatedFrame.EncapsulatedFrame = CreateHeader(nNeigh.AttachedNetwork.AssociatedArea.AreaID, OSPFFrameType.DatabaseDescription);
            OSPFDatabaseDescriptionMessage ospfDBM = CreateDatabaseDescriptionPacket(new LSAHeader[] { }, bSetMasterBit, bSetInit, bSetMore, nNeigh.GetDDSeqCounter());
            ethFrame.EncapsulatedFrame.EncapsulatedFrame.EncapsulatedFrame = ospfDBM;

            LSAHeader[] lHeaders = nNeigh.LSAHeadersToSend.ToArray();

            for (int iC1 = 0; iC1 < lHeaders.Length; iC1++)
            {
                if (ethFrame.Length + 20 > nNeigh.MTU)
                {
                    break;
                }
                ospfDBM.AddItem(lHeaders[iC1]);
                nNeigh.LSAHeadersToSend.Remove(lHeaders[iC1]);
            }


            nNeigh.AttachedNetwork.AttachedInterface.Send(ethFrame, nNeigh.IPAddress);
        }

        private void HandleHello(OSPFCommonHeader ospfHeader, OSPFHelloMessage ospfHello, IPFrame ipv4Frame, EthernetFrame ethFrame)
        {
            IPAddress ipaSource = ipv4Frame.SourceAddress;

            DirectAttachedNetwork nNetwork = null;
            OSPFNeighbour nNeighbour = null;

            DirectAttachedNetwork[] ardan = lNetworks.ToArray();
            OSPFNeighbour[] arNeigh = lNeighbours.ToArray();

            foreach (DirectAttachedNetwork dan in ardan)
            {
                if (MatchSubnet(dan.InterfaceAddress, ipv4Frame.SourceAddress, dan.SubnetMask))
                {
                    nNetwork = dan;
                    break;
                }
            }
            foreach (OSPFNeighbour neigh in arNeigh)
            {
                if (neigh.ID == ospfHeader.RouterID && neigh.AttachedNetwork == nNetwork)
                {
                    nNeighbour = neigh;
                    break;
                }
            }

            if (nNeighbour != null)
            {
                nNeighbour.DeadTimer = iRouterDeadInterval;
                nNeighbour.MACAddress = new MACAddress(ethFrame.Source.AddressBytes);
            }

            if (!ospfHello.ContainsNeighbour(ConvertIntIP(this.iRouterID)) || nNeighbour == null || nNeighbour.State == OSPFState.Down)
            {
                // Start Init State
                if (nNeighbour == null)
                {
                    nNeighbour = new OSPFNeighbour(ospfHeader.RouterID, ipv4Frame.SourceAddress, iRouterDeadInterval);
                    nNeighbour.AttachedNetwork = nNetwork;
                    lNeighbours.Add(nNeighbour);
                }
                if (nNeighbour.State == OSPFState.Down)
                {
                    nNeighbour.DeadTimer = iRouterDeadInterval;
                }
                nNeighbour.State = OSPFState.Init;
                nNeighbour.AttachedNetwork.InterfaceState = OSPFState.Init;
            }

            if(ospfHello.ContainsNeighbour(ConvertIntIP(this.iRouterID)) && nNeighbour != null && nNeighbour.State == OSPFState.Init)
            {
                //Start Two way state

                nNeighbour.State = OSPFState.TwoWay;
                nNeighbour.AttachedNetwork.InterfaceState = OSPFState.TwoWay;

                if (nNetwork.NetworkType == OSPFNetworkType.PointToPoint)
                {
                    //Start P2P Exchange
                    nNeighbour.State = OSPFState.ExStart;
                    nNeighbour.AttachedNetwork.InterfaceState = OSPFState.ExStart;
                    UpdateAreaLSA(ospfHeader.AreaID);
                    InitateExcange(nNeighbour);
                }
                else
                {
                    if (nNetwork.DR == null && !ospfHello.DesignatedRouter.Equals(IPAddress.Any))
                    {
                        nNetwork.DR = GetNeighbourByAddress(ospfHello.DesignatedRouter, nNetwork);
                    }
                    if (nNetwork.BDR == null && !ospfHello.BackupDesignatedRouter.Equals(IPAddress.Any))
                    {
                        nNetwork.BDR = GetNeighbourByAddress(ospfHello.BackupDesignatedRouter, nNetwork);
                    }
                    UpdateDRandBDR(nNetwork);
                    UpdateAreaLSA(ospfHeader.AreaID);
                }
            }

            if (nNeighbour != null && nNeighbour.State == OSPFState.TwoWay && nNeighbour.AttachedNetwork.NetworkType != OSPFNetworkType.PointToPoint) 
            {
                //Start BMA Exchange here

                if ((nNetwork.DR == this && ospfHello.DesignatedRouter.Equals(nNetwork.InterfaceAddress)) || (nNetwork.DR != this && nNetwork.DR.IPAddress.Equals(ospfHello.DesignatedRouter)) &&
                   (nNetwork.BDR == this && ospfHello.BackupDesignatedRouter.Equals(nNetwork.InterfaceAddress)) || (nNetwork.BDR != this && nNetwork.BDR.IPAddress.Equals(ospfHello.BackupDesignatedRouter))) //Check if DR and BDR match
                {
                    nNeighbour.State = OSPFState.ExStart;
                    nNeighbour.AttachedNetwork.InterfaceState = OSPFState.ExStart;
                    if (nNetwork.DR == this)
                    {
                        OSPFNeighbour[] arExchangeNeigh = lNeighbours.ToArray();
                        foreach (OSPFNeighbour neigh in arExchangeNeigh)
                        {
                            InitateExcange(neigh);
                        }
                    }
                    else
                    {
                        InitateExcange((OSPFNeighbour)nNetwork.DR);
                    }
                }
            }
        }

        private OSPFNeighbour GetNeighbourByAddress(IPAddress ipaRouterID, DirectAttachedNetwork dan)
        {
            OSPFNeighbour[] arNeigh = lNeighbours.ToArray();

            foreach (OSPFNeighbour ospfNeigh in arNeigh)
            {
                if (ospfNeigh.IPAddress.Equals(ipaRouterID) && ospfNeigh.AttachedNetwork == dan)
                {
                    return ospfNeigh;
                }
            }

            return null;
        }

        private OSPFNeighbour GetNeighbourByID(uint iRouterID)
        {
            OSPFNeighbour[] arNeigh = lNeighbours.ToArray();

            foreach (OSPFNeighbour ospfNeigh in arNeigh)
            {
                if (iRouterID == ospfNeigh.ID)
                {
                    return ospfNeigh;
                }
            }

            return null;
        }

        private void InitateExcange(OSPFNeighbour nNeigh)
        {
            if (nNeigh.ID < this.iRouterID) //Only do it if this router is the initiator
            {
                SendInitDBDescription(nNeigh, true, true);
            }
        }

        private void SendInitDBDescription(OSPFNeighbour nNeigh, bool bSetMasterBit, bool bSetInit)
        {
            EthernetFrame ethFrame = CreateEthFrame(nNeigh.MACAddress);
            ethFrame.EncapsulatedFrame = CreateIPFrame(nNeigh.AttachedNetwork.InterfaceAddress, nNeigh.IPAddress);
            ethFrame.EncapsulatedFrame.EncapsulatedFrame = CreateHeader(nNeigh.AttachedNetwork.AssociatedArea.AreaID, OSPFFrameType.DatabaseDescription);
            ethFrame.EncapsulatedFrame.EncapsulatedFrame.EncapsulatedFrame = CreateDatabaseDescriptionPacket(new LSAHeader[0], bSetMasterBit, bSetInit, true, nNeigh.GetDDSeqCounter());

            nNeigh.AttachedNetwork.AttachedInterface.Send(ethFrame, nNeigh.IPAddress);
        }

        private OSPFArea[] GetAllAreas()
        {
            OSPFArea[] arArea;
            lock (dictIDArea)
            {
                arArea = new OSPFArea[dictIDArea.Values.Count];
                dictIDArea.Values.CopyTo(arArea, 0);
            }

            return arArea;
        }

        private OSPFDatabaseDescriptionMessage CreateDatabaseDescriptionPacket(LSAHeader[] lsaHeaders, bool bSetMasterBit, bool bSetInit, bool bSetMore, uint iSeqConter)
        {
            OSPFDatabaseDescriptionMessage msgDBD = new OSPFDatabaseDescriptionMessage();
            foreach (LSAHeader lsaHeader in lsaHeaders)
            {
                msgDBD.AddItem(lsaHeader);
            }
            msgDBD.Options = new OSPFOptionsField(this.ospfOptions.Data);
            msgDBD.IsInit = bSetInit;
            msgDBD.IsMore = bSetMore;
            msgDBD.IsMaster = bSetMasterBit;
            msgDBD.InterfaceMTU = (short)iMTU;
            msgDBD.SequenceNumber = iSeqConter;
            return msgDBD;
        }

        private IPAddress ConvertIntIP(uint i)
        {
            byte[] bAddressbytes = new byte[] { (byte)(0xFF & (i >> 24)), (byte)(0xFF & (i >> 16)), (byte)(0xFF & (i >> 8)), (byte)(0xFF & (i)) };
            return new IPAddress(bAddressbytes);
        }

        private uint ConvertIntIP(IPAddress ipa)
        {
            byte[] bAddressbytes = ipa.GetAddressBytes();
            return ((uint)bAddressbytes[0] << 24) + ((uint)bAddressbytes[1] << 16) + ((uint)bAddressbytes[2] << 8) + (uint)bAddressbytes[3];
        }

        private void HandleLSACK(OSPFCommonHeader ospfHeader,  OSPFLSAAcknowledgementMessage ospfLSACK)
        {

        }

        private void HandleLSREQ(OSPFCommonHeader ospfHeader, OSPFLSARequestMessage ospfLSREQ)
        {

        }

        private void HandleLSUPD(OSPFCommonHeader ospfHeader, OSPFLSAUpdateMessage ospfLSUPD)
        {

        }

        public override void Start()
        {
            base.Start();
            tTimer.Start();
            tHello.Start();
        }

        public override void Stop()
        {
            base.Stop();
        }

        public override void Cleanup()
        {
            tTimer.Stop();
            tHello.Stop();
            base.Cleanup();
        }

        #region IOSPFRouter Members - Dirty interface usage.

        public int DeadTimer
        {
            get
            {
                return iRouterDeadInterval;
            }
            set
            {
                iRouterDeadInterval = value;
            }
        }

        public uint ID
        {
            get
            {
                return iRouterID;
            }
            set
            {
                iRouterID = value;
            }
        }

        public bool IsMaster
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public IPAddress IPAddress
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public DirectAttachedNetwork AttachedNetwork
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public OSPFState State
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public MACAddress MACAddress
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public int GetDDSeqCounter()
        {
            throw new NotImplementedException();
        }

        public void SetDDSeqCounter(int iSeqcounter)
        {
            throw new NotImplementedException();
        }

        public void PeekDDSeqCounter(int iSeqcounter)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    [Obsolete("This class is marked depreceated in cause of a sloppy implementation and only limited compatibility..", false)]
    class NetworkEntry
    {
        IPAddress ipa;
        Subnetmask smWildcard;
        uint iAreaID;

        public uint AreaID
        {
            get { return iAreaID; }
            set { iAreaID = value; }
        }

        public IPAddress Network
        {
            get { return ipa; }
            set { ipa = value; }
        }

        public Subnetmask Wildcard
        {
            get { return smWildcard; }
            set { smWildcard = value; }
        }

        public NetworkEntry(IPAddress ipa, Subnetmask smWildcard, uint iAreaID)
        {
            this.ipa = ipa;
            this.smWildcard = smWildcard;
            this.iAreaID = iAreaID;
        }

        public NetworkEntry() : this(IPAddress.Any, new Subnetmask(), 0) { }
    }

    [Obsolete("This class is marked depreceated in cause of a sloppy implementation and only limited compatibility..", false)]
    class DirectAttachedNetwork
    {
        private OSPFNetworkType ospfNetworkType;
        private IPInterface ipiAttachedInterface;
        private IPAddress ipaNetworkAddress;
        private Subnetmask smMask;
        private OSPFArea ospfAssociatedArea;
        private OSPFState ospfInterfaceState;
        private IOSPFRouter neighDR;
        private IOSPFRouter neighBDR;

        /// <summary>
        /// Sets the OSPF backup designated router. Setting it to null will cause the local OSPF instance to become BDR.
        /// </summary>
        public IOSPFRouter BDR
        {
            get { return neighBDR; }
            set { neighBDR = value; }
        }

        /// <summary>
        /// Sets the OSPF designated router. Setting it to null will cause the local OSPF instance to become DR.
        /// </summary>
        public IOSPFRouter DR
        {
            get { return neighDR; }
            set { neighDR = value; }
        }

        public OSPFState InterfaceState
        {
            get { return ospfInterfaceState; }
            set { ospfInterfaceState = value; }
        }

        public OSPFArea AssociatedArea
        {
            get { return ospfAssociatedArea; }
            set { ospfAssociatedArea = value; }
        }

        public Subnetmask SubnetMask
        {
            get { return smMask; }
            set { smMask = value; }
        }

        public IPAddress InterfaceAddress
        {
            get { return ipaNetworkAddress; }
            set { ipaNetworkAddress = value; }
        }

        public IPInterface AttachedInterface
        {
            get { return ipiAttachedInterface; }
            set { ipiAttachedInterface = value; }
        }

        public OSPFNetworkType NetworkType
        {
            get { return ospfNetworkType; }
            set { ospfNetworkType = value; }
        }

        public DirectAttachedNetwork(IPInterface ipiAttachedInterface, OSPFNetworkType ospfNetworkType)
        {
            this.ipiAttachedInterface = ipiAttachedInterface;
            this.ospfNetworkType = ospfNetworkType;
            this.smMask = new Subnetmask();
            this.ipaNetworkAddress = IPAddress.Any;
        }

        public DirectAttachedNetwork() : this(null, OSPFNetworkType.BroadcastMultiAccess) { }
    }

    enum OSPFNetworkType
    {
        PointToPoint = 0,
        BroadcastMultiAccess = 1,
        NonBroadcastMultipleAccess = 2,
        PointToMultipoint = 3
    }

    enum OSPFState
    {
        Down = 0,
        Init = 1,
        TwoWay = 2,
        ExStart = 3,
        Exchange = 4,
        Loading = 5,
        Full = 6
    }

    [Obsolete("This class is marked depreceated in cause of a sloppy implementation and only limited compatibility..", false)]
    class OSPFNeighbour : eExNetworkLibrary.Routing.OSPF.IOSPFRouter
    {
        private uint iID;
        private IPAddress ipaAddress;
        private int iAgeTimer;
        private DirectAttachedNetwork nAttachedNetwork;
        private OSPFState nState;
        private int iPriority;
        private MACAddress macAddr;
        private uint iSeqCounter;
        private OSPFOptionsField ospfOptions;
        private bool bIsMaster;
        private EthernetFrame fLastSendDDDescription;
        private List<LSAHeader> lshToSend;
        private List<LSAHeader> lshReceived;
        private int iMTU;

        public int MTU
        {
            get { return iMTU; }
            set { iMTU = value; }
        }

        internal List<LSAHeader> LSAHeadersReceived
        {
            get { return lshReceived; }
            set { lshReceived = value; }
        }

        internal List<LSAHeader> LSAHeadersToSend
        {
            get { return lshToSend; }
            set { lshToSend = value; }
        }

        internal EthernetFrame LastSendDDDescription
        {
            get { return fLastSendDDDescription; }
            set { fLastSendDDDescription = value; }
        }

        public bool IsMaster
        {
            get { return bIsMaster; }
            set { bIsMaster = value; }
        }

        public OSPFOptionsField Options
        {
            get { return ospfOptions; }
            set { ospfOptions = value; }
        }

        internal uint GetDDSeqCounter()
        {
            return iSeqCounter++;
        }

        internal uint PeekDDSeqCounter()
        {
            return iSeqCounter;
        }

        internal void SetDDSeqCounter(uint iSeqCounter)
        {
            this.iSeqCounter = iSeqCounter;
        }

        public int Priority
        {
            get { return iPriority; }
            set { iPriority = value; }
        }

        public MACAddress MACAddress
        {
            get { return macAddr; }
            set { macAddr = value; }
        }

        public OSPFState State
        {
            get { return nState; }
            set { nState = value; }
        }

        public DirectAttachedNetwork AttachedNetwork
        {
            get { return nAttachedNetwork; }
            set { nAttachedNetwork = value; }
        }

        public int DeadTimer
        {
            get { return iAgeTimer; }
            set { iAgeTimer = value; }
        }

        public uint ID
        {
            get { return iID; }
            set { iID = value; }
        }

        public IPAddress IPAddress
        {
            get { return ipaAddress; }
            set { ipaAddress = value; }
        }

        public OSPFNeighbour(uint iID, IPAddress ipaAddress, int iAgeTimer)
        {
            nState = OSPFState.Down;
            this.iID = iID;
            this.ipaAddress = ipaAddress;
            this.iAgeTimer = iAgeTimer;
            this.MACAddress = MACAddress.Empty;
            lshReceived = new List<LSAHeader>();
            lshToSend = new List<LSAHeader>();
        }

        public OSPFNeighbour() : this(0, IPAddress.Any, 0) { }
    }

    [Obsolete("This class is marked depreceated in cause of a sloppy implementation and only limited compatibility..", false)]
    class NetworkEventArgs : EventArgs
    {
        private NetworkEntry neEntry;

        public NetworkEntry Entry
        {
            get { return neEntry; }
        }

        public NetworkEventArgs(NetworkEntry neEntry)
        {
            this.neEntry = neEntry;
        }
        
    }
}
