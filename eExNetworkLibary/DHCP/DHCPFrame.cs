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

namespace eExNetworkLibrary.DHCP
{
    /// <summary>
    /// This class represents a DHCP frame
    /// </summary>
    public class DHCPFrame : Frame
    {
        public static string DefaultFrameType { get { return FrameTypes.DHCP; } }

        private DHCPType dhtMessageType; //1 Byte
        private HardwareAddressType dhtHardwareType; //1 Byte
        private short sHardwarelen; //1 Byte
        private short sHops; //1 Byte
        private int iTransactionID; //4 Bytes
        private int iSecs; //2 Bytes
        private bool bValidIPFlag; //1 bit (Flag)
        //15 bits padding...
        private IPAddress ipaClientAddress; //4 Byte
        private IPAddress ipaOwnAddress; //4 Byte
        private IPAddress ipaServerAddress; //4 Byte
        private IPAddress ipaRelayAddress;//4 Byte
        private MACAddress macClientMac; //6 Byte
        private string strRequestedServerName;
        private string strRequestedFile;
        private List<DHCPTLVItem> lTLVs;

        #region Props

        /// <summary>
        /// Gets or sets the DHCP type
        /// </summary>
        public DHCPType MessageType
        {
            get { return dhtMessageType; }
            set { dhtMessageType = value; }
        }

        /// <summary>
        /// Gets or sets the hardware address type
        /// </summary>
        public HardwareAddressType HardwareType
        {
            get { return dhtHardwareType; }
            set { dhtHardwareType = value; }
        }

        /// <summary>
        /// Gets or sets the hardware address length
        /// </summary>
        public short Hardwarelen
        {
            get { return sHardwarelen; }
            set { sHardwarelen = value; }
        }

        /// <summary>
        /// Gets or sets the hopcount
        /// </summary>
        public short Hops
        {
            get { return sHops; }
            set { sHops = value; }
        }

        /// <summary>
        /// Gets or sets the transaction ID
        /// </summary>
        public int TransactionID
        {
            get { return iTransactionID; }
            set { iTransactionID = value; }
        }

        /// <summary>
        /// Gets or sets the seconds since the DHCP frame was sent
        /// </summary>
        public int Secs
        {
            get { return iSecs; }
            set { iSecs = value; }
        }

        /// <summary>
        /// Gets or sets the valid IP flag
        /// </summary>
        public bool ValidIPFlag
        {
            get { return bValidIPFlag; }
            set { bValidIPFlag = value; }
        }

        /// <summary>
        /// Gets or sets the client address
        /// </summary>
        public IPAddress ClientAddress
        {
            get { return ipaClientAddress; }
            set { ipaClientAddress = value; }
        }

        /// <summary>
        /// Gets or sets the offered address
        /// </summary>
        public IPAddress OfferedAddress
        {
            get { return ipaOwnAddress; }
            set { ipaOwnAddress = value; }
        }

        /// <summary>
        /// Gets or sets the server address
        /// </summary>
        public IPAddress ServerAddress
        {
            get { return ipaServerAddress; }
            set { ipaServerAddress = value; }
        }

        /// <summary>
        /// Gets or sets the relay address
        /// </summary>
        public IPAddress RelayAddress
        {
            get { return ipaRelayAddress; }
            set { ipaRelayAddress = value; }
        }

        /// <summary>
        /// Gets or sets the client MAC
        /// </summary>
        public MACAddress ClientMac
        {
            get { return macClientMac; }
            set { macClientMac = value; }
        }

        /// <summary>
        /// Gets or sets the requested server's name
        /// <remarks>The maximum length of this parameter is 64 chars</remarks>
        /// </summary>
        public string RequestedServerName
        {
            get { return strRequestedServerName; }
            set
            {
                if (value.Length > 64)
                {
                    throw new ArgumentException("The requested server name is too long. Max 64 chars are supported");
                }
                strRequestedServerName = value;
            }
        }

        /// <summary>
        /// Gets or sets the requested file.
        /// <remarks>The maximum length of this parameter is 128 chars</remarks>
        /// </summary>
        public string RequestedFile
        {
            get { return strRequestedFile; }
            set
            {
                if (value.Length > 128)
                {
                    throw new ArgumentException("The requested file name is too long. Max 128 chars are supported");
                }
                strRequestedFile = value;
            }
        }

        /// <summary>
        /// Returns all TLV items contained in this instance
        /// </summary>
        public DHCPTLVItem[] GetDHCPTLVItems()
        {
            return lTLVs.ToArray();
        }

