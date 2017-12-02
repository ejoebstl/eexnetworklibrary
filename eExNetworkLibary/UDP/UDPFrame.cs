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

namespace eExNetworkLibrary.UDP
{
    /// <summary>
    /// This class represents a UDPFrame
    /// <remarks>
    /// If you change any properties of this frame, you have to manually calculate and set the checksum by crating an IP pseudo header and
    /// using the CalcualteChecksum method with it. The result must be saved into the Checksum property of this frame.
    /// </remarks>
    /// </summary>
    public class UDPFrame : Frame
    {
        public static string DefaultFrameType { get { return FrameTypes.UDP; } }
        private int iSourcePort;
        private int iDestinationPort;
        private byte[] bChecksum;

        /// <summary>
        /// Creates a new instance of this class initialized with default values
        /// </summary>
        public UDPFrame()
        {
            iSourcePort = 0;
            iDestinationPort = 0;
            bChecksum = new byte[2];
        }

        /// <summary>
        /// Creates a new instance of this class with the parsed data of the given byte array
        /// </summary>
        /// <param name="bData">The data to parse</param>
        public UDPFrame(byte[] bData)
        {
            iSourcePort = bData[0] * 256 + bData[1];
            iDestinationPort = bData[2] * 256 + bData[3];
            int iLen = bData[4] * 256 + bData[5];
            bChecksum = new byte[2];
            bChecksum[0] = bData[6];
            bChecksum[1] = bData[7];


            Encapsulate(bData, 8);

        }

        /// <summary>
        /// Returns the checksum for this UDPFrame. This method works not always clean. An alternative is to set the checksum of an UDP frame to an empty byte array with the length of two.
        /// </summary>
        /// <param name="bPseudoHeader">The IP which should be included into the checksum calculation</param>
        /// <returns>The checksum data</returns>
        public byte[] CalculateChecksum(byte[] bPseudoHeader)
        {

            if (bPseudoHeader.Length % 2 != 0)
            {
                throw new ArgumentException("Pseudo header length must be a multiple of two.");
            }

            int iLength = this.Length;

            byte[] bInnerData = fEncapsulatedFrame != null ? fEncapsulatedFrame.FrameBytes : new byte[0];

            byte[] bUDPFrame = new byte[8 + bInnerData.Length + (bInnerData.Length % 2 == 0 ? 0 : 1) + bPseudoHeader.Length];

            bPseudoHeader.CopyTo(bUDPFrame, 0);

            bUDPFrame[bPseudoHeader.Length + 0] = (byte)((iSourcePort >> 8) & 0xFF);
            bUDPFrame[bPseudoHeader.Length + 1] = (byte)((iSourcePort) & 0xFF);
            bUDPFrame[bPseudoHeader.Length + 2] = (byte)((iDestinationPort >> 8) & 0xFF);
            bUDPFrame[bPseudoHeader.Length + 3] = (byte)((iDestinationPort) & 0xFF);
            bUDPFrame[bPseudoHeader.Length + 4] = (byte)((iLength >> 8) & 0xFF);
            bUDPFrame[bPseudoHeader.Length + 5] = (byte)((iLength) & 0xFF);
            bUDPFrame[bPseudoHeader.Length + 6] = 0;
            bUDPFrame[bPseudoHeader.Length + 7] = 0;

            bInnerData.CopyTo(bUDPFrame, bPseudoHeader.Length + 8);

            return ChecksumCalculator.CalculateChecksum(bUDPFrame);
        }

        #region Props

        /// <summary>
        /// Gets or sets the source port
        /// </summary>
        public int SourcePort
        {
            get { return iSourcePort; }
            set { iSourcePort = value; }
        }

        /// <summary>
        /// Gets or sets the destination port
        /// </summary>
        public int DestinationPort
        {
            get { return iDestinationPort; }
            set { iDestinationPort = value; }
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
        /// Gets the frame type for this frame
        /// </summary>
        public override string FrameType
        {
            get { return UDPFrame.DefaultFrameType; }
        }

        /// <summary>
        /// Gets the byte data of this UDP frame and its encapsulated frame
        /// </summary>
        public override byte[] FrameBytes
        {
            get 
            {
                byte[] bRaw = new byte[this.Length];

                bRaw[0] = (byte)((iSourcePort >> 8) & 0xFF);
                bRaw[1] = (byte)((iSourcePort) & 0xFF);
                bRaw[2] = (byte)((iDestinationPort >> 8) & 0xFF);
                bRaw[3] = (byte)((iDestinationPort) & 0xFF);
                bRaw[4] = (byte)((bRaw.Length >> 8) & 0xFF);
                bRaw[5] = (byte)((bRaw.Length) & 0xFF);
                bRaw[6] = bChecksum[0];
                bRaw[7] = bChecksum[1];

                if (fEncapsulatedFrame != null)
                {
                    fEncapsulatedFrame.FrameBytes.CopyTo(bRaw, 8);
                }

                return bRaw;
            }
        }

        /// <summary>
        /// Returns the length of this UDP frame and its encapsulated frame in bytes
        /// </summary>
        public override int Length
        {
            get { return 8 + (fEncapsulatedFrame == null ? 0 : fEncapsulatedFrame.Length); }
        }

        #endregion

        /// <summary>
        /// Returns a string representation of this frame.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string strDescription = this.FrameType.ToString() + ":\n";
            strDescription += "Source: " + this.SourcePort.ToString() + "\n";
            strDescription += "Destination: " + this.DestinationPort.ToString() + "\n";
            strDescription += "Checksum: " + BitConverter.ToInt16(this.Checksum, 0).ToString() + "\n";
            return strDescription;
        }

        /// <summary>
        /// Creates an identical copy of this UDP frame
        /// </summary>
        /// <returns>An identical copy of this UDP frame</returns>
        public override Frame Clone()
        {
            return new UDPFrame(this.FrameBytes);
        }
    }
}
