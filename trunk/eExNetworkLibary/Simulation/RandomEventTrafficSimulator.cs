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
    /// This abstract class represents a base for all kinds of simulator items where events occour according to a given probability.
    /// </summary>
    public abstract class RandomEventTrafficSimulatorItem : TrafficSimulatorModificationItem
    {  
        private double dProbability;
        private Random rRandom;

        /// <summary>
        /// Gets or sets the probability of the event to happen in percent (between 0 and 100).
        /// </summary>
        public double Probability
        {
            get { return dProbability; }
            set
            {
                dProbability = value;
                InvokePropertyChanged();
            }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public RandomEventTrafficSimulatorItem()
        {
            rRandom = new Random();
        }

        /// <summary>
        /// Applies the effect of this simulator chain item to the given frame.
        /// </summary>
        /// <param name="f">The input frame</param>
        public override void Push(Frame f)
        {
            if ((rRandom.NextDouble() * 100) > dProbability) // If random
            {
                CaseNotHappening(f);
            }
            else
            {
                CaseHappening(f);
            }
        }

        /// <summary>
        /// Is called when the case happens.
        /// </summary>
        /// <param name="f">The frame to process</param>
        protected abstract void CaseHappening(Frame f);

        /// <summary>
        /// Is called when the case does not happen.
        /// </summary>
        /// <param name="f">The frame to process</param>
        protected abstract void CaseNotHappening(Frame f);

        /// <summary>
        /// Does nothing
        /// </summary>
        public override void Start()
        {
            //Nothing to do
        }

        /// <summary>
        /// Does nothing
        /// </summary>
        public override void Stop()
        {
            //Nothing to do
        }
    }
}
