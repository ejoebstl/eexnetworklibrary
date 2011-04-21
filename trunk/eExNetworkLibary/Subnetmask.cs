using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Net.Sockets;

namespace eExNetworkLibrary
{
    /// <summary>
    /// Represents an IP subnetmask
    /// </summary>
    public class Subnetmask
    {
        private byte[] bSubnetMask;

        /// <summary>
        /// Gets or sets the mask bytes
        /// </summary>
        public byte[] MaskBytes
        {
            get { return bSubnetMask; }
            set
            {
                bSubnetMask = value;
            }
        }

        /// <summary>
        /// Creates a new IPv4 subnet mask with the value 0.0.0.0
        /// </summary>
        public Subnetmask()
        {
            bSubnetMask = new byte[4];
        }

        public AddressFamily AddressFamily
        {
            get
            {
                if (bSubnetMask.Length == 4)
                {
                    return AddressFamily.InterNetwork;
                }
                if(bSubnetMask.Length == 16)
                {
                    return AddressFamily.InterNetworkV6;
                }

                throw new ArgumentException("Only IPv4 and IPv6 addresses are supported at the moment.");
            }
        }

        /// <summary>
        /// Gets the integer form of this subnetmask, e.g. 4294967040 for 255.255.255.0. This works only for IPv4 subnet masks.
        /// </summary>
        [Obsolete("This property is only valid for IPv4 subnet masks.", false)]
        public uint IntNotation
        {
            get
            {
                if (bSubnetMask.Length != 4)
                    throw new InvalidOperationException("This property is only valid for IPv4 subnet masks.");
                return (uint)((bSubnetMask[0] << 24) + (bSubnetMask[1] << 16) + (bSubnetMask[2] << 8) + (bSubnetMask[3]));
            }
        }

        /// <summary>
        /// Creates a new IPv4 or IPv6 subnetmask with the given value
        /// </summary>
        /// <param name="bMaskBytes">The value to assign to the subnetmask. This value will be copied.</param>
        public Subnetmask(byte[] bMaskBytes)
        {
            if (bMaskBytes.Length != 4 && bMaskBytes.Length != 16) throw new ArgumentException("Only IPv4 or IPv6 subnet masks with four or sixteen bytes length are supported");
            bSubnetMask = new byte[bMaskBytes.Length];
            bMaskBytes.CopyTo(bSubnetMask, 0);
        }


        /// <summary>
        /// Gets the prefix length of this subnetmask, e.g. 24 for 255.255.255.0
        /// </summary>
        public int PrefixLength
        {
            get 
            {
                int iCount = 0;
                for (int iC1 = 0; iC1 < bSubnetMask.Length; iC1++)
                {
                    iCount += Bitcount(bSubnetMask[iC1]);
                }
                return iCount;
            }
        }

        private int Bitcount(byte bToCount)
        {
            int iCount = 0;
            while (bToCount != 0)
            {
                iCount++;
                bToCount &= (byte)(bToCount - 1);
            }
            return iCount;
        }

        private void Check(byte[] bMaskBytes)
        {
            if (bMaskBytes.Length != 4 && bMaskBytes.Length != 16) 
                throw new ArgumentException("Only IPv4 or IPv6 subnet masks with four or sixteen bytes length are supported");

            if(!CheckBytes(bMaskBytes))
                throw new ArgumentException("The mask bytes contain invaild data.");
        }

        private static bool CheckBytes(byte[] bMaskBytes)
        {
            bool bWasZero = false;

            for (int iC1 = 0; iC1 < bMaskBytes.Length * 8; iC1++)
            {
                if (IsBitSet(bMaskBytes, iC1))
                {
                    if (bWasZero)
                    {
                        return false;
                    }
                }
                else
                {
                    bWasZero = true;
                }
            }

            return true;
        }

        private static bool IsBitSet(byte[] bMaskBytes, int iIndex)
        {
            int iByteIndex = iIndex / 8;
            int iBitIndex = iIndex % 8;

            return ((bMaskBytes[iByteIndex] >> (7 - iBitIndex)) & 1) == 1;
        }

