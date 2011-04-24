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
using System.Threading;
using eExNetworkLibrary.Threading;
using System.Net;
using System.ComponentModel;
using eExNetworkLibrary.IP;

namespace eExNetworkLibrary
{
    /// <summary>
    /// This class represents a traffic handler which is capable of
    /// receiving and forwarding traffic directly from an interface and to other 
    /// traffic handlers.
    /// </summary>
    public class DirectInterfaceIOHandler : TrafficHandler
    {
        /// <summary>
        /// A list containing all associated interfaces
        /// </summary>
        protected List<IPInterface> lInterfaces;
        /// <summary>
        /// A list containing all IPAddresses of all associated interfaces
        /// </summary>
        protected List<IPAddress> lLocalAdresses;
        /// <summary>
        /// A counter counting all dropped packets
        /// </summary>
        protected int iDroppedPackets;
        /// <summary>
        /// A conter counting all received packets
        /// </summary>
        protected int iReceivedPackets;

        /// <summary>
        /// This event is fired, when a frame is pushed to the associated interface
        /// </summary>
        public event EventHandler InterfaceFramePushed;
        /// <summary>
        /// This event is firead when a frame is received from the associated interface
        /// </summary>
        public event EventHandler InterfaceFrameReceived;

        /// <summary>
        /// Gets the count of dropped packets
        /// </summary>
        public int DroppedPackets
        {
            get { return iDroppedPackets; }
        }

        /// <summary>
        /// Gets the count of received packets
        /// </summary>
        public int ReceivedPackets
        {
            get { return iReceivedPackets; }
        }

        /// <summary>
        /// Returns a bool indicating whether an IPAddress is used by one of the connected interfaces
        /// </summary>
        /// <param name="ipa">The IPAddress to search for</param>
        /// <returns>A bool indicating whether an IPAddress is used by one of the connected interfaces</returns>
        public bool ContainsLocalAddress(IPAddress ipa)
        {
            return lLocalAdresses.Contains(ipa);
        }

