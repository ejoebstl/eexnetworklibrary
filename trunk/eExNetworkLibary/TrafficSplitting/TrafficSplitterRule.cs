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
        /// Gets the name of this rule
        /// </summary>
        public abstract string Name
        {
            get;
        }

        /// <summary>
        /// Checkes whether this rule matches a given frame.
        /// </summary>
        /// <param name="ethFrame">The Ethernet part of the frame</param>
        /// <param name="ipv4Frame">The IPv4 part of the frame</param>
        /// <param name="udpFrame">The UDP part of the frame</param>
        /// <param name="tcpFrame">The TCP part of the frame</param>
        /// <returns>A bool indicating whether this rule matches a given frame.</returns>
        public abstract bool IsMatch(Ethernet.EthernetFrame ethFrame, IP.IPFrame ipv4Frame, UDP.UDPFrame udpFrame, TCP.TCPFrame tcpFrame);

        /// <summary>
        /// Returns the name of this rule
        /// </summary>
        /// <returns>The name of this rule</returns>
        public abstract override string ToString();

        /// <summary>
        /// Returns a long description of this rules function
        /// </summary>
        /// <returns>A long description of this rules function</returns>
        public abstract string GetLongDescription();

        /// <summary>
        /// Returns a short description of this rules function
        /// </summary>
        /// <returns>A short description of this rules function</returns>
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