        /// <summary>
        /// Adds a TLV item to this instance
        /// </summary>
        /// <param name="dhItem">The TLV item to add</param>
        public void AddDHCPTLVItem(DHCPTLVItem dhItem)
        {
            lTLVs.Add(dhItem);
        }

        /// <summary>
        /// Removes a specific TLV item from this instance
        /// </summary>
        /// <param name="dhItem">The TLV item to remove</param>
        public void RemoveDHCPTLVItem(DHCPTLVItem dhItem)
        {
            lTLVs.Remove(dhItem);
        }

        /// <summary>
        /// Clears all TLV items from this instance
        /// </summary>
        public void ClearDHCPTLVItems()
        {
            lTLVs.Clear();
        }

        /// <summary>
        /// Checks whether a specific TLV item is contained in this instance
        /// </summary>
        /// <param name="dhItem">The TLV item to search for</param>
        /// <returns>A bool indicating whether a specific TLV item is contained in this instance</returns>
        public bool ContainsDHCPTLVItem(DHCPTLVItem dhItem)
        {
            return lTLVs.Contains(dhItem);
        }

        #endregion

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public DHCPFrame()
        {
            dhtMessageType = DHCPType.BootRequest;
            dhtHardwareType = eExNetworkLibrary.HardwareAddressType.Ethernet;
            sHardwarelen = 6;
            sHops = 0;
            iTransactionID = 0;
            iSecs = 0;
            bValidIPFlag = true;
            ipaClientAddress = IPAddress.Any;
            ipaOwnAddress = IPAddress.Any;
            ipaServerAddress = IPAddress.Any;
            ipaRelayAddress = IPAddress.Any;
            macClientMac = MACAddress.Empty;
            strRequestedFile = "";
            strRequestedServerName = "";
            lTLVs = new List<DHCPTLVItem>();
        }

        /// <summary>
        /// Creates a new instance of this class by parsing the given data
        /// </summary>
        /// <param name="bData">The data to parse</param>
        public DHCPFrame(byte[] bData)
        {
            byte[] bAddressBytes = new byte[4];
            byte[] bMacBytes = new byte[6];
            lTLVs = new List<DHCPTLVItem>();
            DHCPTLVItem dhcpItem;
            
            dhtMessageType = (DHCPType)bData[0];
            dhtHardwareType = (HardwareAddressType)bData[1];
            sHardwarelen = (short)bData[2];
            sHops = (short)bData[3];
            iTransactionID = BitConverter.ToInt32(bData, 4);
            iSecs = BitConverter.ToInt16(bData, 8);
            bValidIPFlag = (bData[10] & 0x80) == 1;

            for (int iC1 = 0; iC1 < 4; iC1++)
            {
                bAddressBytes[iC1] = bData[12 + iC1];
            }
            ipaClientAddress = new IPAddress(bAddressBytes);

            for (int iC1 = 0; iC1 < 4; iC1++)
            {
                bAddressBytes[iC1] = bData[16 + iC1];
            }
            ipaOwnAddress = new IPAddress(bAddressBytes);

            for (int iC1 = 0; iC1 < 4; iC1++)
            {
                bAddressBytes[iC1] = bData[20 + iC1];
            }
            ipaServerAddress = new IPAddress(bAddressBytes);

            for (int iC1 = 0; iC1 < 4; iC1++)
            {
                bAddressBytes[iC1] = bData[24 + iC1];
            }
            ipaRelayAddress = new IPAddress(bAddressBytes);

            for (int iC1 = 0; iC1 < 6; iC1++)
            {
                bMacBytes[iC1] = bData[28 + iC1];
            }
            macClientMac = new MACAddress(bMacBytes);

            strRequestedServerName = Encoding.ASCII.GetString(bData, 34, 64).Trim(new char[] { '\0' });
            strRequestedFile = Encoding.ASCII.GetString(bData, 98, 128).Trim(new char[] { '\0' });

            if (bData[236] != 0x63 || bData[237] != 0x82 || bData[238] != 0x53 || bData[239] != 0x63)
            {
                throw new Exception("Invalid DHCP magic number");
            }

            int iC2 = 240;
            while (iC2 < bData.Length)
            {
                if (bData[iC2] == (int)DHCPOptions.End)
                {
                    break;
                }
                dhcpItem = new DHCPTLVItem(bData, iC2);
                lTLVs.Add(dhcpItem);
                iC2 += dhcpItem.Length;
            }
        }

