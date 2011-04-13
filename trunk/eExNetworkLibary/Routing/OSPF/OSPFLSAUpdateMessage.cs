using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary.Routing.OSPF
{
    /// <summary>
    /// This class represents an OSPF LSA update message
    /// </summary>
    public class OSPFLSAUpdateMessage : Frame
    {
        public static string DefaultFrameType { get { return "OSPFLSAUpdateMessage"; } }
        private List<LSAHeader> lsaMessages;

        /// <summary>
        /// Clears all LSA headers from this update message
        /// </summary>
        public void ClearItems()
        {
            lsaMessages.Clear();
        }

        /// <summary>
        /// Adds an LSA header to this update message
        /// </summary>
        /// <param name="lsa">The LSA header to add</param>
        public void AddItem(LSAHeader lsa)
        {
            lsaMessages.Add(lsa);
        }

        /// <summary>
        /// Returns all LSA headers contained in this instance
        /// </summary>
        /// <returns>All LSA headers contained in this instance</returns>
        public LSAHeader[] GetItems()
        {
            return lsaMessages.ToArray();
        }

        /// <summary>
        /// Returns a bool indicating whether a LSAHeader is contained in this LSA update message
        /// </summary>
        /// <param name="lsa">The LSAHeader to search for</param>
        /// <returns>A bool indicating whether a LSAHeader is contained in this LSA update message</returns>
        public bool ContainsItem(LSAHeader lsa)
        {
            return lsaMessages.Contains(lsa);
        }

        /// <summary>
        /// Removes a LSAHeader from this instance
        /// </summary>
        /// <param name="lsa">The LSAHeader to remove</param>
        public void RemoveItem(LSAHeader lsa)
        {
            lsaMessages.Remove(lsa);
        }

        /// <summary>
        /// Returns the type of this frame.
        /// </summary>
        public override string FrameType
        {
            get { return OSPFLSAUpdateMessage.DefaultFrameType; }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public OSPFLSAUpdateMessage()
        {
            lsaMessages = new List<LSAHeader>();
        }

        /// <summary>
        /// Creates a new instance of this class by parsing the given data.
        /// </summary>
        /// <param name="bData">The data to parse</param>
        public OSPFLSAUpdateMessage(byte[] bData)
        {
            lsaMessages = new List<LSAHeader>();

            byte[] bLSAHeader;
            LSAHeader lsaHeader;
            int iC1 = 4;

            int iCount = ((int)bData[0] << 24) + ((int)bData[1] << 16) + ((int)bData[2] << 8) + bData[3];

            while(lsaMessages.Count < iCount)
            {
                bLSAHeader = new byte[bData.Length - iC1];
                for (int iC2 = iC1; iC2 < bData.Length; iC2++)
                {
                    bLSAHeader[iC2 - iC1] = bData[iC2];
                }
                lsaHeader = new LSAHeader(bLSAHeader);
                lsaMessages.Add(lsaHeader);
                iC1 += lsaHeader.Length;
            }
        }

        /// <summary>
        /// Returns the raw byte representation of this frame
        /// </summary>
        public override byte[] FrameBytes
        {
            get 
            {
                int iCount = lsaMessages.Count;
                byte[] bData = new byte[this.Length];

                bData[0] = (byte)((iCount >> 24) & 0xFF);
                bData[1] = (byte)((iCount >> 16) & 0xFF);
                bData[2] = (byte)((iCount >> 8) & 0xFF);
                bData[3] = (byte)((iCount) & 0xFF);

                int iC1 = 4;

                foreach(LSAHeader lsaHeader in lsaMessages)
                {
                    lsaHeader.FrameBytes.CopyTo(bData, iC1);
                    iC1 += lsaHeader.Length;
                }

                return bData;
            }
        }

        /// <summary>
        /// Returns the length of this frame in bytes
        /// </summary>
        public override int Length
        {
            get
            {
                int iLen = 4;

                foreach (LSAHeader lsHeader in lsaMessages)
                {
                    iLen += lsHeader.Length;
                }

                return iLen;
            }
        }

        /// <summary>
        /// Creates an identical copy of this OSPF LSA update message
        /// </summary>
        /// <returns>An identical copy of this OSPF LSA update message</returns>
        public override Frame Clone()
        {
            return new OSPFLSAUpdateMessage(this.FrameBytes);
        }
    }
}
