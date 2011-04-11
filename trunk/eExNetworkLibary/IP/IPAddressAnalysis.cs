using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace eExNetworkLibrary.IP
{
    public class IPv4AddressAnalysis : IPAddressAnalysis
    { /*Alias definition for compatibility reasons */ }

    /// <summary>
    /// This class provides diffrent methods for IPv4 and IPv6 address analysis and the calculating of network and broadcast addresses
    /// </summary>
    public class IPAddressAnalysis
    {
        /// <summary>
        /// Returns the count of all IP addresses in the given address range.
        /// </summary>
        /// <param name="ipaStart">The start IP address of the range</param>
        /// <param name="ipaEnd">The end IP address of the range</param>
        /// <returns>The count of all IP addresses between the given addresses</returns>
        public ulong GetIpCount(IPAddress ipaStart, IPAddress ipaEnd)
        {
            CheckVersion(ipaStart, ipaEnd);

            ulong lSum = 2;

            byte[] byteStartIP = ipaStart.GetAddressBytes();
            byte[] byteEndIP = ipaEnd.GetAddressBytes();

            switch(Compare(byteStartIP, byteEndIP))
            {
                case 0:
                    return 1;
                    break;
                case 1: 
                    byte[] bTemp = byteEndIP;
                    byteEndIP = byteStartIP;
                    byteStartIP = bTemp;
                    break;
            }

            for (int iC1 = byteStartIP.Length - 1; iC1 >= 0; iC1--)
            {
                if (byteEndIP[iC1] - byteStartIP[iC1] != 0)
                {
                    if ((byteStartIP.Length - 1) - iC1 > 8)
                    {
                        throw new ArgumentException("The count of the given IP range is too large to be represented as ulong. Overflow.");
                    }
                    lSum += (ulong)(byteEndIP[iC1] - byteStartIP[iC1]) * (ulong)Math.Pow(2D, (double)((byteStartIP.Length - 1) - iC1) * 8);
                }
            }

            return lSum;
        }

        /// <summary>
        /// Checks whether the given addresses are the same supported type and throws an exception if not..
        /// </summary>
        /// <param name="ipaAddress1">The first address to check</param>
        /// <param name="ipaAddress2">The second address to check.</param>
        private void CheckVersion(IPAddress ipaAddress1, IPAddress ipaAddress2)
        {
            if ((ipaAddress1.AddressFamily != AddressFamily.InterNetwork
                && ipaAddress1.AddressFamily != AddressFamily.InterNetworkV6)
                || ipaAddress1.AddressFamily != ipaAddress2.AddressFamily)
            {
                throw new ArgumentException("Only IPv4 and IPv6 addresses are supported. Furthermore, two diffrent address types (" + ipaAddress1.AddressFamily + " and " + ipaAddress2.AddressFamily + ") cannot be mixed within a call.");
            }
        }

        /// <summary>
        /// Returns all IP addresses in the given address range, including the start and end IPv4 address.
        /// </summary>
        /// <param name="ipaStart">The start IP address of the range</param>
        /// <param name="ipaEnd">The end IP address of the range</param>
        /// <returns>All IP addresses in the given address range</returns>
        public IPAddress[] GetIPRange(IPAddress ipaStart, IPAddress ipaEnd)
        {
            CheckVersion(ipaStart, ipaEnd);

            byte[] byteStartIP = ipaStart.GetAddressBytes();
            byte[] byteEndIP = ipaEnd.GetAddressBytes();

            List<IPAddress> lIPRange = new List<IPAddress>();

            bool bIncrease = Compare(byteStartIP, byteEndIP) == -1;

            lIPRange.Add(new IPAddress(byteStartIP));

            while (Compare(byteStartIP, byteEndIP) != 0)
            {
                if (bIncrease)
                {
                    byteStartIP = Increase(byteStartIP);
                    lIPRange.Add(new IPAddress(byteStartIP));
                }
            }
          
            return lIPRange.ToArray();
        }

        /// <summary>
        /// Compares two byte arrays by their numeric value. 
        /// </summary>
        /// <param name="bA">The first array to compare.</param>
        /// <param name="bB">The second array to compare.</param>
        /// <returns>-1, if bA is smaller than bB<br />
        /// 0, if bA and bB are equal<br />
        /// 1, bB is smaller than bA</returns>
        private int Compare(byte[] bA, byte[] bB)
        {
            if (bA.Length != bB.Length)
            {
                throw new ArgumentException("The length of the arrays to compare must match.");
            }

            for (int iC1 = 0; iC1 < bA.Length; iC1++)
            {
                if (bA[iC1] < bB[iC1])
                {
                    return -1;
                }
                else if (bB[iC1] < bA[iC1])
                {
                    return 1;
                }
            }

            return 0;
        }

        /// <summary>
        /// Returns the classfull broadcast IPv4 address for the given IPv4 network
        /// </summary>
        /// <param name="ipa">The IPv4 network to get the broadcast address for</param>
        /// <returns>The classfull broadcast IPv4 address for the given IPv4 network</returns>
        public IPAddress GetClassfullBroadcastAddress(IPAddress ipa)
        {
            if (ipa.AddressFamily != AddressFamily.InterNetwork)
            {
                throw new ArgumentException("Only IPv4 addresses are supported for classfull address calculations.");
            }

            byte[] ipAddressBytes = ipa.GetAddressBytes();

            if (GetClass(ipa) == IPv4AddressClass.A)
            {
                ipAddressBytes[3] = 0xFF;
                ipAddressBytes[2] = 0xFF;
                ipAddressBytes[1] = 0xFF;
            }
            if (GetClass(ipa) == IPv4AddressClass.B)
            {
                ipAddressBytes[3] = 0xFF;
                ipAddressBytes[2] = 0xFF;
            }
            if (GetClass(ipa) == IPv4AddressClass.C)
            {
                ipAddressBytes[3] = 0xFF;
            }

            return new IPAddress(ipAddressBytes);
        }

        /// <summary>
        /// Returns the classless network IP address for the given IP network and the given IP subnetmask
        /// </summary>
        /// <param name="ipa">The IP network the get the network address for</param>
        /// <param name="sMask">The IP subnet mask to get the network address for</param>
        /// <returns>The classless network IP address for the given IP network and the given IP subnetmask</returns>
        public IPAddress GetClasslessNetworkAddress(IPAddress ipa, Subnetmask sMask)
        {
            if (ipa.AddressFamily != AddressFamily.InterNetwork && ipa.AddressFamily != AddressFamily.InterNetworkV6)
            {
                throw new ArgumentException("Only IPv4 and IPv6 addresses are supported.");
            }

            byte[] bBytes = ipa.GetAddressBytes();
            byte[] bMaskBytes = sMask.MaskBytes;

            if (bBytes.Length != bMaskBytes.Length)
            {
                throw new ArgumentException("The length of the subnet mask must match the length of the address.");
            }

            byte[] bData = new byte[bBytes.Length];

            for (int iC1 = 0; iC1 < bBytes.Length; iC1++)
            {
                bData[iC1] = (byte)(bBytes[iC1] & bMaskBytes[iC1]);
            }

            return new IPAddress(bData);
        }
        
        /// <summary>
        /// Returns the classless broadcast IPv4 address for the given IPv4 network and the given IPv4 subnetmask
        /// </summary>
        /// <param name="ipa">The IPv4 network the get the broadcast address for</param>
        /// <param name="sMask">The IPv4 subnet mask to get the broadcast address for</param>
        /// <returns>The classless broadcast IPv4 address for the given IPv4 network and the given IPv4 subnetmask</returns>
        public IPAddress GetClasslessBroadcastAddress(IPAddress ipa, Subnetmask sMask)
        {
            if (ipa.AddressFamily != AddressFamily.InterNetwork && ipa.AddressFamily != AddressFamily.InterNetworkV6)
            {
                throw new ArgumentException("Only IPv4 and IPv6 addresses are supported.");
            }

            byte[] bBytes = ipa.GetAddressBytes();
            byte[] bMaskBytes = sMask.MaskBytes;

            if (bBytes.Length != bMaskBytes.Length)
            {
                throw new ArgumentException("The length of the subnet mask must match the length of the address.");
            }

            byte[] bData = new byte[bBytes.Length];

            for (int iC1 = 0; iC1 < bBytes.Length; iC1++)
            {
                bData[iC1] = (byte)(bBytes[iC1] | (~bMaskBytes[iC1]));
            }

            return new IPAddress(bData);
        }

        /// <summary>
        /// Returns the classfull network IPv4 address for the given IPv4 network
        /// </summary>
        /// <param name="ipa">The IPv4 network to get the network address for</param>
        /// <returns>The classfull network IPv4 address for the given IPv4 network</returns>
        public IPAddress GetClassfullNetworkAddress(IPAddress ipa)
        {
            if (ipa.AddressFamily != AddressFamily.InterNetwork)
            {
                throw new ArgumentException("Only IPv4 addresses are supported for classfull address calculations.");
            }

            byte[] ipAddressBytes = ipa.GetAddressBytes();

            if (GetClass(ipa) == IPv4AddressClass.A)
            {
                ipAddressBytes[3] = 0x00;
                ipAddressBytes[2] = 0x00;
                ipAddressBytes[1] = 0x00;
            }
            if (GetClass(ipa) == IPv4AddressClass.B)
            {
                ipAddressBytes[3] = 0x00;
                ipAddressBytes[2] = 0x00;
            }
            if (GetClass(ipa) == IPv4AddressClass.C)
            {
                ipAddressBytes[3] = 0x00;
            }

            return new IPAddress(ipAddressBytes);
        }
        
        /// <summary>
        /// Returns the class of the given IPv4 address
        /// </summary>
        /// <param name="ipaIn">The IPv4 address to determine the class for</param>
        /// <returns>The class of the given IPv4 address</returns>
        public IPv4AddressClass GetClass(IPAddress ipaIn)
        {
            if (ipaIn.AddressFamily != AddressFamily.InterNetwork)
            {
                throw new ArgumentException("Only IPv4 addresses are supported for classfull address calculations.");
            }

            byte bFirst = ipaIn.GetAddressBytes()[0];

            if (((int)bFirst & 0x80) == 0)
            {
                return IPv4AddressClass.A;
            }
            if (((int)bFirst & 0x40) == 0)
            {
                return IPv4AddressClass.B;
            }
            if (((int)bFirst & 0x20) == 0)
            {
                return IPv4AddressClass.C;
            }
            if (((int)bFirst & 0x10) == 0)
            {
                return IPv4AddressClass.D;
            }
            if (((int)bFirst & 0x8) == 0)
            {
                return IPv4AddressClass.E;
            }
            return IPv4AddressClass.Unknown;
        }

        /// <summary>
        /// Returns the classfull subnet mask of a given IPv4 network
        /// </summary>
        /// <param name="ipa">The network to get the classfull subnetmask for</param>
        /// <returns>The classfull subnet mask of a given IPv4 network</returns>
        public Subnetmask GetClassfullSubnetMask(IPAddress ipa)
        {
            if (ipa.AddressFamily != AddressFamily.InterNetwork)
            {
                throw new ArgumentException("Only IPv4 addresses are supported for classfull address calculations.");
            }

            IPv4AddressClass ipv4Class = GetClass(ipa);
            Subnetmask sm = new Subnetmask();

            if (ipv4Class == IPv4AddressClass.A)
            {
                sm.MaskBytes[0] = 255;
            }
            if (ipv4Class == IPv4AddressClass.B)
            {
                sm.MaskBytes[0] = 255;
                sm.MaskBytes[1] = 255;
            }
            if (ipv4Class == IPv4AddressClass.C)
            {
                sm.MaskBytes[0] = 255;
                sm.MaskBytes[1] = 255;
                sm.MaskBytes[2] = 255;
            }
            return sm;
        }

        /// <summary>
        /// Gets the privacy level of an IPv4 address
        /// </summary>
        /// <param name="ipaIn">The IPv4 address to get the privacy level for</param>
        /// <returns>THe privacy level of the given IPv4 address</returns>
        public IPv4AddressPrivacyLevel GetPrivacyLevel(IPAddress ipaIn)
        {
            if (ipaIn.AddressFamily != AddressFamily.InterNetwork)
            {
                throw new ArgumentException("Only IPv4 addresses are supported for classfull address calculations.");
            }

            byte[] bBytes = ipaIn.GetAddressBytes();

            if ((int)bBytes[0] == 10)
            {
                return IPv4AddressPrivacyLevel.Private;
            }
            if ((int)bBytes[0] == 172 && (int)bBytes[1] >= 16 && (int)bBytes[1] <= 31)
            {
                return IPv4AddressPrivacyLevel.Private;
            }
            if ((int)bBytes[0] == 192 && (int)bBytes[1] == 168)
            {
                return IPv4AddressPrivacyLevel.Private;
            }
            return IPv4AddressPrivacyLevel.Public;
        }

        /// <summary>
        /// Increases the given IPAddress by one.
        /// </summary>
        /// <param name="ipaAddress">The IPAddress to increase</param>
        /// <returns>The increased IPAddress</returns>
        public IPAddress Increase(IPAddress ipaAddress)
        {
            return new IPAddress(Increase(ipaAddress.GetAddressBytes()));
        }

        /// <summary>
        /// Decreases the given IPAddress by one.
        /// </summary>
        /// <param name="ipaAddress">The IPAddress to increase</param>
        /// <returns>The decreased IPAddress</returns>
        public IPAddress Decrease(IPAddress ipaAddress)
        {
            return new IPAddress(Decrease(ipaAddress.GetAddressBytes()));
        }   
        
        /// <summary>
        /// Increases the given IPAddress bytes by one.
        /// </summary>
        /// <param name="bAddress">The IPAddress to increase as an array of unsigned bytes</param>
        /// <returns>The increased IPAddress as an array of unsigned bytes</returns>
        public byte[] Increase(byte[] bAddress)
        {
            int iResult = 0;

            for (int iC1 = bAddress.Length - 1; iC1 >= 0; iC1--)
            {
                iResult = ((int)bAddress[iC1]) + 1;
                if (iResult > byte.MaxValue)
                {
                    bAddress[iC1] = 0;
                }
                else
                {
                    bAddress[iC1] = (byte)iResult;
                    break;
                }
            }

            return bAddress;
        }

        /// <summary>
        /// Decreases the given IPAddress bytes by one.
        /// </summary>
        /// <param name="bAddress">The IPAddress to increase as an array of unsigned bytes</param>
        /// <returns>The decreased IPAddress as an array of unsigned bytes</returns>
        public byte[] Decrease(byte[] bAddress)
        {
            int iResult = 0;

            for (int iC1 = bAddress.Length - 1; iC1 >= 0; iC1--)
            {
                iResult = ((int)bAddress[iC1]) - 1;
                if (iResult < 0)
                {
                    bAddress[iC1] = 255;
                }
                else
                {
                    bAddress[iC1] = (byte)iResult;
                    break;
                }
            }

            return bAddress;
        }

        /// <summary>
        /// Converts the given address into a solicited node multicast address.
        /// For example, the address fdcb:e462:34c9:5ad6::2 would result in the multicast address FF02::1:FF:2.
        /// </summary>
        /// <param name="ipaAddress">The IP address to convert</param>
        /// <returns>The solicited note multicast address.</returns>
        public IPAddress GetSolicitedNodeMulticastAddress(IPAddress ipaAddress)
        {
            if (ipaAddress.AddressFamily != AddressFamily.InterNetworkV6)
            {
                throw new ArgumentException("Only IPv6 addresses can be converted into solicited node multicast addresses.");
            }

            byte[] bAddressBytes = new byte[] {
                0xFF, 0x02, 0x00, 0x00, 
                0x00, 0x00, 0x00, 0x00, 
                0x00, 0x00, 0x00, 0x00, 
                0xFF, 0x00, 0x00, 0x00};

            Array.Copy(ipaAddress.GetAddressBytes(), 13, bAddressBytes, 13, 3);

            return new IPAddress(bAddressBytes);
        }
    }

    /// <summary>
    /// An enumeration for IPv4 classes
    /// </summary>
    public enum IPv4AddressClass
    {
        /// <summary>
        /// IP class A - Initial byte: 0 - 127 
        /// </summary>
        A = 1,
        /// <summary>
        /// IP class B - Initial byte: 128 - 191 
        /// </summary>
        B = 2,
        /// <summary>
        /// IP class C - Initial byte: 192 - 223 
        /// </summary>
        C = 3,
        /// <summary>
        /// IP class D - Initial byte: 224 - 247
        /// This class contains only multicast addresses
        /// </summary>
        D = 4,
        /// <summary>
        /// IP class E - Initial byte: 248 - 255 
        /// Reserved for experimental use
        /// </summary>
        E = 5,
        /// <summary>
        /// Unkown class
        /// </summary>
        Unknown = 6
    }

    /// <summary>
    /// An enumeration for IPv4 privacy levels
    /// </summary>
    public enum IPv4AddressPrivacyLevel
    {
        /// <summary>
        /// Public addresses
        /// </summary>
        Public = 0,
        /// <summary>
        /// Private addresses. These addresses are not routed on the internet backbone. 
        /// </summary>
        Private = 1
    }
}