        /// <summary>
        /// Returns the type of this frame.
        /// </summary>
        public override string FrameType
        {
            get { return DHCPFrame.DefaultFrameType; }
        }

        /// <summary>
        /// Returns the byte representation of this frame
        /// </summary>
        public override byte[] FrameBytes
        {
            get
            {
                byte[] bData = new byte[this.Length];

                bData[0] = (byte)dhtMessageType;
                bData[1] = (byte)dhtHardwareType;
                bData[2] = (byte)sHardwarelen;
                bData[3] = (byte)sHops;
                BitConverter.GetBytes(iTransactionID).CopyTo(bData, 4);
                BitConverter.GetBytes((short)iSecs).CopyTo(bData, 8);
                bData[10] = (byte)(bValidIPFlag ? 0x80 : 0x00);
                ipaClientAddress.GetAddressBytes().CopyTo(bData, 12);
                ipaOwnAddress.GetAddressBytes().CopyTo(bData, 16);
                ipaServerAddress.GetAddressBytes().CopyTo(bData, 20);
                ipaRelayAddress.GetAddressBytes().CopyTo(bData, 24);
                macClientMac.AddressBytes.CopyTo(bData, 28);
                Encoding.ASCII.GetBytes(strRequestedServerName).CopyTo(bData, 34);
                Encoding.ASCII.GetBytes(strRequestedServerName).CopyTo(bData, 98);
                bData[236] = 0x63;
                bData[237] = 0x82;
                bData[238] = 0x53;
                bData[239] = 0x63;

                int iC2 = 240;

                foreach (DHCPTLVItem dhcpTLV in lTLVs)
                {
                    if (iC2 + dhcpTLV.Length > this.Length - 1)
                    {
                        break;
                    }
                    dhcpTLV.FrameBytes.CopyTo(bData, iC2);
                    iC2 += dhcpTLV.Length;
                }

                bData[iC2] = 0xFF;

                return bData;
            }
        }

        /// <summary>
        /// Returns the string representation of this frame
        /// </summary>
        /// <returns>The string representation of this frame</returns>
        public override string ToString()
        {
            string strDescription = this.FrameType.ToString() + ":\n";
            strDescription += "Message type: " + this.dhtMessageType.ToString() + "\n";
            strDescription += "Hardware type: " + this.dhtHardwareType.ToString() + "\n";
            strDescription += "Hardwarelen: " + this.sHardwarelen + "\n";
            strDescription += "Hops: " + this.sHops + "\n";
            strDescription += "Transact ID: " + this.iTransactionID + "\n";
            strDescription += "Seconds: " + this.iSecs + "\n";
            strDescription += "Valid IP: " + this.bValidIPFlag + "\n";
            strDescription += "Client Address: " + this.ipaClientAddress.ToString() + "\n";
            strDescription += "Own Address: " + this.ipaOwnAddress.ToString() + "\n";
            strDescription += "Server Address: " + this.ipaServerAddress.ToString() + "\n";
            strDescription += "Relay Address: " + this.ipaRelayAddress.ToString() + "\n";
            strDescription += "Client Mac: " + this.macClientMac.ToString() + "\n";
            strDescription += "Requested Name: " + this.strRequestedServerName.ToString() + "\n";
            strDescription += "Requested File: " + this.strRequestedFile.ToString() + "\n";
            foreach (DHCPTLVItem tlv in lTLVs)
            {
                strDescription += "Option: " + tlv.DHCPOptionType.ToString() +"\n";
            }
            return strDescription;
        }

        /// <summary>
        /// Returns the length of this frame in bytes
        /// </summary>
        public override int Length
        {
            get
            {
                int iLen = 240;
                foreach (DHCPTLVItem dhcpTLV in lTLVs)
                {
                    iLen += dhcpTLV.Length;
                }
                iLen++;
                iLen += (32 - (iLen % 32));
                return iLen;
            }
        }

        /// <summary>
        /// Returns a new identical copy of this frame
        /// </summary>
        /// <returns>A new identical copy of this frame</returns>
        public override Frame Clone()
        {
            return new DHCPFrame(this.FrameBytes);
        }
    }

    /// <summary>
    /// An enumeration for DHCP types
    /// </summary>
    public enum DHCPType
    {
        /// <summary>
        /// Boot request
        /// </summary>
        BootRequest = 1,
        /// <summary>
        /// Boot reply
        /// </summary>
        BootReply = 2
    }
}
