using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.Utilities;

namespace eExNetworkLibrary.Routing.OSPF
{
    /// <summary>
    /// This class represents the OSPF common header of all OSPF messages.
    /// The specific OSPF message for this header (Database description, Hello message, etc.) should be
    /// placed as encapsulated frame of this frame.
    /// </summary>
    public class OSPFCommonHeader : Frame
    {
        public static string DefaultFrameType { get { return FrameTypes.OSPF; } }

        private byte bVersion; //1 byte version
        private OSPFFrameType tType; //1 byte type
        //2 byte packet len
        private uint iRouterID;
        private uint iAreaID;
        //2 byte checksum
        private OSPFAuthenticationType oAuthType;
        private byte[] bAuthentication;
        private byte[] bAttachedData;

        #region Props

        /// <summary>
        /// Gets or sets the OSPF version
        /// </summary>
        public byte Version
        {
            get { return bVersion; }
            set { bVersion = value; }
        }

        /// <summary>
        /// Gets or sets the OSPF frame type
        /// </summary>
        public OSPFFrameType OSPFType
        {
            get { return tType; }
            set { tType = value; }
        }

        /// <summary>
        /// Gets or sets the router ID
        /// </summary>
        public uint RouterID
        {
            get { return iRouterID; }
            set { iRouterID = value; }
        }

        /// <summary>
        /// Gets or sets the area ID
        /// </summary>
        public uint AreaID
        {
            get { return iAreaID; }
            set { iAreaID = value; }
        }

        /// <summary>
        /// Gets or sets the OSPF authentication type
        /// </summary>
        public OSPFAuthenticationType AuthType
        {
            get { return oAuthType; }
            set { oAuthType = value; }
        }

        /// <summary>
        /// Gets or sets the value of the authentication data
        /// </summary>
        public byte[] AuthenticationValue
        {
            get { return bAuthentication; }
            set { bAuthentication = value; }
        }

        /// <summary>
        /// Gets or sets the attached data, for example, LLS data
        /// </summary>
        public byte[] AttachedData
        {
            get { return bAttachedData; }
            set { bAttachedData = value; }
        }

        #endregion

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public OSPFCommonHeader()
        {
            bVersion = 2;
            tType = OSPFFrameType.Hello;
            iRouterID = 0;
            iAreaID = 0;
            oAuthType = OSPFAuthenticationType.NoAuthentication;
            bAuthentication = new byte[8];
            bAttachedData = new byte[0];
        }

        /// <summary>
        /// Creates a new instance of this class by parsing the given data.
        /// The corresponding sub-frames (Database description, Hello message etc.) will 
        /// automatically instanced and placed into this frames encapsulated frame property.
        /// </summary>
        /// <param name="bData">The data to parse</param>
        public OSPFCommonHeader(byte[] bData)
        {
            bVersion = bData[0];
            tType = (OSPFFrameType)bData[1];
            int iLen = ((int)bData[2] << 8) + bData[3];
            iRouterID = ((uint)bData[4] << 24) + ((uint)bData[5] << 16) + ((uint)bData[6] << 8) + bData[7];
            iAreaID = ((uint)bData[8] << 24) + ((uint)bData[9] << 16) + ((uint)bData[10] << 8) + bData[11];
            // 2 byte checksum pad
            oAuthType = (OSPFAuthenticationType)(((int)bData[14] << 8) + bData[15]);
            bAuthentication = new byte[8];
            for (int iC1 = 16; iC1 < 24; iC1++ )
            {
                bAuthentication[iC1 - 16] = bData[iC1];
            }

            byte[] bEncFrameBytes = new byte[iLen - 24];

            for (int iC1 = 24; iC1 < iLen; iC1++)
            {
                bEncFrameBytes[iC1 - 24] = bData[iC1];
            }

            switch (tType)
            {
                case OSPFFrameType.DatabaseDescription: this.fEncapsulatedFrame = new OSPFDatabaseDescriptionMessage(bEncFrameBytes);
                    break;
                case OSPFFrameType.Hello: this.fEncapsulatedFrame = new OSPFHelloMessage(bEncFrameBytes);
                    break;
                case OSPFFrameType.LinkStateAcknowledgement: this.fEncapsulatedFrame = new OSPFLSAAcknowledgementMessage(bEncFrameBytes);
                    break;
                case OSPFFrameType.LinkStateUpdate: this.fEncapsulatedFrame = new OSPFLSAUpdateMessage(bEncFrameBytes);
                    break;
                case OSPFFrameType.LinkStateRequest: this.fEncapsulatedFrame = new OSPFLSARequestMessage(bEncFrameBytes);
                    break;
                default: this.fEncapsulatedFrame = new RawDataFrame(bEncFrameBytes);
                    break;
            }

            bAttachedData = new byte[bData.Length - iLen];
            for (int iC1 = iLen; iC1 < bData.Length; iC1++)
            {
                bAttachedData[iC1 - iLen] = bData[iC1];
            }
        }

