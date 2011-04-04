using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace eExNetworkLibrary.IP
{
    /// <summary>
    /// This class provides diffrent methods for IPv4 address analysis and the calculating of network and broadcast addresses
    /// </summary>
    public class IPv4AddressAnalysis
    {
        /// <summary>
        /// Returns the count of all IPv4 addresses in the given address range.
        /// </summary>
        /// <param name="ipaStart">The start IPv4 address of the range</param>
        /// <param name="ipaEnd">The end IPv4 address of the range</param>
        /// <returns>The count of all IPv4 addresses between the given addresses</returns>
        public long GetIpCount(IPAddress ipaStart, IPAddress ipaEnd)
        {
            CheckForV4(ipaStart);
            CheckForV4(ipaEnd);

            int iStartOctet1 = Convert.ToInt32(ipaStart.GetAddressBytes()[0]);
            int iStartOctet2 = Convert.ToInt32(ipaStart.GetAddressBytes()[1]);
            int iStartOctet3 = Convert.ToInt32(ipaStart.GetAddressBytes()[2]);
            int iStartOctet4 = Convert.ToInt32(ipaStart.GetAddressBytes()[3]);

            int iEndOctet1 = Convert.ToInt32(ipaEnd.GetAddressBytes()[0]);
            int iEndOctet2 = Convert.ToInt32(ipaEnd.GetAddressBytes()[1]);
            int iEndOctet3 = Convert.ToInt32(ipaEnd.GetAddressBytes()[2]);
            int iEndOctet4 = Convert.ToInt32(ipaEnd.GetAddressBytes()[3]);

            long lSum = iEndOctet4 - iStartOctet4;
            lSum += (iEndOctet3 - iStartOctet3) * 256;
            lSum += (iEndOctet2 - iStartOctet2) * 256 * 256;
            lSum += (iEndOctet1 - iStartOctet1) * 256 * 256 * 256;

            return lSum;
        }

        /// <summary>
        /// Checks whether the given address is an IPv4Address and throws an exception if not.
        /// </summary>
        /// <param name="ipaAddress">The address to check</param>
        private void CheckForV4(IPAddress ipaAddress)
        {
            if (ipaAddress.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
            {
                throw new ArgumentException("The eExNetworkLibrary.IP.IPv4AddressAnalysis only supports IPv4 addresses.");
            }
        }

        /// <summary>
        /// Returns all IPv4 addresses in the given address range, including the start and end IPv4 address.
        /// </summary>
        /// <param name="ipaStart">The start IPv4 address of the range</param>
        /// <param name="ipaEnd">The end IPv4 address of the range</param>
        /// <returns>All IPv4 addresses in the given address range</returns>
        public IPAddress[] GetIPRange(IPAddress ipaStart, IPAddress ipaEnd)
        {
            CheckForV4(ipaStart);
            CheckForV4(ipaEnd);

            byte[] byteStartIP = ipaStart.GetAddressBytes();
            byte[] byteEndIP = ipaEnd.GetAddressBytes();

            List<IPAddress> lIPRange = new List<IPAddress>();

            while((byteStartIP[0] << 24) + (byteStartIP[1] << 16) + (byteStartIP[2] << 8) + (byteStartIP[3]) <= (byteEndIP[0] << 24) + (byteEndIP[1] << 16) + (byteEndIP[2] << 8) + (byteEndIP[3]))
            {
                lIPRange.Add(new IPAddress(byteStartIP));

                byteStartIP = Increase(byteStartIP);
            }

            return lIPRange.ToArray();
        }

        /// <summary>
        /// Returns the classfull broadcast IPv4 address for the given IPv4 network
        /// </summary>
        /// <param name="ipa">The IPv4 network to get the broadcast address for</param>
        /// <returns>The classfull broadcast IPv4 address for the given IPv4 network</returns>
        public IPAddress GetClassfullBroadcastAddress(IPAddress ipa)
        {
            CheckForV4(ipa);

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
        /// Returns the classless network IPv4 address for the given IPv4 network and the given IPv4 subnetmask
        /// </summary>
        /// <param name="ipa">The IPv4 network the get the network address for</param>
        /// <param name="sMask">The IPv4 subnet mask to get the network address for</param>
        /// <returns>The classless network IPv4 address for the given IPv4 network and the given IPv4 subnetmask</returns>
        public IPAddress GetClasslessNetworkAddress(IPAddress ipa, Subnetmask sMask)
        {
            CheckForV4(ipa);

            byte[] bBytes = ipa.GetAddressBytes();
            if (bBytes.Length != 4)
            {
                throw new ArgumentException("Only IPv4 Addresses are supported");
            }
            byte[] bMaskBytes = sMask.MaskBytes;
            byte[] bData = new byte[4];

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
            CheckForV4(ipa);

            byte[] bBytes = ipa.GetAddressBytes();
            if (bBytes.Length != 4)
            {
                throw new ArgumentException("Only IPv4 Addresses are supported");
            }
            byte[] bMaskBytes = sMask.MaskBytes;

            for (int iC1 = 0; iC1 < bBytes.Length; iC1++)
            {
                bBytes[iC1] = (byte)(bBytes[iC1] | (~bMaskBytes[iC1]));
            }

            return new IPAddress(bBytes);
        }

        /// <summary>
        /// Returns the classfull network IPv4 address for the given IPv4 network
        /// </summary>
        /// <param name="ipa">The IPv4 network to get the network address for</param>
        /// <returns>The classfull network IPv4 address for the given IPv4 network</returns>
        public IPAddress GetClassfullNetworkAddress(IPAddress ipa)
        {
            CheckForV4(ipa);

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
            CheckForV4(ipaIn);

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
            CheckForV4(ipa);

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
            CheckForV4(ipaIn);

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
        /// Increases the given IPv4Address by one.
        /// </summary>
        /// <param name="ipaAddress">The IPv4Address to increase</param>
        /// <returns>The increased IPv4Address</returns>
        public IPAddress Increase(IPAddress ipaAddress)
        {
            return new IPAddress(Increase(ipaAddress.GetAddressBytes()));
        }

        /// <summary>
        /// Decreases the given IPv4Address by one.
        /// </summary>
        /// <param name="ipaAddress">The IPv4Address to increase</param>
        /// <returns>The decreased IPv4Address</returns>
        public IPAddress Decrease(IPAddress ipaAddress)
        {
            return new IPAddress(Decrease(ipaAddress.GetAddressBytes()));
        }   
        
        /// <summary>
        /// Increases the given IPv4Address by one.
        /// </summary>
        /// <param name="bAddress">The IPv4Address to increase as an array of unsigned bytes</param>
        /// <returns>The increased IPv4Address as an array of unsigned bytes</returns>
        public byte[] Increase(byte[] bAddress)
        {
            int iResult = 0;

            for (int iC1 = bAddress.Length - 1; iC1 >= 0; iC1--)
            {
                iResult = ((int)bAddress[iC1]) + 1;
                if (iResult > 255)
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
        /// Decreases the given IPv4Address by one.
        /// </summary>
        /// <param name="bAddress">The IPv4Address to increase as an array of unsigned bytes</param>
        /// <returns>The decreased IPv4Address as an array of unsigned bytes</returns>
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
