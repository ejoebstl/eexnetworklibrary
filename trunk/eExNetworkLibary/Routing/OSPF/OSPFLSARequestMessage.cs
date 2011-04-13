using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary.Routing.OSPF
{
    /// <summary>
    /// This class represents an OSPF LSA request message
    /// </summary>
    public class OSPFLSARequestMessage : Frame
    {
        public static string DefaultFrameType { get { return "OSPFLSARequestMessage"; } }

        private List<LSARequestItem> lLSARequestList;

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public OSPFLSARequestMessage()
        {
            lLSARequestList = new List<LSARequestItem>();
        }

        /// <summary>
        /// Adds a LSA request item to this LSA request message
        /// </summary>
        /// <param name="item">The LSA request item to add</param>
        public void AddLSARequestItem(LSARequestItem item)
        {
            lLSARequestList.Add(item);
        }

        /// <summary>
        /// Removes a LSA request item from this LSA request message
        /// </summary>
        /// <param name="item">The LSA request item to remove</param>
        public void RemoveLSARequestItem(LSARequestItem item)
        {
            lLSARequestList.Remove(item);
        }

        /// <summary>
        /// Checkes whether a specific LSA request item is contained in this frame
        /// </summary>
        /// <param name="item">The item to search for</param>
        /// <returns>A bool indicating whether a specific LSA request item is contained in this frame</returns>
        public bool ContainsLSARequestItem(LSARequestItem item)
        {
            return lLSARequestList.Contains(item);
        }

        /// <summary>
        /// Returns all LSA request items contained in this message
        /// </summary>
        /// <returns>All LSA request items contained in this message</returns>
        public LSARequestItem[] GetLSARequestItems()
        {
            return lLSARequestList.ToArray();
        }

        /// <summary>
        /// Removes all LSA request items from this message
        /// </summary>
        public void ClearLSARequestItems()
        {
            lLSARequestList.Clear();
        }

        /// <summary>
        /// Creates a new instance of this class by parsing the given data
        /// </summary>
        /// <param name="bData">The data to parse</param>
        public OSPFLSARequestMessage(byte[] bData)
            : this()
        {
            byte[] bRequestItem = new byte[12];

            for (int iC1 = 0; iC1 < bData.Length; iC1 += 12)
            {
                for (int iC2 = 0; iC2 < 12; iC2++)
                {
                    bRequestItem[iC2] = bData[iC2 + iC1];
                }
                lLSARequestList.Add(new LSARequestItem(bRequestItem));
            }
        }

        /// <summary>
        /// Returns the type of this frame.
        /// </summary>
        public override string FrameType
        {
            get { return OSPFLSARequestMessage.DefaultFrameType; }
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

                foreach (LSARequestItem lsaRequest in lLSARequestList)
                {
                    lsaRequest.Bytes.CopyTo(bData, iC1);
                    iC1 += 12;
                }

                return bData;
            }
        }

        /// <summary>
        /// Returns the length of this frame in bytes
        /// </summary>
        public override int Length
        {
            get { return lLSARequestList.Count * 12; }
        }

        /// <summary>
        /// Represents a signle LSA request item contained in an OSPF LSA request message
        /// </summary>
        public class LSARequestItem : HelperStructure
        {
            private LSType lsType; // 4 byte
            private int iLinkStateID; // 4 byte
            private int iAdvertisingRouterID; // 4 byte

            /// <summary>
            /// Gets or sets the link state type
            /// </summary>
            public LSType LinkStateType
            {
                get { return lsType; }
                set { lsType = value; }
            }

            /// <summary>
            /// Gets or sets the link state ID
            /// </summary>
            public int LinkStateID
            {
                get { return iLinkStateID; }
                set { iLinkStateID = value; }
            }

            /// <summary>
            /// Gets or sets the advertising router ID
            /// </summary>
            public int AdvertisingRouterID
            {
                get { return iAdvertisingRouterID; }
                set { iAdvertisingRouterID = value; }
            }

            /// <summary>
            /// Creates a new instance of this class
            /// </summary>
            public LSARequestItem()
            {
                lsType = LSType.Unknown;
                iLinkStateID = 0;
                iAdvertisingRouterID = 0;
            }

            /// <summary>
            /// Creates a new instance of this class by parsing the given data
            /// </summary>
            /// <param name="bData">The data to parse</param>
            public LSARequestItem(byte[] bData)
                : this(bData, 0)
            { }


            /// <summary>
            /// Creates a new instance of this class by parsing the given data starting at a given index
            /// </summary>
            /// <param name="bData">The data to parse</param>
            /// <param name="iStartIndex">The index from which parsing starts</param>
            public LSARequestItem(byte[] bData, int iStartIndex)
            {
                lsType = (LSType)((int)bData[iStartIndex + 0] << 24) + ((int)bData[iStartIndex + 1] << 16) + ((int)bData[iStartIndex + 2] << 8) + bData[iStartIndex + 3];
                iLinkStateID = ((int)bData[iStartIndex + 4] << 24) + ((int)bData[iStartIndex + 5] << 16) + ((int)bData[iStartIndex + 6] << 8) + bData[iStartIndex + 7];
                iAdvertisingRouterID = ((int)bData[iStartIndex + 8] << 24) + ((int)bData[iStartIndex + 9] << 16) + ((int)bData[iStartIndex + 10] << 8) + bData[iStartIndex + 11];
            }

            /// <summary>
            /// Returns the length of this structure in bytes (12)
            /// </summary>
            public override int Length
            {
                get { return 12; }
            }

            /// <summary>
            /// Returns the raw byte representation of this LSA request item
            /// </summary>
            public override byte[] Bytes
            {
                get
                {
                    byte[] bData = new byte[this.Length];

                    bData[0] = (byte)((((int)lsType) >> 24) & 0xFF);
                    bData[1] = (byte)((((int)lsType) >> 16) & 0xFF);
                    bData[2] = (byte)((((int)lsType) >> 8) & 0xFF);
                    bData[3] = (byte)(((int)lsType) & 0xFF);
                    bData[4] = (byte)((iLinkStateID >> 24) & 0xFF);
                    bData[5] = (byte)((iLinkStateID >> 16) & 0xFF);
                    bData[6] = (byte)((iLinkStateID >> 8) & 0xFF);
                    bData[7] = (byte)(iLinkStateID & 0xFF);
                    bData[8] = (byte)((iAdvertisingRouterID >> 24) & 0xFF);
                    bData[9] = (byte)((iAdvertisingRouterID >> 16) & 0xFF);
                    bData[10] = (byte)((iAdvertisingRouterID >> 8) & 0xFF);
                    bData[11] = (byte)(iAdvertisingRouterID & 0xFF);

                    return bData;
                }
            }

            /// <summary>
            /// Compares this LSA request item to another object
            /// </summary>
            /// <param name="obj">The object to compare this instance to</param>
            /// <returns>A bool indicating whether this instance and the given object are equal</returns>
            public override bool Equals(object obj)
            {
                return obj is LSARequestItem &&
                    ((LSARequestItem)obj).lsType == this.lsType &&
                    ((LSARequestItem)obj).iAdvertisingRouterID == this.iAdvertisingRouterID &&
                    ((LSARequestItem)obj).iLinkStateID == this.iLinkStateID;
            }

            /// <summary>
            /// Returns the hash code of this LSA request item
            /// </summary>
            /// <returns>The hash code of this LSA request item</returns>
            public override int GetHashCode()
            {
                return iLinkStateID ^ iAdvertisingRouterID;
            }
        }

        /// <summary>
        /// Returns an identical copy of this frame
        /// </summary>
        /// <returns>An identical copy of this frame</returns>
        public override Frame Clone()
        {
            return new OSPFLSARequestMessage(this.FrameBytes);
        }
    }
}
