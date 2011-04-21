using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.ComponentModel;
using eExNetworkLibrary.Threading;
using System.Net;
using eExNetworkLibrary.Utilities;
using eExNetworkLibrary.Monitoring;
using eExNetworkLibrary.IP;
using eExNetworkLibrary.Ethernet;

namespace eExNetworkLibrary.Routing
{
    /// <summary>
    /// This class represents a router which routes packets to the correct connected interface.
    /// </summary>
    public class Router : DirectInterfaceIOHandler, IRouter
    {
        private List<TrafficAnalyzer> lRoutedTrafficAnalyzer;
        private RoutingTable rtRoutingtable;
        private int iRoutedPackets;
        private bool bExcludeMulticast;

        /// <summary>
        /// Gets or sets the count of overall routed packets.
        /// </summary>
        public int RoutedPackets
        {
            get { return iRoutedPackets; }
        }

        #region RoutingAnalyzer

        /// <summary>
        /// Returns a bool indicating whether this router contains a specific traffic analyzer for its routed traffic.
        /// </summary>
        /// <param name="taAnalyzer">The traffic analyzer to search for.</param>
        /// <returns>A bool indicating whether this router contains a specific traffic analyzer for its routed traffic</returns>
        public bool ContainsRoutedTrafficAnalyzer(TrafficAnalyzer taAnalyzer)
        {
            return lRoutedTrafficAnalyzer.Contains(taAnalyzer);
        }

        /// <summary>
        /// Adds a traffic analyzer to this router, which will analyze the traffic routed by this router.
        /// </summary>
        /// <param name="taAnalyzer">The traffic analyzer to attach.</param>
        public void AddRoutingTrafficAnalyzer(TrafficAnalyzer taAnalyzer)
        {
            lRoutedTrafficAnalyzer.Add(taAnalyzer);
        }

        /// <summary>
        /// Removes a routing traffic analyzer from this router.
        /// </summary>
        /// <param name="taAnalyzer">The traffic analyzer to remove.</param>
        public void RemoveRoutingTrafficAnalyzer(TrafficAnalyzer taAnalyzer)
        {
            lRoutedTrafficAnalyzer.Remove(taAnalyzer);
        }

        /// <summary>
        /// Returns all connected routed traffic analyzers
        /// </summary>
        /// <returns>All connected routed traffic analyzers</returns>
        public TrafficAnalyzer[] GetRoutedTrafficAnalyzers()
        {
            return lRoutedTrafficAnalyzer.ToArray();
        }

        #endregion

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public Router()
            : this(true)
        {

        }

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="bInsertOSRoutesOnStrartup">A bool indicating whether routes from the operating system should be imported on startup.</param>
        public Router(bool bInsertOSRoutesOnStrartup)
        {
            lRoutedTrafficAnalyzer = new List<TrafficAnalyzer>();
            rtRoutingtable = new RoutingTable();
            iRoutedPackets = 0;
            bExcludeMulticast = true;
            if (bInsertOSRoutesOnStrartup)
            {
                RoutingEntry[] arEntries = SystemRouteQuery.GetOSRoutes();

                foreach (RoutingEntry re in arEntries)
                {
                    this.rtRoutingtable.AddRoute(re);
                }
            }
            
            
        }

        /// <summary>
        /// Gets the routing table of this router.
        /// </summary>
        public RoutingTable RoutingTable
        {
            get { return rtRoutingtable; }
        }

        /// <summary>
        /// Stops this router and clears its routingtable
        /// </summary>
        public override void Stop()
        {
            if (bIsRunning)
            {
                rtRoutingtable.Clear();
                base.Stop();
            }
        }

        /// <summary>
        /// Starts this router
        /// </summary>
        public override void Start()
        {
            if (!bIsRunning)
            {
                base.Start();
            }
        }

        /// <summary>
        /// Stops the routing process and shuts down all interfaces.
        /// </summary>
        public void ShutdownRouter()
        {
            Stop();
            foreach (IPInterface iInterface in lInterfaces)
            {
                iInterface.Stop();
            }
        }