        /// <summary>
        /// Parses a string to a subnetmask. Possible inputs are: <br />
        /// A string in the format of XXX.XXX.XXX.XXX, where XXX is a number between 0 and 255 for IPv4 subnet masks.<br />
        /// A string in the format of XXXX, where XXXX will be put into the subnet portion of an IPv6 subnet mask. <br />
        /// A string in the format of XXXX:XXXX:XXXX:XXXX:XXXX:XXXX, where XXXX is a hex value between 0 and FFFF for IPv6 subnet masks. 
        /// </summary>
        /// <param name="strString">The string to parse</param>
        /// <returns>A subnetmask</returns>
        public static Subnetmask Parse(string strString)
        {
            Subnetmask smMask;

            if(!TryParse(strString, out smMask))
            {
                throw new ArgumentException("Input string was in the wrong format.");
            }

            return smMask;
        }

        /// <summary>
        /// Parses a given prefix length to a subnetmask. Possible inputs are: <br />
        /// A number between 0 and 32 for an IPv4 address (InterNetwork) <br />
        /// A number between 0 and 128 for an IPv6 address (InterNetworkV6)
        /// </summary>
        /// <param name="iPrefixLength">The prefix length of the subnetmask, e.g /24 as integer.</param>
        /// <param name="afDesiredAddress">The address family of the subnet mask, InterNetwork or InterNetworkV6</param>
        /// <returns></returns>
        public static Subnetmask Parse(int iPrefixLength, AddressFamily afDesiredAddress)
        {
            Subnetmask smMask;

            if (afDesiredAddress != AddressFamily.InterNetworkV6 && afDesiredAddress != AddressFamily.InterNetwork)
            {
                throw new ArgumentException("Only IPv4 and IPv6 addresses are supported at the moment.");
            }

            if (!TryParse(iPrefixLength, afDesiredAddress, out smMask))
            {
                throw new ArgumentException("Input number was in the wrong range.");
            }

            return smMask;
        }

        /// <summary>
        /// Returns the string representation of this subnetmask
        /// </summary>
        /// <returns>The string representation of this subnetmask</returns>
        public override string ToString()
        {
            string strReturn = "";

            if (bSubnetMask.Length == 4)
            {
                for (int iC1 = 0; iC1 < bSubnetMask.Length; iC1++)
                {
                    strReturn += bSubnetMask[iC1].ToString();
                    if (iC1 != bSubnetMask.Length - 1)
                    {
                        strReturn += ".";
                    }
                }

                return strReturn;
            }
            else if (bSubnetMask.Length == 16)
            {
                for (int iC1 = 0; iC1 < bSubnetMask.Length; iC1 += 2)
                {
                    strReturn += ((bSubnetMask[iC1] << 8) + bSubnetMask[iC1 + 1]).ToString("x");
                    if (iC1 != bSubnetMask.Length - 2)
                    {
                        strReturn += ":";
                    }
                }

                return strReturn;
            }

            throw new ArgumentException("Only IPv4 and IPv6 addresses are supported at the moment.");
        }

