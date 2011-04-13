using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using eExNetworkLibrary.Utilities;

namespace eExNetworkLibrary.Routing.OSPF
{
    /// <summary>
    /// This class represents an OSPF LSA header, the common part of each LSA
    /// </summary>
    public class LSAHeader : Frame
    {
        public static string DefaultFrameType { get { return "OSPFLSAHeader"; } }

        private short iLSAge; // 2 byte
        private OSPFOptionsField ospfOptions;
        private LSType lsType; // 2 bytes
        private uint iLinkStateID;
        private uint iAdvertisingRouter;
        private uint iLSSequenceNumber;
        private int iOrigalLength;
        // 2 byte checksum
        // 2 byte length
        private ChecksumCalculator cCalc;
        private byte[] bOriginalChecksum;

        /// <summary>
        /// Defines the LS maximum age
        /// </summary>
        public const int LS_MaxAge = 3600;

        /// <summary>
        /// Cached original Length for LSA Acknowledgements and DB Descriptions. Set this value to -1 to use a self-calculated length in the output frame.
        /// This property is important for database descriptions and LSA acknowledgements, because LSA headers without a body are used.
        /// </summary>
        public int OrigalLength
        {
            get { return iOrigalLength; }
            set { iOrigalLength = value; }
        }

        /// <summary>
        /// Cached original checksum for LSA Acknowledgements and DB Descriptions. Set this array to a zero-length array to use a self-calculated checksum in the output frame. 
        /// This property is important for database descriptions and LSA acknowledgements, because LSA headers without a body are used.
        /// </summary>
        public byte[] OriginalChecksum
        {
            get { return bOriginalChecksum; }
            set { bOriginalChecksum = value; }
        }

        /// <summary>
        /// Gets or sets the OSPF options field
        /// </summary>
        public OSPFOptionsField Options
        {
            get { return ospfOptions; }
            set { ospfOptions = value; }
        }

        /// <summary>
        /// Gets or sets the LS type
        /// </summary>
        public LSType LSType
        {
            get { return lsType; }
            set { lsType = value; }
        }

        /// <summary>
        /// Gets or sets the link state ID
        /// </summary>
        public uint LinkStateID
        {
            get { return iLinkStateID; }
            set { iLinkStateID = value; }
        }

        /// <summary>
        /// Gets or sets the ID of the advertising router
        /// </summary>
        public uint AdvertisingRouter
        {
            get { return iAdvertisingRouter; }
            set { iAdvertisingRouter = value; }
        }

        /// <summary>
        /// Gets or sets the sequence number
        /// </summary>
        public uint SequenceNumber
        {
            get { return iLSSequenceNumber; }
            set { iLSSequenceNumber = value; }
        }

