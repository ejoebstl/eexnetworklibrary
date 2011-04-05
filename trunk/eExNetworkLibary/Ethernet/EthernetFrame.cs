using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.IP;
using eExNetworkLibrary.ARP;

namespace eExNetworkLibrary.Ethernet
{
    /// <summary>
    /// This class represents a simple ethernet frame
    /// </summary>
    public class EthernetFrame : Frame
    {
        private MACAddress maSource;
        private MACAddress maDestination;
        private bool bVlanTagExists;
        private int iVlanID;
        private int iVlanPriotity;
        private bool bCanocialFormatIndicator;
        private EtherType etEtherType;

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public EthernetFrame()
        {
            maSource = new MACAddress();
            maDestination = new MACAddress();
            bVlanTagExists = false;
            iVlanID = 0;
            iVlanPriotity = 0;
            bCanocialFormatIndicator = false;
            etEtherType = EtherType.IPv4;
        }

        /// <summary>
        /// Creates a new instance of this class by parsing the given data
        /// </summary>
        /// <param name="bData">The data to parse</param>
        public EthernetFrame(byte[] bData)
        {
            //Pad is not used by pcap.
            //if (bData.Length < 64)
            //{
            //    throw new ArgumentException("Invalid packet length");
            //}
            byte[] bSourceAddressbytes = new byte[6];
            byte[] bDestinationAddressbytes = new byte[6];
            byte[] bEncapsulatedData = null;

            for (int iC1 = 0; iC1 < 6; iC1++)
            {
                bDestinationAddressbytes[iC1] = bData[iC1];
                bSourceAddressbytes[iC1] = bData[iC1 + 6];
            }

            maSource = new MACAddress(bSourceAddressbytes);
            maDestination = new MACAddress(bDestinationAddressbytes);

            
            etEtherType = (EtherType)((bData[12] << 8) + bData[13]);

            if (etEtherType == EtherType.VLANTag)
            {
                bVlanTagExists = true;
                bCanocialFormatIndicator = (bData[14] & 0x80) > 0 ? true : false;
                iVlanPriotity = (bData[14] & 0x70) >> 4;
                iVlanID = (bData[14] & 0x0F) * 256 + bData[15];
                etEtherType = (EtherType)(bData[16] * 256 + bData[17]);

                bEncapsulatedData = new byte[bData.Length - 18];
                for (int iC1 = 0; iC1 < bEncapsulatedData.Length; iC1++)
                {
                    bEncapsulatedData[iC1] = bData[iC1 + 18];
                }
            }
            else
            {
                bVlanTagExists = false;
                bEncapsulatedData = new byte[bData.Length - 14];
                for (int iC1 = 0; iC1 < bEncapsulatedData.Length; iC1++)
                {
                    bEncapsulatedData[iC1] = bData[iC1 + 14];
                }
            }

            if (this.etEtherType == EtherType.IPv4)
            {
                this.EncapsulatedFrame = new IPv4Frame(bEncapsulatedData);
            } 
            else if (this.etEtherType == EtherType.IPv6)
            {
                this.EncapsulatedFrame = new IPv6Frame(bEncapsulatedData);
            }
            else if (this.etEtherType == EtherType.ARP)
            {
                this.EncapsulatedFrame = new ARPFrame(bEncapsulatedData);
            }
            else
            {
                this.EncapsulatedFrame = new RawDataFrame(bEncapsulatedData);
            }
        }

        #region Props

        /// <summary>
        /// Gets or sets the source MAC address
        /// </summary>
        public MACAddress Source
        {
            get { return maSource; }
            set { maSource = value; }
        }

        /// <summary>
        /// Gets or sets the destination MAC address
        /// </summary>
        public MACAddress Destination
        {
            get { return maDestination; }
            set { maDestination = value; }
        }

        /// <summary>
        /// Gets or sets a bool indicating whether a VLAN tag exists
        /// </summary>
        public bool VlanTagExists
        {
            get { return bVlanTagExists; }
            set { bVlanTagExists = value; }
        }

        /// <summary>
        /// Gets or sets the VLAN ID. This will be ignored if the property VlanTagExists is set to false.
        /// </summary>
        public int VlanID
        {
            get { return iVlanID; }
            set { iVlanID = value; }
        }

        /// <summary>
        /// Gets or sets the VLAN priority. This will be ignored if the property VlanTagExists is set to false.
        /// </summary>
        public int VlanPriotity
        {
            get { return iVlanPriotity; }
            set { iVlanPriotity = value; }
        }

