using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary.Routing
{
    /// <summary>
    /// This class buildes the base for routing process implementations like RIP or OSPF.
    /// </summary>
    public class RoutingProcess : DirectInterfaceIOHandler
    {
        private List<RoutingEntry> lEntries;
        private IRouter rtRouterToManage;
        private object oRouteLock;

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        public RoutingProcess()
        {
            oRouteLock = new object();
            lEntries = new List<RoutingEntry>();
        }

        /// <summary>
        /// Returns all routing entries owned by this routing process.
        /// </summary>
        protected RoutingEntry[] RoutingEntries
        {
            get { return lEntries.ToArray(); }
        }

        /// <summary>
        /// Gets or sets the router of which the routing tables should be updated. If a router is detached this way, all routes owned by this instance will automatically be removed.
        /// </summary>
        public IRouter RouterToManage
        {
            get 
            {
                IRouter rReturn;
                lock (oRouteLock)
                {
                    rReturn = rtRouterToManage;
                }
                return rReturn; 
            }
            set
            {
                lock (oRouteLock)
                {
                    if (rtRouterToManage != null)
                    {
                        rtRouterToManage.RoutingTable.RouteRemoved -= new RoutingTable.RoutingTableEventHandler(RoutingTable_RouteRemoved);
                        foreach (RoutingEntry re in lEntries.ToArray())
                        {
                            rtRouterToManage.RoutingTable.RemoveRoute(re);
                        }
                    }
                    rtRouterToManage = value;
                    if (rtRouterToManage != null)
                    {
                        foreach (RoutingEntry re in lEntries.ToArray())
                        {
                            rtRouterToManage.RoutingTable.AddRoute(re);
                        }
                        rtRouterToManage.RoutingTable.RouteRemoved += new RoutingTable.RoutingTableEventHandler(RoutingTable_RouteRemoved);
                    }
                }
            }
        }

        void RoutingTable_RouteRemoved(object sender, RoutingTableEventArgs args)
        {
            if (args.Entry.Owner == RoutingEntryOwner.RIP && lEntries.Contains(args.Entry))
            {
                lEntries.Remove(args.Entry);
            }
        }

        /// <summary>
        /// Removes a routing entry from this instance and the router to manage. 
        /// </summary>
        /// <param name="re">The routing entry to remove.</param>
        protected void RemoveEntry(RoutingEntry re)
        {
            lEntries.Remove(re);
            RouterToManage.RoutingTable.RemoveRoute(re);
        }

        /// <summary>
        /// Invokes the routing entry updated event for a specific routing entry.
        /// </summary>
        /// <param name="re">The routing entry which has been updated.</param>
        protected void InvokeEntryUpdated(RoutingEntry re)
        {
            IRouter rRouter = RouterToManage;
            if (RouterToManage != null)
            {
                RouterToManage.RoutingTable.InvokeRouteUpdated(re);
            }
        }

        /// <summary>
        /// Adds a routing entry to this instance and the router to manage. 
        /// </summary>
        /// <param name="re">The routing entry to add</param>
        protected void AddRoutingEntry(RoutingEntry re)
        {
            lEntries.Add(re);
            IRouter rRouter = RouterToManage;
            lock (oRouteLock)
            {
                if (RouterToManage != null)
                {
                    RouterToManage.RoutingTable.AddRoute(re);
                }
            }
        }
    }
}
