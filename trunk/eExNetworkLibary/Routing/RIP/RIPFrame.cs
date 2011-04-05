using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace eExNetworkLibrary.Routing.RIP
{
    /// <summary>
    /// This class represents a RIP frame
    /// </summary>
    public class RIPFrame : Frame
    {
        private RipCommand ripCommand;
        private uint iVersion;
        private byte[] bReservedField;
        private List<RIPUpdate> lRipUpdates;

        /// <summary>
        /// Gets the default RIPv2 multicast address.
        /// </summary>
        public static IPAddress RIPv2MulticastAddress
        {
            get { return new IPAddress(new byte[] { 224, 0, 0, 9 }); }
        }

        /// <summary>
        /// Creates a new RIP frame by parsing the given data.
        /// </summary>
        /// <param name="bData">The data to parse</param>
        public RIPFrame(byte[] bData)
        {
            ripCommand = (RipCommand)(bData[0]);
            iVersion = bData[1];
            bReservedField = new byte[2];
            bReservedField[0] = bData[2];
            bReservedField[1] = bData[3];
            byte[] bUpdateData = new byte[20];
            lRipUpdates = new List<RIPUpdate>();

            for (int iC1 = 4; iC1 < bData.Length; iC1 += 20)
            {
                for (int iC2 = iC1; iC2 < iC1 + 20; iC2++)
                {
                    bUpdateData[iC2 - iC1] = bData[iC2];
                }
                lRipUpdates.Add(new RIPUpdate(bUpdateData));
            }
        }

        /// <summary>
        /// Gets or sets the RIP command
        /// </summary>
        public RipCommand Command
        {
            get { return ripCommand; }
            set { ripCommand = value; }
        }

        /// <summary>
        /// Gets or sets the version of this RIP frame
        /// </summary>
        public uint Version
        {
            get { return iVersion; }
            set { iVersion = value; }
        }
        
        /// <summary>
        /// Returns all RIP updates contained in this frame.
        /// </summary>
        /// <returns>All RIP updates contained in this frame</returns>
        public RIPUpdate[] GetUpdates()
        {
            return lRipUpdates.ToArray();
        }

        /// <summary>
        /// Adds a RIP update to this frame
        /// </summary>
        /// <param name="ripUpdate">The RIP update to add</param>
        public void AddUpdate(RIPUpdate ripUpdate)
        {
            lRipUpdates.Add(ripUpdate);
        }

        /// <summary>
        /// Removes a RIP update from this frame.
        /// </summary>
        /// <param name="ripUpdate">The update to remove</param>
        public void RemoveUpdate(RIPUpdate ripUpdate)
        {
            lRipUpdates.Remove(ripUpdate);
        }

        /// <summary>
        /// Clears all RIP updates from this frame.
        /// </summary>
        public void ClearUpdates()
        {
            lRipUpdates.Clear();
        }

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        public RIPFrame()
        {
            lRipUpdates = new List<RIPUpdate>();
            bReservedField = new byte[2];
            iVersion = 2;
            ripCommand = RipCommand.RIPRequest;
        }

        /// <summary>
        /// Returns FrameType.RIP
        /// </summary>
        public override FrameType FrameType
        {
            get { return FrameType.RIP; }
        }

        /// <summary>
        /// Gets the raw byte representation of this frame.
        /// </summary>
        public override byte[] FrameBytes
        {
            get 
            {
                byte[] bData = new byte[this.Length];
                bData[0] = (byte)ripCommand;
                bData[1] = (byte)iVersion;
                bData[2] = bReservedField[0];
                bData[3] = bReservedField[1];

                int iOffset = 4;

                foreach (RIPUpdate ripUpdate in lRipUpdates)
                {
                    ripUpdate.Raw.CopyTo(bData, iOffset);
                    iOffset += ripUpdate.Length;
                }

                return bData;
            }
        }

        /// <summary>
        /// Gets the length of this frame's bytes.
        /// </summary>
        public override int Length
        {
            get
            {
                int iLen = 4;
                foreach (RIPUpdate ripUpdate in lRipUpdates)
                {
                    iLen += ripUpdate.Length;
                }
                return iLen;
            }
        }

        /// <summary>
        /// Returns a string representation of this frame.
        /// </summary>
        /// <returns>A string representation of this frame</returns>
        public override string ToString()
        {
            string strString = "";
            strString += this.FrameType.ToString() + " Version: " + iVersion.ToString() + "\n";
            strString += "Version: " + this.iVersion + "\n";
            strString += "Command: " + this.ripCommand.ToString() + "\n";
            foreach (RIPUpdate ripUpdate in lRipUpdates)
            {
                strString += ripUpdate.ToString((int)iVersion) ;
            }

            return strString;
        }
        
        /// <summary>
        /// Clones this frame.
        /// </summary>
        /// <returns>A new, identical RIPFrame</returns>
        public override Frame Clone()
        {
            return new RIPFrame(this.FrameBytes);
        }
    }

    /// <summary>
    /// Represents a RIP update, which is usually contained in a RIP frame.
    /// </summary>
    public class RIPUpdate
    {
        private RIPEntryAddressFamily afiAddressFamilyIdentifier;
        private byte[] bRouteTag;
        private IPAddress ipaAddress;
        private Subnetmask smSubnetMask;
        private IPAddress ipaNextHop;
        private uint iMetric;

        /// <summary>
        /// Gets or sets the address family identifier.
        /// </summary>
        public RIPEntryAddressFamily AddressFamilyIdentifier
        {
            get { return afiAddressFamilyIdentifier; }
            set { afiAddressFamilyIdentifier = value; }
        }

        /// <summary>
        /// Gets or sets the IPAddress of the destination
        /// </summary>
        public IPAddress Address
        {
            get { return ipaAddress; }
            set { ipaAddress = value; }
        }

        /// <summary>
        /// Gets or sets the IPAddress of the RIPv2 next hop
        /// </summary>
        public IPAddress Ripv2NextHop
        {
            get { return ipaNextHop; }
            set { ipaNextHop = value; }
        }

        /// <summary>
        /// Gets or sets the subnetmask for RIPv2
        /// </summary>
        public Subnetmask Ripv2SubnetMask
        {
            get { return smSubnetMask; }
            set { smSubnetMask = value; }
        }

        /// <summary>
        /// Gets or sets the RIPv2 route tag-
        /// </summary>
        public byte[] Ripv2RouteTag
        {
            get { return bRouteTag; }
            set { bRouteTag = value; }
        }

        /// <summary>
        /// Gets or sets the metric
        /// </summary>
        public uint Metric
        {
            get { return iMetric; }
            set { iMetric = value; }
        }

        /// <summary>
        /// Creates a new instance of this class by parsing the given data.
        /// </summary>
        /// <param name="bData">The data to parse.</param>
        public RIPUpdate(byte[] bData)
        {
            afiAddressFamilyIdentifier = (RIPEntryAddressFamily)((bData[0] << 8) + bData[1]);
            bRouteTag = new byte[2];
            bRouteTag[0] = bData[2];
            bRouteTag[1] = bData[3];
            byte[] bAddressBytes = new byte[4];
            bAddressBytes[0] = bData[4];
            bAddressBytes[1] = bData[5];
            bAddressBytes[2] = bData[6];
            bAddressBytes[3] = bData[7];
            ipaAddress = new IPAddress(bAddressBytes);
            smSubnetMask = new Subnetmask();
            smSubnetMask.MaskBytes[0] = bData[8];
            smSubnetMask.MaskBytes[1] = bData[9];
            smSubnetMask.MaskBytes[2] = bData[10];
            smSubnetMask.MaskBytes[3] = bData[11];
            bAddressBytes[0] = bData[12];
            bAddressBytes[1] = bData[13];
            bAddressBytes[2] = bData[14];
            bAddressBytes[3] = bData[15];
            ipaNextHop = new IPAddress(bAddressBytes);
            iMetric = (uint)((bData[16] << 24) + (bData[17] << 16) + (bData[18] << 8) + bData[19]);
        }

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        public RIPUpdate()
        {
            afiAddressFamilyIdentifier = RIPEntryAddressFamily.IPv4;
            bRouteTag = new byte[2];
            smSubnetMask = new Subnetmask();
            ipaNextHop = IPAddress.Any;
            ipaAddress = IPAddress.Any;
            iMetric = 0;
        }

        /// <summary>
        /// Creates a new instance of this class with the given properties.
        /// </summary>
        /// <param name="afiAddressFamilyIdentifier">The address family identifier</param>
        /// <param name="bRouteTag">The route tag for RIPv2, a byte array of length two</param>
        /// <param name="smMask">The subnetmask for RIPv2</param>
        /// <param name="ipaNextHop">The IPAddress of the next hop for RIPv2</param>
        /// <param name="ipaAddress">The IPAddress of the destination</param>
        /// <param name="iMetric">The metric</param>
        public RIPUpdate(RIPEntryAddressFamily afiAddressFamilyIdentifier, byte[] bRouteTag, Subnetmask smMask, IPAddress ipaNextHop, IPAddress ipaAddress, uint iMetric)
        {
            this.afiAddressFamilyIdentifier = afiAddressFamilyIdentifier;
            if (bRouteTag.Length != 2)
            {
                throw new ArgumentException("Invalid route tag. Must be a byte-array of len two");
            }
            this.bRouteTag = bRouteTag;
            this.smSubnetMask = smMask;
            this.ipaNextHop = ipaNextHop;
            this.ipaAddress = ipaAddress;
            this.iMetric = iMetric;
        }

        /// <summary>
        /// Returns the raw byte representation of this RIP update.
        /// </summary>
        public byte[] Raw
        {
            get
            {
                byte[] bData = new byte[this.Length];
                bData[0] = (byte)((((int)(afiAddressFamilyIdentifier)) >> 8) & 0xFF);
                bData[1] = (byte)(((int)(afiAddressFamilyIdentifier)) & 0xFF);
                bData[2] = bRouteTag[0];
                bData[3] = bRouteTag[1];
                byte[] bAddressBytes = ipaAddress.GetAddressBytes();
                bData[4] = bAddressBytes[0];
                bData[5] = bAddressBytes[1];
                bData[6] = bAddressBytes[2];
                bData[7] = bAddressBytes[3];
                bData[8] = smSubnetMask.MaskBytes[0];
                bData[9] = smSubnetMask.MaskBytes[1];
                bData[10] = smSubnetMask.MaskBytes[2];
                bData[11] = smSubnetMask.MaskBytes[3];
                bAddressBytes = ipaNextHop.GetAddressBytes();
                bData[12] = bAddressBytes[0];
                bData[13] = bAddressBytes[1];
                bData[14] = bAddressBytes[2];
                bData[15] = bAddressBytes[3];
                bData[16] = (byte)((iMetric >> 24) & 0xFF);
                bData[17] = (byte)((iMetric >> 16) & 0xFF);
                bData[18] = (byte)((iMetric >> 8) & 0xFF);
                bData[19] = (byte)((iMetric) & 0xFF);
                return bData;
            }
        }

        /// <summary>
        /// Returns the length of this RIP update in bytes (always 20)
        /// </summary>
        public int Length
        {
            get { return 20; }
        }

        /// <summary>
        /// Returns the string representation of this RIP frame as RIPv1 frame.
        /// </summary>
        /// <returns>The string representation of this RIP frame as RIPv1 frame</returns>
        public override string ToString()
        {
            return ToString(1);
        }

        /// <summary>
        /// Returns the string representation of this frame as the given RIP version.
        /// </summary>
        /// <param name="iVersion">The version of this frame.</param>
        /// <returns>A string describing this frame according to the given version.</returns>
        public string ToString(int iVersion)
        {
            if (iVersion == 1)
            {
                return "Address Family Identifier: " + afiAddressFamilyIdentifier.ToString() + ", Address: " + ipaAddress.ToString() + ", Metric" + iMetric + "\n";
            }
            else if(iVersion == 2)
            {
                return "Address Family Identifier: " + afiAddressFamilyIdentifier.ToString() +
                    ", Route Tag: " + bRouteTag[0].ToString("x02") + bRouteTag[1].ToString("x02") +
                    ", Address: " + ipaAddress.ToString() +
                    ", Subnet Mask: " + smSubnetMask.ToString() +
                    ", Next Hop: " + ipaNextHop.ToString() +
                    ", Metric: " + iMetric + "\n";
            }
            else 
            {
                return "";
            }
        }
    }

    /// <summary>
    /// An enum defining several RIP commands.
    /// </summary>
    public enum RipCommand
    {
        /// <summary>
        /// A RIP request
        /// </summary>
        RIPRequest = 1,
        /// <summary>
        /// A RIP response
        /// </summary>
        RIPResponse = 2,
        /// <summary>
        /// The RIP TraceOn command
        /// </summary>
        TraceOn = 3,
        /// <summary>
        /// The RIP TrafeOff command
        /// </summary>
        TraceOf = 4,
        /// <summary>
        /// A reserved RIP command
        /// </summary>
        Reserved = 5 
    }

    /// <summary>
    /// The RIP update's address family.
    /// </summary>
    public enum RIPEntryAddressFamily
    {
        /// <summary>
        /// Internet protocol version 4
        /// </summary>
        IPv4 = 2
    }
}