        /// <summary>
        /// Returns the type of this frame.
        /// </summary>
        public override string FrameType
        {
            get { return OSPFCommonHeader.DefaultFrameType; }
        }

        /// <summary>
        /// Gets the raw byte representation of this frame and the encapsulated frame
        /// </summary>
        public override byte[] FrameBytes
        {
            get 
            {
                int iLen = this.Length;
                byte[] bData = new byte[iLen];
                bData[0] = (byte)bVersion;
                bData[1] = (byte)tType;
                bData[2] = (byte)(((iLen - bAttachedData.Length) >> 8) & 0xFF);
                bData[3] = (byte)((iLen - bAttachedData.Length) & 0xFF);
                bData[4] = (byte)((iRouterID >> 24) & 0xFF);
                bData[5] = (byte)((iRouterID >> 16) & 0xFF);
                bData[6] = (byte)((iRouterID >> 8) & 0xFF);
                bData[7] = (byte)((iRouterID) & 0xFF);
                bData[8] = (byte)((iAreaID >> 24) & 0xFF);
                bData[9] = (byte)((iAreaID >> 16) & 0xFF);
                bData[10] = (byte)((iAreaID >> 8) & 0xFF);
                bData[11] = (byte)((iAreaID) & 0xFF);

                //Leave Checksum Fields Empty - Will have to calculate this later. 

                bData[14] = (byte)(((int)oAuthType >> 8) & 0xFF);
                bData[15] = (byte)(((int)oAuthType) & 0xFF);

                //Leave Authentication Fields Empty - Will have to add this after checksum.

                byte[] bEncFrameBytes = EncapsulatedFrame.FrameBytes;

                for (int iC1 = 24; iC1 < bEncFrameBytes.Length + 24; iC1++)
                {
                    bData[iC1] = bEncFrameBytes[iC1 - 24];
                }

                //Calculate the checksum
                byte[] bChecksum = ChecksumCalculator.CalculateChecksum(bData);

                //Insert the checksum
                bData[12] = bChecksum[0];
                bData[13] = bChecksum[1];

                //Insert the authentication            
                for (int iC1 = 16; iC1 < 24; iC1++ )
                {
                    bData[iC1] = bAuthentication[iC1 - 16];
                }
                int iAttachIndex = 24;

                if (fEncapsulatedFrame != null)
                {
                    byte[] bEncap = fEncapsulatedFrame.FrameBytes;

                    for (int iC1 = 0; iC1 < bEncap.Length; iC1++)
                    {
                        bData[iC1 + 24] = bEncap[iC1];
                    }
                    iAttachIndex += bEncap.Length;
                }

                bAttachedData.CopyTo(bData, iAttachIndex);
                return bData;
            }
        }

        /// <summary>
        /// Gets the length of this frame and the encapsulated frame in bytes
        /// </summary>
        public override int Length
        {
            get
            {
                int iLen = 24 + (fEncapsulatedFrame != null ? fEncapsulatedFrame.Length : 0) + bAttachedData.Length;
                return iLen % 2 == 0 ? iLen : iLen + 1;
            }
        }

        /// <summary>
        /// Returns an identical copy of this frame
        /// </summary>
        /// <returns>An identical copy of this frame</returns>
        public override Frame Clone()
        {
            return new OSPFCommonHeader(this.FrameBytes);
        }
    }

    /// <summary>
    /// An enumeration for OSPF authentication types
    /// </summary>
    public enum OSPFAuthenticationType
    {
        /// <summary>
        /// No authentication is used
        /// </summary>
        NoAuthentication = 0,
        /// <summary>
        /// Simple password authentication is used
        /// </summary>
        SimplePasswordAuthentication = 1,
        /// <summary>
        /// A cryptographic authentication (MD5) i used
        /// </summary>
        CryptographicAuthentication = 2
    }

    /// <summary>
    /// A enumeration for all OSPF frame types
    /// </summary>
    public enum OSPFFrameType
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// OSPF hello message
        /// </summary>
        Hello = 1,
        /// <summary>
        /// OSPF database description message
        /// </summary>
        DatabaseDescription = 2,
        /// <summary>
        /// OSPF link state request message
        /// </summary>
        LinkStateRequest = 3,
        /// <summary>
        /// OSPF link state update message
        /// </summary>
        LinkStateUpdate = 4,
        /// <summary>
        /// OSPF link state acknowledgement message
        /// </summary>
        LinkStateAcknowledgement = 5
    }
}
