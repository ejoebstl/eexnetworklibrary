using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using eExNetworkLibrary.IP;

namespace eExNetworkLibrary.Routing
{
    /// <summary>
    /// This class represents a routing table and has full support of querying routes per destination and metric. 
    /// <remarks>All public members of this class are thread safe.</remarks>
    /// </summary>
    public class RoutingTable
    {
        private List<RoutingEntry> lAllRoutes;

        /// <summary>
        /// This delegate is used to handle routing table changes
        /// </summary>
        /// <param name="sender">The class which rised this event</param>
        /// <param name="args">The arguments</param>
        public delegate void RoutingTableEventHandler(object sender, RoutingTableEventArgs args);
        /// <summary>
        /// This event is rised when a route is added.
        /// </summary>
        public event RoutingTableEventHandler RouteAdded;
        /// <summary>
        /// This event is rised whan a route is removed.
        /// </summary>
        public event RoutingTableEventHandler RouteRemoved;
        /// <summary>
        /// This event is rised when a rout is updated.
        /// The updating class has to rise this event by calling the corresponding method. 
        /// </summary>
        public event RoutingTableEventHandler RouteUpdated;

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        public RoutingTable()
        {
            lAllRoutes = new List<RoutingEntry>();
        }

        /// <summary>
        /// Adds a routing entry to this routing table.
        /// </summary>
        /// <param name="reToAdd">The routing entry to add</param>
        public void AddRoute(RoutingEntry reToAdd)
        {
            lock (lAllRoutes)
            {
                lAllRoutes.Add(reToAdd);
            }
            Invoke(RouteAdded, new RoutingTableEventArgs(reToAdd, this));
        }

        /// <summary>
        /// Removes a routing entry from this routing table.
        /// </summary>
        /// <param name="reToRemove">The routing entry to remove</param>
        public void RemoveRoute(RoutingEntry reToRemove)
        {
            lock (lAllRoutes)
            {
                lAllRoutes.Remove(reToRemove);
            }
            Invoke(RouteRemoved, new RoutingTableEventArgs(reToRemove, this));
        }

        /// <summary>
        /// Gets the best match route with the lowest metric to the given destination.
        /// </summary>
        /// <param name="ipa">The destination to search the route for.</param>
        /// <returns>The best route to the destination, or null if no route is found.</returns>
        public RoutingEntry GetRouteToDestination(IPAddress ipa)
        {
            int iMetric = int.MaxValue;
            uint iMask = 0;
            int iMaskFav = 0;
            RoutingEntry reFavourite = null;
            lock (lAllRoutes)
            {
                foreach (RoutingEntry re in lAllRoutes)
                {
                    if (ipa.AddressFamily == re.Destination.AddressFamily &&
                        IPAddressAnalysis.GetClasslessNetworkAddress(re.Destination, re.Subnetmask).Equals(IPAddressAnalysis.GetClasslessNetworkAddress(ipa, re.Subnetmask)))
                    {
                        if (iMask > iMaskFav)
                        {
                            iMetric = re.Metric;
                            reFavourite = re;
                            iMaskFav = reFavourite.Subnetmask.PrefixLength;
                        }
                        else if (re.Metric < iMetric && iMask == iMaskFav)
                        {
                            iMetric = re.Metric;
                            reFavourite = re;
                            iMaskFav = reFavourite.Subnetmask.PrefixLength;
                        }
                    }
                }
            }

            return reFavourite;
        }

        private int SubnetmaskToInt(Subnetmask seMask)
        {
            byte[] bMaskBytes = seMask.MaskBytes;

            return BitConverter.ToInt32(bMaskBytes, 0);
        }

        /// <summary>
        /// Returns a bool indicating whether this routing table contains a specific entry
        /// </summary>
        /// <param name="reEntry">The entry to search for</param>
        /// <returns>A bool indicating whether this routing table contains a specific entry</returns>
        public bool ContainsEntry(RoutingEntry reEntry)
        {
            lock (lAllRoutes)
            {
                return lAllRoutes.Contains(reEntry);
            }
        }

        /// <summary>
        /// Gets all matching routes for a destination.
        /// </summary>
        /// <param name="ipa">The destination to get the routes for.</param>
        /// <returns>An array filled with all routes for the given destination.</returns>
        public RoutingEntry[] GetRoutes(IPAddress ipa)
        {
            List<RoutingEntry> lRe = new List<RoutingEntry>();
            lock (lAllRoutes)
            {
                foreach (RoutingEntry re in lAllRoutes)
                {
                    if (ipa.AddressFamily == re.Destination.AddressFamily &&
                        IPAddressAnalysis.GetClasslessNetworkAddress(re.Destination, re.Subnetmask).Equals(IPAddressAnalysis.GetClasslessNetworkAddress(ipa, re.Subnetmask)))
                    {
                        lRe.Add(re);
                    }
                }
            }

            return lRe.ToArray();
        }

        /// <summary>
        /// Gets all routes.
        /// </summary>
        /// <returns>All routes in this routing table.</returns>
        public RoutingEntry[] GetRoutes()
        {
            lock (lAllRoutes)
            {
                return lAllRoutes.ToArray();
            }
        }

        /// <summary>
        /// Clears all routes from this routing table.
        /// </summary>
        public void Clear()
        {
            lock (lAllRoutes)
            {
                lAllRoutes.Clear();
            }
        }

        /// <summary>
        /// Rises the RouteUpdated event. If a class changes a routing entry, it has to rise this event immediatly after changing the routing entry.
        /// </summary>
        /// <param name="re">The changed routing entry.</param>
        public void InvokeRouteUpdated(RoutingEntry re)
        {
            this.Invoke(RouteUpdated, new RoutingTableEventArgs(re, this));
        }

        /// <summary>
        /// Invokes a delegate asynchronously.
        /// </summary>
        /// <param name="d">The delegate to invoke</param>
        /// <param name="param">The parameters</param>
        protected void Invoke(Delegate d, object param)
        {
            if (d != null)
            {
                foreach (Delegate dDelgate in d.GetInvocationList())
                {
                    if (dDelgate.Target != null && dDelgate.Target.GetType().GetInterface(typeof(System.ComponentModel.ISynchronizeInvoke).Name, true) != null
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
    /// This class represents a simple class to store information about routing table events.
    /// </summary>
    public class RoutingTableEventArgs : EventArgs
    {
        private RoutingEntry reEntry;
        private RoutingTable rtOwner;

        /// <summary>
        /// Gets or sets the routing table which owned the route.
        /// </summary>
        public RoutingTable Owner
        {
            get { return rtOwner; }
            set { rtOwner = value; }
        }

        /// <summary>
        /// Gets or sets the route.
        /// </summary>
        public RoutingEntry Entry
        {
            get { return reEntry; }
            set { reEntry = value; }
        }

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="reEntry">The routing entry</param>
        /// <param name="rtOwner">The routing table owning the routing entry</param>
        public RoutingTableEventArgs(RoutingEntry reEntry, RoutingTable rtOwner)
        {
            this.reEntry = reEntry;
            this.rtOwner = rtOwner;
        }
    }
}
