using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace eExNetworkLibrary.Routing
{
    /// <summary>
    /// This class represents a routing entry.
    /// </summary>
    public class RoutingEntry
    {
        private IPAddress ipaDestination;
        private IPAddress ipaNextHop;
        private int iMetric;
        private Subnetmask bSubnetMask;
        private IPInterface ipiNextHop;
        private RoutingEntryOwner reoOwner;

        /// <summary>
        /// Gets or sets the owner of this routing entry.
        /// </summary>
        public RoutingEntryOwner Owner
        {
            get { return reoOwner; }
            set { reoOwner = value; }
        }

        /// <summary>
        /// Gets or sets the next hop's interface associated with this router or null, if the next hop interface is not directly known.
        /// </summary>
        public IPInterface NextHopInterface
        {
            get { return ipiNextHop; }
            set { ipiNextHop = value; }
        }

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        public RoutingEntry()
        {
            ipaDestination = IPAddress.Any;
            ipaNextHop = IPAddress.Any;
            iMetric = 0;
            bSubnetMask = new Subnetmask();
            reoOwner = RoutingEntryOwner.Unknown;
        }

        /// <summary>
        /// Creates a new instance of this class with the given properties.
        /// </summary>
        /// <param name="ipaDestination">The destination IP</param>
        /// <param name="ipaNextHop">The next hop's IP</param>
        /// <param name="iMetric">The metric</param>
        /// <param name="bSubnetMask">The subnetmask of the destination</param>
        /// <param name="reoOwner">The owner of this route</param>
        public RoutingEntry(IPAddress ipaDestination, IPAddress ipaNextHop, int iMetric, Subnetmask bSubnetMask, RoutingEntryOwner reoOwner)
        {
            this.ipaDestination = ipaDestination;
            this.ipaNextHop = ipaNextHop;
            this.iMetric = iMetric;
            this.bSubnetMask = bSubnetMask;
            this.reoOwner = reoOwner;
        }

        /// <summary>
        /// Gets or sets the destination IP
        /// </summary>
        public IPAddress Destination
        {
            get { return ipaDestination; }
            set { ipaDestination = value; }
        }

        /// <summary>
        /// Gets or sets the next hop's IP
        /// </summary>
        public IPAddress NextHop
        {
            get { return ipaNextHop; }
            set { ipaNextHop = value; }
        }

        /// <summary>
        /// Gets or sets the metric
        /// </summary>
        public int Metric
        {
            get { return iMetric; }
            set { iMetric = value; }
        }

        /// <summary>
        /// Gets or sets the subnetmask of the destination
        /// </summary>
        public Subnetmask Subnetmask
        {
            get { return bSubnetMask; }
            set { bSubnetMask = value; }
        }

        /// <summary>
        /// Compares whether two routing entries are equal or not. 
        /// </summary>
        /// <param name="obj">The routing entry to compare to this instance.</param>
        /// <returns>A bool indicating whether the two routing entries are equal or not</returns>
        public override bool Equals(object obj)
        {
            if (typeof(RoutingEntry) != obj.GetType())
            {
                return false;
            }
            RoutingEntry re = (RoutingEntry)obj;

            if (re.Metric == this.Metric && re.Subnetmask.Equals(this.Subnetmask) && re.NextHop.Equals(this.NextHop) && re.Destination.Equals(this.Destination) && this.Owner == re.Owner)
            {
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Returns the hash code of this object.
        /// </summary>
        /// <returns>The hash code of this object.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    /// <summary>
    /// Defines the owner protocol of this routing entry.
    /// </summary>
    public enum RoutingEntryOwner
    {
        /// <summary>
        /// Unknown sources
        /// </summary>
        Unknown = -1,
        /// <summary>
        /// Route from the operating system
        /// </summary>
        System = 0,
        /// <summary>
        /// User entered static routes
        /// </summary>
        UserStatic = 1,
        /// <summary>
        /// A direct route to a subnet connected to an interface
        /// </summary>
        Interface = 2,
        /// <summary>
        /// An OSPF route
        /// </summary>
        OSPF = 3,
        /// <summary>
        /// A RIP route
        /// </summary>
        RIP = 4,
        /// <summary>
        /// A BGP route
        /// </summary>
        BGP = 5,
        /// <summary>
        /// An EIGRP route
        /// </summary>
        EIGRP = 6,
        /// <summary>
        /// A route from any other routing protocol
        /// </summary>
        Other = 7
    }
}
