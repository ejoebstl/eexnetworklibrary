using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace eExNetworkLibrary
{
    /// <summary>
    /// This class represents a MACAddress
    /// </summary>
    public class MACAddress
    {
        /// <summary>
        /// Returns an empty MACAddress (00:00:00:00:00:00)
        /// </summary>
        public static MACAddress Empty
        {
            get { return new MACAddress(new byte[] { 0, 0, 0, 0, 0, 0 }); }
        }

        /// <summary>
        /// Returns the famous MACAddress DE:AD:CA:FE:BA:BE
        /// </summary>
        public static MACAddress DeadCafeBabe
        {
            get { return new MACAddress(new byte[] { 0xDE, 0xEA, 0xCA, 0xFE, 0xBA, 0xBE }); }
        }

        /// <summary>
        /// Returns a broadcast MACAddress (FF:FF:FF:FF:FF:FF)
        /// </summary>
        public static MACAddress Broadcast
        {
            get { return new MACAddress(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }); }
        }

        /// <summary>
        /// Returns the famous MACAddress BA:D0:CA:BL:E0:00
        /// </summary>
        public static MACAddress Badcable
        {
            get { return new MACAddress(new byte[] { 0xBA, 0xD0, 0xCA, 0xB7, 0xE0, 0x00 }); }
        }

        private byte[] bAddressBytes;

        /// <summary>
        /// Returns the address bytes of the current MACAddress
        /// </summary>
        public byte[] AddressBytes
        {
            get { return bAddressBytes; }
            set
            {
                if (bAddressBytes.Length != 6) throw new ArgumentException("The input array has a wrong number of bytes");
                bAddressBytes = value;
            }
        }

        /// <summary>
        /// Creates a new MACAddress with the specified address bytes.
        /// </summary>
        /// <param name="bAddressBytes"></param>
        public MACAddress(byte[] bAddressBytes)
        {
            if (bAddressBytes.Length != 6) throw new ArgumentException("The input array has a wrong number of bytes");
            this.bAddressBytes = bAddressBytes;
        }

        /// <summary>
        /// Creates a new, empty MACAddress
        /// </summary>
        public MACAddress()
        {
            this.bAddressBytes = new byte[6];
        }

        /// <summary>
        /// Converts this MACAddress into a string
        /// </summary>
        /// <returns>A string representing the current MACAddress</returns>
        public override string ToString()
        {
            return bAddressBytes[0].ToString("x02") + ":" +
                   bAddressBytes[1].ToString("x02") + ":" +
                   bAddressBytes[2].ToString("x02") + ":" +
                   bAddressBytes[3].ToString("x02") + ":" +
                   bAddressBytes[4].ToString("x02") + ":" +
                   bAddressBytes[5].ToString("x02");
        }

        /// <summary>
        /// Parses a string to a MACAddress.
        /// </summary>
        /// <param name="strIn">An input string in the form X:X:X:X:X:X, where X is a hexadecimal number between 0 and FF</param>
        /// <returns>A MACAddress</returns>
        public static MACAddress Parse(string strIn)
        {
            MACAddress macReturn = new MACAddress();
            string[] strDigits = strIn.Split(':');

            if (strDigits.Length != 6)
            {
                throw new Exception("Invalid MAC address. Digits must be seperated by ':'.");
            }

            for (int iC1 = 0; iC1 < strDigits.Length; iC1++)
            {
                macReturn.AddressBytes[iC1] = Convert.ToByte(strDigits[iC1], 16);
            }

            return macReturn;
        }

        /// <summary>
        /// Creates a multicast MAC address for the given IPv6 address. 
        /// For example, the IP address fdcb:e462:34c9:5ad6::2 would result in a MAC address of 33:33:ff:00:00:02
        /// </summary>
        /// <param name="ipaAddress">The IPv6 address to get the multicast MAC for</param>
        /// <returns>The multicast MAC for the given address</returns>
        public static MACAddress MulticastFromIPv6Address(IPAddress ipaAddress)
        {
            if (ipaAddress.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                throw new ArgumentException("Only IPv6 addresses are supported.");
            }

            byte[] bAddressBytes = ipaAddress.GetAddressBytes();
            byte[] bMACBytes = new byte[]{0x33, 0x33, 0xFF, 0x00, 0x00, 0x00};

            Array.Copy(bAddressBytes, 13, bMACBytes, 3, 3);

            return new MACAddress(bMACBytes);
        }

        /// <summary>
        /// Gets a bool determining whether this MACAddress is empty
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return this.bAddressBytes[0] == 0x00 && this.bAddressBytes[1] == 0x00 && this.bAddressBytes[2] == 0x00 && this.bAddressBytes[3] == 0x00 && this.bAddressBytes[4] == 0x00 && this.bAddressBytes[5] == 0x00;
            }
        }

        /// <summary>
        /// Gets a bool determining whether this MACAddress is a broadcast address
        /// </summary>
        public bool IsBroadcast
        {
            get
            {
                return this.bAddressBytes[0] == 0xFF && this.bAddressBytes[1] == 0xFF && this.bAddressBytes[2] == 0xFF && this.bAddressBytes[3] == 0xFF && this.bAddressBytes[4] == 0xFF && this.bAddressBytes[5] == 0xFF;
            }
        }

        /// <summary>
        /// Compares this MACAddress to an object
        /// </summary>
        /// <param name="obj">The object to compare to this MACAddress</param>
        /// <returns>A bool inicating, whether <paramref name="obj"/> equals to this MACAddress</returns>
        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(MACAddress))
            {
                MACAddress mcObj = (MACAddress)obj;
                if (this.bAddressBytes.Length != mcObj.AddressBytes.Length)
                {
                    return false;
                }
                for (int iC1 = 0; iC1 < mcObj.AddressBytes.Length; iC1++)
                {
                    if (this.bAddressBytes[iC1] != mcObj.AddressBytes[iC1])
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the hash code of this MACAddress
        /// </summary>
        /// <returns>The hash code of this MACAddress</returns>
        public override int GetHashCode()
        {
            return (bAddressBytes[0] << 24) + (bAddressBytes[1] << 16) + (bAddressBytes[4]) << (8 + bAddressBytes[5]);
        }
    }
}
