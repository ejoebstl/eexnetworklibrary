using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary.Routing.OSPF
{
    /// <summary>
    /// This class represents an OSPF router LSA
    /// </summary>
    public class RouterLSA : Frame
    {
        public static string DefaultFrameType { get { return "OSPFRouterLSA"; } }

        // 5 bit pad
        private bool bIsVirtualEndpoint;
        private bool bIsASBoundaryRouter;
        private bool bIsAreaBorderRouter;
        // 8 bit pad
        // 2 byte linkcount
        // n links

        private List<LinkItem> lLinks;

        /// <summary>
        /// Gets or sets a bool indicating whether the announced router is a virtual endpoint
        /// </summary>
        public bool IsVirtualEndpoint
        {
            get { return bIsVirtualEndpoint; }
            set { bIsVirtualEndpoint = value; }
        }

        /// <summary>
        /// Gets or sets a bool indicating whether the announced router is a boundary router
        /// </summary>
        public bool IsASBoundaryRouter
        {
            get { return bIsASBoundaryRouter; }
            set { bIsASBoundaryRouter = value; }
        }

        /// <summary>
        /// Gets or sets a bool indicating whether the announced router is an area border router
        /// </summary>
        public bool IsAreaBorderRouter
        {
            get { return bIsAreaBorderRouter; }
            set { bIsAreaBorderRouter = value; }
        }

        /// <summary>
        /// Clears all link items from this frame
        /// </summary>
        public void ClearLinkItems()
        {
            lLinks.Clear();
        }

        /// <summary>
        /// Adds a link item to this frame
        /// </summary>
        /// <param name="link">The link item to add</param>
        public void AddLinkItem(LinkItem link)
        {
            lLinks.Add(link);
        }

        /// <summary>
        /// Returns all link items contained in this router LSA
        /// </summary>
        /// <returns>All link items contained in this router LSA</returns>
        public LinkItem[] GetLinkItems()
        {
            return lLinks.ToArray();
        }

        /// <summary>
        /// Returns a bool indicating whether a specific link item is contained in this router LSA
        /// </summary>
        /// <param name="link">The link item to search for.</param>
        /// <returns>A bool indicating whether a specific link item is contained in this router LSA</returns>
        public bool ContainsLinkItem(LinkItem link)
        {
            return lLinks.Contains(link);
        }

        /// <summary>
        /// Removes a specific link item from this router LSA
        /// </summary>
        /// <param name="link">The link item to remove</param>
        public void RemoveLinkItem(LinkItem link)
        {
            lLinks.Remove(link);
        }

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        public RouterLSA()
        {
            bIsVirtualEndpoint = false;
            bIsASBoundaryRouter = false;
            bIsAreaBorderRouter = false;
            lLinks = new List<LinkItem>();
        }

        /// <summary>
        /// Creates a new instance of this class by parsing the given data.
        /// </summary>
        /// <param name="bData"></param>
        public RouterLSA(byte[] bData)
        {
            lLinks = new List<LinkItem>();
            bIsVirtualEndpoint = (bData[0] & 0x4) != 0;
            bIsASBoundaryRouter = (bData[0] & 0x2) != 0;
            bIsAreaBorderRouter = (bData[0] & 0x1) != 0;

            int iCount = (int)((bData[2] << 8) + bData[3]);
            int iC1 = 0;
            int iC2 = 4;

            while (iC1 < iCount)
            {
                LinkItem li = new LinkItem(bData, iC2);
                iC2 += li.Length;
                lLinks.Add(li);
                iC1++;
            }
        }

        /// <summary>
        /// Returns the type of this frame.
        /// </summary>
        public override string FrameType
        {
            get { return RouterLSA.DefaultFrameType; }
        }

        /// <summary>
        /// Returns the raw byte representation of this frame
        /// </summary>
        public override byte[] FrameBytes
        {
            get 
            {
                byte[] bData = new byte[this.Length];
                bData[0] |= (byte)(bIsAreaBorderRouter ? 0x1 : 0);
                bData[0] |= (byte)(bIsASBoundaryRouter ? 0x2 : 0);
                bData[0] |= (byte)(bIsVirtualEndpoint ? 0x4 : 0);

                int iCount = lLinks.Count;

                bData[2] = (byte)((iCount >> 8) & 0xFF);
                bData[3] = (byte)((iCount) & 0xFF);

                int iC1 = 4;

                foreach (LinkItem li in lLinks)
                {
                    li.Bytes.CopyTo(bData, iC1);
                    iC1 += li.Length;
                }

                return bData;
            }
        }

        /// <summary>
        /// Returns an identical copy of this router LSA
        /// </summary>
        /// <returns>An identical copy of this router LSA</returns>
        public override Frame Clone()
        {
            return new RouterLSA(this.FrameBytes);
        }

        /// <summary>
        /// Returns the length of this router LSA in bytes
        /// </summary>
        public override int Length
        {
            get
            {
                int iLen = 4;

                foreach(LinkItem li in lLinks)
                {
                    iLen += li.Length;
                }

                return iLen;
            }
        }

        /// <summary>
        /// This class represents a link item contained in a OSPF router LSA
        /// </summary>
        public class LinkItem : HelperStructure
        {
            private uint iLinkID; // 4 byte
            private uint iLinkData; // 4 byte
            private LinkType tType; // 1 byte
            // 1 byte count of TOS
            private short sZeroTOSMetric; // 2 byte
            // TOS... n * 4 byte
            private List<TOSItem> lTOS;

            /// <summary>
            /// Gets or sets the link ID
            /// </summary>
            public uint LinkID
            {
                get { return iLinkID; }
                set { iLinkID = value; }
            }

            /// <summary>
            /// Gets or sets the data associated with the link
            /// </summary>
            public uint LinkData
            {
                get { return iLinkData; }
                set { iLinkData = value; }
            }

            /// <summary>
            /// Gets or sets the type of the link
            /// </summary>
            public LinkType Type
            {
                get { return tType; }
                set { tType = value; }
            }

            /// <summary>
            /// Gets or sets the zero TOS metric
            /// </summary>
            public short ZeroTOSMetric
            {
                get { return sZeroTOSMetric; }
                set { sZeroTOSMetric = value; }
            }

            /// <summary>
            /// Clears all TOS items contained in this structure
            /// </summary>
            public void ClearTOSItems()
            {
                lTOS.Clear();
            }

            /// <summary>
            /// Adds a TOS item to this link item
            /// </summary>
            /// <param name="tos">The TOS item to add</param>
            public void AddTOSItem(TOSItem tos)
            {
                lTOS.Add(tos);
            }

            /// <summary>
            /// Returns all TOS items contained in this link item
            /// </summary>
            /// <returns>All TOS items contained in this link item</returns>
            public TOSItem[] GetTOSItems()
            {
                return lTOS.ToArray();
            }

            /// <summary>
            /// Returns a bool indicating whether a specific TOS item is contained in this link item
            /// </summary>
            /// <param name="tos">The TOS item to search for</param>
            /// <returns>A bool indicating whether a specific TOS item is contained in this link item</returns>
            public bool ContainsTOSItem(TOSItem tos)
            {
                return lTOS.Contains(tos);
            }

            /// <summary>
            /// Removes a specific TOS item from this link item
            /// </summary>
            /// <param name="tos">The TOS item to remove</param>
            public void RemoveTOSItem(TOSItem tos)
            {
                lTOS.Remove(tos);
            }

            /// <summary>
            /// Creates a new instance of this class
            /// </summary>
            public LinkItem()
            {
                iLinkData = 0;
                tType = LinkType.Unknown;
                sZeroTOSMetric = 0;
                lTOS = new List<TOSItem>();
            }

            /// <summary>
            /// Creates a new instance of this class by parsing the given data.
            /// </summary>
            /// <param name="bData">The data to parse</param>
            public LinkItem(byte[] bData)
                : this(bData, 0)
            { }

            /// <summary>
            /// Creates a new instance of this class by parsing the given data, starting at a given index.
            /// </summary>
            /// <param name="bData">The data to parse</param>
            /// <param name="iStartIndex">The index at which parsing starts</param>
            public LinkItem(byte[] bData, int iStartIndex)
            {
                lTOS = new List<TOSItem>();
                iLinkID = ((uint)bData[iStartIndex + 0] << 24) + ((uint)bData[iStartIndex + 1] << 16) + ((uint)bData[iStartIndex + 2] << 8) + bData[iStartIndex + 3];
                iLinkData = ((uint)bData[iStartIndex + 4] << 24) + ((uint)bData[iStartIndex + 5] << 16) + ((uint)bData[iStartIndex + 6] << 8) + bData[iStartIndex + 7];
                tType = (LinkType)bData[iStartIndex + 8];
                int iCount = (int)bData[iStartIndex + 9];
                sZeroTOSMetric = (short)(((int)bData[iStartIndex + 10] << 8) + bData[iStartIndex + 11]);

                byte[] bTemp = new byte[4];

                for (int iC1 = 0; iC1 < iCount; iC1++)
                {
                    for (int iC2 = 0; iC2 < 4; iC2++)
                    {
                        bTemp[iC2] = bData[iStartIndex + iC2 + (iC1 * 4) + 12];
                    }
                    lTOS.Add(new TOSItem(bTemp));
                }
            }

            /// <summary>
            /// Returns the length of this link item in bytes
            /// </summary>
            public override int Length
            {
                get { return 12 + (lTOS.Count * 4); }
            }

            /// <summary>
            /// Returns the raw byte representation of this frame
            /// </summary>
            public override byte[] Bytes
            {
                get
                {
                    byte[] bData = new byte[this.Length];

                    bData[0] = (byte)((iLinkID >> 24) & 0xFF);
                    bData[1] = (byte)((iLinkID >> 16) & 0xFF);
                    bData[2] = (byte)((iLinkID >> 8) & 0xFF);
                    bData[3] = (byte)((iLinkID) & 0xFF);
                    bData[4] = (byte)((iLinkData >> 24) & 0xFF);
                    bData[5] = (byte)((iLinkData >> 16) & 0xFF);
                    bData[6] = (byte)((iLinkData >> 8) & 0xFF);
                    bData[7] = (byte)(iLinkData & 0xFF);
                    bData[8] = (byte)tType;
                    bData[9] = (byte)lTOS.Count;
                    bData[10] = (byte)((sZeroTOSMetric >> 8) & 0xFF);
                    bData[11] = (byte)(sZeroTOSMetric & 0xFF);

                    int iC2 = 12;

                    foreach (TOSItem tos in lTOS)
                    {
                        tos.Bytes.CopyTo(bData, iC2);
                        iC2 += 4;
                    }

                    return bData;
                }
            }

            /// <summary>
            /// Compares this link item to another object.
            /// </summary>
            /// <param name="obj">The object to compare this item to</param>
            /// <returns>A bool indicating whether the object equals this instance</returns>
            public override bool Equals(object obj)
            {
                return obj is LinkItem &&
                    ((LinkItem)obj).iLinkData == this.iLinkData &&
                    ((LinkItem)obj).iLinkID == this.iLinkID &&
                    ((LinkItem)obj).sZeroTOSMetric == this.sZeroTOSMetric &&
                    ((LinkItem)obj).tType == this.tType;
            }

            /// <summary>
            /// Gets the hash code of this object
            /// </summary>
            /// <returns>The hash code of this object</returns>
            public override int GetHashCode()
            {
                return (int)((int)iLinkData ^ (int)iLinkID ^ (int)sZeroTOSMetric ^ (int)tType);
            }

        }

        /// <summary>
        /// An enum defining OSPF link types
        /// </summary>
        public enum LinkType
        {
            /// <summary>
            /// An unknown link
            /// </summary>
            Unknown = 0,
            /// <summary>
            /// A point to point link
            /// </summary>
            P2P = 1,
            /// <summary>
            /// A transit link
            /// </summary>
            Transit = 2,
            /// <summary>
            /// A stub link
            /// </summary>
            Stub = 3,
            /// <summary>
            /// A virtual link
            /// </summary>
            Virtual = 4
        }

        /// <summary>
        /// This class represents a TOS item contained in an OSPF 
        /// </summary>
        public class TOSItem : HelperStructure
        {
            private byte bTOS; // 1 byte
            // 1 byte zero
            private short sMetric; // 2 byte

            /// <summary>
            /// Gets or sets the metric
            /// </summary>
            public short Metric
            {
                get { return sMetric; }
                set { sMetric = value; }
            }

            /// <summary>
            /// Gets or sets the TOS (Type of Service)
            /// </summary>
            public byte TOS
            {
                get { return bTOS; }
                set { bTOS = value; }
            }

            /// <summary>
            /// Creates a new instance of this class
            /// </summary>
            public TOSItem()
            {
                bTOS = 0;
                sMetric = 0;
            }

            /// <summary>
            /// Creates a new instance of this class by parsing the given data
            /// </summary>
            /// <param name="bData">The data to parse</param>
            public TOSItem(byte[] bData)
            {
                bTOS = bData[0];
                sMetric = (short)((bData[2] << 8) + bData[3]);
            }

            /// <summary>
            /// Returns the length of a TOS item in bytes (4)
            /// </summary>
            public override int Length
            {
                get { return 4; }
            }

            /// <summary>
            /// Returns the raw byte representation of this frame
            /// </summary>
            public override byte[] Bytes
            {
                get
                {
                    byte[] bData = new byte[4];
                    bData[0] = bTOS;
                    bData[2] = (byte)((sMetric >> 8) & 0xFF);
                    bData[3] = (byte)((sMetric) & 0xFF);
                    return bData;
                }
            }

            /// <summary>
            /// Compares this TOS item to another object.
            /// </summary>
            /// <param name="obj">The object to compare this TOS item to</param>
            /// <returns>A bool indicating whether the given object and this instance are equal</returns>
            public override bool Equals(object obj)
            {
                return obj is TOSItem &&
                    ((TOSItem)obj).sMetric == this.sMetric &&
                    ((TOSItem)obj).bTOS == this.bTOS;
            }

            /// <summary>
            /// Gets the hash code of this object
            /// </summary>
            /// <returns>The hash code of this object</returns>
            public override int GetHashCode()
            {
                return (int)bTOS + (int)(sMetric * 256);
            }
        }
    }
}
