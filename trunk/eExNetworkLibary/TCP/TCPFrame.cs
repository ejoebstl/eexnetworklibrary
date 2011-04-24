// This source file is part of the eEx Network Library
//
// Author: 	    Emanuel Jöbstl <emi@eex-dev.net>
// Weblink: 	http://network.eex-dev.net
//		        http://eex.codeplex.com
//
// Licensed under the GNU Library General Public License (LGPL) 
//
// (c) eex-dev 2007-2011

using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using eExNetworkLibrary.Utilities;

namespace eExNetworkLibrary.TCP
{
    /// <summary>
    /// Represents a TCP frame
    /// <remarks>
    /// If you change any properties of this frame, you have to manually calculate and set the checksum by crating an IP pseudo header and
    /// using the CalcualteChecksum method with it. The result must be saved into the Checksum property of this frame.
    /// </remarks>
    /// </summary>
    public class TCPFrame : Frame
    {
        public static string DefaultFrameType { get { return FrameTypes.TCP; } }

        private int iSourcePort;
        private int iDestinationPort;
        private uint iSequenceNumber;
        private uint iAcknowledgmentNumber;
        private bool bUrgent;
        private bool bAcknowledgement;
        private bool bPush;
        private bool bReset;
        private bool bSynchronize;
        private bool bFinish;
        private uint iWindow;
        private byte[] bChecksum;
        private uint iUrgentPointer;
        private TCPOptions oOptions;


        /// <summary>
        /// Creates a new empty instance of this class
        /// </summary>
        public TCPFrame()
        {
            iSourcePort = 0;
            iDestinationPort = 0;
            iSequenceNumber = 0;
            iAcknowledgmentNumber = 0;
            bUrgent = false;
            bAcknowledgement = false;
            bPush = false;
            bReset = false;
            bSynchronize = false;
            bFinish = false;
            iWindow = 0;
            bChecksum = new byte[2];
            iUrgentPointer = 0;
            oOptions = new TCPOptions();
            fEncapsulatedFrame = null;
        }

        /// <summary>
        /// Creates a new instance of this class by parsing the given data
        /// </summary>
        /// <param name="bData">The byte array to parse</param>
        public TCPFrame(byte[] bData)
        {
            int iDataOffset; //In bits.
            iSourcePort = bData[0] * 256 + bData[1];
            iDestinationPort = bData[2] * 256 + bData[3];
            iSequenceNumber = (uint)(bData[4] * (uint)(256 * 256 * 256) + bData[5] * (uint)(256 * 256) + bData[6] * (uint)256 + bData[7]);
            iAcknowledgmentNumber = (uint)(bData[8] * (uint)(256 * 256 * 256) + bData[9] * (uint)(256 * 256) + bData[10] * (uint)256 + bData[11]);
            iDataOffset = (int)((bData[12] & 0xF0) >> 4) * 4;

            if (iDataOffset < 20)
            {
                throw new ArgumentException("Invalid packet header");
            }

            bUrgent = (bData[13] & 0x20) > 0;
            bAcknowledgement = (bData[13] & 0x10) > 0;
            bPush = (bData[13] & 0x8) > 0;
            bReset = (bData[13] & 0x4) > 0;
            bSynchronize = (bData[13] & 0x2) > 0;
            bFinish = (bData[13] & 0x1) > 0;
            iWindow = bData[14] * (uint)256 + bData[15];
            bChecksum = new byte[2];
            bChecksum[0] = bData[16];
            bChecksum[1] = bData[17];
            iUrgentPointer = bData[18] * (uint)256 + bData[19];

            byte[] bOptions = new byte[iDataOffset - 20];

            for (int iC1 = 20; iC1 < iDataOffset; iC1++)
            {
                bOptions[iC1 - 20] = bData[iC1];
            }

            this.oOptions = new TCPOptions(bOptions);

            byte[] bEncapsulatedData = new byte[bData.Length - iDataOffset];

            for (int iC1 = iDataOffset; iC1 < bData.Length; iC1++)
            {
                bEncapsulatedData[iC1 - iDataOffset] = bData[iC1];
            }

            this.fEncapsulatedFrame = new RawDataFrame(bEncapsulatedData);
        }

