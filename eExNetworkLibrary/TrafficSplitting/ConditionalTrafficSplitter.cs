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

namespace eExNetworkLibrary.TrafficSplitting
{
    /// <summary>
    /// This traffic handler is capable of filtering traffic according to specific rules.
    /// These rules define whether a frame should be forwarded to the OutputA or OutputB handler or should be dropped on a Match.
    /// If no rule matches, the frame is forwarded to handler A per default.
    /// </summary>
    public class ConditionalTrafficSplitter : TrafficHandler
    {
        private TrafficHandler thB;
        private List<TrafficSplitterRule> tsrRules;

        /// <summary>
        /// This delegate is used to handle traffic splitter rule events
        /// </summary>
        /// <param name="sender">The calling object</param>
        /// <param name="args">The arguments</param>
        public delegate void TrafficSplitterEventHandler(object sender, TrafficRuleEventArgs args);
        
        /// <summary>
        /// This event is fired when a rule is added
        /// </summary>
        public event TrafficSplitterEventHandler RuleAdded;
        
        /// <summary>
        /// This event is fired when a rule is removed
        /// </summary>
        public event TrafficSplitterEventHandler RuleRemoved;
        
        /// <summary>
        /// This event is fired when a frame is forwarded to OutputHandlerB
        /// </summary>
        public event EventHandler FrameForwardedB;

        /// <summary>
        /// Adds a rule to this conditional traffic splitter
        /// </summary>
        /// <param name="tsr">The rule to add</param>
        public void AddRule(TrafficSplitterRule tsr)
        {
            lock (tsrRules)
            {
                tsrRules.Add(tsr);
                InvokeExternalAsync(RuleAdded, new TrafficRuleEventArgs(tsr));
            }
        }

        /// <summary>
        /// Removes a rule from this conditional traffic splitter
        /// </summary>
        /// <param name="tsr">The rule to remove</param>
        public void RemoveRule(TrafficSplitterRule tsr)
        {
            lock (tsrRules)
            {
                tsrRules.Remove(tsr);
                InvokeExternalAsync(RuleRemoved, new TrafficRuleEventArgs(tsr));
            }
        }

        /// <summary>
        /// Returns a bool indicating whether a rule is contained in this conditional traffic splitter
        /// </summary>
        /// <param name="tsr">The rule to search for</param>
        /// <returns>A bool indicating whether a rule is contained in this conditional traffic splitter</returns>
        public bool ContainsRule(TrafficSplitterRule tsr)
        {
            lock (tsrRules)
            {
                return tsrRules.Contains(tsr);
            }
        }

        /// <summary>
        /// Removes all traffic splitter rules from this conditional traffic splitter
        /// </summary>
        public void ClearRules()
        {
            lock (tsrRules)
            {
                foreach (TrafficSplitterRule tsr in tsrRules)
                {
                    InvokeExternalAsync(RuleRemoved, new TrafficRuleEventArgs(tsr));
                }
                tsrRules.Clear();
            }
        }

        /// <summary>
        /// Gets all traffic splitter rules from this conditional traffic splitter
        /// </summary>
        /// <returns></returns>
        public TrafficSplitterRule[] GetRules()
        {
            lock (tsrRules)
            {
                return tsrRules.ToArray();
            }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public ConditionalTrafficSplitter()
            : base()
        {
            tsrRules = new List<TrafficSplitterRule>();
        }

        /// <summary>
        /// Gets or sets the OutputB handler.
        /// </summary>
        public TrafficHandler OutputB
        {
            get { return this.thB; }
            set { this.thB = value; }
        }

        /// <summary>
        /// Gets or sets the OutputA handler.
        /// </summary>
        public TrafficHandler OutputA
        {
            get { return this.OutputHandler; }
            set { this.OutputHandler = value; }
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public override void Cleanup()
        {
            //Do nothing
        }

        /// <summary>
        /// Applies all known rules sequentially to the given frame, until a rule matches
        /// </summary>
        /// <param name="fInputFrame">The frame to analyze</param>
        protected override void HandleTraffic(Frame fInputFrame)
        {
            IP.IPFrame ipv4Frame = GetIPv4Frame(fInputFrame);
            UDP.UDPFrame udpFrame = GetUDPFrame(fInputFrame);
            TCP.TCPFrame tcpFrame = GetTCPFrame(fInputFrame);
            Ethernet.EthernetFrame ethFrame = GetEthernetFrame(fInputFrame);

            lock (tsrRules)
            {
                foreach (TrafficSplitterRule tsr in tsrRules)
                {
                    if (tsr.IsMatch(fInputFrame, ethFrame, ipv4Frame, udpFrame, tcpFrame))
                    {
                        if (tsr.Action == TrafficSplitterActions.SendToA)
                        {
                            NotifyA(fInputFrame);
                            return;
                        }
                        else if (tsr.Action == TrafficSplitterActions.SendToB)
                        {
                            NotifyB(fInputFrame);
                            return;
                        }
                        else if (tsr.Action == TrafficSplitterActions.Drop)
                        {
                            //Drop
                            PushDroppedFrame(fInputFrame);
                            return;
                        }
                    }
                }
            }

            NotifyA(fInputFrame);
        }

        /// <summary>
        /// Forwardes a frame to output handler A
        /// </summary>
        /// <param name="fInputFrame">The frame to forward</param>
        protected void NotifyA(Frame fInputFrame)
        {
            base.NotifyNext(fInputFrame);
        }

        /// <summary>
        /// Forwardes a frame to output handler B
        /// </summary>
        /// <param name="fInputFrame">The frame to forward</param>
        protected void NotifyB(Frame fInputFrame)
        {
            if (thB != null)
            {
                thB.PushTraffic(fInputFrame);
                this.InvokeExternalAsync(FrameForwardedB);
            }
        }
    }

    /// <summary>
    /// A simple class used to carry properties for traffic rule events
    /// </summary>
    public class TrafficRuleEventArgs : EventArgs
    {
        private TrafficSplitterRule tsrRule;

        /// <summary>
        /// The rule associated with the event
        /// </summary>
        public TrafficSplitterRule Rule
        {
            get { return tsrRule; }
        }

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="tsrRule">The rule associated with the event</param>
        public TrafficRuleEventArgs(TrafficSplitterRule tsrRule)
        {
            this.tsrRule = tsrRule;
        }
    }
}
