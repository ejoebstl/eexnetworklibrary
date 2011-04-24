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

namespace eExNetworkLibrary.Monitoring
{
    /// <summary>
    /// This class represents the superclass of all traffic analyzers.
    /// Traffic analyzers must not have any output handlers also they must not generate any output and they must not edit any incomin frame
    /// The purpose of a traffic analyzer is to provide a stable base for doing multiple, paralell traffic analysis tasks without affecting the original frame.
    /// If you want to generate or change traffic on the fly, derive from TrafficModifier or TrafficHandler instead.
    /// </summary>
    public abstract class TrafficAnalyzer: TrafficHandler
    {
        /// <summary>
        /// Setting output handlers is not supported by traffic analyzers
        /// </summary>
        public override TrafficHandler OutputHandler
        {
            get { return null; }
            set { throw new InvalidOperationException("Traffic analyzers must not have any output"); }
        }

        /// <summary>
        /// Does nothing
        /// </summary>
        /// <param name="fInputFrame"></param>
        protected override void NotifyNext(Frame fInputFrame)
        {
            //Do nothing (Discard)
        }

        /// <summary>
        /// Analyzes the given frame
        /// </summary>
        /// <param name="fInputFrame">The frame to analyze</param>
        protected abstract override void HandleTraffic(Frame fInputFrame);
    }
}