        /// <summary>
        /// Calculates the TCP checksum of this frame
        /// </summary>
        /// <param name="bPseudoHeader">The IP pseudo header to add to the checksum</param>
        /// <returns>The checksum data</returns>
        public byte[] CalculateChecksum(byte[] bPseudoHeader)
        {
            if (bPseudoHeader.Length % 2 != 0)
            {
                throw new ArgumentException("Pseudo header length must be a multiple of two.");
            }

            byte[] bInnerData = this.fEncapsulatedFrame != null ? this.fEncapsulatedFrame.FrameBytes : new byte[0];

            int iTotalLen = bPseudoHeader.Length + Length;
            if(iTotalLen % 2 != 0)
            {
                iTotalLen += 1;
            }

            byte[] bTCPFrame = new byte[iTotalLen];

            int iDataOffset = 20 + oOptions.OptionLength;
            if (iDataOffset % 4 != 0)
            {
                iDataOffset += 4 - (iDataOffset % 4);
            }

            bPseudoHeader.CopyTo(bTCPFrame, 0);

            bTCPFrame[bPseudoHeader.Length + 0] = (byte)((iSourcePort >> 8) & 0xFF);
            bTCPFrame[bPseudoHeader.Length + 1] = (byte)((iSourcePort) & 0xFF);
            bTCPFrame[bPseudoHeader.Length + 2] = (byte)((iDestinationPort >> 8) & 0xFF);
            bTCPFrame[bPseudoHeader.Length + 3] = (byte)((iDestinationPort) & 0xFF);
            bTCPFrame[bPseudoHeader.Length + 4] = (byte)((iSequenceNumber >> 24) & 0xFF);
            bTCPFrame[bPseudoHeader.Length + 5] = (byte)((iSequenceNumber >> 16) & 0xFF);
            bTCPFrame[bPseudoHeader.Length + 6] = (byte)((iSequenceNumber >> 8) & 0xFF);
            bTCPFrame[bPseudoHeader.Length + 7] = (byte)((iSequenceNumber) & 0xFF);
            bTCPFrame[bPseudoHeader.Length + 8] = (byte)((iAcknowledgmentNumber >> 24) & 0xFF);
            bTCPFrame[bPseudoHeader.Length + 9] = (byte)((iAcknowledgmentNumber >> 16) & 0xFF);
            bTCPFrame[bPseudoHeader.Length + 10] = (byte)((iAcknowledgmentNumber >> 8) & 0xFF);
            bTCPFrame[bPseudoHeader.Length + 11] = (byte)((iAcknowledgmentNumber) & 0xFF);
            bTCPFrame[bPseudoHeader.Length + 12] = (byte)((((iDataOffset) / 4) << 4) & 0xF0);
            bTCPFrame[bPseudoHeader.Length + 13] |= (byte)(bUrgent ? 0x20 : 0x00);
            bTCPFrame[bPseudoHeader.Length + 13] |= (byte)(bAcknowledgement ? 0x10 : 0x00);
            bTCPFrame[bPseudoHeader.Length + 13] |= (byte)(bPush ? 0x8 : 0x00);
            bTCPFrame[bPseudoHeader.Length + 13] |= (byte)(bReset ? 0x4 : 0x00);
            bTCPFrame[bPseudoHeader.Length + 13] |= (byte)(bSynchronize ? 0x2 : 0x00);
            bTCPFrame[bPseudoHeader.Length + 13] |= (byte)(bFinish ? 0x1 : 0x00);
            bTCPFrame[bPseudoHeader.Length + 14] = (byte)((iWindow >> 8) & 0xFF);
            bTCPFrame[bPseudoHeader.Length + 15] = (byte)((iWindow) & 0xFF);
            bTCPFrame[bPseudoHeader.Length + 16] = 0;
            bTCPFrame[bPseudoHeader.Length + 17] = 0;
            bTCPFrame[bPseudoHeader.Length + 18] = (byte)((iUrgentPointer >> 8) & 0xFF);
            bTCPFrame[bPseudoHeader.Length + 19] = (byte)((iUrgentPointer) & 0xFF);

            oOptions.Raw.CopyTo(bTCPFrame, bPseudoHeader.Length + 20);

            bInnerData.CopyTo(bTCPFrame, bPseudoHeader.Length + iDataOffset);

            return ChecksumCalculator.CalculateChecksum(bTCPFrame);
        }

