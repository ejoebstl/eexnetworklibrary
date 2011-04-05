using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary.TrafficSplitting
{
    /// <summary>
    /// This rule is capable of filtering traffic according to tcp or udp ports
    /// </summary>
    public class PortRule : TrafficSplitterRule
    {
        private int iSourcePort;
        private int iDestinationPort;
        private TransportProtocol tProtocol;

        /// <summary>
        /// Gets or sets the source port for which matches occour
        /// </summary>
        public int SourcePort
        {
            get { return iSourcePort; }
            set { iSourcePort = value; }
        }

        /// <summary>
        /// Gets or sets the destination port for which matches occour
        /// </summary>
        public int DestinationPort
        {
            get { return iDestinationPort; }
            set { iDestinationPort = value; }
        }

        /// <summary>
        /// Gets or sets the transport protocol (UDP, TCP or both) for which matches occour
        /// </summary>
        public TransportProtocol Protocol
        {
            get { return tProtocol; }
            set { tProtocol = value; }
        }

        /// <summary>
        /// Gets this rule's name
        /// </summary>
        public override string Name
        {
            get { return "Port Rule"; }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public PortRule()
        {
            iSourcePort = 80;
            iDestinationPort = 80;
            tProtocol = TransportProtocol.Any;
            lLogic = Logic.Or;
        }

        /// <summary>
        /// Checkes whether this rule matches a given frame.
        /// </summary>
        /// <param name="ethFrame">The Ethernet part of the frame</param>
        /// <param name="ipv4Frame">The IPv4 part of the frame</param>
        /// <param name="udpFrame">The UDP part of the frame</param>
        /// <param name="tcpFrame">The TCP part of the frame</param>
        /// <returns>A bool indicating whether this rule matches a given frame.</returns>
        public override bool IsMatch(eExNetworkLibrary.Ethernet.EthernetFrame ethFrame, eExNetworkLibrary.IP.IPFrame ipv4Frame, eExNetworkLibrary.UDP.UDPFrame udpFrame, eExNetworkLibrary.TCP.TCPFrame tcpFrame)
        {
            if (tProtocol == TransportProtocol.TCP || (tProtocol == TransportProtocol.Any && tcpFrame != null))
            {
                if (tcpFrame != null)
                {
                    if (lLogic == Logic.And)
                    {
                        return tcpFrame.DestinationPort == iDestinationPort && tcpFrame.SourcePort == iSourcePort;
                    }
                    else
                    {
                        return tcpFrame.DestinationPort == iDestinationPort || tcpFrame.SourcePort == iSourcePort;
                    }
                }
                else
                {
                    return false;
                }
            }
            else if (tProtocol == TransportProtocol.UDP || (tProtocol == TransportProtocol.Any && udpFrame != null))
            {
                if (udpFrame != null)
                {
                    if (lLogic == Logic.And)
                    {
                        return udpFrame.DestinationPort == iDestinationPort && udpFrame.SourcePort == iSourcePort;
                    }
                    else
                    {
                        return udpFrame.DestinationPort == iDestinationPort || udpFrame.SourcePort == iSourcePort;
                    }
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns the name of this rule
        /// </summary>
        /// <returns>The name of this rule</returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Returns a long description of this rules function
        /// </summary>
        /// <returns>A long description of this rules function</returns>
        public override string GetLongDescription()
        {
            return "If " + tProtocol.ToString() + " Source Port is " + this.iSourcePort.ToString() + " " + lLogic.ToString() + " " + tProtocol.ToString() + " Destination Port is " + iDestinationPort.ToString() + " then " + this.Action.ToString();
        }

        /// <summary>
        /// Returns a short description of this rules function
        /// </summary>
        /// <returns>A short description of this rules function</returns>
        public override string GetShortDescription()
        {
            return tProtocol.ToString() + ", " + this.iSourcePort.ToString() + "/" + this.iDestinationPort.ToString() + ":" + Action.ToString();
        }
    }

    /// <summary>
    /// An enumeration for transport protocols for which the rule should apply
    /// </summary>
    public enum TransportProtocol
    {
        /// <summary>
        /// TCP
        /// </summary>
        TCP = 0,
        /// <summary>
        /// UDP
        /// </summary>
        UDP = 1,
        /// <summary>
        /// Any
        /// </summary>
        Any = 3
    }

    /// <summary>
    /// The rule logic for source and destination
    /// </summary>
    public enum Logic
    {
        /// <summary>
        /// Logic.And will cause the rule to only return a match if the rule applies for source and destination.
        /// </summary>
        And = 0,
        /// <summary>
        /// Logic.Or will cause to rule to return a match method if the rule applies for source or destination.
        /// </summary>
        Or = 1
    }
}
