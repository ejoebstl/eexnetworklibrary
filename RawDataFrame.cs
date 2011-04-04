using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary
{
    /// <summary>
    /// Represents a raw byte data frame, which simply stores non-parsed frame bytes
    /// </summary>
    public class RawDataFrame : Frame
    {
        private byte[] bData;

        /// <summary>
        /// A constructor which stores the given byte array.
        /// </summary>
        /// <param name="bData">The byte array to store</param>
        public RawDataFrame(byte[] bData)
        {
            this.bData = new byte[bData.Length];
            bData.CopyTo(this.bData, 0);
        }

        /// <summary>
        /// Gets the type of this frame.
        /// </summary>
        public override FrameType FrameType
        {
            get { return FrameType.ByteData; }
        }

        /// <summary>
        /// Gets this frames converted to bytes.
        /// </summary>
        public override byte[] FrameBytes
        {
            get
            {
                byte[] bData = new byte[this.bData.Length];
                this.bData.CopyTo(bData, 0);
                return bData;

            }
        }

        /// <summary>
        /// Gets or sets this frames data
        /// </summary>
        public byte[] Data
        {
            get { return bData; }
            set { bData = value; }
        }

        /// <summary>
        /// Gets the length of the bytes of this frame.
        /// </summary>
        public override int Length
        {
            get { return bData.Length; }
        }

        /// <summary>
        /// Creates a string representation of this frame
        /// </summary>
        /// <returns>The string represenation of this frame</returns>
        public override string ToString()
        {
            string strDescription = this.FrameType.ToString() + ":\n";
            for (int iC1 = 0; iC1 < bData.Length; iC1++)
            {
                strDescription += bData[iC1].ToString("x02") + " ";
            }
            return strDescription + "\n";
        }

        /// <summary>
        /// Clones this frame.
        /// </summary>
        /// <returns>An identic clone of this frame</returns>
        public override Frame Clone()
        {
            return new RawDataFrame((byte[])this.FrameBytes.Clone());
        }
    }
}
