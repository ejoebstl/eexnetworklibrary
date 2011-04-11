using System;
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
        /// Must return the type of this frame.
        /// </summary>
        public abstract FrameType FrameType { get; }

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

    /// <summary>
    /// Describes several types of well-known eExNetworkLibrary frames.
    /// </summary>
    public enum FrameType
    {
        /// <summary>
        /// A traffic description frame
        /// </summary>
        TrafficDescriptionFrame = -1,
        /// <summary>
        /// An ethernet frame
        /// </summary>
        Ethernet = 0,
        /// <summary>
        /// An IP frame
        /// </summary>
        IP = 1,
        /// <summary>
        /// A UDP frame
        /// </summary>
        UDP = 2,
        /// <summary>
        /// A TCP frame
        /// </summary>
        TCP = 3, 
        /// <summary>
        /// A raw data (unparsed) frame
        /// </summary>
        ByteData = 4,
        /// <summary>
        /// An ARP frame
        /// </summary>
        ARP = 5, 
        /// <summary>
        /// A RIP frame
        /// </summary>
        RIP = 6,
        /// <summary>
        /// A TLV item
        /// </summary>
        TLVItem = 7,
        /// <summary>
        /// A DHCP frame
        /// </summary>
        DHCP = 8,
        /// <summary>
        /// An ICMP frame
        /// </summary>
        ICMP = 9,
        /// <summary>
        /// An OSPF header
        /// </summary>
        OSPFHeader = 10,
        /// <summary>
        /// An OSPF hello message
        /// </summary>
        OSPFHello = 11,
        /// <summary>
        /// An OSPF database description message
        /// </summary>
        OSPFDatabaseDescription = 12,
        /// <summary>
        /// An OSPF database description message
        /// </summary>
        OSPFLSAHeader = 13,
        /// <summary>
        /// An OSPF router LSA
        /// </summary>
        OSPFRouterLSA = 14,
        /// <summary>
        /// An OSPF network LSA
        /// </summary>
        OSPFNetworkLSA = 15,
        /// <summary>
        /// An OSPF summary LSA
        /// </summary>
        OSPFSummaryLSA = 16,
        /// <summary>
        /// An OSPF external LSA
        /// </summary>
        OSPFExternalLSA = 18,
        /// <summary>
        /// An OSPF LSA request
        /// </summary>
        OSPFLSARequest = 19,
        /// <summary>
        /// An OSPF LSA acknowledgement
        /// </summary>
        OSPFLSAcknowledgement = 20,
        /// <summary>
        /// An OSPF LSA update
        /// </summary>
        OSPFLSAUpdate = 21,
        /// <summary>
        /// A DNS frame
        /// </summary>
        DNS = 22,
        /// <summary>
        /// User protocol implementations
        /// </summary>
        UserDefined = 255
    }
}
