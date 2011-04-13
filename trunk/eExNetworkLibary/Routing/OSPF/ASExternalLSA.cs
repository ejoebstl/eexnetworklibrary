using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace eExNetworkLibrary.Routing.OSPF
{
    /// <summary>
    /// This class represents an OSPF autonomous system external LSA
    /// </summary>
    public class ASExternalLSA : Frame
    {
        public static string DefaultFrameType { get { return "OSPFASExternalLSA"; } }

        private List<ASExternalItem> lItems;
        private Subnetmask smNetmask;

        /// <summary>
        /// Gets or sets the subnetmask
        /// </summary>
        public Subnetmask Netmask
        {
            get { return smNetmask; }
            set { smNetmask = value; }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public ASExternalLSA()
        {
            lItems = new List<ASExternalItem>();
            smNetmask = new Subnetmask();
        }

        /// <summary>
        /// Creates a new instance of this class by parsing the given data starting at the given index
        /// </summary>
        /// <param name="bData">The data to parse</param>
        /// <param name="iStartIndex">The index to start parsing</param>
        public ASExternalLSA(byte[] bData, int iStartIndex)
            : this()
        {
            byte[] bMaskData = new byte[4];
            for (int iC1 = iStartIndex; iC1 < 4; iC1++)
            {
                bMaskData[iC1 - iStartIndex] = bData[iStartIndex];
            }
            smNetmask = new Subnetmask(bMaskData);
            for (int iC1 = iStartIndex + 12; iC1 < bData.Length; iC1 += 12)
            {
                lItems.Add(new ASExternalItem(bData, iC1));
            }
        }

        /// <summary>
        /// Creates a new instance of this class by parsing the given data
        /// </summary>
        /// <param name="bData">The data to parse</param>
        public ASExternalLSA(byte[] bData) : this(bData, 0) { }

        /// <summary>
        /// Removes all autonomous system external LSA items from this frame.
        /// </summary>
        public void ClearExternalItems()
        {
            lItems.Clear();
        }

        /// <summary>
        /// Adds a autonomous system external LSA item to this frame.
        /// </summary>
        /// <param name="lsa">The autonomous system external LSA item to add</param>
        public void AddExternalItem(ASExternalItem lsa)
        {
            lItems.Add(lsa);
        }

        /// <summary>
        /// Returns all autonomous system external LSA items contained in this frame
        /// </summary>
        /// <returns>All autonomous system external LSA items contained in this frame</returns>
        public ASExternalItem[] GetExternalItems()
        {
            return lItems.ToArray();
        }

        /// <summary>
        /// Returns a bool indicating whether a specific autonomous system external LSA item is contained in this frame
        /// </summary>
        /// <param name="lsa">The autonomous system external LSA item to search for</param>
        /// <returns>A bool indicating whether a specific autonomous system external LSA item is contained in this frame</returns>
        public bool ContainsExternalItem(ASExternalItem lsa)
        {
            return lItems.Contains(lsa);
        }

        /// <summary>
        /// Removes a autonomous system external LSA item from this frame.
        /// </summary>
        /// <param name="lsa">The autonomous system external LSA item to remove</param>
        public void RemoveExternalItem(ASExternalItem lsa)
        {
            lItems.Remove(lsa);
        }

        /// <summary>
        /// Returns the type of this frame.
        /// </summary>
        public override string FrameType
        {
            get { return ASExternalLSA.DefaultFrameType; }
        }

        /// <summary>
        /// Returns the raw byte representation of this autonomous system external LSA
        /// </summary>
        public override byte[] FrameBytes
        {
            get 
            {
                byte[] bData = new byte[this.Length];

                smNetmask.MaskBytes.CopyTo(bData, 0);

                int iC1 = 4;

                foreach (ASExternalItem lsaItem in lItems)
                {
                    lsaItem.Bytes.CopyTo(bData, iC1);
                    iC1 += 12;
                }

                return bData;
            }
        }

        /// <summary>
        /// Returns the length of this autonomous system external LSA in bytes
        /// </summary>
        public override int Length
        {
            get { return lItems.Count * 12 + 4; }
        }

        /// <summary>
        /// Returns an identical copy of this frame
        /// </summary>
        /// <returns>An identical copy of this frame</returns>
        public override Frame Clone()
        {
            return new ASExternalLSA(this.FrameBytes);
        }

        /// <summary>
        /// This class represents an autonomous system external LSA item contained in an autonomous system external LSA
        /// </summary>
        public class ASExternalItem
        {
            private bool bEBit; // 1 bit
            private byte bTOS; // 7 bit
            private int iMetric; //3 byte
            private IPAddress ipaAddress; //4 byte 
            private byte[] bExternalRouteTag; //4 byte

            /// <summary>
            /// Gets or sets the external route tag
            /// </summary>
            public byte[] ExternalRouteTag
            {
                get { return bExternalRouteTag; }
                set { bExternalRouteTag = value; }
            }

            /// <summary>
            /// Gets or sets the E-bit
            /// </summary>
            public bool EBit
            {
                get { return bEBit; }
                set { bEBit = value; }
            }

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
            /// Gets or sets the IP-address
            /// </summary>
            public IPAddress Address
            {
                get { return ipaAddress; }
                set { ipaAddress = value; }
            }

            /// <summary>
            /// Creates a new instance of this class
            /// </summary>
            public ASExternalItem()
            {
                bEBit = false;
                bTOS = 0;
                iMetric = 0;
                ipaAddress = IPAddress.Any;
                bExternalRouteTag = new byte[4];
            }

            /// <summary>
            /// Creates a new instance of this class by parsing the given data starting at a given index
            /// </summary>
            /// <param name="bData">The data to parse</param>
            /// <param name="iStartIndex">The index from which parsing starts</param>
            public ASExternalItem(byte[] bData, int iStartIndex)
            {
                bEBit = (bData[iStartIndex] & 0x80) > 0;
                bTOS = (byte)(bData[iStartIndex] & 0x7F); 
                iMetric = (((int)bData[iStartIndex + 1] << 16) + ((int)bData[iStartIndex + 2] << 8) + bData[iStartIndex + 3]);

                byte[] bAddressBytes = new byte[4];

                for (int iC1 = iStartIndex + 4; iC1 < iStartIndex + 8; iC1++)
                {
                    bAddressBytes[iC1 - iStartIndex] = bData[iC1];
                }

                ipaAddress = new IPAddress(bAddressBytes);

                bExternalRouteTag = new byte[4];

                for (int iC1 = iStartIndex + 8; iC1 < iStartIndex + 12; iC1++)
                {
                    bExternalRouteTag[iC1 - iStartIndex] = bData[iC1];
                }
            }

            /// <summary>
            /// Creates a new instance of this class by parsing the given data starting at a given index
            /// </summary>
            /// <param name="bData">The data to parse</param>
            public ASExternalItem(byte[] bData) : this(bData, 0) { }

            /// <summary>
            /// Gets the length of this autonomous system external LSA item in bytes (12)
            /// </summary>
            public int Length
            {
                get { return 12; }
            }

            /// <summary>
            /// Gets the raw byte representation of this frame
            /// </summary>
            public byte[] Bytes
            {
                get 
                {
                    byte[] bData = new byte[this.Length];
                    bData[0] = bTOS;
                    bData[0] &= 0x7F;
                    bData[0] |= (byte)(bEBit ? 0x80 : 0);
                    bData[1] = (byte)((iMetric >> 16) & 0xFF);
                    bData[2] = (byte)((iMetric >> 8) & 0xFF);
                    bData[3] = (byte)((iMetric) & 0xFF);

                    ipaAddress.GetAddressBytes().CopyTo(bData, 4);
                    bExternalRouteTag.CopyTo(bData, 8);

                    return bData;
                }
            }
        }
    }
}
