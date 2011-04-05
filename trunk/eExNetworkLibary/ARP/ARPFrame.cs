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
            if (iHardwareAddressLength != 6)
            {
                throw new Exception("MAC address must be 6 bytes long");
            }
            if (iProtocolAddressLength != 4)
            {
                throw new Exception("IP address must be 4 bytes long");
            }
            arpOperation = (ARPOperation)((int)((bData[6] << 8) + bData[7]));

            macSource = new MACAddress();
            macSource.AddressBytes[0] = bData[8];
            macSource.AddressBytes[1] = bData[9];
            macSource.AddressBytes[2] = bData[10];
            macSource.AddressBytes[3] = bData[11];
            macSource.AddressBytes[4] = bData[12];
            macSource.AddressBytes[5] = bData[13];

            byte[] bSourceIP = new byte[4];
            bSourceIP[0] = bData[14];
            bSourceIP[1] = bData[15];            bSourceIP[2] = bData[16];
            bSourceIP[3] = bData[17];

            ipaSource = new IPAddress(bSourceIP);

            macDestination = new MACAddress();
            macDestination.AddressBytes[0] = bData[18];
            macDestination.AddressBytes[1] = bData[19];
            macDestination.AddressBytes[2] = bData[20];
            macDestination.AddressBytes[3] = bData[21];
            macDestination.AddressBytes[4] = bData[22];
            macDestination.AddressBytes[5] = bData[23];

            byte[] bDestinationIP = new byte[4];
            bDestinationIP[0] = bData[24];
            bDestinationIP[1] = bData[25];
            bDestinationIP[2] = bData[26];
            bDestinationIP[3] = bData[27];

            ipaDestination = new IPAddress(bDestinationIP);
            byte[] bPad = new byte[bData.Length - 28];

            for (int iC1 = 0; iC1 < bPad.Length; iC1++)
            {
                bPad[iC1] = bData[iC1 + 28];
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
            set { ipaSource = value; }
        }

        /// <summary>
        /// Gets or sets the destination IP address
        /// </summary>
        public IPAddress DestinationIP
        {
            get { return ipaDestination; }
            set { ipaDestination = value; }
        }

        /// <summary>
        /// Returns FrameType.ARP
        /// </summary>
        public override FrameType FrameType
        {
            get 
            {
                return FrameType.ARP;
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
                bData[0] = (byte)((((uint)arpHardwareAddressType) >> 8) & 0xFF);
                bData[1] = (byte)((((uint)arpHardwareAddressType)) & 0xFF);
                bData[2] = (byte)((((uint)arpProtocolAddressType) >> 8) & 0xFF);
                bData[3] = (byte)((((uint)arpProtocolAddressType)) & 0xFF);
                bData[4] = 6;
                bData[5] = 4;
                bData[6] = (byte)((((uint)arpOperation) >> 8) & 0xFF);
                bData[7] = (byte)((((uint)arpOperation)) & 0xFF);

                bData[8] = macSource.AddressBytes[0];
                bData[9] = macSource.AddressBytes[1];
                bData[10] = macSource.AddressBytes[2];
                bData[11] = macSource.AddressBytes[3];
                bData[12] = macSource.AddressBytes[4];
                bData[13] = macSource.AddressBytes[5];

                byte[] bSourceIP = ipaSource.GetAddressBytes();
                
                if (bSourceIP.Length != 4)
                {
                    throw new Exception("IP address must be 4 bytes long");
                }

                bData[14] = bSourceIP[0];
                bData[15] = bSourceIP[1];
                bData[16] = bSourceIP[2];
                bData[17] = bSourceIP[3];

                bData[18] = macDestination.AddressBytes[0];
                bData[19] = macDestination.AddressBytes[1];
                bData[20] = macDestination.AddressBytes[2];
                bData[21] = macDestination.AddressBytes[3];
                bData[22] = macDestination.AddressBytes[4];
                bData[23] = macDestination.AddressBytes[5];

                byte[] bDestinationIP = ipaDestination.GetAddressBytes();

                if (bDestinationIP.Length != 4)
                {
                    throw new Exception("IP address must be 4 bytes long");
                }

                bData[24] = bDestinationIP[0];
                bData[25] = bDestinationIP[1];
                bData[26] = bDestinationIP[2];
                bData[27] = bDestinationIP[3];

                if (fEncapsulatedFrame != null)
                {
                    fEncapsulatedFrame.FrameBytes.CopyTo(bData, 28);
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
                return 28 + (fEncapsulatedFrame == null ? 0 : fEncapsulatedFrame.Length);
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
