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
using eExNetworkLibrary.Utilities;
using System.Net;
using System.Threading;
using eExNetworkLibrary.Ethernet;
using eExNetworkLibrary.ARP;
using System.Net.NetworkInformation;

namespace eExNetworkLibrary
{
    /// <summary>
    /// The IPInterface class provides an abstract base for interface 
    /// implementations like the Ethernet interface. 
    /// Interfaces are not supposed to use the OutputHandler, 
    /// instead they provide a PacketCaptured event. 
    /// When this event is invoked, each event handler receives a 
    /// separate copy of the captured frame. This means, multiple handlers of the type DirectInterfaceIO
    /// can be attached to an IPInterface. 
    /// </summary>
    public abstract class IPInterface : TrafficHandler
    {
        /// <summary>
        /// All IP addresses of this interface
        /// </summary>
        private List<IPAddress> ipaIpAddresses;
        /// <summary>
        /// All subnetmasks of this interface
        /// </summary>
        private List<Subnetmask> subNetmasks;
        private Dictionary<IPAddress, Subnetmask> dictAddresses;

        /// <summary>
        /// The gateways of this interface - must be set by deriving class in the constructor
        /// </summary>
        protected List<IPAddress> ipStandardgateways;
        /// <summary>
        /// Indicates whether a shutdown is in progress. An interface must immideately stop receiving traffic when this variable is set to true
        /// </summary>
        protected bool bShutdownPending;

        /// <summary>
        /// The adapter type of this interface - must be set by deriving class
        /// </summary>
        protected NetworkInterfaceType aType;

        /// <summary>
        /// This delegate is used to handle address events
        /// </summary>
        /// <param name="sender">The object which fired the event</param>
        /// <param name="args">The event arguments</param>
        public delegate void AddressEventHandler(object sender, AddressEventArgs args);

        /// <summary>
        /// This delegate is used for handling captured frames
        /// </summary>
        /// <param name="fFrame">The captured frame</param>
        /// <param name="sender">The calling object</param>
        public delegate void PacketCapturedHandler(Frame fFrame, object sender);
        
        /// <summary>
        /// This delegate is used for handling captured bytes
        /// </summary>
        /// <param name="bData">The captured bytes</param>
        /// <param name="sender">The calling object</param>
        public delegate void BytesCapturedHandler(byte[] bData, object sender);

        /// <summary>
        /// This event is fired when a packet is captured at this interface
        /// </summary>
        public event PacketCapturedHandler PacketCaptured;

        /// <summary>
        /// This event is fired when bytes are captured at this interface
        /// </summary>
        public event BytesCapturedHandler BytesCaptured;

        /// <summary>
        /// This event is fired when an IPAddress and a Subnetmask are added to this IPInterface
        /// </summary>
        public event AddressEventHandler AddressAdded;

        /// <summary>
        /// This event is fired when an IPAddress and a Subnetmask are removed from this IPInterface
        /// </summary>
        public event AddressEventHandler AddressRemoved;

        /// <summary>
        /// This event is fired when delays are higher than 250 milliseconds.
        /// </summary>
        public event EventHandler DelayWarning; 

        #region Props

        /// <summary>
        /// Gets a bool inidcating whether this interface is online and running
        /// </summary>
        public bool IsUp
        {
            get { return bIsRunning; }
        }

        /// <summary>
        /// Gets this interfaces standard gateways
        /// </summary>
        public IPAddress[] Standardgateways
        {
            get { return ipStandardgateways.ToArray(); }
        }

        /// <summary>
        /// Gets this interfaces subnetmasks
        /// </summary>
        public Subnetmask[] Subnetmasks
        {
            get { return subNetmasks.ToArray(); }
        }

        /// <summary>
        /// Gets this interfaces IPAddresses
        /// </summary>
        public IPAddress[] IpAddresses
        {
            get { return ipaIpAddresses.ToArray(); }
        }

        /// <summary>
        /// Must return this interfaces description
        /// </summary>
        public abstract string Description
        {
            get;
        }

        /// <summary>
        ///  Must return this interfaces name
        /// </summary>
        public abstract string Name
        {
            get;
        }

