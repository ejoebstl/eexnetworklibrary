using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.Utilities;
using System.Threading;
using eExNetworkLibrary.Ethernet;
using System.Net;
using eExNetworkLibrary.ARP;
using eExNetworkLibrary.IP;

namespace eExNetworkLibrary
{
    /// <summary>
    /// This class represents an IP-interface which is opened with WinPcap to support layer 2 sniffing and injection
    /// </summary>
    public class EthernetInterface : IPInterface
    {
        private WinPcapDotNet wpcDevice;
        private WinPcapInterface wpcInterface;

        private Thread tWorker;
        private Queue<byte[]> qFrameQueue;
        private string strDNSName;
        private AutoResetEvent areWorkToDo;
        private HostTable arpHostTable;
        private bool bExcludeOwnTraffic;
        private bool bExcludeLocalHostTraffic;
        private object oInterfaceStartStopLock;
        private IPAddressAnalysis ipaAnalysis;

        private MACAddress maMacAddress;


        private bool bAutoAnswerARPRequests;

        private List<MACAddress> lmacSpoofAdresses;
        private MACAddress macprimarySpoofAddress;

        private bool bRun;

        /// <summary>
        /// Gets the ARPTable of this interface
        /// </summary>
        public HostTable ARPTable
        {
            get { return arpHostTable; }
        }

