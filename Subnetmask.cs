using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace eExNetworkLibrary
{
    /// <summary>
    /// Represents a IPv4 subnetmask
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
                if (value.Length != 4) throw new ArgumentException("Only IPv4 subnet masks with four bytes length are supported");
                bSubnetMask = value;
            }
        }

        /// <summary>
        /// Creates a new IPv4 subnetmask with the value 0.0.0.0
        /// </summary>
        public Subnetmask()
        {
            bSubnetMask = new byte[4];
        }

        /// <summary>
        /// Gets the integer form of this subnetmask, e.g. 4294967040 for 255.255.255.0
        /// </summary>
        public uint IntNotation
        {
            get { return (uint)((bSubnetMask[0] << 24) + (bSubnetMask[1] << 16) + (bSubnetMask[2] << 8) + (bSubnetMask[3])); }
        }

        /// <summary>
        /// Creates a new IPv4 subnetmask with the given value
        /// </summary>
        /// <param name="bMaskBytes">The value to assign to the subnetmask. This value will be copied.</param>
        public Subnetmask(byte[] bMaskBytes)
        {
            if (bMaskBytes.Length != 4) throw new ArgumentException("Only IPv4 subnet masks with four bytes length are supported");
            bSubnetMask = new byte[4];
            bMaskBytes.CopyTo(bSubnetMask, 0);
        }


        /// <summary>
        /// Gets the slash notation of this subnetmask, e.g. 24 for 255.255.255.0
        /// </summary>
        public int SlashNotation
        {
            get { return Bitcount(bSubnetMask[0]) + Bitcount(bSubnetMask[1]) + Bitcount(bSubnetMask[2]) + Bitcount(bSubnetMask[3]); }
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

        /// <summary>
        /// Parses a string to a subnetmask. The string has to be in the format X.X.X.X where X is a number between 0 and 255
        /// </summary>
        /// <param name="strString">The string to parse</param>
        /// <returns>A subnetmask</returns>
        public static Subnetmask Parse(string strString)
        {
            Subnetmask smMask = new Subnetmask();
            string[] strSplit = strString.Split('.');
            if (strSplit.Length != 4)
            {
                throw new ArgumentException("Only IPv4 subnet masks in the format X.X.X.X are supported");
            }
            for (int iC1 = 0; iC1 < strSplit.Length; iC1++)
            {
                smMask.MaskBytes[iC1] = Byte.Parse(strSplit[iC1]);
            }
            return smMask;
        }

        /// <summary>
        /// Returns the string representation of this subnetmask
        /// </summary>
        /// <returns>The string representation of this subnetmask</returns>
        public override string ToString()
        {
            return bSubnetMask[0] + "." + bSubnetMask[1] + "." + bSubnetMask[2] + "." + bSubnetMask[3];
        }

        /// <summary>
        /// Determines whether this subnetmask equals another subnetmask
        /// </summary>
        /// <param name="obj">An object to compare to this subnetmask</param>
        /// <returns>True, if the <paramref name="obj"/>equals this subnetmask, false if not.</returns>
        public override bool Equals(object obj)
        {
            if(obj.GetType() == typeof(Subnetmask))
            {
                Subnetmask sm = (Subnetmask)obj;
                if(sm.MaskBytes[0] == bSubnetMask[0] && sm.MaskBytes[1] == bSubnetMask[1] && sm.MaskBytes[2] == bSubnetMask[2] && sm.MaskBytes[3] == bSubnetMask[3])
                {
                    return true;
                }
            }
            return false;

        }

        /// <summary>
        /// Returns the hash code of this subnetmask.
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            return (int)this.IntNotation;
        }

        /// <summary>
        /// Tries to parse a string to a subnetmask.
        /// </summary>
        /// <param name="strMask">The string to parse</param>
        /// <param name="sSubnetmask">The subnetmask to return</param>
        /// <returns>True, if parsing was successfull, false if not</returns>
        public static bool TryParse(string strMask, out Subnetmask sSubnetmask)
        {
            Subnetmask smMask = new Subnetmask();
            string[] strSplit = strMask.Split('.');
            if (strSplit.Length != 4)
            {
                sSubnetmask = null;
                return false;
            }
            for (int iC1 = 0; iC1 < strSplit.Length; iC1++)
            {
                smMask.MaskBytes[iC1] = Byte.Parse(strSplit[iC1]);
            }
            sSubnetmask = smMask;
            return true;
        }
    }
}
