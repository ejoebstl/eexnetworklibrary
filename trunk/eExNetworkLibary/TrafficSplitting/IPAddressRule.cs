using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace eExNetworkLibrary.TrafficSplitting
{
    /// <summary>
    /// This rule is capable of filtering traffic according to IP-Addresses
    /// </summary>
    public class IPAddressRule : TrafficSplitterRule
    {
        private IPAddress ipaSource;
        private Subnetmask smSourceWildcard;
        private IPAddress ipaDestination;
        private Subnetmask smDestinationWildcard;
        private IP.IPAddressAnalysis ipv4Analysis;

        /// <summary>
        /// Gets or sets the source address for which matches occour
        /// </summary>
        public IPAddress Source
        {
            get { return ipaSource; }
            set { ipaSource = value; }
        }

        /// <summary>
        /// Gets or sets the destination address for which matches occour
        /// </summary>
        public IPAddress Destination
        {
            get { return ipaDestination; }
            set { ipaDestination = value; }
        }

        /// <summary>
        /// Gets or sets the destination wildcard for which matches occour
        /// </summary>
        public Subnetmask DestinationWildcard
        {
            get { return smDestinationWildcard; }
            set { smDestinationWildcard = value; }
        }

        /// <summary>
        /// Gets the name of this rule
        /// </summary>
        public override string Name
        {
            get { return "IP Address Rule"; }
        }

        /// <summary>
        /// Gets or sets the source wildcard for which matches occour
        /// </summary>
        public Subnetmask SourceWildcard
        {
            get { return smSourceWildcard; }
            set { smSourceWildcard = value; }
        }

        /// <summary>
        /// Creates a new instance of this rule
        /// </summary>
        public IPAddressRule()
        {
            ipaDestination = IPAddress.Any;
            ipaSource = IPAddress.Any;
            smSourceWildcard = new Subnetmask();
            smDestinationWildcard = new Subnetmask();
            ipv4Analysis = new eExNetworkLibrary.IP.IPAddressAnalysis();
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
            if (ipv4Frame != null)
            {
                if (lLogic == Logic.Or)
                {
                    return MatchWildcard(ipv4Frame.SourceAddress, ipaSource, smSourceWildcard) ||
                        MatchWildcard(ipv4Frame.DestinationAddress, ipaDestination, smDestinationWildcard);
                }
                else
                {
                    return MatchWildcard(ipv4Frame.SourceAddress, ipaSource, smSourceWildcard) &&
                        MatchWildcard(ipv4Frame.DestinationAddress, ipaDestination, smDestinationWildcard);
                }
            }
            return false;
        }

        private bool MatchWildcard(IPAddress ipa1, IPAddress ipa2, Subnetmask sWildcard)
        {
            byte[] bAddress1 = ipa1.GetAddressBytes();
            byte[] bAddress2 = ipa2.GetAddressBytes();
            byte[] bWildcard = sWildcard.MaskBytes;

            bool bMatch = true;

            for (int iC1 = 0; iC1 < 4; iC1++)
            {
                if ((bAddress1[iC1] & (~bWildcard[iC1])) != (bAddress2[iC1] & (~bWildcard[iC1])))
                {
                    bMatch = false;
                }
            }

            return bMatch;
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
            return "If Source is " + ipaSource.ToString() + " " + lLogic.ToString() + " Destination is " + ipaDestination.ToString() + " then " + this.Action.ToString();
        }

        /// <summary>
        /// Returns a short description of this rules function
        /// </summary>
        /// <returns>A short description of this rules function</returns>
        public override string GetShortDescription()
        {
            return ipaSource.ToString() + " " + lLogic.ToString() + " " + ipaDestination.ToString() + ":" + Action.ToString();
        }
    }
}
