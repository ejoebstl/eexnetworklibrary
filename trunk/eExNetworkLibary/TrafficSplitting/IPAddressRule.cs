using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace eExNetworkLibrary.TrafficSplitting
{
    /// <summary>
    /// This rule is capable of filtering traffic according to IP-Addresses.
    /// <example>
    /// <code>
    /// //Match all frames which have a source of 192.168.0.0/24, with any desination
    /// IPAddressRule ipRuleLocalSubnet = new IPAddressRule();
    /// ipRuleLocalSubnet.Source = new IPAddress(new byte[] { 192, 168, 0, 0 });
    /// ipRuleLocalSubnet.SourceWildcard = new Subnetmask(new byte[] { 0, 0, 0, 255 });
    /// ipRuleLocalSubnet.Destination = null;
    /// 
    /// //Match all frames which have a source of 192.168.0.0/24, with a destination of 85.158.181.28
    /// IPAddressRule ipLocalSubnetToServer = new IPAddressRule();
    /// ipLocalSubnetToServer.Source = new IPAddress(new byte[] { 192, 168, 0, 0 });
    /// ipLocalSubnetToServer.SourceWildcard = new Subnetmask(new byte[] { 0, 0, 0, 255 });
    /// ipLocalSubnetToServer.Destination = new IPAddress(new byte[] { 85, 158, 181, 28 });
    /// 
    /// //Match all frames which have a source or destination of 192.168.0.0/24
    /// IPAddressRule ipFromOrToLocalSubnet = new IPAddressRule();
    /// ipLocalSubnetToServer.Address = new IPAddress(new byte[] { 192, 168, 0, 0 });
    /// ipLocalSubnetToServer.Wildcard = new Subnetmask(new byte[] { 0, 0, 0, 255 });
    /// </code>
    /// </example>
    /// </summary>
    public class IPAddressRule : TrafficSplitterRule
    {
        private IPAddress ipaAddress;
        private Subnetmask smWildcard;
        private IPAddress ipaSource;
        private Subnetmask smSourceWildcard;
        private IPAddress ipaDestination;
        private Subnetmask smDestinationWildcard;

        /// <summary>
        /// Gets or sets an address, source or destination, for which matches occour. Set this property to null to ignore this condition.
        /// If this address is not set to null, this setting overrides Source and Destination.
        /// </summary>
        public IPAddress Address
        {
            get { return ipaAddress; }
            set { ipaAddress = value; }
        }

        /// <summary>
        /// Gets or sets a wildcard, source or destination, for which matches occour. Set this wildcard to null to use no wildcard.
        /// </summary>
        public Subnetmask Wildcard
        {
            get { return smWildcard; }
            set { smWildcard = value; }
        }

        /// <summary>
        /// Gets or sets the source address for which matches occour. Set this property to null to match any source address.
        /// </summary>
        public IPAddress Source
        {
            get { return ipaSource; }
            set { ipaSource = value; }
        }

        /// <summary>
        /// Gets or sets the destination address for which matches occour. Set this property to null to match any destination address.
        /// </summary>
        public IPAddress Destination
        {
            get { return ipaDestination; }
            set { ipaDestination = value; }
        }

        /// <summary>
        /// Gets or sets the destination wildcard for which matches occour. Set this wildcard to null to use no wildcard.
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
        /// Gets or sets the source wildcard for which matches occour. Set this wildcard to null to use no wildcard.
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
            ipaDestination = null;
            ipaSource = null;
            smSourceWildcard = null;
            smDestinationWildcard = null;
            ipaAddress = null;
            smWildcard = null;
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
            if (ipFrame != null)
            {
                bool bResult;

                if (ipaAddress != null)
                {
                    bResult = Match(ipFrame.SourceAddress, ipaAddress, smWildcard) ||
                        Match(ipFrame.DestinationAddress, ipaAddress, smWildcard);
                }
                else
                {
                    bResult = Match(ipFrame.SourceAddress, ipaSource, smSourceWildcard) &&
                        Match(ipFrame.DestinationAddress, ipaDestination, smDestinationWildcard);
                }

                return bResult && base.IsMatch(frame, ethFrame, ipFrame, udpFrame, tcpFrame);
            }
            return false;
        }

        private bool Match(IPAddress ipa1, IPAddress ipa2, Subnetmask sWildcard)
        {
            if (ipa2 == null || ipa1 == null)
            {
                return true; //Any
            }
            if (ipa1.AddressFamily != ipa2.AddressFamily)
            {
                return false; //Wrong address type. 
            }
            if (sWildcard == null)
            {
                return ipa1.Equals(ipa2); //No wildcard
            }
            if (ipa1.AddressFamily != sWildcard.AddressFamily)
            {
                return false; //Wrong address type. 
            }

            byte[] bAddress1 = ipa1.GetAddressBytes();
            byte[] bAddress2 = ipa2.GetAddressBytes();
            byte[] bWildcard = sWildcard.MaskBytes;

            bool bMatch = true;

            for (int iC1 = 0; iC1 < bAddress1.Length; iC1++)
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
        /// Returns a long description of this rules condition, without the action.
        /// </summary>
        /// <returns>A long description of this rules condition</returns>
        public override string GetLongDescription()
        {
            if (ipaAddress != null)
            {
                return "If any address is " + ipaAddress.ToString() + (smWildcard != null ? " (Wildcard " + smWildcard.PrefixLength + ")" : "");
            }
            else
            {
                string strSourceString = null;
                string strDstString = null;

                if (ipaSource != null)
                {
                    strSourceString = "source port is " + ipaSource.ToString() + (smSourceWildcard != null ? " (Wildcard " + smSourceWildcard.ToString() + ")" : "");
                }
                if (ipaDestination != null)
                {
                    strDstString = "destination port is " + ipaDestination.ToString() + (smDestinationWildcard != null ? " (Wildcard " + smDestinationWildcard.ToString() + ")" : "");
                }

                if (strSourceString != null && strDstString != null)
                {
                    return "If " + strSourceString + " and " + strDstString;
                }
                else if (strSourceString == null && strDstString != null)
                {
                    return "If " + strDstString;
                }
                else if (strSourceString != null && strDstString == null)
                {
                    return "If " + strSourceString;
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
            if (ipaAddress != null)
            {
                return "Addr == " + ipaAddress.ToString() + (smWildcard != null ? " & " + smWildcard.PrefixLength : "");
            }
            else
            {
                string strSourceString = null;
                string strDstString = null;

                if (ipaSource != null)
                {
                    strSourceString = "Src == " + ipaSource.ToString() + (smSourceWildcard != null ? " & " + smSourceWildcard.ToString() : "");
                }
                if (ipaDestination != null)
                {
                    strDstString = "Dst == " + ipaDestination.ToString() + (smDestinationWildcard != null ? " & " + smDestinationWildcard.ToString() : "");
                }

                if (strSourceString != null && strDstString != null)
                {
                    return strSourceString + " && " + strDstString;
                }
                else if (strSourceString == null && strDstString != null)
                {
                    return strDstString;
                }
                else if (strSourceString != null && strDstString == null)
                {
                    return strSourceString;
                }
                else
                {
                    return "true";
                }
            }
        }
    }
}
