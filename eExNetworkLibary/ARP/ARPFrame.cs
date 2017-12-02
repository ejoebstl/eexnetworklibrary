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
using eExNetworkLibrary.Ethernet;

namespace eExNetworkLibrary.ARP
{
    /// <summary>
    /// This class represents an ARP frame.
    /// <remarks>This class currently supports only MAC and IPv4 addresses.</remarks>
    /// </summary>
    public class ARPFrame : Frame
    {
        public static string DefaultFrameType { get { return FrameTypes.ARP; } }

        private HardwareAddressType arpHardwareAddressType;
        private EtherType arpProtocolAddressType;
        private ARPOperation arpOperation;

        private MACAddress macSource;
        private MACAddress macDestination;
        private IPAddress ipaSource;
        private IPAddress ipaDestination;
        
        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public ARPFrame()
        {
            arpHardwareAddressType = HardwareAddressType.Ethernet;
            arpProtocolAddressType = EtherType.IPv4;
            arpOperation = ARPOperation.Request;

            macSource = new MACAddress();
            macDestination = new MACAddress();
            ipaSource = IPAddress.Any;
            ipaDestination = IPAddress.Any;
            this.fEncapsulatedFrame = null;
        }
        
        /// <summary>
        /// Creates a newinstance of this class from the given data.
        /// </summary>
        /// <param name="bData">The data to parse.</param>
        public ARPFrame(byte[] bData)
        {
            arpHardwareAddressType = (HardwareAddressType)((int)((bData[0] << 8) + bData[1]));
            arpProtocolAddressType = (EtherType)((int)((bData[2] << 8) + bData[3]));
            int iHardwareAddressLength = bData[4];
            int iProtocolAddressLength = bData[5];
            arpOperation = (ARPOperation)((int)((bData[6] << 8) + bData[7]));


            if(arpHardwareAddressType != HardwareAddressType.Ethernet || (arpProtocolAddressType != EtherType.IPv4 && arpProtocolAddressType != EtherType.IPv6))
            {
                throw new ArgumentException("Only IPv6 and IPv4 in conjunction with ethernet is supported at the moment.");
            }

            if (iHardwareAddressLength != 6 && arpHardwareAddressType != HardwareAddressType.Ethernet)
            {
                throw new ArgumentException("The hardware address type of the ARP frame indicates Ethernet, but the address data is not 6 bytes long.");
            }

            if (iProtocolAddressLength != 4 && EtherType.IPv4 == arpProtocolAddressType)
            {
                throw new ArgumentException("The protocol address type of the ARP frame indicates IPv4, but the address data is not 4 bytes long.");
            }
            if (iProtocolAddressLength != 16 && EtherType.IPv6 == arpProtocolAddressType)
            {
                throw new ArgumentException("The protocol address type of the ARP frame indicates IPv6, but the address data is not 16 bytes long.");
            }
            
            int iC1 = 8;

            byte[] bAddress = new byte[iHardwareAddressLength];

            for(int iC2 = 0; iC2 < iHardwareAddressLength; iC2++)
            Array.Copy(bData, iC1, bAddress, 0, iHardwareAddressLength); 
            iC1 += iHardwareAddressLength;
            macSource = new MACAddress(bAddress);

            
            bAddress = new byte[iProtocolAddressLength];
            Array.Copy(bData, iC1, bAddress, 0, iProtocolAddressLength); 
            iC1 += iProtocolAddressLength;
            ipaSource = new IPAddress(bAddress);


            bAddress = new byte[iHardwareAddressLength];
            Array.Copy(bData, iC1, bAddress, 0, iHardwareAddressLength); 
            iC1 += iHardwareAddressLength;
            macDestination = new MACAddress(bAddress);

            bAddress = new byte[iProtocolAddressLength];
            Array.Copy(bData, iC1, bAddress, 0, iProtocolAddressLength); 
            iC1 += iProtocolAddressLength;
            ipaDestination = new IPAddress(bAddress);

            byte[] bPad = new byte[bData.Length - iC1];

            for (int iC2 = 0; iC2 < bPad.Length; iC2++)
            {
                bPad[iC2] = bData[iC2 + iC1];
            }

            this.fEncapsulatedFrame = new RawDataFrame(bPad);
        }

        #region Props

        /// <summary>
        /// Gets or sets the hardware address type
        /// </summary>
        public HardwareAddressType HardwareAddressType
        {
            get { return arpHardwareAddressType; }
            set { arpHardwareAddressType = value; }
        }

        /// <summary>
        /// Gets or sets the protocol address type
        /// </summary>
        public EtherType ProtocolAddressType
        {
            get { return arpProtocolAddressType; }
            set { arpProtocolAddressType = value; }
        }

        /// <summary>
        /// Gets or sets the ARP operation
        /// </summary>
        public ARPOperation Operation
        {
            get { return arpOperation; }
            set { arpOperation = value; }
        }

        /// <summary>
        /// Gets or sets the source MAC address
        /// </summary>
        public MACAddress SourceMAC
        {
            get { return macSource; }
            set { macSource = value; }
        }

        /// <summary>
        /// Gets or sets the destination MAC address
        /// </summary>
        public MACAddress DestinationMAC
        {
            get { return macDestination; }
            set { macDestination = value; }
        }

        /// <summary>
        /// Gets or sets the source IP address
        /// </summary>
        public IPAddress SourceIP
        {
            get { return ipaSource; }
            set
            {
                if (value.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork && value.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    throw new ArgumentException("Only IPv4 and IPv6 are supported at the moment.");
                }
                ipaSource = value;
            }
        }

