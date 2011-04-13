using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary.Routing.OSPF
{
    /// <summary>
    /// This frame represents an OSPF summary LSA
    /// </summary>
    public class SummaryLSA : Frame
    {
        private List<SummaryLSAItem> lItems;
        private Subnetmask smNetmask;

        public static string DefaultFrameType { get { return "OSPFSummaryLSA"; } }

        /// <summary>
        /// Gets or sets the subnetmask for the summary LSA
        /// </summary>
        public Subnetmask Netmask
        {
            get { return smNetmask; }
            set { smNetmask = value; }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public SummaryLSA()
        {
            lItems = new List<SummaryLSAItem>();
            smNetmask = new Subnetmask();
        }

        /// <summary>
        /// Creates a new instance of this class by parsing the given data beginning from the given start index.
        /// </summary>
        /// <param name="bData">The data to parse</param>
        /// <param name="iStartIndex">The index to start parsing from</param>
        public SummaryLSA(byte[] bData, int iStartIndex)
            : this()
        {
            byte[] bMaskData = new byte[4];
            for (int iC1 = iStartIndex; iC1 < 4; iC1++)
            {
                bMaskData[iC1 - iStartIndex] = bData[iStartIndex];
            }
            smNetmask = new Subnetmask(bMaskData);
            for (int iC1 = iStartIndex + 4; iC1 < bData.Length; iC1 += 4)
            {
                lItems.Add(new SummaryLSAItem(bData, iC1));
            }
        }

        /// <summary>
        /// Creates a new instance of this class by parsing the given data.
        /// </summary>
        /// <param name="bData">The data to parse</param>
        public SummaryLSA(byte[] bData) : this(bData, 0) { }

        /// <summary>
        /// Clears all LSA summary items
        /// </summary>
        public void ClearSummaryItems()
        {
            lItems.Clear();
        }

        /// <summary>
        /// Adds a LSA summary item to this frame.
        /// </summary>
        /// <param name="net">The LSA summary item to add</param>
        public void AddSummaryItem(SummaryLSAItem net)
        {
            lItems.Add(net);
        }

        /// <summary>
        /// Returns all LSA summary items contained in this frame.
        /// </summary>
        /// <returns>All LSA summary items contained in this frame</returns>
        public SummaryLSAItem[] GetSummaryItems()
        {
            return lItems.ToArray();
        }

        /// <summary>
        /// Returns a bool indicating whether this frame contains a specific summary LSA item.
        /// </summary>
        /// <param name="net">The summary LSA item to search for</param>
        /// <returns>A bool indicating whether this frame contains a specific summary LSA item</returns>
        public bool ContainsSummaryItem(SummaryLSAItem net)
        {
            return lItems.Contains(net);
        }

        /// <summary>
        /// Removes a summary LSA item from this frame.
        /// </summary>
        /// <param name="net">The summary LSA item to remove</param>
        public void RemoveSummaryItem(SummaryLSAItem net)
        {
            lItems.Remove(net);
        }

        /// <summary>
        /// Returns the type of this frame.
        /// </summary>
        public override string FrameType
        {
            get { return SummaryLSA.DefaultFrameType; }
        }

        /// <summary>
        /// Returns the raw byte representation of this frame.
        /// </summary>
        public override byte[] FrameBytes
        {
            get 
            {
                byte[] bData = new byte[this.Length];

                smNetmask.MaskBytes.CopyTo(bData, 0);

                int iC1 = 4;

                foreach (SummaryLSAItem lsaItem in lItems)
                {
                    lsaItem.Bytes.CopyTo(bData, iC1);
                    iC1 += 4;
                }

                return bData;
            }
        }

        /// <summary>
        /// Returns the length of this frame in bytes
        /// </summary>
        public override int Length
        {
            get { return lItems.Count * 4 + 4; }
        }

        /// <summary>
        /// Represents a summary LSA item contained in a summary LSA
        /// </summary>
        public class SummaryLSAItem : HelperStructure
        {
            private byte bTOS;
            private int iMetric;

            /// <summary>
            /// Gets or sets the TOS
            /// </summary>
            public byte TOS
            {
                get { return bTOS; }
                set { bTOS = value; }
            }

            /// <summary>
            /// Gets or sets the metric
            /// </summary>
            public int Metric
            {
                get { return iMetric; }
                set { iMetric = value; }
            }

            /// <summary>
            /// Creates a new instance of this class
            /// </summary>
            public SummaryLSAItem()
            {
                bTOS = 0;
                iMetric = 0;
            }

            /// <summary>
            /// Creates a new instance of this class by parsing the given data starting at a given index.
            /// </summary>
            /// <param name="bData">The data to parse</param>
            /// <param name="iStartIndex">The index to start parsing from</param>
            public SummaryLSAItem(byte[] bData, int iStartIndex)
            {
                bTOS = bData[iStartIndex];
                iMetric = (((int)bData[iStartIndex + 1] << 16) + ((int)bData[iStartIndex + 2] << 8) + bData[iStartIndex + 3]);
            }

            /// <summary>
            /// reates a new instance of this class by parsing the given data
            /// </summary>
            /// <param name="bData">The data to parse</param>
            public SummaryLSAItem(byte[] bData) : this(bData, 0) { }

            /// <summary>
            /// Returns 4, the length of every LSA item in bytes
            /// </summary>
            public override int Length
            {
                get { return 4; }
            }

            /// <summary>
            /// Returns the raw byte representation of this helper structure.
            /// </summary>
            public override byte[] Bytes
            {
                get
                {
                    byte[] bData = new byte[this.Length];
                    bData[0] = bTOS;
                    bData[1] = (byte)((iMetric >> 16) & 0xFF);
                    bData[2] = (byte)((iMetric >> 8) & 0xFF);
                    bData[3] = (byte)((iMetric) & 0xFF);

                    return bData;
                }
            }
        }

        /// <summary>
        /// Returns an identical copy of this summary LSA
        /// </summary>
        /// <returns>An identical copy of this summary LSA</returns>
        public override Frame Clone()
        {
            return new SummaryLSA(this.FrameBytes);
        }

    }
}