        /// <summary>
        /// Returns all addresses used in connected interfaces
        /// </summary>
        /// <returns>All addresses used in connected interfaces</returns>
        public IPAddress[] GetLocalAdresses()
        {
            return lLocalAdresses.ToArray();
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public DirectInterfaceIOHandler()
        {
            lInterfaces = new List<IPInterface>();
            lLocalAdresses = new List<IPAddress>();
            iReceivedPackets = 0;
            iDroppedPackets = 0;
            iReceivedPackets = 0;
        }

        /// <summary>
        /// Returns all connected interfaces.
        /// </summary>
        /// <returns>All connected interfaces</returns>
        public IPInterface[] GetInterfaces()
        {
            return lInterfaces.ToArray();
        }

        /// <summary>
        /// Connects an interface
        /// </summary>
        /// <param name="ipInterface">The interface to connect</param>
        public virtual void AddInterface(IPInterface ipInterface)
        {
            ipInterface.PacketCaptured += new eExNetworkLibrary.IPInterface.PacketCapturedHandler(ipInterface_PacketCaptured);
            lInterfaces.Add(ipInterface);
            ipInterface.AddressAdded += new IPInterface.AddressEventHandler(ipInterface_AddressAdded);
            ipInterface.AddressRemoved += new IPInterface.AddressEventHandler(ipInterface_AddressRemoved);
            if (!ipInterface.IsRunning)
            {
                ipInterface.Start();
            }
            for (int iC1 = 0; iC1 < ipInterface.IpAddresses.Length && iC1 < ipInterface.Subnetmasks.Length; iC1++)
            {
                lLocalAdresses.Add(ipInterface.IpAddresses[iC1]);
            }
        }

        /// <summary>
        /// Returns all IPInterfaces connected with this DirectInterfaceIO and subnets matching the given address.
        /// </summary>
        /// <param name="ipaAddress">The address to search a match for.</param>
        /// <returns>All IPInterfaces with subnets matching the given address</returns>
        protected IPInterface[] GetInterfacesForAddress(IPAddress ipaAddress)
        {
            List<IPInterface> lReturnInterfaces = new List<IPInterface>();
            foreach (IPInterface ipi in lInterfaces)
            {
                for (int iC1 = 0; iC1 < ipi.IpAddresses.Length && iC1 < ipi.Subnetmasks.Length; iC1++)
                {
                    IPAddress[] ipaAddresses = ipi.IpAddresses;
                    Subnetmask[] smMasks = ipi.Subnetmasks;
                    if (ipaAddresses[iC1].AddressFamily == ipaAddress.AddressFamily &&
                        IPAddressAnalysis.GetClasslessNetworkAddress(ipaAddresses[iC1], smMasks[iC1]).Equals(IPAddressAnalysis.GetClasslessNetworkAddress(ipaAddress, smMasks[iC1])))
                    {
                        lReturnInterfaces.Add(ipi);
                    }
                }
            }
            return lReturnInterfaces.ToArray();
        }

        void ipInterface_AddressRemoved(object sender, AddressEventArgs args)
        {
            lLocalAdresses.Remove(args.IP);
        }

        void ipInterface_AddressAdded(object sender, AddressEventArgs args)
        {
            if (lLocalAdresses.Contains(args.IP))
            {
                lLocalAdresses.Remove(args.IP);
            }
        }

        /// <summary>
        /// Returns a bool indicating if a specific interface is associated with this direct interface IO handler
        /// </summary>
        /// <param name="ipInterface">The interface to search for</param>
        /// <returns>A bool indicating if a specific interface is associated with this direct interface IO handler</returns>
        public bool ContainsInterface(IPInterface ipInterface)
        {
            return lInterfaces.Contains(ipInterface);
        }

        /// <summary>
        /// Removes an interface
        /// </summary>
        /// <param name="ipInterface">The interface to remove</param>
        public virtual void RemoveInterface(IPInterface ipInterface)
        {
            ipInterface.PacketCaptured -= new eExNetworkLibrary.IPInterface.PacketCapturedHandler(ipInterface_PacketCaptured);
            lInterfaces.Remove(ipInterface);
            ipInterface.AddressAdded -= new IPInterface.AddressEventHandler(ipInterface_AddressAdded);
            ipInterface.AddressRemoved -= new IPInterface.AddressEventHandler(ipInterface_AddressRemoved);

            for (int iC1 = 0; iC1 < ipInterface.IpAddresses.Length && iC1 < ipInterface.Subnetmasks.Length; iC1++)
            {
                lLocalAdresses.Remove(ipInterface.IpAddresses[iC1]);
            }
        }

        void ipInterface_PacketCaptured(Frame fFrame, object sender)
        {
            InvokeInterfaceFrameReceived();
            iReceivedPackets++;

            if (OutputHandler != null)
            {
                NotifyNext(fFrame);
            }
        }

        /// <summary>
        /// Stops this handlers worker threads
        /// </summary>
        public override void Stop()
        {
            base.Stop();
        }

        /// <summary>
        /// Starts this handlers worker threads
        /// </summary>
        public override void Start()
        {
            base.Start();
        }

        /// <summary>
        /// Stops this handlers worker threads
        /// </summary>
        public override void Pause()
        {
            Stop();
        }

        /// <summary>
        /// Sends the given frame out to all connected interfaces without changing it.
        /// </summary>
        /// <param name="fInputFrame">The frame to send</param>
        protected override void HandleTraffic(Frame fInputFrame)
        {
           foreach(IPInterface ipi in lInterfaces)
           {
               ipi.Send(fInputFrame);
           }
           InvokeInterfaceFramePushed();
        }
     
        /// <summary>
        /// Rises the FrameReceived event.
        /// </summary>
        protected void InvokeInterfaceFrameReceived()
        {
            InvokeExternalAsync(InterfaceFrameReceived);
        }

        /// <summary>
        /// Rises the FramePushed event.
        /// </summary>
        protected void InvokeInterfaceFramePushed()
        {
            InvokeExternalAsync(InterfaceFramePushed);
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public override void Cleanup()
        {
            //Don't need to do anything on init shutdown. 
        }
    }
}
