using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary.Routing.OSPF
{
    /// <summary>
    /// This class represents an OSPF LSA acknowledgement message
    /// </summary>
    public class OSPFLSAAcknowledgementMessage : Frame
    {
        public static string DefaultFrameType { get { return "OSPFLSAAcknowledgementMessage"; } }
        private List<LSAHeader> lLSAHeaders;

        /// <summary>
        /// Removes all LSA headers from this acknowledgement message
        /// </summary>
        public void ClearItems()
        {
            lLSAHeaders.Clear();
        }

        /// <summary>
        /// Adds an LSA header to this frame
        /// </summary>
        /// <param name="lsa">The LSA header to add</param>
        public void AddItem(LSAHeader lsa)
        {
            lLSAHeaders.Add(lsa);
        }

        /// <summary>
        /// Gets all LSA headers contained in this frame
        /// </summary>
        /// <returns>All LSA headers contained in this frame</returns>
        public LSAHeader[] GetItems()
        {
            return lLSAHeaders.ToArray();
        }

        /// <summary>
        /// Returns a bool indicating whether a specific LSA header is contained in this frame
        /// </summary>
        /// <param name="lsa">The LSA header to search for</param>
        /// <returns>A bool indicating whether a specific LSA header is contained in this frame</returns>
        public bool ContainsItem(LSAHeader lsa)
        {
            return lLSAHeaders.Contains(lsa);
        }

        /// <summary>
        /// Removes a specific LSA header
        /// </summary>
        /// <param name="lsa">The LSA header to remove</param>
        public void RemoveItem(LSAHeader lsa)
        {
            lLSAHeaders.Remove(lsa);
        }

        /// <summary>
        /// Returns the type of this frame.
        /// </summary>
        public override string FrameType
        {
            get { return OSPFLSAAcknowledgementMessage.DefaultFrameType; }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public OSPFLSAAcknowledgementMessage()
        {
            lLSAHeaders = new List<LSAHeader>();
        }

        /// <summary>
        /// Creates a new instance of this class by parsing the given data
        /// </summary>
        /// <param name="bData">The data to parse</param>
        public OSPFLSAAcknowledgementMessage(byte[] bData)
        {
            lLSAHeaders = new List<LSAHeader>();

            byte[] bLSAHeader = new byte[20];

            for (int iC1 = 0; iC1 < bData.Length; iC1 += 20)
            {
                for (int iC2 = 0; iC2 < 20; iC2++)
                {
                    bLSAHeader[iC2] = bData[iC2 + iC1];
                }
                lLSAHeaders.Add(new LSAHeader(bLSAHeader, false));
            }
        }

        /// <summary>
        /// Returns the raw byte representation of this frame
        /// </summary>
        public override byte[] FrameBytes
        {
            get
            {
                byte[] bData = new byte[this.Length];
                int iC1 = 0;

                foreach(LSAHeader lsaHeader in lLSAHeaders)
                {
                    lsaHeader.FrameBytes.CopyTo(bData, iC1);
                    iC1 += 20;
                }

                return bData;
            }
        }

        /// <summary>
        /// Returns the length of this frame in bytes
        /// </summary>
        public override int Length
        {
            get { return (lLSAHeaders.Count * 20); }
        }

        /// <summary>
        /// Returns an identical copy of this frame
        /// </summary>
        /// <returns>An identical copy of this frame</returns>
        public override Frame Clone()
        {
            return new OSPFLSAAcknowledgementMessage(this.FrameBytes);
        }
    }
}
