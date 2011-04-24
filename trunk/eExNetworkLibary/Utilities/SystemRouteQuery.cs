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
using eExNetworkLibrary.Routing;
using System.Management;
using System.Net;

namespace eExNetworkLibrary.Utilities
{
    /// <summary>
    /// This class is capable of getting the system's routing table.
    /// </summary>
    public class SystemRouteQuery
    {
        /// <summary>
        /// Returns all routes from the operating system
        /// </summary>
        /// <returns>All routes from the operating system</returns>
        public static RoutingEntry[] GetOSRoutes()
        {
            List<RoutingEntry> lReEntry = new List<RoutingEntry>(); 
            
            try
            {
                ManagementClass mcConf = new ManagementClass("Win32_IP4RouteTable");

                ManagementObjectCollection acConfs = mcConf.GetInstances();

                foreach (ManagementObject moObject in acConfs)
                {
                    lReEntry.Add(new RoutingEntry(IPAddress.Parse((string)moObject["Destination"]), IPAddress.Parse((string)moObject["NextHop"]), (int)moObject["Metric1"], Subnetmask.Parse((string)moObject["Mask"]), RoutingEntryOwner.System)); 
                }
            }
            catch (Exception) { }

            return lReEntry.ToArray();
        }
    }
}
