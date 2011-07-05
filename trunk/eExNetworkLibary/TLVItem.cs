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

namespace eExNetworkLibrary
{
    /// <summary>
    /// This class represents a simple Type-Lentgh-Value item which can be used by various protocols.
    /// The length of the type and the length-field is one byte.
    /// </summary>
    public class TLVItem : Frame
    {
        public static string DefaultFrameType { get { return "TLVItem"; } }

        private int tlvType;
        private byte[] bData;

        /// <summary>
        /// Gets or sets the TLV type of this TLV item
        /// </summary>
        public int Type
        {
            get { return tlvType; }
            set { tlvType = value; }
        }

        /// <summary>
        /// Gets or sets the TLV data
        /// </summary>
        public byte[] Data
        {
            get { return bData; }
            set { bData = value; }
        }

        /// <summary>
        /// Creates a new, empty instance of this class
        /// </summary>
        public TLVItem()
        {
            bData = new byte[0];
            tlvType = 0;
        }

        /// <summary>
        /// Creates a new instance of this class by parsing <paramref name="bByte"/>
        /// </summary>
        /// <param name="bByte">The byte array to parse</param>
        public TLVItem(byte[] bByte) : this(bByte, 0) { }


        /// <summary>
        /// Creates a new instance of this class by parsing <paramref name="bByte"/> starting at <paramref name="iStartIndex"/>
        /// </summary>
        /// <param name="bByte">The byte array to parse</param>
        /// <param name="iStartIndex">The index at which parsing should start</param>
        public TLVItem(byte[] bByte, int iStartIndex)
        {
            int iLen = bByte[1 + iStartIndex];
            tlvType = bByte[0 + iStartIndex];
            bData = new byte[iLen];

            for (int iC1 = 0; iC1 < iLen; iC1++)
            {
                bData[iC1] = bByte[iC1 + 2 + iStartIndex];
            }
        }

        /// <summary>
        /// Gets the type of this frame.
        /// </summary>
        public override string FrameType
        {
            get { return TLVItem.DefaultFrameType; }
        }

        /// <summary>
        /// Gets this frames converted to bytes.
        /// </summary>
        public override byte[] FrameBytes
        {
            get
            {
                byte[] bData = new byte[Length];
                bData[0] = (byte)tlvType;
                bData[1] = (byte)this.bData.Length;

                for (int iC1 = 0; iC1 < this.bData.Length; iC1++)
                {
                    bData[iC1 + 2] = this.bData[iC1];
                }

                return bData;
            }
        }

        /// <summary>
        /// Gets the length of the bytes of this frame.
        /// </summary>
        public override int Length
        {
            get { return 2 + bData.Length; }
        }

        /// <summary>
        /// Clones this frame.
        /// </summary>
        /// <returns>An identic clone of this frame</returns>
        public override Frame Clone()
        {
            return new TLVItem(this.FrameBytes);
        }
    }
}
