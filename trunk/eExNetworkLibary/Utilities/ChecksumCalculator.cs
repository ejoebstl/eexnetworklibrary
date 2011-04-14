using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary.Utilities
{
    /// <summary>
    /// A simple checksum calculator, which can be used to calculate IP checksums and so on.
    /// </summary>
    public static class ChecksumCalculator
    {
        /// <summary>
        /// Calculates a checksum from the given data
        /// </summary>
        /// <param name="bData">The data to calculate the checksum from</param>
        /// <returns>The resulting checksum</returns>
        public static byte[] CalculateChecksum(byte[] bData)
        {
            if (bData.Length % 2 != 0)
            {
                throw new ArgumentException("Data to calculate checksum from must be a multiple of two.");
            }

            UInt32 iChecksum = 0;
            int iIndex = 0;

            while (iIndex < bData.Length)
            {
                iChecksum += (uint)BitConverter.ToUInt16(bData, iIndex);
                iIndex += 2;

                if (iChecksum > 0xffff)
                {
                    iChecksum = (iChecksum >> 16) + (iChecksum & 0xffff);
                    iChecksum += (iChecksum >> 16);
                }
            }

            return BitConverter.GetBytes((UInt16)~iChecksum);
        }
    }
}