        /// <summary>
        /// Gets or sets the LS age
        /// </summary>
        public short LSAge
        {
            get { return iLSAge; }
            set { iLSAge = value; }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public LSAHeader()
        {
            iLSAge = 0;
            ospfOptions = new OSPFOptionsField();
            lsType = LSType.Unknown;
            iLinkStateID = 0;
            iAdvertisingRouter = 0;
            iLSSequenceNumber = 0;
            cCalc = new ChecksumCalculator();
            iOrigalLength = -1;
            bOriginalChecksum = new byte[0];
        }

        /// <summary>
        /// Creates a new instance of this class by parsing the given data.
        /// This constructor also creates the LSA body,
        /// which is set as encapsulated frame of the created instance.
        /// </summary>
        /// <param name="bData">The data to parse</param>
        public LSAHeader(byte[] bData)
            : this(bData, true)
        { }

        /// <summary>
        /// Creates a new instance of this class by parsing the given data.
        /// </summary>
        /// <param name="bData">The data to parse</param>
        /// <param name="bCreateBody">A bool indicating whether a LSA body should be created. Set this property to false for database descriptions and LS acknowledgements messages</param>
        public LSAHeader(byte[] bData, bool bCreateBody)
        {
            cCalc = new ChecksumCalculator();
            iLSAge = (short)((bData[0] << 8) + bData[1]);
            ospfOptions = new OSPFOptionsField(bData[2]);
            lsType = (LSType)bData[3];
            iLinkStateID = ((uint)bData[4] << 24) + ((uint)bData[5] << 16) + ((uint)bData[6] << 8) + bData[7];
            iAdvertisingRouter = ((uint)bData[8] << 24) + ((uint)bData[9] << 16) + ((uint)bData[10] << 8) + bData[11];

            iLSSequenceNumber = ((uint)bData[12] << 24) + ((uint)bData[13] << 16) + ((uint)bData[14] << 8) + bData[15];
            // 2 byte checksum
            int iLen = ((bData[18] << 8) + bData[19]) - 20;

            if (bCreateBody)
            {
                iOrigalLength = -1;
                bOriginalChecksum = new byte[0];
                byte[] bBodyBytes = new byte[iLen];

                for (int iC1 = 0; iC1 < iLen; iC1++)
                {
                    bBodyBytes[iC1] = bData[iC1 + 20];
                }

                if (bBodyBytes.Length > 0)
                {
                    if (lsType == LSType.Router)
                    {
                        fEncapsulatedFrame = new RouterLSA(bBodyBytes);
                    }
                    else if (lsType == LSType.External)
                    {
                        fEncapsulatedFrame = new ASExternalLSA(bBodyBytes);
                    }
                    else if (lsType == LSType.Network)
                    {
                        fEncapsulatedFrame = new NetworkLSA(bBodyBytes);
                    }
                    else if (lsType == LSType.Summary_IP)
                    {
                        fEncapsulatedFrame = new SummaryLSA(bBodyBytes);
                    }
                    else if (lsType == LSType.Summary_ASBR)
                    {
                        fEncapsulatedFrame = new SummaryLSA(bBodyBytes);
                    }
                    else if (lsType == LSType.NSSA)
                    {
                        fEncapsulatedFrame = new ASExternalLSA(bBodyBytes);
                    }
                    else
                    {
                        fEncapsulatedFrame = new RawDataFrame(bBodyBytes);
                    }
                }
            }
            else
            {
                iOrigalLength = iLen + 20;
                bOriginalChecksum = new byte[2];
                bOriginalChecksum[0] = bData[16];
                bOriginalChecksum[1] = bData[17];
            }
        }

        /// <summary>
        /// Returns the type of this frame.
        /// </summary>
        public override string FrameType
        {
            get { return LSAHeader.DefaultFrameType; }
        }

        /// <summary>
        /// Returns the raw byte representation of this frame
        /// </summary>
        public override byte[] FrameBytes
        {
            get 
            {
                byte[] bData = new byte[this.Length];
                //Don't insert LSAge now
                bData[2] = ospfOptions.Data;
                bData[3] = (byte)lsType;

                bData[4] = (byte)((iLinkStateID >> 24) & 0xFF);
                bData[5] = (byte)((iLinkStateID >> 16) & 0xFF);
                bData[6] = (byte)((iLinkStateID >> 8) & 0xFF);
                bData[7] = (byte)((iLinkStateID) & 0xFF);

                bData[8] = (byte)((iAdvertisingRouter >> 24) & 0xFF);
                bData[9] = (byte)((iAdvertisingRouter >> 16) & 0xFF);
                bData[10] = (byte)((iAdvertisingRouter >> 8) & 0xFF);
                bData[11] = (byte)((iAdvertisingRouter) & 0xFF);


                bData[12] = (byte)((iLSSequenceNumber >> 24) & 0xFF);
                bData[13] = (byte)((iLSSequenceNumber >> 16) & 0xFF);
                bData[14] = (byte)((iLSSequenceNumber >> 8) & 0xFF);
                bData[15] = (byte)((iLSSequenceNumber) & 0xFF);

                //Don't insert checksum now. we will calculate it later. 

                if (iOrigalLength == -1)
                {
                    bData[18] = (byte)((bData.Length >> 8) & 0xFF);
                    bData[19] = (byte)((bData.Length) & 0xFF);
                }
                else
                {
                    bData[18] = (byte)((iOrigalLength >> 8) & 0xFF);
                    bData[19] = (byte)((iOrigalLength) & 0xFF);
                }

                if (fEncapsulatedFrame != null)
                {
                    byte[] bBodyBytes = fEncapsulatedFrame.FrameBytes;

                    for (int iC1 = 0; iC1 < bBodyBytes.Length; iC1++)
                    {
                        bData[iC1 + 20] = bBodyBytes[iC1];
                    }
                }

                //Calculate Checksum and insert LSAge

                byte[] bChecksum;
                if(bOriginalChecksum.Length != 2)
                {
                    bChecksum = cCalc.CalculateChecksum(bData);
                }
                else
                {
                    bChecksum = bOriginalChecksum;
                }

                bData[16] = bChecksum[0];
                bData[17] = bChecksum[1];

                bData[0] = (byte)((iLSAge >> 8) & 0xFF);
                bData[1] = (byte)((iLSAge) & 0xFF);

                return bData;
            }
        }

        /// <summary>
        /// Gets the length of this frame and its encapsulated frame in bytes
        /// </summary>
        public override int Length
        {
            get
            {
                int iLen = 20 + (fEncapsulatedFrame != null ? fEncapsulatedFrame.Length : 0);
                return iLen % 2 == 0 ? iLen : iLen + 1;
            }
        }

        /// <summary>
        /// Returns an identical copy of this frame
        /// </summary>
        /// <returns>An identical copy of this frame</returns>
        public override Frame Clone()
        {
            return new LSAHeader(this.FrameBytes);
        }
    }

    /// <summary>
    /// An enumeration for diffrent LSA types
    /// </summary>
    public enum LSType
    {
        /// <summary>
        /// Unknwon
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Router LSA
        /// </summary>
        Router = 1,
        /// <summary>
        /// Network LSA
        /// </summary>
        Network = 2,
        /// <summary>
        /// Summary LSA
        /// </summary>
        Summary_IP = 3,
        /// <summary>
        /// Autonomous system border router summary
        /// </summary>
        Summary_ASBR = 4,
        /// <summary>
        /// External
        /// </summary>
        External = 5,
        /// <summary>
        /// Group membership
        /// </summary>
        GroupMembership = 6,
        /// <summary>
        /// NSSA
        /// </summary>
        NSSA = 7,
        /// <summary>
        /// Not used / reserved
        /// </summary>
        NotUsed = 8,
        /// <summary>
        /// Opaque9
        /// </summary>
        Opaque9 = 9,
        /// <summary>
        /// Opaque10
        /// </summary>
        Opaque10 = 10,
        /// <summary>
        /// Opaque11
        /// </summary>
        Opaque11 = 11
    }
}
