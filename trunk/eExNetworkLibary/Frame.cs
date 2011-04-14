﻿using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary
{
    /// <summary>
    /// This class represents an abstract captured network frame.
    /// </summary>
    public abstract class Frame
    {
        /// <summary>
        /// The frame encapsulated in this frame.
        /// </summary>
        protected Frame fEncapsulatedFrame;

        /// <summary>
        /// Must return the type of this frame as string.
        /// </summary>
        public abstract string FrameType { get; }

        /// <summary>
        /// Must return this frame and its encapsulated frames converted to bytes.
        /// </summary>
        public abstract byte[] FrameBytes { get; }

        /// <summary>
        /// Gets or sets the frame encapsulated in this frame
        /// </summary>
        public Frame EncapsulatedFrame
        {
            get { return fEncapsulatedFrame; }
            set { fEncapsulatedFrame = value; }
        }

        /// <summary>
        /// Copies the given data into a raw data frame and sets it as the encapsulated frame. If the given parameters would result in an empty frame, the encapsulated frame is set to null instead.
        /// </summary>
        /// <param name="bData">The data to copy.</param>
        /// <param name="iStartIndex">The index at which copying begins.</param>
        protected void Encapsulate(byte[] bData, int iStartIndex)
        {
            if (bData.Length - iStartIndex == 0)
            {
                this.fEncapsulatedFrame = null;
            }
            else
            {
                this.fEncapsulatedFrame = new RawDataFrame(bData, iStartIndex, bData.Length - iStartIndex);
            }
        }

        /// <summary>
        /// Must return the length of the bytes contained in this frame and its encapsulated frames
        /// </summary>
        public abstract int Length { get; }

        /// <summary>
        /// Must return an identical copy of this frame.
        /// </summary>
        /// <returns>An identic clone of this frame</returns>
        public abstract Frame Clone();
    }
}
