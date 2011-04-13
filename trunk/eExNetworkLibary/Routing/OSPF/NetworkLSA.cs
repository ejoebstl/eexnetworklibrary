using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary.Routing.OSPF
{
    /// <summary>
    /// This class represents an OSPF network LSA
    /// </summary>
    public class NetworkLSA : Frame
    {
        public static string DefaultFrameType { get { return "OSPFNetworkLSA"; } }

        private List<NetworkLSAItem> lItems; 
        private Subnetmask smNetmask;

        /// <summary>
        /// Gets or sets the netmask
        /// </summary>
        public Subnetmask Netmask
        {
            get { return smNetmask; }
            set { smNetmask = value; }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public NetworkLSA()
        {
            lItems = new List<NetworkLSAItem>();
        }

        /// <summary>
        /// Creates a new instance of this class by parsing the given data starting at the given index
        /// </summary>
        /// <param name="bData">The data to parse</param>
        /// <param name="iStartIndex">The index to start parsing from</param>
        public NetworkLSA(byte[] bData, int iStartIndex)
            : this()
        {
            byte[] bMaskData = new byte[4];
            for (int iC1 = iStartIndex; iC1 < 4; iC1++)
            {
                bMaskData[iC1 - iStartIndex] = bData[iC1];
            }
            smNetmask = new Subnetmask(bMaskData);
            for (int iC1 = iStartIndex + 4; iC1 < bData.Length; iC1 += 8)
            {
                lItems.Add(new NetworkLSAItem(bData, iC1));
            }
        }

        /// <summary>
        /// Creates a new instance of this class by parsing the given data
        /// </summary>
        /// <param name="bData">The data to parse</param>
        public NetworkLSA(byte[] bData) : this(bData, 0) { }
        
        /// <summary>
        /// Removes all network LSA items
        /// </summary>
        public void ClearNetworkItems()
        {
            lItems.Clear();
        }

        /// <summary>
        /// Adds a network LSA item
        /// </summary>
        /// <param name="net">The network LSA item to add</param>
        public void AddNetworkItem(NetworkLSAItem net)
        {
            lItems.Add(net);
        }

        /// <summary>
        /// Returns all network LSA items contained in this frame
        /// </summary>
        /// <returns>All network LSA items contained in this frame</returns>
        public NetworkLSAItem[] GetNetworkItems()
        {
            return lItems.ToArray();
        }

        /// <summary>
        /// Check whether a specific network LSA item is contained in this frame
        /// </summary>
        /// <param name="net">The network LSA item to search for</param>
        /// <returns>A bool indicating whether a specific network LSA item is contained in this frame</returns>
        public bool ContainsNetworkItem(NetworkLSAItem net)
        {
            return lItems.Contains(net);
        }

        /// <summary>
        /// Removes a network LSA item from this frame
        /// </summary>
        /// <param name="net">The network LSA item to remove</param>
        public void RemoveNetworkItem(NetworkLSAItem net)
        {
            lItems.Remove(net);
        }

        /// <summary>
        /// Returns the type of this frame.
        /// </summary>
        public override string FrameType
        {
            get { return NetworkLSA.DefaultFrameType; }
        }

        /// <summary>
        /// Returns the raw byte representation of this frame
        /// </summary>
        public override byte[] FrameBytes
        {
            get 
            {
                byte[] bData = new byte[this.Length];

                smNetmask.MaskBytes.CopyTo(bData, 0);

                int iC1 = 4;

                foreach (NetworkLSAItem lsaItem in lItems)
                {
                    lsaItem.Bytes.CopyTo(bData, iC1);
                    iC1 += 8;
                }

                return bData;
            }
        }

        /// <summary>
        /// Returns the length of this frame in bytes
        /// </summary>
        public override int Length
        {
            get { return lItems.Count * 8 + 4; }
        }

        /// <summary>
        /// This class represents a network LSA item used in a network LSA
        /// </summary>
        public class NetworkLSAItem
        {
            private Subnetmask smMask;
            private uint iAttachedRouterID;

            /// <summary>
            /// Gets or sets the attached router's ID
            /// </summary>
            public uint AttachedRouterID
            {
                get { return iAttachedRouterID; }
                set { iAttachedRouterID = value; }
            }

            /// <summary>
            /// Gets or sets the subnetmask
            /// </summary>
            public Subnetmask Mask
            {
                get { return smMask; }
                set { smMask = value; }
            }

            /// <summary>
            /// Creates a new instance of this class
            /// </summary>
            public NetworkLSAItem()
            {
                smMask = new Subnetmask();
                iAttachedRouterID = 0;
            }

            /// <summary>
            /// Creates a new instance of this class by parsing the given data starting at the given index
            /// </summary>
            /// <param name="bData">The data to parse</param>
            /// <param name="iStartIndex">The indext at which parsing begins</param>
            public NetworkLSAItem(byte[] bData, int iStartIndex)
            {
                smMask = new Subnetmask(new byte[] { bData[iStartIndex], bData[iStartIndex + 1], bData[iStartIndex + 2], bData[iStartIndex + 3] });
                iAttachedRouterID = ((uint)bData[iStartIndex + 4] << 24) + ((uint)bData[iStartIndex + 5] << 16) + ((uint)bData[iStartIndex + 6] << 8) + bData[iStartIndex + 7];
            }

            /// <summary>
            /// Creates a new instance of this class by parsing the given data
            /// </summary>
            /// <param name="bData">The data to parse</param>
            public NetworkLSAItem(byte[] bData) : this(bData, 0) { }

            /// <summary>
            /// Returns the length of this network LSA in bytes (8)
            /// </summary>
            public int Length
            {
                get { return 8; }
            }

            /// <summary>
            /// Returns the raw byte representation of this network LSA
            /// </summary>
            public byte[] Bytes
            {
                get
                {
                    byte[] bData = new byte[this.Length];
                    smMask.MaskBytes.CopyTo(bData, 0);
                    bData[4] = (byte)((iAttachedRouterID >> 24) & 0xFF);
                    bData[5] = (byte)((iAttachedRouterID >> 16) & 0xFF);
                    bData[6] = (byte)((iAttachedRouterID >> 8) & 0xFF);
                    bData[7] = (byte)((iAttachedRouterID) & 0xFF);

                    return bData;
                }
            }
        }

        /// <summary>
        /// Returns an identical copy of this frame
        /// </summary>
        /// <returns>An identical copy of this frame</returns>
        public override Frame Clone()
        {
            return new NetworkLSA(this.FrameBytes);
        }
    }
}
