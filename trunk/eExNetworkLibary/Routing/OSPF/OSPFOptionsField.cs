using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary.Routing.OSPF
{
    /// <summary>
    /// This class represents an OSPF options field
    /// </summary>
    public class OSPFOptionsField
    {
        private bool bDNBit;
        private bool bOBit;
        private bool bDemandCircuitsSupported;
        private bool bContainsLLSData;
        private bool bSupportsNSSA;
        private bool bIsMulticastCapable;
        private bool bIsExternalRouteCapable;
        private bool bTBit;

        /// <summary>
        /// Gets or sets the T-bit, which indicates the router's TOS capability.
        /// </summary>
        public bool TBit
        {
            get { return bTBit; }
            set { bTBit = value; }
        }

        /// <summary>
        /// Gets or sets the E-bit, which indicates the router's external routing capability.
        /// </summary>
        public bool EBit
        {
            get { return bIsExternalRouteCapable; }
            set { bIsExternalRouteCapable = value; }
        }

        /// <summary>
        /// Gets or sets the MC-bit, which indicates the router's multicast capability.
        /// </summary>
        public bool MCBit
        {
            get { return bIsMulticastCapable; }
            set { bIsMulticastCapable = value; }
        }

        /// <summary>
        /// Gets or sets the routers DN-bit
        /// </summary>
        public bool DNBit
        {
            get { return bDNBit; }
            set { bDNBit = value; }
        }

        /// <summary>
        /// Gets or sets the OSPF O-bit, which indicates the use of opaque-LSAs
        /// </summary>
        public bool OBit
        {
            get { return bOBit; }
            set { bOBit = value; }
        }

        /// <summary>
        /// Gets or sets a bit, which indicates the router's demand circuits capability.
        /// </summary>
        public bool DemandCircuitsSupported
        {
            get { return bDemandCircuitsSupported; }
            set { bDemandCircuitsSupported = value; }
        }

        /// <summary>
        /// Gets or sets a bit indicating whether the OSPF frame this options belong contains LLS data.
        /// </summary>
        public bool ContainsLLSData
        {
            get { return bContainsLLSData; }
            set { bContainsLLSData = value; }
        }

        /// <summary>
        /// Gets or sets a bit, which indicates the router's NSSA capability.
        /// </summary>
        public bool SupportsNSSA
        {
            get { return bSupportsNSSA; }
            set { bSupportsNSSA = value; }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public OSPFOptionsField() { }

        /// <summary>
        /// Creates a new instance of this class by parsing the given data
        /// </summary>
        /// <param name="bData">The data to parse</param>
        public OSPFOptionsField(byte bData)
        {
            bTBit = (bData & 0x1) != 0;
            bIsExternalRouteCapable = (bData & 0x2) != 0;
            bIsMulticastCapable = (bData & 0x4) != 0;
            bSupportsNSSA = (bData & 0x8) != 0;
            bContainsLLSData = (bData & 0x10) != 0;
            bOBit = (bData & 0x20) != 0;
            bDNBit = (bData & 0x40) != 0;
        }

        /// <summary>
        /// Returns this OSPF option class compressed to a single byte
        /// </summary>
        public byte Data
        {
            get
            {
                byte bData = 0;
                bData |= (byte)(bTBit ? 0x1 : 0);
                bData |= (byte)(bIsExternalRouteCapable ? 0x2 : 0);
                bData |= (byte)(bIsMulticastCapable ? 0x4 : 0);
                bData |= (byte)(bSupportsNSSA ? 0x8 : 0);
                bData |= (byte)(bContainsLLSData ? 0x10 : 0);
                bData |= (byte)(bOBit ? 0x20 : 0);
                bData |= (byte)(bDNBit ? 0x40 : 0);
                return bData;
            }
        }

        /// <summary>
        /// Returns the length of the OSPF options field in bytes (1)
        /// </summary>
        public int Length
        {
            get { return 1; }
        }
    }
}