        /// <summary>
        /// Gets or sets a bool determining whether this interface should automatically answer ARPRequests for its IPAddresses.
        /// </summary>
        public bool AutoAnswerARPRequests
        {
            get { return bAutoAnswerARPRequests; }
            set
            {
                if (bAutoAnswerARPRequests != value)
                {
                    bAutoAnswerARPRequests = value;
                    InvokePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the WinPcap kernel level filter expression associated with this interface. 
        /// <remarks>The filter expression will be optimized and the subnetmask used to compile the expression is the first subnetmask of this interface or 255.255.255.255 if no subnetmask is present.</remarks>
        /// </summary>
        public string FilterExpression
        {
            get
            {
                return wpcDevice.Filter != null ? wpcDevice.Filter.FilterExpression : "";
            }
            set
            {
                wpcDevice.Filter = wpcDevice.CompileFilter(value, true, Subnetmasks.Length > 0 ? Subnetmasks[0] : new Subnetmask());
            }
        }

        /// <summary>
        /// Gets or sets a bool determining whether this interface should automatically filter its own sent traffic from the input packets.
        /// <remarks>As this method forces an interface re-open, it causes the WinPcap filter expression of this interface to be recompiled and some packets to pass the driver without being read.</remarks>
        /// </summary>
        public bool ExcludeOwnTraffic
        {
            get { return bExcludeOwnTraffic; }
            set
            {
                if (bExcludeOwnTraffic != value)
                {
                    bExcludeOwnTraffic = value;
                    lock (oInterfaceStartStopLock)
                    {
                        WinPcapFilter wpcFilter = wpcDevice.Filter;
                        CloseDevice();
                        OpenDevice();
                        //Re-assign the filter cause device has been restarted. 
                        wpcDevice.Filter = wpcFilter;
                    }
                    InvokePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets a bool indicating whether all traffic addressed to the localhost at IP level should be filtered out by the FrameCaptured event. 
        /// </summary>
        public bool ExcludeLocalHostTraffic
        {
            get { return bExcludeLocalHostTraffic; }
            set
            {
                if (bExcludeLocalHostTraffic != value)
                {
                    bExcludeLocalHostTraffic = value;
                    InvokePropertyChanged();
                }
            }
        }
        
        /// <summary>
        /// Gets the MACAddress of this interface, as it is known to the operating system.
        /// </summary>
        public MACAddress MACAddress
        {
            get { return maMacAddress; }
        }

        /// <summary>
        /// Gets or sets the primary address of this interface. This address will be used by this interface for network communication.
        /// </summary>
        public MACAddress PrimaryMACAddress
        {
            get { return macprimarySpoofAddress; }
            set
            {
                if (lmacSpoofAdresses.Contains(macprimarySpoofAddress))
                {
                    lmacSpoofAdresses.Remove(macprimarySpoofAddress);
                }
                macprimarySpoofAddress = value;
                if (!lmacSpoofAdresses.Contains(macprimarySpoofAddress))
                {
                    lmacSpoofAdresses.Add(macprimarySpoofAddress);
                }

                InvokePropertyChanged();
            }
        }


        /// <summary>
        /// Returns all known network adapters
        /// </summary>
        /// <returns>All known WinPcap network adapters</returns>
        public static WinPcapInterface[] GetAllPcapInterfaces()
        {
            return WinPcapDotNet.GetAllDevices().ToArray();
        }

        /// <summary>
        /// Gets this interfaces description
        /// </summary>
        public override string Description
        {
            get { return wpcInterface.Description; }
        }

        /// <summary>
        /// Gets this interfaces name
        /// </summary>
        public override string Name
        {
            get { return wpcInterface.Name; }
        }

        /// <summary>
        /// Gets this interfaces DNS name
        /// </summary>
        public override string DNSName
        {
            get { return strDNSName; }
        }
        
        /// <summary>
        /// Creates a new instance of this class, listening to the given interface
        /// </summary>
        /// <param name="wpcInterface">A WinPcapInterface which defines the interface to listen to</param>
        public EthernetInterface(WinPcapInterface wpcInterface)
        {
            if (InterfaceConfiguration.GetAdapterTypeForInterface(wpcInterface.Name) != System.Net.NetworkInformation.NetworkInterfaceType.Ethernet &&
                InterfaceConfiguration.GetAdapterTypeForInterface(wpcInterface.Name) != System.Net.NetworkInformation.NetworkInterfaceType.Ethernet3Megabit &&
                InterfaceConfiguration.GetAdapterTypeForInterface(wpcInterface.Name) != System.Net.NetworkInformation.NetworkInterfaceType.FastEthernetFx &&
                InterfaceConfiguration.GetAdapterTypeForInterface(wpcInterface.Name) != System.Net.NetworkInformation.NetworkInterfaceType.FastEthernetT &&
                InterfaceConfiguration.GetAdapterTypeForInterface(wpcInterface.Name) != System.Net.NetworkInformation.NetworkInterfaceType.GigabitEthernet &&
                InterfaceConfiguration.GetAdapterTypeForInterface(wpcInterface.Name) != System.Net.NetworkInformation.NetworkInterfaceType.Wireless80211)
            {
                throw new ArgumentException("Only enabled ethernet interfaces are supported at the moment.");
            }
            if (wpcInterface.Name == null)
            {
                throw new ArgumentException("Cannot create an interface instance for an interface not properly recognised by WinPcap."); 
            }

            ipaAnalysis = new IPAddressAnalysis();

            oInterfaceStartStopLock = new object();
            bExcludeOwnTraffic = true;
            bExcludeLocalHostTraffic = true;
            arpHostTable = new HostTable();
            lmacSpoofAdresses = new List<MACAddress>();
            bRun = false;
            bShutdownPending = false;

            qFrameQueue = new Queue<byte[]>();
            bIsRunning = false;
            areWorkToDo = new AutoResetEvent(false);
            this.wpcInterface = wpcInterface;
            wpcDevice = new WinPcapDotNet();
            this.maMacAddress = InterfaceConfiguration.GetMacAddressForInterface(wpcInterface.Name);
            this.aType = InterfaceConfiguration.GetAdapterTypeForInterface(wpcInterface.Name);

            IPAddress[] arip = InterfaceConfiguration.GetIPAddressesForInterface(wpcInterface.Name);
            Subnetmask[] smMask = InterfaceConfiguration.GetIPSubnetsForInterface(wpcInterface.Name);

            for (int iC1 = 0; iC1 < arip.Length && iC1 < smMask.Length; iC1++)
            {
                this.AddAddress(arip[iC1], smMask[iC1]);
            }

            ipStandardgateways.AddRange(InterfaceConfiguration.GetIPStandardGatewaysForInterface(wpcInterface.Name));
            this.strDNSName = Dns.GetHostName();
            PrimaryMACAddress = this.MACAddress;
        }

        void wpcDevice_BytesCaptured(WinPcapCaptureHeader wpcHeader, byte[] bPacketData, object sender)
        {
            if (!bShutdownPending)
            {
                try
                {
                    InvokeBytesCaptured(bPacketData);
                    ProcessReceivedData(bPacketData, wpcHeader);
                }
                catch (Exception ex)
                {
                    InvokeExceptionThrown(ex);
                }
            }
        }

        void ProcessReceivedData(byte[] bBacketData, WinPcapCaptureHeader wpcHeader)
        {
            EthernetFrame fFrame = new EthernetFrame(bBacketData);

            if (!(IsLocalHostTraffic(fFrame) && bExcludeLocalHostTraffic))
            {
                TrafficDescriptionFrame tdf = new TrafficDescriptionFrame(this, wpcHeader.Timestamp);
                tdf.EncapsulatedFrame = fFrame;

                UtilitzeARP(tdf);
                InvokeFrameForwarded();
                InvokePacketCaptured(tdf);
                if (OutputHandler != null)
                {
                    NotifyNext(tdf);
                }
                if (AutoAnswerARPRequests)
                {
                    HandleARP(fFrame);
                }
            }
        }

        private bool IsLocalHostTraffic(EthernetFrame fFrame)
        {
            IPFrame ipFrame = GetIPFrame(fFrame);
            return ipFrame != null && (InterfaceConfiguration.IsLocalAddress(ipFrame.SourceAddress) || InterfaceConfiguration.IsLocalAddress(ipFrame.DestinationAddress));
        }        
        
        /// <summary>
        /// Adds a MACAddress here to announce it as spoofed address. The interface will not pass traffic with this source MACAddress to connected traffic handlers if the property AutoExcludeOwnTraffic is also set.
        /// </summary>
        /// <param name="macAddress">The MACAddress to add</param>
        public void AddToSpoofedAddresses(MACAddress macAddress)
        {
            lmacSpoofAdresses.Add(macAddress);
        }

        /// <summary>
        /// Removes a MACAddress from the spoofed address list
        /// </summary>
        /// <param name="macAddress">The MACAddress to remove</param>
        public void RemoveFromSpoofedAddresses(MACAddress macAddress)
        {
            lmacSpoofAdresses.Remove(macAddress);
        }

        /// <summary>
        /// Returns whether a MACAddress is contained in this interfaces spoofed address list.
        /// </summary>
        /// <param name="macAddress">The MACAddress to search for</param>
        /// <returns>A bool indicating whether a MACAddress is contained in this interfaces spoofed address list.</returns>
        public bool UsesSpoofedAddress(MACAddress macAddress)
        {
            return lmacSpoofAdresses.Contains(macAddress);
        }

        /// <summary>
        /// Returns whether a MACAddress is used by this interface.
        /// </summary>
        /// <param name="macAddress">The MACAddress to search for</param>
        /// <returns>A bool indicating whether a MACAddress is used by this interface.</returns>
        public bool UsesAddress(MACAddress macAddress)
        {
            if (maMacAddress.Equals(macAddress))
            {
                return true;
            }

            if (lmacSpoofAdresses.Contains(macAddress))
            {
                return true;
            }

            return false;
        }        
        
        /// <summary>
        /// Checks a frame for ARP requests and handles the ARP request
        /// </summary>
        /// <param name="fFrame">The frame to check for ARP requests</param>
        protected void HandleARP(Frame fFrame)
        {
            ARPFrame arpFrame = GetARPFrame(fFrame);
            if (arpFrame != null && arpFrame.Operation == ARPOperation.Request && base.ContainsAddress(arpFrame.DestinationIP))
            {
                EthernetFrame ethReplyFrame = new EthernetFrame();
                ethReplyFrame.Destination = arpFrame.SourceMAC;
                ethReplyFrame.Source = this.PrimaryMACAddress;
                ethReplyFrame.EtherType = EtherType.ARP;

                ARPFrame arpReplyFrame = new ARPFrame();
                arpReplyFrame.SourceIP = arpFrame.DestinationIP;
                arpReplyFrame.SourceMAC = this.macprimarySpoofAddress;
                arpReplyFrame.DestinationMAC = arpFrame.SourceMAC;
                arpReplyFrame.DestinationIP = arpFrame.SourceIP;
                arpReplyFrame.Operation = ARPOperation.Reply;
                arpReplyFrame.HardwareAddressType = HardwareAddressType.Ethernet;
                arpReplyFrame.ProtocolAddressType = EtherType.IPv4;

                ethReplyFrame.EncapsulatedFrame = arpReplyFrame;

                this.Send(ethReplyFrame);
            }
        }

        /// <summary>
        /// Pushes bytes to the output queue as they are.
        /// </summary>
        /// <param name="bBytes">The bytes to send.</param>
        public override void Send(byte[] bBytes)
        {
            if (bIsRunning)
            {
                lock (qFrameQueue)
                {
                    qFrameQueue.Enqueue(bBytes);
                }
                areWorkToDo.Set();
            }
            else
            {
                throw new InvalidOperationException("Trying to send data while the interface is down is not possible");
            }
        }

        private void MainWorkingLoop()
        {
            byte[] bFrameBytes = null;
            int iCount = 0;
            try
            {
                while (bRun)
                {
                    lock (qFrameQueue)
                    {
                        iCount = qFrameQueue.Count;
                    }

                    while (iCount > 0)
                    {
                        try
                        {
                            lock (qFrameQueue)
                            {
                                bFrameBytes = qFrameQueue.Dequeue();
                            }
                            if (bFrameBytes != null)
                            {
                                lock (oInterfaceStartStopLock)
                                {
                                    wpcDevice.SendPacket(bFrameBytes);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            TrafficDescriptionFrame tdf = new TrafficDescriptionFrame(this, DateTime.Now);
                            tdf.EncapsulatedFrame = new EthernetFrame(bFrameBytes);
                            PushDroppedFrame(tdf);
                            InvokeExceptionThrown(ex);
                        }

                        lock (qFrameQueue)
                        {
                            iCount = qFrameQueue.Count;
                        }
                    }
                    areWorkToDo.WaitOne();
                }
            }
            finally
            {
                lock (qFrameQueue)
                {
                    qFrameQueue.Clear();
                }
            }
        }

        /// <summary>
        /// Stops this interface's processing threads and closes the underlying interface.
        /// </summary>
        public override void Stop()
        {
            if (bIsRunning)
            {
                lock (oInterfaceStartStopLock)
                {
                    bRun = false;
                    CloseDevice();
                    areWorkToDo.Set();
                }
                tWorker.Join();
                tWorker = null;
                base.Stop();
            }
        }

        /// <summary>
        /// Starts this interface's processing threads and opens the underlying interface for sniffing.
        /// </summary>
        public override void Start()
        {
            if (!bIsRunning)
            {
                lock (oInterfaceStartStopLock)
                {
                    base.Start();
                    bRun = true;
                    tWorker = new Thread(MainWorkingLoop);
                    tWorker.Name = "IP Interface Worker Thread (WinPcap, " + this.Description + ")";
                    tWorker.Start();
                    OpenDevice();
                    bIsRunning = true;
                }
            }
        }

        private void OpenDevice()
        {
            if (bExcludeOwnTraffic)
            {
                wpcDevice.OpenDevice(wpcInterface, PcapOpenflags.Promiscuous | PcapOpenflags.Max_Responsiveness | PcapOpenflags.Nocapture_Local);
            }
            else
            {
                wpcDevice.OpenDevice(wpcInterface, PcapOpenflags.Promiscuous | PcapOpenflags.Max_Responsiveness);
            }
            wpcDevice.StartCapture();
            wpcDevice.ExceptionThrown += new ExceptionEventHandler(wpcDevice_ExceptionThrown);
            wpcDevice.BytesCaptured += new WinPcapDotNet.ByteCapturedHandler(wpcDevice_BytesCaptured);
        }

        void wpcDevice_ExceptionThrown(object sender, ExceptionEventArgs args)
        {
            this.InvokeExceptionThrown(args.Exception);
        }

        private void CloseDevice()
        {
            wpcDevice.StopCapture();
            wpcDevice.CloseDevice();
            wpcDevice.BytesCaptured -= new WinPcapDotNet.ByteCapturedHandler(wpcDevice_BytesCaptured);
            wpcDevice.ExceptionThrown -= new ExceptionEventHandler(wpcDevice_ExceptionThrown);
        }

        /// <summary>
        /// Pushes this frame to the output queue and updates the ethernet component of this frame according to the given destination address and interface properties.
        /// </summary>
        /// <param name="fFrame">The frame to send. This frame must contain an IPv4 frame.</param>
        /// <param name="ipaDestination">The next hop's IP address of the given frame</param>
        public override void Send(Frame fFrame, IPAddress ipaDestination)
        {
            EthernetFrame ethFrame = this.GetEthernetFrame(fFrame);
            TrafficDescriptionFrame tdfFrame = (TrafficDescriptionFrame)this.GetFrameByType(fFrame, FrameType.TrafficDescriptionFrame);
            IPFrame ipFrame = GetIPFrame(fFrame);

            if (ipFrame == null)
            {
                throw new InvalidOperationException("Cannot send an non-ip frame via the Send(Frame, IPAddress) method. In this case you have to implement data link handling by overriding this method and use the Send(fFrame) or Send(byte[]) method");
            }

            if (ethFrame == null)
            {
                ethFrame = new EthernetFrame();
            }

            if (tdfFrame == null)
            {
                tdfFrame = new TrafficDescriptionFrame(null, DateTime.Now);
            }

            if (ipaDestination.Equals(IPAddress.Broadcast))
            {
                ethFrame.Destination = new MACAddress(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
            }
            else if (this.arpHostTable.Contains(ipaDestination))
            {
                ethFrame.Destination = arpHostTable.GetEntry(ipaDestination).MAC;
            }
            else
            {
                SendARPRequest(ipaDestination);
                ethFrame.Destination = new MACAddress(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
            }
            if (ipFrame.Version == 4)
            {
                ethFrame.EtherType = EtherType.IPv4;
            }
            else if (ipFrame.Version == 6)
            {
                ethFrame.EtherType = EtherType.IPv6;
            }
            if (!lmacSpoofAdresses.Contains(ethFrame.Destination))
            {
                ethFrame.Source = this.PrimaryMACAddress;
            }
            ethFrame.EncapsulatedFrame = ipFrame;
            tdfFrame.EncapsulatedFrame = ethFrame;

            Send(tdfFrame);
        }


        /// <summary>
        /// Checks for available ARP messages and updates the ARP table.
        /// </summary>
        /// <param name="fInputFrame">A frame to analyze.</param>
        private void UtilitzeARP(Frame fInputFrame)
        {
            ARP.ARPFrame fArpFrame = GetARPFrame(fInputFrame);

            if (fArpFrame != null)
            {
                MACAddress macMacAddress = fArpFrame.SourceMAC;
                IPAddress ipAddress = fArpFrame.SourceIP;

                if (!macMacAddress.IsEmpty) //Check the addresses
                {
                    if (!this.UsesSpoofedAddress(macMacAddress)) //If none of the addresses is contained in the spoof lists
                    {
                        //Use the addresses

                        if (this.ARPTable.Contains(macMacAddress)) //Check MAC and insert
                        {
                            if (this.ARPTable.GetEntry(macMacAddress).IP != ipAddress)
                            {
                                if (this.ARPTable.Contains(macMacAddress))
                                {
                                    this.ARPTable.RemoveHost(macMacAddress);
                                }
                                if (this.ARPTable.Contains(ipAddress))
                                {
                                    this.ARPTable.RemoveHost(ipAddress);
                                }

                                this.ARPTable.AddHost(new eExNetworkLibrary.ARP.ARPHostEntry(macMacAddress, ipAddress));
                            }
                        }
                        else if (this.ARPTable.Contains(ipAddress)) //Check IP and insert
                        {
                            if (this.ARPTable.GetEntry(ipAddress).MAC != macMacAddress)
                            {
                                if (this.ARPTable.Contains(macMacAddress))
                                {
                                    this.ARPTable.RemoveHost(macMacAddress);
                                }
                                if (this.ARPTable.Contains(ipAddress))
                                {
                                    this.ARPTable.RemoveHost(ipAddress);
                                }

                                this.ARPTable.AddHost(new eExNetworkLibrary.ARP.ARPHostEntry(macMacAddress, ipAddress));
                            }
                        }
                        else
                        {
                            this.ARPTable.AddHost(new eExNetworkLibrary.ARP.ARPHostEntry(macMacAddress, ipAddress));
                        }
                    }
                }
            }
        }


        private void SendARPRequest(IPAddress ipaQuery)
        {
            IPAddress[] ipaSources = GetSourceIPsForARPRequest(ipaQuery);
            if (ipaSources.Length > 0)
            {
                foreach (IPAddress ipa in ipaSources)
                {
                    eExNetworkLibrary.ARP.ARPFrame arpRequest = new eExNetworkLibrary.ARP.ARPFrame();
                    arpRequest.SourceIP = ipa;
                    arpRequest.SourceMAC = this.PrimaryMACAddress;
                    arpRequest.DestinationMAC = MACAddress.Empty;
                    arpRequest.HardwareAddressType = HardwareAddressType.Ethernet;
                    arpRequest.DestinationIP = ipaQuery;

                    if (ipa.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        arpRequest.ProtocolAddressType = EtherType.IPv4;
                    }
                    else if (ipa.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                    {
                        arpRequest.ProtocolAddressType = EtherType.IPv6;
                    }

                    EthernetFrame ethFrame = new EthernetFrame();
                    ethFrame.Source = this.PrimaryMACAddress;
                    ethFrame.Destination = new MACAddress(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                    ethFrame.EncapsulatedFrame = arpRequest;
                    ethFrame.EtherType = eExNetworkLibrary.EtherType.ARP;

                    TrafficDescriptionFrame tdf = new TrafficDescriptionFrame(null, DateTime.Now);

                    tdf.EncapsulatedFrame = ethFrame;

                    this.Send(ethFrame);
                }
            }
            else
            {
                throw new ArgumentException("An ARP request could not be send for the following destination and interface, because the interface has no IP addresses assigned for this subnet: " + ipaQuery + ", " + this.Description);
            }
        }

        private IPAddress[] GetSourceIPsForARPRequest(IPAddress ipaQuery)
        {
            List<IPAddress> lAddresses = new List<IPAddress>();

            foreach (IPAddress ipa in this.IpAddresses)
            {
                if (ipa.AddressFamily == ipaQuery.AddressFamily)
                {
                    if (ipaAnalysis.GetClasslessNetworkAddress(ipa, this.GetMaskForAddress(ipa)).Equals(ipaAnalysis.GetClasslessNetworkAddress(ipaQuery, this.GetMaskForAddress(ipa))))
                    {
                        lAddresses.Add(ipa);
                    }
                }
            }

            return lAddresses.ToArray();
        }

        /// <summary>
        /// Pushes this frame to the output qeueue as it is, without changin anything.
        /// </summary>
        /// <param name="fFrame">The frame to send.</param>
        public override void Send(Frame fFrame)
        {
            Send(fFrame.FrameBytes);
        }
    }
}
