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
    /// This rule is capable of filtering traffic according to tcp or udp ports.
    /// <example>
    /// <code>
    /// //Math all TCP frames with a source port of 1234 and a destination port of 80
    /// PortRule prPortToHTTP = new PortRule();
    /// prPortToHTTP.Protocol = TransportProtocol.TCP;
    /// prPortToHTTP.SourcePort = 1234;
    /// prPortToHTTP.DestinationPort = 80;
    /// 
    /// //Match all UDP or TCP frames with a destination port of 1234 and any source port
    /// PortRule prUPD = new PortRule();
    /// prPortToHTTP.Protocol = TransportProtocol.Any;
    /// prUPD.SourcePort = -1;
    /// prUPD.DestinationPort = 80;
    /// 
    /// //Match all TCP frames with source or destination port 80
    /// PortRule prHTTP = new PortRule();
    /// prHTTP.Protocol = TransportProtocol.TCP;
    /// prHTTP.Port = 80;
    /// </code>
    /// </example>
    /// </summary>
    public class PortRule : TrafficSplitterRule
    {
        private int iPort;
        private int iSourcePort;
        private int iDestinationPort;
        private TransportProtocol tProtocol;

        /// <summary>
        /// Gets or sets a port, source or destination, for which matches occour. To ignore this condition, set the port to -1.
        /// If this port is not set to -1, this setting overrides SourcePort and DestinationPort.
        /// </summary>
        public int Port
        {
            get { return iPort; }
            set
            {
                if (value > 65565 && value < -1)
                    throw new ArgumentException("Setting ports larger than 65565 and smaller than -1 is not possible.");
                iPort = value;

            }
        }

        /// <summary>
        /// Gets or sets the source port for which matches occour. Set this property to -1 for any source port. 
        /// </summary>
        public int SourcePort
        {
            get { return iSourcePort; }
            set
            {
                if (value > 65565 && value < -1)
                    throw new ArgumentException("Setting ports larger than 65565 and smaller than -1 is not possible.");
                iSourcePort = value;
            }
        }

        /// <summary>
        /// Gets or sets the destination port for which matches occour. Set this property to -1 for any destination port.
        /// </summary>
        public int DestinationPort
        {
            get { return iDestinationPort; }
            set
            {
                if (value > 65565 && value < -1)
                    throw new ArgumentException("Setting ports larger than 65565 and smaller than -1 is not possible."); 
                iDestinationPort = value;
            }
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
            iSourcePort = -1;
            iDestinationPort = -1;
            iPort = -1;
            tProtocol = TransportProtocol.Any;
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
        public override bool IsMatch(Frame frame, eExNetworkLibrary.Ethernet.EthernetFrame ethFrame, eExNetworkLibrary.IP.IPFrame ipFrame, eExNetworkLibrary.UDP.UDPFrame udpFrame, eExNetworkLibrary.TCP.TCPFrame tcpFrame)
        {
            bool bResult;

            int iFrameSourcePort = 0;
            int iFrameDestinationPort = 0;

            if((this.tProtocol == TransportProtocol.Any || this.tProtocol == TransportProtocol.TCP) && tcpFrame != null)
            {
                iFrameDestinationPort = tcpFrame.DestinationPort;
                iFrameSourcePort = tcpFrame.SourcePort;
            }
            else if ((this.tProtocol == TransportProtocol.Any || this.tProtocol == TransportProtocol.TCP) && udpFrame != null)
            {
                iFrameSourcePort = udpFrame.SourcePort;
                iFrameDestinationPort = udpFrame.DestinationPort;
            }
            else
            {
                return false; //We have no TCP/UDP frame.
            }

            if (iPort != -1)
            {
                //Single port match
                bResult = iPort == iFrameDestinationPort || iPort == iFrameSourcePort;
            }
            else
            {
                bResult = (iSourcePort == -1 || iSourcePort == iFrameSourcePort) && (iDestinationPort == -1 || iDestinationPort == iFrameDestinationPort);
            }

            return bResult && base.IsMatch(frame, ethFrame, ipFrame, udpFrame, tcpFrame);
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
        /// Returns a long description of this rules condition, without the action.
        /// </summary>
        /// <returns>A long description of this rules condition</returns>
        public override string GetLongDescription()
        {
            if (iPort != -1)
            {
                return "If any port is " + iPort + " and protocol is " + tProtocol.ToString();
            }
            else
            {
                string strSourceString = null;
                string strDstString= null;

                if (iSourcePort != -1)
                {
                    strSourceString = "source port is " + iSourcePort.ToString();
                }
                if (iDestinationPort != -1)
                {
                    strDstString = "destination port is " + iDestinationPort.ToString();
                }

                if (strSourceString != null && strDstString != null)
                {
                    return "If " + strSourceString + " and " + strDstString + " and protocol is " + tProtocol.ToString();
                }
                else if (strSourceString == null && strDstString != null)
                {
                    return "If " + strDstString + " and protocol is " + tProtocol.ToString();
                }
                else if (strSourceString != null && strDstString == null)
                {
                    return "If " + strSourceString + " and protocol is " + tProtocol.ToString();
                }
                else
                {
                    return "true";
                }
            }
        }

        /// <summary>
        /// Returns a short description of this rules condition, without the action.
        /// </summary>
        /// <returns>A short description of this rules condition</returns>
        public override string GetShortDescription()
        {
            if (iPort != -1)
            {
                return "Port == " + iPort + " && Protocol == " + tProtocol.ToString();
            }
            else
            {
                string strSourceString = null;
                string strDstString = null;

                if (iSourcePort != -1)
                {
                    strSourceString = "Src == " + iSourcePort.ToString();
                }
                if (iDestinationPort != -1)
                {
                    strDstString = "Dst ==" + iSourcePort.ToString();
                }

                if (strSourceString != null && strDstString != null)
                {
                    return "" + strSourceString + " && " + strDstString + " && Protocol == " + tProtocol.ToString();
                }
                else if (strSourceString == null && strDstString != null)
                {
                    return "" + strDstString + " && Protocol == " + tProtocol.ToString();
                }
                else if (strSourceString != null && strDstString == null)
                {
                    return "" + strSourceString + " && Protocol == " + tProtocol.ToString();
                }
                else
                {
                    return "true";
                }
            }
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
}
