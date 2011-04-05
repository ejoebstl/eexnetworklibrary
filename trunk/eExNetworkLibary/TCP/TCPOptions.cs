using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary.TCP
{
    /// <summary>
    /// Represents the Options part of a TCP frame
    /// </summary>
    public class TCPOptions
    {
        private List<TCPOption> lOptions;

        #region Props

        /// <summary>
        /// Returns all single options
        /// </summary>
        public TCPOption[] Options
        {
            get { return lOptions.ToArray(); }
        }

        /// <summary>
        /// Returns the length of the data of this frame part in bytes
        /// </summary>
        public int OptionLength
        {
            get
            {
                int iLength = 0;
                foreach (TCPOption oOption in lOptions)
                {
                    iLength += oOption.OptionLength;
                }

                return iLength + ((4 - iLength) % 4);
            }
        }

        /// <summary>
        /// Returns this frame part converted to bytes
        /// </summary>
        public byte[] Raw
        {
            get
            {
                byte[] bRaw = new byte[this.OptionLength];
                int iOffset = 0;
                foreach (TCPOption oOption in lOptions)
                {
                    oOption.Raw.CopyTo(bRaw, iOffset);
                    iOffset += oOption.OptionLength;
                    if (oOption.OptionKind == TCPOptionKind.EndOfList)
                    {
                        break;
                    }
                }
                return bRaw;
            }
        }


        #endregion

        /// <summary>
        /// Adds a single TCP option
        /// </summary>
        /// <param name="oOption">The option to add</param>
        public void AddOption(TCPOption oOption)
        {
            lOptions.Add(oOption);
        }

        /// <summary>
        /// Removes a single TCP option
        /// </summary>
        /// <param name="oOption">The option to remove</param>
        public void RemoveOption(TCPOption oOption)
        {
            lOptions.Remove(oOption);
        }

        /// <summary>
        /// Creates a new instance of this class with the contents specified in the given byte array
        /// </summary>
        /// <param name="bOptionBytes">The byte array to parse</param>
        public TCPOptions(byte[] bOptionBytes)
        {
            lOptions = new List<TCPOption>();
            int iOffset = 0;
            TCPOption oOption;
            while (iOffset < bOptionBytes.Length)
            {
                byte[] bSubBytes = new byte[bOptionBytes.Length - iOffset];
                for (int iC1 = iOffset; iC1 < bOptionBytes.Length; iC1++)
                {
                    bSubBytes[iC1 - iOffset] = bOptionBytes[iC1];
                }
                oOption = new TCPOption(bSubBytes);
                iOffset += oOption.OptionLength;
                lOptions.Add(oOption);
            }
        }

        /// <summary>
        /// Creates a new empty instance of this class
        /// </summary>
        public TCPOptions()
        {
            lOptions = new List<TCPOption>();
        }

        /// <summary>
        /// Returns a string representation of this class
        /// </summary>
        /// <returns>A string representation of this class</returns>
        public override string ToString()
        {
            string strDescription = "";
            foreach (TCPOption oOption in lOptions)
            {
                strDescription = oOption.ToString() + "\n";
            }
            return strDescription;
        }
    }

    /// <summary>
    /// Represents a single option
    /// </summary>
    public class TCPOption
    {
        private TCPOptionKind iOptionKind;
        private byte[] bOptionData;

        /// <summary>
        /// Creates a new instance of this class by parsing the specified byte array
        /// </summary>
        /// <param name="bOptionBytes">The data to parse</param>
        public TCPOption(byte[] bOptionBytes)
        {
            this.iOptionKind = (TCPOptionKind)(bOptionBytes[0]);

            if (iOptionKind != TCPOptionKind.EndOfList && iOptionKind != TCPOptionKind.NoOperation)
            {
                int iOptionLength = (int)(bOptionBytes[1]);
                this.bOptionData = new byte[iOptionLength - 2];
                for (int iC1 = 2; iC1 < iOptionLength; iC1++)
                {
                    bOptionData[iC1 - 2] = bOptionBytes[iC1];
                }
            }
            else
            {
                this.bOptionData = new byte[0];
            }
        }

        /// <summary>
        /// Creates a new empty instance of this class
        /// </summary>
        public TCPOption()
        {
            iOptionKind = TCPOptionKind.NoOperation;
            bOptionData = new byte[0];
        }

        #region Props

        /// <summary>
        /// Gets or sets the option data
        /// </summary>
        public byte[] OptionData
        {
            get { return bOptionData; }
            set { bOptionData = value; }
        }

        /// <summary>
        /// Gets the length of this option
        /// </summary>
        public int OptionLength
        {
            get
            {
                if (iOptionKind == TCPOptionKind.EndOfList || iOptionKind == TCPOptionKind.NoOperation)
                {
                    return 1;
                }
                else
                {
                    return bOptionData.Length + 2;
                }
            }
        }

        /// <summary>
        /// Returns the kind of this option
        /// </summary>
        public TCPOptionKind OptionKind
        {
            get { return iOptionKind; }
            set { iOptionKind = value; }
        }

        /// <summary>
        /// Gets the byte representation of this option
        /// </summary>
        public byte[] Raw
        {
            get
            {
                byte[] bNewBytes = new byte[(iOptionKind == TCPOptionKind.EndOfList || iOptionKind == TCPOptionKind.NoOperation) ? 1 : bOptionData.Length + 2];

                bNewBytes[0] = (byte)iOptionKind;

                if (bNewBytes.Length > 1)
                {
                    bNewBytes[1] = Convert.ToByte(bOptionData.Length + 2);

                    bOptionData.CopyTo(bNewBytes, 2);
                }

                return bNewBytes;
            }
        }

        #endregion

        /// <summary>
        /// Returns the string representation of this object
        /// </summary>
        /// <returns>The string representation of this object</returns>
        public override string ToString()
        {
            string strDescription = "TCP Option: " + iOptionKind.ToString() + "/";
            for (int iC1 = 0; iC1 < bOptionData.Length; iC1++)
            {
                strDescription += bOptionData[iC1].ToString("x02") + " ";
            }
            return strDescription;
        }
    }

    #region Enums

    /// <summary>
    /// Specifies various TCP options.
    /// More Details can be found here: http://www.iana.org/assignments/tcp-parameters/tcp-parameters.xml
    /// </summary>
    public enum TCPOptionKind
    {
        /// <summary>
        /// End of the TCP options list
        /// </summary>
        EndOfList = 0,
        /// <summary>
        /// No operation
        /// </summary>
        NoOperation = 1,
        /// <summary>
        /// TCP maximum segment size
        /// </summary>
        MaximumSegmentSize = 2,
        /// <summary>
        /// TCP Window Scale (WSOPT)
        /// </summary>
        WindowScale = 3,
        /// <summary>
        /// TCP SACK Permitted
        /// </summary>
        SACKPermitted = 4,
        /// <summary>
        /// SACK
        /// </summary>
        SACK = 5,
        /// <summary>
        /// TCP Echo (Obsoleted)
        /// </summary>
        Echo = 6,
        /// <summary>
        /// TCP Echo reply (Obsoleted)
        /// </summary>
        EchoReply = 7,
        /// <summary>
        /// Time Stamp Option
        /// </summary>
        TSOPT = 8,
        /// <summary>
        /// Partial Order Connection Permitted
        /// </summary>
        PartialOrderConnectionPermitted = 9,
        /// <summary>
        /// Partial Order Service Profile
        /// </summary>
        PartialOrderServiceProfile = 10,
        /// <summary>
        /// CC [RFC1644]
        /// </summary>
        CC = 11,
        /// <summary>
        /// CC.New [RFC1644]
        /// </summary>
        CCNew = 12,
        /// <summary>
        /// CC.Echo [RFC1644]
        /// </summary>
        CCEcho = 13,
        /// <summary>
        /// TCP Alternate Checksum Request
        /// </summary>
        AlternateChecksumRequest = 14,
        /// <summary>
        /// TCP Alternate Checksum Data
        /// </summary>
        AlternateChecksumData = 15,
        /// <summary>
        /// Skeeter
        /// </summary>
        Skeeter = 16,
        /// <summary>
        /// Bubba
        /// </summary>
        Bubba = 17,
        /// <summary>
        /// TCP Trailer Checksum Option
        /// </summary>
        TrailerChecksumOption = 18,
        /// <summary>
        /// MD5 Signature Option (Obsoleted)
        /// </summary>
        MD5SignatureOption = 19,
        /// <summary>
        /// SCPS Capabilities
        /// </summary>
        SCPSCapabilities = 20,
        /// <summary>
        /// Selective Negative Acknowledgements
        /// </summary>
        SlectiveNegativeAcknowledgements = 21,
        /// <summary>
        /// Record Boundaries
        /// </summary>
        RecordBoundaries = 22,
        /// <summary>
        /// Corruption Experienced
        /// </summary>
        CorruptionExperienced = 23,
        /// <summary>
        /// SNAP
        /// </summary>
        SNAP = 24,
        /// <summary>
        /// TCP Compression Filter
        /// </summary>
        TCPCompressionFilter = 26,
        /// <summary>
        /// Quick Start Response
        /// </summary>
        QuickStartResponse = 27,
        /// <summary>
        /// User Timeout Operation
        /// </summary>
        UserTimeoutOperation = 28,
        /// <summary>
        /// TCP Authentication Option (TCP-AO)
        /// </summary>
        TCPAuthentiactionOption = 29
    }
    #endregion
}