        /// <summary>
        /// Gets or sets the destination IP address
        /// </summary>
        public IPAddress DestinationIP
        {
            get { return ipaDestination; }
            set
            {
                if (value.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork && value.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    throw new ArgumentException("Only IPv4 and IPv6 are supported at the moment.");
                }
                ipaDestination = value;
            }
        }

        /// <summary>
        /// Returns the type of this frame.
        /// </summary>
        public override string FrameType
        {
            get 
            {
                return ARPFrame.DefaultFrameType;
            }
        }

        /// <summary>
        /// Returns the raw byte representation of this frame
        /// </summary>
        public override byte[] FrameBytes
        {
            get 
            {
                if (ipaSource.AddressFamily != ipaDestination.AddressFamily)
                {
                    throw new InvalidOperationException("Cannot mix up IPv4 and IPv6 within one ARP frame.");
                }

                int iProtocolAddressLength = ipaSource.GetAddressBytes().Length;

                byte[] bData = new byte[this.Length];
                bData[0] = (byte)((((uint)arpHardwareAddressType) >> 8) & 0xFF);
                bData[1] = (byte)((((uint)arpHardwareAddressType)) & 0xFF);
                bData[2] = (byte)((((uint)arpProtocolAddressType) >> 8) & 0xFF);
                bData[3] = (byte)((((uint)arpProtocolAddressType)) & 0xFF);
                bData[4] = 6;
                bData[5] = (byte)iProtocolAddressLength;
                bData[6] = (byte)((((uint)arpOperation) >> 8) & 0xFF);
                bData[7] = (byte)((((uint)arpOperation)) & 0xFF);

                int iC1 = 8;

                Array.Copy(macSource.AddressBytes, 0, bData, iC1, 6);
                iC1 += 6;
                Array.Copy(ipaSource.GetAddressBytes(), 0, bData, iC1, iProtocolAddressLength);
                iC1 += iProtocolAddressLength;
                Array.Copy(macDestination.AddressBytes, 0, bData, iC1, 6);
                iC1 += 6;
                Array.Copy(ipaDestination.GetAddressBytes(), 0, bData, iC1, iProtocolAddressLength);
                iC1 += iProtocolAddressLength;

                if (fEncapsulatedFrame != null)
                {
                    fEncapsulatedFrame.FrameBytes.CopyTo(bData, iC1);
                }

                return bData;
            }
        }

        /// <summary>
        /// Returns the length of this frame and the encapsulated frames in bytes
        /// </summary>
        public override int Length
        {
            get 
            {
                return 20 + (2 * ipaSource.GetAddressBytes().Length) + (fEncapsulatedFrame == null ? 0 : fEncapsulatedFrame.Length);
            }
        }

        #endregion

        /// <summary>
        /// Returns the string representation of this frame
        /// </summary>
        /// <returns>The string representation of this frame</returns>
        public override string ToString()
        {
            string strDescription = this.FrameType.ToString() + ":\n";
            strDescription += "Source: " + this.SourceMAC + "/" + this.SourceIP + "\n";
            strDescription += "Destination: " + this.DestinationMAC + "/" + this.DestinationIP + "\n";
            strDescription += "Operation: " + this.Operation + "\n";
            strDescription += "Hardware address type: " + this.HardwareAddressType + "\n";
            strDescription += "Protocol address type: " + this.ProtocolAddressType + "\n";
            return strDescription;
        }

        /// <summary>
        /// Creates an identical copy of this class
        /// </summary>
        /// <returns>An identical copy of this class</returns>
        public override Frame Clone()
        {
            return new ARPFrame(this.FrameBytes);
        }
    }

    /// <summary>
    /// An enumeration for ARP operations
    /// </summary>
    public enum ARPOperation
    {
        /// <summary>
        /// Request
        /// </summary>
        Request = 1,
        /// <summary>
        /// Reply
        /// </summary>
        Reply = 2,
        /// <summary>
        /// Request reverse
        /// </summary>
        RequestReverse = 3,
        /// <summary>
        /// Reply reverse
        /// </summary>
        ReplyReverse = 4,
        /// <summary>
        /// Dynamic reverse ARP request
        /// </summary>
        DRARPRequest = 5,
        /// <summary>
        /// Dynamic reverse ARP reply
        /// </summary>
        DRARPReply = 6,
        /// <summary>
        /// Dynamic reverse ARP error
        /// </summary>
        DRARPError = 7,
        /// <summary>
        /// Inverse ARP request
        /// </summary>
        InARPRequest = 8,
        /// <summary>
        /// Inverse ARP reply
        /// </summary>
        InARPReply = 9,
        /// <summary>
        /// ARP not acknowledged
        /// </summary>
        ARPNAK = 10,
        /// <summary>
        /// ?
        /// </summary>
        MARSRequest = 11,
        /// <summary>
        /// ?
        /// </summary>
        MARSMulti = 12,
        /// <summary>
        /// ?
        /// </summary>
        MARSMServ = 13,
        /// <summary>
        /// ?
        /// </summary>
        MARSJoin = 14,
        /// <summary>
        /// ?
        /// </summary>
        MARSLeave = 15,
        /// <summary>
        /// ?
        /// </summary>
        MARSNAK = 16,
        /// <summary>
        /// ?
        /// </summary>
        MARSUnserv = 17,
        /// <summary>
        /// ?
        /// </summary>
        MARSSJoin = 18,
        /// <summary>
        /// ?
        /// </summary>
        MARSSLeave = 19,
        /// <summary>
        /// ?
        /// </summary>
        MARSGrouplistRequest = 20,
        /// <summary>
        /// ?
        /// </summary>
        MARSGrouplistReply = 21,
        /// <summary>
        /// ?
        /// </summary>
        MARSRedirectMap = 22,
        /// <summary>
        /// ?
        /// </summary>
        MAPOSUNARP = 23
    }
}