        /// <summary>
        /// Checks whether the given frame has to be routed and routes it to it's destination.
        /// </summary>
        /// <param name="fInputFrame">The frame to route.</param>
        protected override void HandleTraffic(Frame fInputFrame)
        {
            if(IsMulticast(fInputFrame) && bExcludeMulticast)
            {
                iDroppedPackets++;
                PushDroppedFrame(fInputFrame);
            }
            else if (RoutingNeeded(fInputFrame))
            {
                RouteFrame(fInputFrame);
            }
        }

        private bool IsMulticast(Frame fInputFrame)
        {
            IPFrame ipFrame = GetIPFrame(fInputFrame);

            if (ipFrame != null)
            {
                byte bFirstByte = ipFrame.DestinationAddress.GetAddressBytes()[0];

                if (ipFrame.Version == 6 && bFirstByte == 0xFF)
                {
                    return true;
                }
                if (ipFrame.Version == 4 && bFirstByte >= 224 && bFirstByte <= 239)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Adds an interface to this router.
        /// </summary>
        /// <param name="ipi">The IPInterface to add.</param>
        public override void AddInterface(IPInterface ipi)
        {
            for (int iC1 = 0; iC1 < ipi.IpAddresses.Length && iC1 < ipi.Subnetmasks.Length; iC1++)
            {
                RoutingEntry re = new RoutingEntry(ipi.IpAddresses[iC1], IPAddressAnalysis.GetClasslessNetworkAddress(ipi.IpAddresses[iC1], ipi.Subnetmasks[iC1]), 0, ipi.Subnetmasks[iC1], RoutingEntryOwner.Interface);
                re.NextHopInterface = ipi;
                this.rtRoutingtable.AddRoute(re);
            }
            ipi.AddressAdded += new IPInterface.AddressEventHandler(ipi_AddressAdded);
            ipi.AddressRemoved += new IPInterface.AddressEventHandler(ipi_AddressRemoved);
            base.AddInterface(ipi);
        }

        void ipi_AddressRemoved(object sender, AddressEventArgs args)
        {
            RoutingEntry reEntry = new RoutingEntry(args.IP, args.IP, 0, args.Netmask, RoutingEntryOwner.Interface);
            reEntry.NextHopInterface = args.Interface;
            rtRoutingtable.RemoveRoute(reEntry);
            lLocalAdresses.Remove(args.IP);
        }

        void ipi_AddressAdded(object sender, AddressEventArgs args)
        {
            lLocalAdresses.Add(args.IP);
            RoutingEntry reEntry = new RoutingEntry(args.IP, args.IP, 0, args.Netmask, RoutingEntryOwner.Interface);
            reEntry.NextHopInterface = args.Interface;
            rtRoutingtable.AddRoute(reEntry);
        }

        /// <summary>
        /// Removes an interface from this router.
        /// </summary>
        /// <param name="ipi">The IPInterface to remove.</param>
        public override void RemoveInterface(IPInterface ipi)
        {
            for (int iC1 = 0; iC1 < ipi.IpAddresses.Length && iC1 < ipi.Subnetmasks.Length; iC1++)
            {
                RoutingEntry re = new RoutingEntry(ipi.IpAddresses[0], ipi.IpAddresses[0], 0, ipi.Subnetmasks[0], RoutingEntryOwner.Interface);
                re.NextHopInterface = ipi;
                this.rtRoutingtable.RemoveRoute(re);
            }
            ipi.AddressAdded -= new IPInterface.AddressEventHandler(ipi_AddressAdded);
            ipi.AddressRemoved -= new IPInterface.AddressEventHandler(ipi_AddressRemoved);
            base.RemoveInterface(ipi);
        }

        private bool RoutingNeeded(Frame fInputFrame)
        {
            IP.IPFrame fIpFrame = GetIPFrame(fInputFrame);

            if(fIpFrame == null)
            {
                return false;
            }
            else if (lLocalAdresses.Contains(fIpFrame.DestinationAddress))
            {
                return false;
            }

            return true;
        }

        private void RouteFrame(Frame fInputFrame)
        {
            IPFrame ipFrame = GetIPFrame(fInputFrame);

            IPInterface ipintOutInt = null;
            IPAddress ipaDestination = null;
            IPAddress ipaNextHop = null;

            if (ipFrame != null) // If it is an IP frame
            {
                ipaDestination = ipFrame.DestinationAddress;

                if (IsInLocalSubnet(ipaDestination)) // If destination is in the local subnet
                {
                    #region local subnet routing
                    ipintOutInt = GetInterfaceForIPSubnet(ipaDestination);
                    if (ipintOutInt != null)
                    {
                        ipintOutInt.Send(fInputFrame, ipaDestination);
                        iRoutedPackets++;
                        InvokeInterfaceFramePushed();
                        PushRoutedFrame(fInputFrame);
                    }
                    else
                    {
                        //*FRAMEDROP/Interface error*
                        iDroppedPackets++;
                        PushDroppedFrame(fInputFrame);
                        throw new RoutingException("The destination is known to be in a local subnet, but no valid interface was available for routing (" + ipaDestination + ").");
                    }
                    #endregion
                }
                else // Routing
                {
                    #region routing
                    RoutingEntry reEntry = rtRoutingtable.GetRouteToDestination(ipaDestination); //Lookup
                    if (reEntry != null)
                    {
                        ipintOutInt = reEntry.NextHopInterface;
                        ipaNextHop = reEntry.NextHop;
                    }

                    while (reEntry != null && ipintOutInt == null) //Iteriere durch tabelle
                    {
                        reEntry = rtRoutingtable.GetRouteToDestination(reEntry.NextHop);
                        if (reEntry.Owner == RoutingEntryOwner.Interface)
                        {
                            ipintOutInt = reEntry.NextHopInterface;
                        }
                        else
                        {
                            ipaNextHop = reEntry.NextHop;
                        }
                    }

                    if (ipintOutInt != null && ipaNextHop != null)
                    {
                        ipintOutInt.Send(fInputFrame, ipaNextHop);
                        iRoutedPackets++;
                        InvokeInterfaceFramePushed();
                        PushRoutedFrame(fInputFrame);
                    }
                    else
                    {
                        //*FRAMEDROP/Route error*
                        iDroppedPackets++;
                        PushDroppedFrame(fInputFrame);
                        throw new RoutingException("No route is known for the given destination (" + ipaDestination + ").");
                    }
                    #endregion
                }
            }
        }

        private bool IsInLocalSubnet(IPAddress ipaDestination)
        {
            RoutingEntry re = rtRoutingtable.GetRouteToDestination(ipaDestination);
            return re != null && re.Owner == RoutingEntryOwner.Interface;
        }

        private IPInterface GetInterfaceForIPSubnet(IPAddress ipaDestination)
        {
            RoutingEntry re = rtRoutingtable.GetRouteToDestination(ipaDestination);
            if (re != null && re.Owner == RoutingEntryOwner.Interface)
            {
                return re.NextHopInterface;
            }
            return null;
        }

        /// <summary>
        /// Pushes a routed frame to the connected routed traffic analyzers.
        /// </summary>
        /// <param name="fFrame">The frame to push.</param>
        protected void PushRoutedFrame(Frame fFrame)
        {
            foreach (TrafficAnalyzer ta in lRoutedTrafficAnalyzer)
            {
                ta.PushTraffic(fFrame);
            }
        }

        /// <summary>
        /// Gets the name of this routing instance. 
        /// </summary>
        public string Name
        {
            get { return base.Name; }
        }

        public bool DropMulticastFrames
        {
            get { return bExcludeMulticast; }
            set { bExcludeMulticast = value; }
        }
    }

    /// <summary>
    /// This class represents an exception occoured during the routing process
    /// This excpetion occours on errors during the forwarding process of a frame, e.g. no route or no ARP entry for the destination.
    /// See the message of the exception for more details
    /// </summary>
    public class RoutingException : Exception
    {
        /// <summary>
        /// Creates a new instance of this class with the given params
        /// </summary>
        /// <param name="strMessage">The message of this exception</param>
        public RoutingException(string strMessage) : base(strMessage) { }
        /// <summary>
        /// Creates a new instance of this class with the given params
        /// </summary>
        /// <param name="strMessage">The message of this exception</param>
        /// <param name="exInnerException">The inner exception of this exception</param>
        public RoutingException(string strMessage, Exception exInnerException) : base(strMessage, exInnerException) { }
    }
}
