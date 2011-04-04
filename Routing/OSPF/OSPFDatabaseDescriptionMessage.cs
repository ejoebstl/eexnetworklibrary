using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary.Routing.OSPF
{
    /// <summary>
    /// Represents an OSPF database description message
    /// </summary>
    public class OSPFDatabaseDescriptionMessage : Frame
    {
        // 2 byte MTU
        private short sInterfaceMTU;
        // 1 byte options
        private OSPFOptionsField ospfOptions;
        // 1 byte options
        private bool bIBit;
        private bool bMBit;
        private bool bMSBit;
        private bool bOOBResyncBit;
        // 4 byte seqnum
        private uint iDDSequenceNumber;

        private List<LSAHeader> lLSAHeaders;

        #region props

        /// <summary>
        /// Gets or sets the OSPF options field
        /// </summary>
        public OSPFOptionsField Options
        {
            get { return ospfOptions; }
            set { ospfOptions = value; }
        }

        /// <summary>
        /// Gets or sets the sequence number
        /// </summary>
        public uint SequenceNumber
        {
            get { return iDDSequenceNumber; }
            set { iDDSequenceNumber = value; }
        }

        /// <summary>
        /// Gets or sets the interface MTU
        /// </summary>
        public short InterfaceMTU
        {
            get { return sInterfaceMTU; }
            set { sInterfaceMTU = value; }
        }

        /// <summary>
        /// Gets or sets a bool indicating whether the init-bit is set
        /// </summary>
        public bool IsInit
        {
            get { return bIBit; }
            set { bIBit = value; }
        }

        /// <summary>
        /// Gets or sets a bool indicating whether the OOB-resync-bit is set
        /// </summary>
        public bool IsOOBResync
        {
            get { return bOOBResyncBit; }
            set { bOOBResyncBit = value; }
        }

        /// <summary>
        /// Gets or sets a bool indicating whether the more-bit is set
        /// </summary>
        public bool IsMore
        {
            get { return bMBit; }
            set { bMBit = value; }
        }

        /// <summary>
        /// Gets or sets a bool indicating whether the master-bit is set
        /// </summary>
        public bool IsMaster
        {
            get { return bMSBit; }
            set { bMSBit = value; }
        }

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
        /// Returns FrameType.OSPFDatabaseDescription
        /// </summary>
        public override FrameType FrameType
        {
            get { return FrameType.OSPFDatabaseDescription; }
        }

        #endregion

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public OSPFDatabaseDescriptionMessage()
        {
            sInterfaceMTU = 0;
            ospfOptions = new OSPFOptionsField();

            iDDSequenceNumber = 0;
            lLSAHeaders = new List<LSAHeader>();
        }

        /// <summary>
        /// Creates a new instance of this class by parsing the given data
        /// </summary>
        /// <param name="bData">The data to parse</param>
        public OSPFDatabaseDescriptionMessage(byte[] bData)
        {
            sInterfaceMTU = (short)(((int)bData[0] << 8) + bData[1]);
            ospfOptions = new OSPFOptionsField(bData[2]);
            bMSBit = (bData[3] & 0x1) != 0;
            bMBit = (bData[3] & 0x2) != 0;
            bIBit = (bData[3] & 0x4) != 0;
            bOOBResyncBit = (bData[3] & 0x8) != 0;
            iDDSequenceNumber = ((uint)bData[4] << 24) + ((uint)bData[5] << 16) + ((uint)bData[6] << 8) + bData[7];

            lLSAHeaders = new List<LSAHeader>();

            byte[] bLSAHeader = new byte[20];

            for (int iC1 = 8; iC1 < bData.Length; iC1 += 20)
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
                bData[0] = (byte)((sInterfaceMTU >> 8) & 0xFF);
                bData[1] = (byte)((sInterfaceMTU) & 0xFF);
                bData[2] = ospfOptions.Data;
                bData[3] |= (byte)(bMSBit ? 0x1 : 0);
                bData[3] |= (byte)(bMBit ? 0x2 : 0);
                bData[3] |= (byte)(bIBit ? 0x4 : 0);
                bData[3] |= (byte)(bOOBResyncBit ? 0x8 : 0);
                bData[4] = (byte)((iDDSequenceNumber >> 24) & 0xFF);
                bData[5] = (byte)((iDDSequenceNumber >> 16) & 0xFF);
                bData[6] = (byte)((iDDSequenceNumber >> 8) & 0xFF);
                bData[7] = (byte)((iDDSequenceNumber) & 0xFF);

                int iC1 = 8;

                foreach(LSAHeader lsaHeader in lLSAHeaders)
                {
                    byte[] bHeader = lsaHeader.FrameBytes;
                    for (int iC2 = 0; iC2 < 20; iC2++)
                    {
                        bData[iC1 + iC2] = bHeader[iC2];
                    }
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
            get { return 8 + (lLSAHeaders.Count * 20); }
        }

        /// <summary>
        /// Returns an identical copy of this frame
        /// </summary>
        /// <returns>An identical copy of this frame</returns>
        public override Frame Clone()
        {
            return new OSPFDatabaseDescriptionMessage(this.FrameBytes);
        }
    }
}
