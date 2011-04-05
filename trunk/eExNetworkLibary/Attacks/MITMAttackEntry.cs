using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.ARP;
using System.Net;

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
            set { arphVictimBob = value; }
        }

        /// <summary>
        /// Gets or sets the IP address of the second victim
        /// </summary>
        public IPAddress VictimAlice
        {
            get { return arphVictimAlice; }
            set { arphVictimAlice = value; }
        }

        /// <summary>
        /// Creates a new instance of this class with the given params
        /// </summary>
        /// <param name="arphVictimBoB">The IP address of the first victim</param>
        /// <param name="arphVictimAlice">The IP address of the second victim</param>
        public MITMAttackEntry(IPAddress arphVictimBoB, IPAddress arphVictimAlice)
        {
            this.arphVictimBob = arphVictimBoB;
            this.arphVictimAlice = arphVictimAlice;
        }

        /// <summary>
        /// Creates a new instance of this class 
        /// </summary>
        public MITMAttackEntry()
        {
            this.arphVictimBob = IPAddress.Any;
            this.arphVictimAlice = IPAddress.Any;
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