        /// <summary>
        /// Gets or sets a bool indicating whether the cnaocial format indicator has been set
        /// </summary>
        public bool CanocialFormatIndicator
        {
            get { return bCanocialFormatIndicator; }
            set { bCanocialFormatIndicator = value; }
        }

        /// <summary>
        /// Gets or sets the ethernet type
        /// </summary>
        public EtherType EtherType
        {
            get { return etEtherType; }
            set { etEtherType = value; }
        }

        /// <summary>
        /// Returns FrameType.Ethernet. 
        /// </summary>
        public override FrameType FrameType
        {
            get { return FrameType.Ethernet; }
        }

        /// <summary>
        /// Returns the byte representation of this frame
        /// </summary>
        public override byte[] FrameBytes
        {
            get
            {
                byte[] bRaw = new byte[this.Length];

                for (int iC1 = 0; iC1 < 6; iC1++)
                {
                    bRaw[iC1] = maDestination.AddressBytes[iC1];
                    bRaw[iC1 + 6] = maSource.AddressBytes[iC1];
                }

                if (bVlanTagExists)
                {
                    bRaw[12] = (byte)(((int)EtherType.VLANTag) >> 8);
                    bRaw[13] = (byte)((int)EtherType.VLANTag & 0xFF);
                    bRaw[14] |= (byte)(bCanocialFormatIndicator ? 0x80 : 0x0);
                    bRaw[14] |= (byte)((iVlanPriotity << 4) & 0x70);
                    bRaw[14] |= (byte)(((int)(iVlanID / 256)) & 0x0F);
                    bRaw[15] |= (byte)(iVlanID & 0xFF);
                    bRaw[16] = (byte)(((int)etEtherType) >> 8);
                    bRaw[17] = (byte)etEtherType;

                    if (fEncapsulatedFrame != null)
                    {
                        byte[] bEncapsulatedData = fEncapsulatedFrame.FrameBytes;
                        for (int iC1 = 0; iC1 < bEncapsulatedData.Length; iC1++)
                        {
                            bRaw[iC1 + 18] = bEncapsulatedData[iC1];
                        }
                    }

                }
                else
                {
                    bRaw[12] = (byte)(((int)etEtherType) >> 8);
                    bRaw[13] = (byte)etEtherType;

                    if (fEncapsulatedFrame != null)
                    {
                        byte[] bEncapsulatedData = fEncapsulatedFrame.FrameBytes;
                        for (int iC1 = 0; iC1 < bEncapsulatedData.Length; iC1++)
                        {
                            bRaw[iC1 + 14] = bEncapsulatedData[iC1];
                        }
                    }
                }

                return bRaw;
            }
        }

        /// <summary>
        /// Returns the length of this frame and its encapsulated frame in bytes
        /// </summary>
        public override int Length
        {
            get 
            {
                int iLen;
                if (fEncapsulatedFrame != null)
                {
                    if (bVlanTagExists)
                    {
                        iLen = 18 + fEncapsulatedFrame.Length;
                    }
                    else
                    {
                        iLen = 14 + fEncapsulatedFrame.Length;
                    }
                }
                else
                {
                    if (bVlanTagExists)
                    {
                        iLen = 18;
                    }
                    else
                    {
                        iLen = 14;
                    }
                }
                if (iLen < 64)
                    iLen = 64;

                return iLen;
            }
        }

        #endregion

        /// <summary>
        /// Creates a new, identical instance of this frame
        /// </summary>
        /// <returns>A new, identical instance of this frame</returns>
        public override Frame Clone()
        {
            return new EthernetFrame(this.FrameBytes);
        }

        /// <summary>
        /// Returns the string representation of this frame
        /// </summary>
        /// <returns>The string representation of this frame</returns>
        public override string ToString()
        {
            string strDescription = this.FrameType.ToString() + ":\n";
            strDescription += "Source: " + this.Source.ToString() + "\n";
            strDescription += "Destination: " + this.Destination.ToString() + "\n";
            strDescription += "Format indicator: " + this.CanocialFormatIndicator + "\n";
            strDescription += "Ether type: " + this.EtherType.ToString() + "\n";
            if (this.VlanTagExists)
            {
                strDescription += "Vlan ID: " + this.VlanID + "\n";
                strDescription += "Vlan Priority: " + this.VlanPriotity + "\n";
            }
            return strDescription;
        }
    }
}
