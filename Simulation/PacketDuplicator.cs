using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.Ethernet;

namespace eExNetworkLibrary.Simulation
{
    /// <summary>
    /// This class is capable of duplicating packets according to a given probability. 
    /// </summary>
    public class PacketDuplicator : RandomEventTrafficSimulatorItem
    {
        /// <summary>
        /// Duplicates the frame
        /// </summary>
        /// <param name="f">The frame to duplicate</param>
        protected override void CaseHappening(Frame f)
        {
            //Clone the frame
            this.Next.Push(f.Clone());
            this.Next.Push(f);
        }

        /// <summary>
        /// Forwards the frame
        /// </summary>
        /// <param name="f">the frame to forward</param>
        protected override void CaseNotHappening(Frame f)
        {
            this.Next.Push(f); //Simply forward
        }
    }
}
