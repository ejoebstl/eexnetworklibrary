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
using System.Timers;

namespace eExNetworkLibrary.ARP
{
    /// <summary>
    /// This class represents an host table for IP/MAC mappings.
    /// <remarks>This class and all its public members are thread safe.</remarks>
    /// </summary>
    public class HostTable
    {
        private Dictionary<IPAddress, ARPHostEntry> dIPHostTable;
        private Dictionary<MACAddress, ARPHostEntry> dMACHostTable;
        private Timer tTimer;

        /// <summary>
        /// This delegate represents the method used to handle ARP host table event args
        /// </summary>
        /// <param name="sender">The class which rised the event</param>
        /// <param name="args">The event args</param>
        public delegate void ARPHostTableEventHandler(object sender, HostTableEventArgs args);

        /// <summary>
        /// This event is fired when an ARP entry is removed
        /// </summary>
        public event ARPHostTableEventHandler EntryRemoved;

        /// <summary>
        /// This event is fired when an ARP entry is added
        /// </summary>
        public event ARPHostTableEventHandler EntryAdded;

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public HostTable() : this(true)
        {

        }

        /// <summary>
        /// Creates a new instance of this class
        /// <param name="bTimeout">True, if ARP entries should be auto removed when they become invalid, otherwise false.</param>
        /// </summary>
        public HostTable(bool bTimeout)
        {
            dIPHostTable = new Dictionary<IPAddress, ARPHostEntry>();
            dMACHostTable = new Dictionary<MACAddress, ARPHostEntry>();
            if (bTimeout)
            {
                tTimer = new Timer(1000);
                tTimer.Elapsed += new ElapsedEventHandler(tTimer_Elapsed);
                tTimer.Start();
            }
        }

        ~HostTable()
        {
            if (tTimer != null)
            {
                tTimer.Stop();
                tTimer.Dispose();
            }
        }