        /// <summary>
        /// Returns a new identical instance of this frame
        /// </summary>
        /// <returns>A new identical instance of this frame</returns>
        public override Frame Clone()
        {
            return new TCPFrame(this.FrameBytes);
        }

        #region Props

        /// <summary>
        /// Gets or sets the TCP source port
        /// </summary>
        public int SourcePort
        {
            get { return iSourcePort; }
            set { iSourcePort = value; }
        }

        /// <summary>
        /// Gets or sets the TCP destination port
        /// </summary>
        public int DestinationPort
        {
            get { return iDestinationPort; }
            set { iDestinationPort = value; }
        }

        /// <summary>
        /// Gets or sets the TCP sequence number
        /// </summary>
        public uint SequenceNumber
        {
            get { return iSequenceNumber; }
            set { iSequenceNumber = value; }
        }

        /// <summary>
        /// Gets or sets the TCP acknowledgement number
        /// </summary>
        public uint AcknowledgementNumber
        {
            get { return iAcknowledgmentNumber; }
            set { iAcknowledgmentNumber = value; }
        }

        /// <summary>
        /// Gets or sets the data offset in bits
        /// </summary>
        public int DataOffset //In bits.
        {
            get { return 20 + oOptions.OptionLength; }
        }

        /// <summary>
        /// Gets or sets the urgent flag
        /// </summary>
        public bool Urgent
        {
            get { return bUrgent; }
            set { bUrgent = value; }
        }

        /// <summary>
        /// Gets or sets the value of the acknowledgement flag
        /// </summary>
        public bool AcknowledgementFlagSet
        {
            get { return bAcknowledgement; }
            set { bAcknowledgement = value; }
        }

        /// <summary>
        /// Gets or sets the value of the push flag 
        /// </summary>
        public bool PushFlagSet
        {
            get { return bPush; }
            set { bPush = value; }
        }

        /// <summary>
        /// Gets or sets the value of the reset flag 
        /// </summary>
        public bool ResetFlagSet
        {
            get { return bReset; }
            set { bReset = value; }
        }

        /// <summary>
        /// Gets or sets the value of the sync flag 
        /// </summary>
        public bool SynchronizeFlagSet
        {
            get { return bSynchronize; }
            set { bSynchronize = value; }
        }

        /// <summary>
        /// Gets or sets the value of the finish flag 
        /// </summary>
        public bool FinishFlagSet
        {
            get { return bFinish; }
            set { bFinish = value; }
        }

        /// <summary>
        /// Gets or sets the TCP window size
        /// </summary>
        public uint Window
        {
            get { return iWindow; }
            set { iWindow = value; }
        }

        /// <summary>
        /// Gets or sets the checksum
        /// </summary>
        public byte[] Checksum
        {
            get { return bChecksum; }
            set { bChecksum = value; }
        }

        /// <summary>
        /// Gets or sets the urgent pointer
        /// </summary>
        public uint UrgentPointer
        {
            get { return iUrgentPointer; }
            set { iUrgentPointer = value; }
        }

        /// <summary>
        /// Gets or sets the TCP options
        /// </summary>
        public TCPOptions Options
        {
            get { return oOptions; }
            set { oOptions = value; }
        }

        /// <summary>
        /// Returns the type of this frame.
        /// </summary>
        public override string FrameType
        {
            get { return TCPFrame.DefaultFrameType; }
        }

