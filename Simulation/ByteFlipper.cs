using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary.Simulation
{
    /// <summary>
    /// This simulator item class is capable of flipping bits inside a frame's data. 
    /// </summary>
    public class ByteFlipper : PacketCorrupter
    {
        Random rRandom;

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public ByteFlipper()
        {
            rRandom = new Random();
        }

        /// <summary>
        /// Flips a random chosen count of bits inside the given frame
        /// </summary>
        /// <param name="bData">The data to corrupt</param>
        /// <returns>The corrupted data</returns>
        protected override byte[] DoErrors(byte[] bData)
        {
            int iErrorCount = rRandom.Next(iMinErrorCount, iMaxErrorCount + 1);
            int iErrorIndex;
            byte bErrorByte;

            for (int iC1 = 0; iC1 < iErrorCount; iC1++)
            {
                iErrorIndex = rRandom.Next(0, bData.Length);
                bErrorByte = (byte)(1 << rRandom.Next(0, 8));

                bData[iErrorIndex] = (byte)((bData[iErrorIndex] & (~bErrorByte)) | (~(bData[iErrorIndex] & (bErrorByte))));
            }

            return bData;
        }
    }
}