        void tTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (dMACHostTable)
            {
                lock (dIPHostTable)
                {
                    DateTime dtNow = DateTime.Now;
                    foreach (ARPHostEntry arpEntry in this.GetKnownHosts())
                    {
                        if (arpEntry.IsStatic == false && arpEntry.ValidUtil < dtNow)
                        {
                            dIPHostTable.Remove(arpEntry.IP);
                            InvokeExternalAsync(EntryRemoved, new HostTableEventArgs(arpEntry));
                            dMACHostTable.Remove(arpEntry.MAC);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds a host entry to this host table. This will not overwrite static entries.
        /// </summary>
        /// <param name="arphEntry">The host entry to add.</param>
        public void AddHost(ARPHostEntry arphEntry)
        {
            bool bAdded = false;

            lock (dMACHostTable)
            {
                if (dMACHostTable.ContainsKey(arphEntry.MAC))
                {
                    if (!dMACHostTable[arphEntry.MAC].IsStatic)
                    {
                        InvokeExternalAsync(EntryRemoved, new HostTableEventArgs(dMACHostTable[arphEntry.MAC]));
                        dMACHostTable[arphEntry.MAC] = arphEntry;
                        bAdded = true;
                    }
                }
                else
                {
                    dMACHostTable.Add(arphEntry.MAC, arphEntry);
                    bAdded = true;
                }
            }
            lock (dIPHostTable)
            {
                if (dIPHostTable.ContainsKey(arphEntry.IP))
                {
                    if (!dIPHostTable[arphEntry.IP].IsStatic)
                    {
                        dIPHostTable[arphEntry.IP] = arphEntry;
                        bAdded = true;
                    }
                }
                else
                {
                    dIPHostTable.Add(arphEntry.IP, arphEntry);
                    bAdded = true;
                }
            }
            if (bAdded)
            {
                InvokeExternalAsync(EntryAdded, new HostTableEventArgs(arphEntry));
            }
        }

        /// <summary>
        /// Removes a host associated with a specific IP address
        /// </summary>
        /// <param name="ipaAddress">The IP address to remove the host for.</param>
        public void RemoveHost(IPAddress ipaAddress)
        {
            lock (dMACHostTable)
            {
                lock (dIPHostTable)
                {
                    if (dIPHostTable.ContainsKey(ipaAddress))
                    {
                        dMACHostTable.Remove(dIPHostTable[ipaAddress].MAC);
                        InvokeExternalAsync(EntryRemoved, new HostTableEventArgs(dIPHostTable[ipaAddress]));
                        dIPHostTable.Remove(ipaAddress);
                    }
                }
            }
        }

        /// <summary>
        /// Returns the host entry for a specific IP address
        /// </summary>
        /// <param name="ipa">The IP address to get the host entry for</param>
        /// <returns>The host entry for a specific IP address</returns>
        public ARPHostEntry GetEntry(IPAddress ipa)
        {
            lock (dIPHostTable)
            {
                return dIPHostTable[ipa];
            }
        }

        /// <summary>
        /// Removes a host associated with a specific MAC address
        /// </summary>
        /// <param name="mca">The MAC address to remove the host for.</param>
        public void RemoveHost(MACAddress mca)
        {
            lock (dMACHostTable)
            {
                lock (dIPHostTable)
                {
                    if (dMACHostTable.ContainsKey(mca))
                    {
                        dIPHostTable.Remove(dMACHostTable[mca].IP);
                        InvokeExternalAsync(EntryRemoved, new HostTableEventArgs(dMACHostTable[mca]));
                        dMACHostTable.Remove(mca);
                    }
                }
            }
        }

        /// <summary>
        /// Returns a bool indicating if a specific IP address is known in this host table
        /// </summary>
        /// <param name="ipa">The IP address to search for</param>
        /// <returns>A bool indicating if a specific IP address is known in this host table</returns>
        public bool Contains(IPAddress ipa)
        {
            lock (dIPHostTable)
            {
                return dIPHostTable.ContainsKey(ipa);
            }
        }

        /// <summary>
        /// Returns a bool indicating if a specific MAC address is known in this host table
        /// </summary>
        /// <param name="mca">The MAC address to search for</param>
        /// <returns>A bool indicating if a specific MAC address is known in this host table</returns>
        public bool Contains(MACAddress mca)
        {
            lock (dMACHostTable)
            {
                return dMACHostTable.ContainsKey(mca);
            }
        }

        /// <summary>
        /// Returns the host entry for a specific MAC address
        /// </summary>
        /// <param name="mca">The MAC address to get the host entry for</param>
        /// <returns>The host entry for a specific MAC address</returns>
        public ARPHostEntry GetEntry(MACAddress mca)
        {
            lock (dMACHostTable)
            {
                return dMACHostTable[mca];
            }
        }

        /// <summary>
        /// Clears this host table.
        /// </summary>
        public void Clear()
        {
            lock (dMACHostTable)
            {
                dMACHostTable.Clear();
            }
            lock (dIPHostTable)
            {
                foreach (ARPHostEntry entry in dIPHostTable.Values)
                {
                    InvokeExternalAsync(EntryRemoved, new HostTableEventArgs(entry));
                }
                dIPHostTable.Clear();
            }
        }

        /// <summary>
        /// Returns all hosts known in this host table
        /// </summary>
        /// <returns>All hosts known in this host table</returns>
        public ARPHostEntry[] GetKnownHosts()
        {
            ARPHostEntry[] ipa = new ARPHostEntry[dIPHostTable.Count];
            dIPHostTable.Values.CopyTo(ipa, 0);
            return ipa;
        }

        private void InvokeExternalAsync(Delegate d, object param)
        {
            if (d != null)
            {
                foreach (Delegate dDelgate in d.GetInvocationList())
                {
                    if (dDelgate.Target != null && dDelgate.Target is System.ComponentModel.ISynchronizeInvoke
                        && ((System.ComponentModel.ISynchronizeInvoke)(dDelgate.Target)).InvokeRequired)
                    {
                        ((System.ComponentModel.ISynchronizeInvoke)(dDelgate.Target)).BeginInvoke(dDelgate, new object[] { this, param });
                    }
                    else
                    {
                        dDelgate.DynamicInvoke(this, param);
                    }
                }
            }
        }
    }

    /// <summary>
    /// This class represents some data associated with ARP host table events
    /// </summary>
    public class HostTableEventArgs : EventArgs
    {
        private ARPHostEntry ahEntry;

        /// <summary>
        /// Gets the ARP host entry associated with this event
        /// </summary>
        public ARPHostEntry Entry
        {
            get { return ahEntry; }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="ahEntry">The ARP host entry associated with this event</param>
        public HostTableEventArgs(ARPHostEntry ahEntry)
        {
            this.ahEntry = ahEntry;
        }
    }
}