        /// <summary>
        /// Must return this interfaces DNS name
        /// </summary>
        public abstract string DNSName
        {
            get;
        }

        /// <summary>
        /// Returns the AdapterType of this interface
        /// </summary>
        public NetworkInterfaceType AdapterType
        {
            get { return aType; }
        }

        #endregion

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        protected IPInterface()
        {
            ipaIpAddresses = new List<IPAddress>();
            ipStandardgateways = new List<IPAddress>();
            subNetmasks = new List<Subnetmask>();
            dictAddresses = new Dictionary<IPAddress, Subnetmask>();
        }

        #region OutboundTrafficHandling

        /// <summary>
        /// Pushes this frame to the output queue after updating layer 2 data according to the properties of this interface. 
        /// </summary>
        /// <param name="fFrame">The frame to send.</param>
        /// <param name="ipaDestination">The destination of the given frame</param>
        public abstract void Send(Frame fFrame, IPAddress ipaDestination);

        /// <summary>
        /// Pushes this frame to the output qeueue as it is, without changin anything.
        /// </summary>
        /// <param name="fFrame">The frame to send.</param>
        public abstract void Send(Frame fFrame);
        
        /// <summary>
        /// Pushes the given bytes to the output queue as they are.
        /// </summary>
        /// <param name="bBytes">The bytes to send.</param>
        public abstract void Send(byte[] bBytes);

        #endregion

        /// <summary>
        /// Stops this IPInterface.
        /// </summary>
        public override void Pause()
        {
            Stop();
        }

        /// <summary>
        /// Adds an IP address and the corresponding subnetmask to this interface
        /// </summary>
        /// <param name="ipa">The IPAddress to add</param>
        /// <param name="smMask">The subnetmask to add</param>
        public void AddAddress(IPAddress ipa, Subnetmask smMask)
        {
            dictAddresses.Add(ipa, smMask);
            subNetmasks.Add(smMask);
            ipaIpAddresses.Add(ipa);
            InvokeAddressAdded(new AddressEventArgs(ipa, smMask, this));
        }

        /// <summary>
        /// Returns the subnetmask for an IPAddress associated to this interface
        /// </summary>
        /// <param name="ipa">The IPAddress for which the subnetmask should be searched</param>
        /// <returns>The subnetmask of the given IPAddres</returns>
        public Subnetmask GetMaskForAddress(IPAddress ipa)
        {
            return dictAddresses[ipa];
        }

        /// <summary>
        /// Removes an IPAddress and its corresponding subnetmask from this interface
        /// </summary>
        /// <param name="ipa">The IPAddress to remove</param>
        public void RemoveAddress(IPAddress ipa)
        {
            int iIndex = ipaIpAddresses.IndexOf(ipa);
            ipaIpAddresses.RemoveAt(iIndex);
            Subnetmask smMask = subNetmasks[iIndex];
            subNetmasks.RemoveAt(iIndex);
            dictAddresses.Remove(ipa);
            InvokeAddressRemoved(new AddressEventArgs(ipa, smMask, this));
        }

        #region InboundTrafficHandling

        /// <summary>
        /// Rises the BytesCaptured event
        /// </summary>
        /// <param name="bBuffer">The bytes which were captured</param>
        protected void InvokeBytesCaptured(byte[] bBuffer)
        {
            if (BytesCaptured != null)
            {
                foreach (Delegate dDelgate in BytesCaptured.GetInvocationList())
                {
                    if (dDelgate.Target != null && dDelgate.Target is System.ComponentModel.ISynchronizeInvoke
                        && ((System.ComponentModel.ISynchronizeInvoke)(dDelgate.Target)).InvokeRequired)
                    {
                        ((System.ComponentModel.ISynchronizeInvoke)(dDelgate.Target)).BeginInvoke(dDelgate, new object[] { bBuffer, this });
                    }
                    else
                    {
                        ((BytesCapturedHandler)dDelgate)(bBuffer, this);
                    }
                }
            }
        }


        private void InvokeDelayWarning()
        {
            InvokeExternalAsync(DelayWarning);
        }

