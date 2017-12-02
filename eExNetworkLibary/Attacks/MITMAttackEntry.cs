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
using eExNetworkLibrary.ARP;
using System.Net;
using eExNetworkLibrary.IP;

namespace eExNetworkLibrary.Attacks
{
    /// <summary>
    /// This class represents attack targets for man in the middle attacks.
    /// </summary>
    public class MITMAttackEntry
    {
        private IPAddress arphVictimBob;
        private IPAddress arphVictimAlice;

        private bool bIsRoutingFromAliceToBob;
        private bool bIsRoutingFromBobToAlice;

        /// <summary>
        /// Gets or sets the IP address of the first victim
        /// </summary>
        public IPAddress VictimBob
        {
            get { return arphVictimBob; }
        }

        /// <summary>
        /// Gets or sets the IP address of the second victim
        /// </summary>
        public IPAddress VictimAlice
        {
            get { return arphVictimAlice; }
        }

        /// <summary>
        /// Creates a new instance of this class with the given params
        /// </summary>
        /// <param name="arphVictimBoB">The IP address of the first victim</param>
        /// <param name="arphVictimAlice">The IP address of the second victim</param>
        public MITMAttackEntry(IPAddress arphVictimBob, IPAddress arphVictimAlice)
        {
            if (arphVictimAlice.AddressFamily != arphVictimBob.AddressFamily)
            {
                throw new ArgumentException("Cannot mix up diffrent types of addresses.");
            }
            this.arphVictimBob = arphVictimBob;
            this.arphVictimAlice = arphVictimAlice;
        }

        /// <summary>
        /// Gets or sets a bool indicating whether routing from the first victim to the second victim is done
        /// </summary>
        public bool IsRoutingFromAliceToBob
        {
            get { return bIsRoutingFromAliceToBob; }
            set { bIsRoutingFromAliceToBob = value; }
        }

        /// <summary>
        /// Gets or sets a bool indicating whether routing from the second victim to the first victim is done
        /// </summary>
        public bool IsRoutingFromBobToAlice
        {
            get { return bIsRoutingFromBobToAlice; }
            set { bIsRoutingFromBobToAlice = value; }
        }

        /// <summary>
        /// Gets a bool indicating whether full routing is done
        /// </summary>
        public bool IsFullRouting
        {
            get { return bIsRoutingFromAliceToBob && bIsRoutingFromBobToAlice; }
        }

        /// <summary>
        /// Returns a bool indicating whether an object equals this instance
        /// </summary>
        /// <param name="obj">The object to compare to this instance</param>
        /// <returns>A bool indicating whether an object equals this instance</returns>
        public override bool Equals(object obj)
        {
            if (obj is MITMAttackEntry)
            {
                MITMAttackEntry comp = (MITMAttackEntry)obj;

                return this.arphVictimAlice.Equals(comp.arphVictimAlice) && this.arphVictimBob.Equals(comp.arphVictimBob);
            }
            return false;
        }

        /// <summary>
        /// Returns the hash code of this instance
        /// </summary>
        /// <returns>The hash code of this instance</returns>
        public override int GetHashCode()
        {
            int iIPBob = BitConverter.ToInt32(arphVictimBob.GetAddressBytes(), 0);
            int iIPAlice = BitConverter.ToInt32(arphVictimAlice.GetAddressBytes(), 0);
            return iIPAlice ^ iIPBob;
        }
    }
}