        /// <summary>
        /// Determines whether this subnetmask equals another subnetmask
        /// </summary>
        /// <param name="obj">An object to compare to this subnetmask</param>
        /// <returns>True, if the <paramref name="obj"/>equals this subnetmask, false if not.</returns>
        public override bool Equals(object obj)
        {
            if(obj is Subnetmask)
            {
                Subnetmask sObj = (Subnetmask)obj;

                if (sObj.bSubnetMask.Length != this.bSubnetMask.Length)
                {
                    return false;
                }

                for (int iC1 = 0; iC1 < bSubnetMask.Length; iC1++)
                {
                    if (sObj.bSubnetMask[iC1] != this.bSubnetMask[iC1])
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;

        }

        /// <summary>
        /// Returns the hash code of this subnetmask.
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            return (int)this.PrefixLength;
        }

        /// <summary>
        /// Tries to parse a string to a subnetmask. Possible inputs are: <br />
        /// A string in the format of XXX.XXX.XXX.XXX, where XXX is a number between 0 and 255 for IPv4 subnet masks. <br />
        /// A string in the format of XXXX, where XXXX will be put into the subnet portion of an IPv6 subnet mask. <br />
        /// A string in the format of XXXX:XXXX:XXXX:XXXX:XXXX:XXXX, where XXXX is a hex value between 0 and FFFF for IPv6 subnet masks. 
        /// </summary>
        /// <param name="strString">The string to parse</param>
        /// <param name="sSubnetmask">When this method returns, contains the parsed subnet mask if parsing was a success.</param>
        /// <returns>A bool indicating whether parsing was successfull</returns>
        public static bool TryParse(string strString, out Subnetmask sSubnetmask)
        {
            byte[] bMaskData;
            int iParse = 0;

            sSubnetmask = null;

            if (strString.Contains("."))
            {
                string[] strSplit = strString.Split('.');
                if (strSplit.Length == 4)
                {
                    bMaskData = new byte[4];
                    for (int iC1 = 0; iC1 < 4; iC1++)
                    {
                        if(Int32.TryParse(strSplit[iC1], out iParse))
                        {
                            bMaskData[iC1] = (byte)iParse;
                        }
                        else
                        {
                            return false;
                        }
                    }

                    if (CheckBytes(bMaskData))
                    {
                        sSubnetmask = new Subnetmask(bMaskData);
                    }
                }
            }
            else if (strString.Contains(":"))
            {
                string[] strSplit = strString.Split(':');

                if (strSplit.Length == 8)
                {
                    bMaskData = new byte[16];
                    for (int iC1 = 0; iC1 < 16; iC1 += 2)
                    {
                        if (Int32.TryParse(strSplit[iC1 / 2], System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.CurrentCulture, out iParse))
                        {
                            bMaskData[iC1] = (byte)((iParse >> 8) & 0xFF);
                            bMaskData[iC1 + 1] = (byte)(iParse & 0xFF);
                        }
                        else
                        {
                            return false;
                        }
                    }

                    if (CheckBytes(bMaskData))
                    {
                        sSubnetmask = new Subnetmask(bMaskData);
                    }
                }
            }
            else if (strString.Length <= 4)
            {
                Subnetmask sDummy;
                if (TryParse("FFFF:FFFF:FFFF:FFFF:" + strString + ":0:0:0", out sDummy))
                {
                    sSubnetmask = sDummy;
                }
            }

            return sSubnetmask != null;
        }   
        
        /// <summary>
        /// Tries to parse an integer to a subnetmask.
        /// </summary>
        /// <param name="iSlashNotation">The short slash notation of the subnetmask, e.g /24 as integer.</param>
        /// <param name="afDesiredAddress">The address family of the subnet mask, InterNetwork or InterNetworkV6</param>
        /// <param name="sSubnetmask">When this method returns, contains the parsed subnet mask if parsing was a success.</param>
        /// <returns>A bool indicating whether parsing was successfull</returns>
        public static bool TryParse(int iSlashNotation, AddressFamily afDesiredAddress, out Subnetmask sSubnetmask)
        {
            sSubnetmask = null;

            if (iSlashNotation >= 0)
            {
                if (afDesiredAddress == AddressFamily.InterNetwork)
                {
                    if (iSlashNotation <= 32)
                    {
                        byte[] bData = new byte[4];

                        for (int iC1 = 0; iC1 < 32 && iSlashNotation > 0; iC1++)
                        {
                            SetBit(bData, iC1, true);
                            iSlashNotation--;
                        }

                        if (CheckBytes(bData))
                        {
                            sSubnetmask = new Subnetmask(bData);
                        }
                    }
                }
                else if(afDesiredAddress == AddressFamily.InterNetworkV6)
                {
                    if (iSlashNotation <= 128)
                    {
                        byte[] bData = new byte[16];

                        for (int iC1 = 0; iC1 < 128 && iSlashNotation > 0; iC1++)
                        {
                            SetBit(bData, iC1, true);
                            iSlashNotation--;
                        }

                        if (CheckBytes(bData))
                        {
                            sSubnetmask = new Subnetmask(bData);
                        }
                    }
                }
            }

            return sSubnetmask != null;
        }

        private static void SetBit(byte[] bData, int iBitIndex, bool bValue)
        {
            int iByteIndex = iBitIndex / 8;
            iBitIndex = iBitIndex % 8;

            if (bValue)
            {
                bData[iByteIndex] |= (byte)(1 << (7 - iBitIndex));
            }
            else
            {
                bData[iByteIndex] &= (byte)(~(1 << (7 - iBitIndex)));
            }
        }

        public static Subnetmask IPv4Default
        {
            get { return new Subnetmask(new byte[] { 255, 255, 255, 0 }); }
        }

        public static Subnetmask IPv4Empty
        {
            get { return new Subnetmask(new byte[] { 0, 0, 0, 0 }); }
        }

        public static Subnetmask IPv6Empty
        {
            get { return new Subnetmask(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }); }
        }

        public static Subnetmask IPv6Default
        {
            get { return new Subnetmask(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0, 0, 0, 0, 0, 0, 0, 0 }); }
        }
    }
}
