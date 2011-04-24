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

namespace eExNetworkLibrary.Simulation
{
    /// <summary>
    /// This class represents a WANEmulator derived from Simulator. This class is capable of constraining speed, delaying the traffic, corrupting the traffic and more.<br />
    /// Please do not try to route corrupted traffic - protocol parsing will fail. Instead, use the WAN emulator between two DirectInterfaceIO Handlers.
    /// </summary>
    public class WANEmulator : Simulator
    {
        private SpeedConstrainer scSpeedConstrainer;
        private PacketDropper pdPacketDropper;
        private PacketDuplicator pdPacketDuplicator;
        private PacketReorderer pdPacketReorderer;
        private ByteFlipper bfByteFlipper;
        private DelayJitter tjTimeJitter;

        /// <summary>
        /// Gets the speed constrainer of this instance.
        /// </summary>
        public SpeedConstrainer SpeedConstrainer
        {
            get { return scSpeedConstrainer; }
        }

        /// <summary>
        /// Gets the packet dropper of this instance.
        /// </summary>
        public PacketDropper PacketDropper
        {
            get { return pdPacketDropper; }
        }

        /// <summary>
        /// Gets the packet duplicator of this instance.
        /// </summary>
        public PacketDuplicator PacketDuplicator
        {
            get { return pdPacketDuplicator; }
        }

        /// <summary>
        /// Gets the packet reorderer of this instance.
        /// </summary>
        public PacketReorderer PacketReorderer
        {
            get { return pdPacketReorderer; }
        }

        /// <summary>
        /// Gets the byte flipper of this instance.
        /// </summary>
        public ByteFlipper ByteFlipper
        {
            get { return bfByteFlipper; }
        }

        /// <summary>
        /// Gets the byte delay jitter of this instance.
        /// </summary>
        public DelayJitter DelayJitter
        {
            get { return tjTimeJitter; }
        }

        /// <summary>
        /// Creates all simulation items and linkes them together.
        /// </summary>
        /// <returns>The first item in the linked item list.</returns>
        protected override ITrafficSimulatorChainItem AddSimulatorChainItems()
        {
            scSpeedConstrainer = new SpeedConstrainer();
            pdPacketDropper = new PacketDropper();
            pdPacketDuplicator = new PacketDuplicator();
            pdPacketReorderer = new PacketReorderer();
            bfByteFlipper = new ByteFlipper();
            tjTimeJitter = new DelayJitter();

            scSpeedConstrainer.Next = pdPacketDropper;
            pdPacketDropper.Next = pdPacketDuplicator;
            pdPacketDuplicator.Next = pdPacketReorderer;
            pdPacketReorderer.Next = bfByteFlipper;
            bfByteFlipper.Next = tjTimeJitter;
            tjTimeJitter.Next = this;

            return scSpeedConstrainer;
        }
    }
}