        /// <summary>
        /// Rises the PacketCaptured event with the given frame
        /// </summary>
        /// <param name="fFrame">The frame which was captured</param>
        protected void InvokePacketCaptured(Frame fFrame)
        {
            if(PacketCaptured != null)
            {
                foreach (Delegate dDelgate in PacketCaptured.GetInvocationList())
                {
                    if (dDelgate.Target != null && dDelgate.Target is DirectInterfaceIOHandler)
                    {
                        ((DirectInterfaceIOHandler)dDelgate.Target).OnPacketCaptured(fFrame, this);
                    }
                    else if (dDelgate.Target != null && dDelgate.Target is System.ComponentModel.ISynchronizeInvoke
                        && ((System.ComponentModel.ISynchronizeInvoke)(dDelgate.Target)).InvokeRequired)
                    {
                        ((System.ComponentModel.ISynchronizeInvoke)(dDelgate.Target)).BeginInvoke(dDelgate, new object[] { fFrame, this });
                    }
                    else
                    {
                        ((PacketCapturedHandler)dDelgate)(fFrame, this);
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Pushes the given frame to the output queue of the underlying interface without changing the frame.
        /// </summary>
        /// <param name="fInputFrame">The frame to send</param>
        protected override void HandleTraffic(Frame fInputFrame)
        {
            Send(fInputFrame);
        }

        #region Address Events



        private void InvokeAddressRemoved(AddressEventArgs args)
        {
            InvokeExternalAsync(AddressRemoved, args);
        }

        private void InvokeAddressAdded(AddressEventArgs args)
        {
            InvokeExternalAsync(AddressAdded, args);
        }

        #endregion


        /// <summary>
        /// Causes this interface to stop forwarding traffic. 
        /// </summary>
        public override void Cleanup()
        {
            bShutdownPending = true; //Stop to forward traffic
        }

        /// <summary>
        /// Checks wheter a given address is used by this interface
        /// </summary>
        /// <param name="iPAddress">The address to search for</param>
        /// <returns>A bool indicating wheter a given address is used by this interface</returns>
        public bool ContainsAddress(IPAddress iPAddress)
        {
            return ipaIpAddresses.Contains(iPAddress);
        }
    }

    /// <summary>
    /// Represents a EventArgs for address changes
    /// </summary>
    public class AddressEventArgs : EventArgs
    {
        private IPAddress ipa;
        private Subnetmask smMask;
        private IPInterface ipiInterface;

        /// <summary>
        /// Gets the IPAddress
        /// </summary>
        public IPAddress IP
        {
            get { return ipa; }
        }

        /// <summary>
        /// Gets the subnetmask
        /// </summary>
        public Subnetmask Netmask
        {
            get { return smMask; }
        }

        /// <summary>
        /// Gets the interface
        /// </summary>
        public IPInterface Interface
        {
            get { return ipiInterface; }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="ipa">The IPAddress</param>
        /// <param name="smMask">The subnetmask</param>
        /// <param name="ipiInterface">The interface</param>
        public AddressEventArgs(IPAddress ipa, Subnetmask smMask, IPInterface ipiInterface)
        {
            this.ipa = ipa;
            this.smMask = smMask;
            this.ipiInterface = ipiInterface;
        }
    }

    /// <summary>
    /// This class represents an exception occoured during the sending process of an interface
    /// This exception occours on errors during the forwarding process of a frame, e.g. data link or physical errors errors
    /// See the message of the exception for more details
    /// </summary>
    public class InterfaceException : Exception
    {
        /// <summary>
        /// Creates a new instance of this class with the given params
        /// </summary>
        /// <param name="strMessage">The message of this exception</param>
        public InterfaceException(string strMessage) : base(strMessage) { }
        /// <summary>
        /// Creates a new instance of this class with the given params
        /// </summary>
        /// <param name="strMessage">The message of this exception</param>
        /// <param name="exInnerException">The inner exception of this exception</param>
        public InterfaceException(string strMessage, Exception exInnerException) : base(strMessage, exInnerException) { }
    }
}
