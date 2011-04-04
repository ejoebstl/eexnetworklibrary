using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary.Routing
{
    /// <summary>
    /// Provides an interface from which all routers must derive.
    /// </summary>
    public interface IRouter
    {
        /// <summary>
        /// Gets the routing table of a router.
        /// </summary>
        RoutingTable RoutingTable { get; }
        /// <summary>
        /// Gets the name of a router.
        /// </summary>
        string Name { get; }
    }
}