        /// <summary>
        /// Gets the frame converted to its byte representation
        /// </summary>
        public override byte[] FrameBytes
        {
            get 
            {
                byte[] bRaw = new byte[this.Length];

                int iDataOffset = 20 + oOptions.OptionLength;
                if (iDataOffset % 4 != 0)
                {
                    iDataOffset += 4 - (iDataOffset % 4);
                }

                bRaw[0] = (byte)((iSourcePort >> 8) & 0xFF);
                bRaw[1] = (byte)((iSourcePort) & 0xFF);
                bRaw[2] = (byte)((iDestinationPort >> 8) & 0xFF);
                bRaw[3] = (byte)((iDestinationPort) & 0xFF);
                bRaw[4] = (byte)((iSequenceNumber >> 24) & 0xFF);
                bRaw[5] = (byte)((iSequenceNumber >> 16) & 0xFF);
                bRaw[6] = (byte)((iSequenceNumber >> 8) & 0xFF);
                bRaw[7] = (byte)((iSequenceNumber) & 0xFF);
                bRaw[8] = (byte)((iAcknowledgmentNumber >> 24) & 0xFF);
                bRaw[9] = (byte)((iAcknowledgmentNumber >> 16) & 0xFF);
                bRaw[10] = (byte)((iAcknowledgmentNumber >> 8) & 0xFF);
                bRaw[11] = (byte)((iAcknowledgmentNumber) & 0xFF);
                bRaw[12] = (byte)((((iDataOffset) / 4) << 4) & 0xF0);
                bRaw[13] |= (byte)(bUrgent ? 0x20 : 0x00);
                bRaw[13] |= (byte)(bAcknowledgement ? 0x10 : 0x00);
                bRaw[13] |= (byte)(bPush ? 0x8 : 0x00);
                bRaw[13] |= (byte)(bReset ? 0x4 : 0x00);
                bRaw[13] |= (byte)(bSynchronize ? 0x2 : 0x00);
                bRaw[13] |= (byte)(bFinish ? 0x1 : 0x00);
                bRaw[14] = (byte)((iWindow >> 8) & 0xFF);
                bRaw[15] = (byte)((iWindow) & 0xFF);
                bRaw[16] = bChecksum[0];
                bRaw[17] = bChecksum[1];
                bRaw[18] = (byte)((iUrgentPointer >> 8) & 0xFF);
                bRaw[19] = (byte)((iUrgentPointer) & 0xFF);

                oOptions.Raw.CopyTo(bRaw, 20);

                if (fEncapsulatedFrame != null)
                {
                    fEncapsulatedFrame.FrameBytes.CopyTo(bRaw, iDataOffset);
                }

                return bRaw;
            }
        }

        /// <summary>
        /// Gets the length of the bytes of this frame and its encapsulated frames in bytes
        /// </summary>
        public override int Length
        {
            get
            {
                int iLen = 20 + oOptions.OptionLength;
                if (iLen % 4 != 0)
                {
                    iLen += 4 - (iLen % 4);
                }
                iLen += (fEncapsulatedFrame != null ? fEncapsulatedFrame.Length : 0);
                return iLen;
            }
        }

        #endregion

        /// <summary>
        /// Returns a string representation of this frame.
        /// </summary>
        /// <returns>A string representation of this frame</returns>
        public override string ToString()
        {
            string strDescription = this.FrameType.ToString() + ":\n";
            strDescription += "Source: " + this.SourcePort.ToString() + "\n";
            strDescription += "Destination: " + this.DestinationPort.ToString() + "\n";
            strDescription += "Acknowledgement flag set: " + this.AcknowledgementFlagSet + "\n";
            strDescription += "Finish flag set: " + this.FinishFlagSet + "\n";
            strDescription += "Push flag set: " + this.PushFlagSet + "\n";
            strDescription += "Reset flag set: " + this.ResetFlagSet + "\n";
            strDescription += "Synchronize flag set: " + this.SynchronizeFlagSet + "\n";
            strDescription += "Sequence number: " + this.SequenceNumber + "\n";
            strDescription += "Acknowledgement number: " + this.AcknowledgementNumber + "\n";
            strDescription += "Urgend: " + this.Urgent + "\n";
            strDescription += "Urgend pointer: " + this.UrgentPointer + "\n";
            strDescription += "Window: " + this.Window + "\n";
            strDescription += this.Options.ToString();
            return strDescription;
        }

    }
}
