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
    /// This class is the base of all conditional traffic splitter rules
    /// </summary>
    public abstract class TrafficSplitterRule
    {
        List<TrafficSplitterRule> lChildRules;

        protected TrafficSplitterRule()
        {
            lChildRules = new List<TrafficSplitterRule>();
        }

        /// <summary>
        /// Adds a child condition to this rule. 
        /// Actions of child rules are ignored, but they are validated and the result is and-conjuncted with the result of the parent rule. 
        /// <br />
        /// If there are multiple child rules, and the result of at least one child rule is true, and the result of this rule is true, the end result is true.<br />
        /// If there are multiple child rules, and the result of all child rules is false, the end result is flase.<br />
        /// If the result of this rule is false, the end result is false.<br />
        /// </summary>
        /// <param name="cChild"></param>
        public void AddChildRule(TrafficSplitterRule cChild)
        {
            lock (lChildRules) { lChildRules.Add(cChild); }
        }

        /// <summary>
        /// Removes the given child condition.
        /// </summary>
        /// <param name="cChild">The child to remove</param>
        public void RemoveChildRule(TrafficSplitterRule cChild)
        {
            lock (lChildRules) { lChildRules.Remove(cChild); }
        }

        /// <summary>
        /// Checks whether a given child condition is contained by this condition.
        /// </summary>
        /// <param name="cChild">The child to check for.</param>
        /// <returns>A bool indicating whether a given child condition is contained by this condition.</returns>
        public bool ContainsChildRule(TrafficSplitterRule cChild)
        {
            lock (lChildRules) { return lChildRules.Contains(cChild); }
        }

        /// <summary>
        /// Clears all child conditions.
        /// </summary>
        public void ClearChildRules()
        {
            lock (lChildRules) { lChildRules.Clear(); }
        }

        /// <summary>
        /// Gets all child conditions.
        /// </summary>
        public TrafficSplitterRule[] ChildRules
        {
            get { lock (lChildRules) { return lChildRules.ToArray(); } }
        }

        /// <summary>
        /// The action to do on a match(drop, send to a, send to b)
        /// </summary>
        protected TrafficSplitterActions tsaAction;

        /// <summary>
        /// The action to do on a match(drop, send to a, send to b)
        /// </summary>
        public TrafficSplitterActions Action
        {
            get { return tsaAction; }
            set { tsaAction = value; }
        }

        /// <summary>
        /// Gets or sets the protocol parser of this rule. 
        /// </summary>
        public ProtocolParsing.ProtocolParser ProtcolParser
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the name of this rule
        /// </summary>
        public abstract string Name
        {
            get;
        }

        /// <summary>
        /// Checkes whether this rule matches a given frame.
        /// </summary>
        /// <param name="frame">The original frame</param>
        /// <param name="ethFrame">The Ethernet part of the frame</param>
        /// <param name="ipv4Frame">The IPv4 part of the frame</param>
        /// <param name="udpFrame">The UDP part of the frame</param>
        /// <param name="tcpFrame">The TCP part of the frame</param>
        /// <returns>A bool indicating whether this rule matches a given frame.</returns>
        public virtual bool IsMatch(Frame frame, Ethernet.EthernetFrame ethFrame, IP.IPFrame ipFrame, UDP.UDPFrame udpFrame, TCP.TCPFrame tcpFrame)
        {
            lock (lChildRules)
            {
                if (lChildRules.Count == 0)
                {
                    return true; //Nothing to validate
                }
                foreach (TrafficSplitterRule tsrRule in lChildRules)
                {
                    if (tsrRule.IsMatch(frame, ethFrame, ipFrame, udpFrame, tcpFrame))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the name of this rule
        /// </summary>
        /// <returns>The name of this rule</returns>
        public abstract override string ToString();

        /// <summary>
        /// Returns a long description of this rules condition, without the action. 
        /// </summary>
        /// <returns>A long description of this rules condition</returns>
        public abstract string GetLongDescription();

        /// <summary>
        /// Returns a short description of this rules condition, without the action. 
        /// </summary>
        /// <returns>A short description of this rules condition</returns>
        public abstract string GetShortDescription();

    }
    
    /// <summary>
    /// Action to do on a match (drop, send to a, send to b)
    /// </summary>
    public enum TrafficSplitterActions
    {
        /// <summary>
        /// Sends the frame to output handler A
        /// </summary>
        SendToA = 0,
        /// <summary>
        /// Sends the frame to output handler B
        /// </summary>
        SendToB = 1,
        /// <summary>
        /// Drops the frame
        /// </summary>
        Drop = 2
    }
}
