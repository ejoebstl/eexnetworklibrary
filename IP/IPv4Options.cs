using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary.IP
{
    /// <summary>
    /// This class represents the IPv4 options field
    /// </summary>
    public class IPv4Options
    {
        private List<IPOption> lOptions;

        #region Props

        /// <summary>
        /// Returns all contained options
        /// </summary>
        public IPOption[] Options
        {
            get{ return lOptions.ToArray();}
        }

        /// <summary>
        /// Returns the length of this structure in bytes
        /// </summary>
        public int OptionLength
        {
            get
            {
                int iLength = 0;
                foreach (IPOption oOption in lOptions)
                {
                    iLength += oOption.OptionLength;
                }

                return iLength + ((4 - iLength) % 4);
            }
        }

        /// <summary>
        /// Returns the raw byte representation of this tructure
        /// </summary>
        public byte[] Raw
        {
            get
            {
                byte[] bRaw = new byte[this.OptionLength];
                int iOffset = 0;
                foreach (IPOption oOption in lOptions)
                {
                    oOption.Raw.CopyTo(bRaw, iOffset);
                    iOffset += oOption.OptionLength;
                    if(oOption.OptionNumber == IPOptionNumber.EndOfList)
                    {
                        break;
                    }
                }
                return bRaw;
            }
        }
        

        #endregion

        /// <summary>
        /// Adds an option to this structure
        /// </summary>
        /// <param name="oOption">The option to add</param>
        public void AddOption(IPOption oOption)
        {
            lOptions.Add(oOption);
        }

        /// <summary>
        /// Removes an option from this structure
        /// </summary>
        /// <param name="oOption">The option to remove</param>
        public void RemoveOption(IPOption oOption)
        {
            lOptions.Remove(oOption);
        }

        /// <summary>
        /// Creates a new instance of this class by parsing the given data
        /// </summary>
        /// <param name="bOptionBytes">The data to parse</param>
        public IPv4Options(byte[] bOptionBytes)
        {
            lOptions = new List<IPOption>();
            int iOffset = 0;
            IPOption oOption;
            while(iOffset < bOptionBytes.Length)
            {
                byte[] bSubBytes = new byte[bOptionBytes.Length - iOffset];
                for (int iC1 = iOffset; iC1 < bOptionBytes.Length; iC1++)
                {
                    bSubBytes[iC1 - iOffset] = bOptionBytes[iC1];
                }
                oOption = new IPOption(bSubBytes);
                iOffset += oOption.OptionLength;
                lOptions.Add(oOption);
            }
        }

        /// <summary>
        /// Creates a new, empty instance of this class
        /// </summary>
        public IPv4Options()
        {
            lOptions = new List<IPOption>();
        }

        /// <summary>
        /// Returns a string representation of this class.
        /// </summary>
        /// <returns>A string representation of this class.</returns>
        public override string ToString()
        {
            string strDescription = "";
            foreach (IPOption oOption in lOptions)
            {
                strDescription = oOption.ToString() + "\n";
            }
            return strDescription;
        }
    }

    /// <summary>
    /// This class represents a single IP Option
    /// </summary>
    public class IPOption
    {
        private bool bCopyFlag;
        private IPOptionClass iOptionClass;
        private IPOptionNumber iOptionNumber;
        private byte[] bOptionData;

        /// <summary>
        /// Creates a new instance of this class by parsing the given data
        /// </summary>
        /// <param name="bOptionBytes">The data to parse</param>
        public IPOption(byte[] bOptionBytes)
        {
            this.bCopyFlag = ((int)((bOptionBytes[0] & 0x80) >> 7)) == 1;
            this.iOptionClass = (IPOptionClass)((bOptionBytes[0] & 0x60) >> 5);
            this.iOptionNumber = (IPOptionNumber)(bOptionBytes[0] & 0x1F);

            if (!((iOptionNumber == IPOptionNumber.EndOfList || iOptionNumber == IPOptionNumber.NoOperation) && iOptionClass == IPOptionClass.Control))
            {
                int iOptionLength = (int)(bOptionBytes[1]);

                if (iOptionLength > 2)
                {
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
            else
            {
                this.bOptionData = new byte[0];
            }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public IPOption()
        {
            bCopyFlag = false;
            iOptionClass = IPOptionClass.Control;
            iOptionNumber = IPOptionNumber.NoOperation;
            bOptionData = new byte[0];
        }

        /// <summary>
        /// Returns a string representation of this class.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string strDescription = "IP Option: " + iOptionNumber.ToString() + "/" + iOptionNumber.ToString() + "/";
            for (int iC1 = 0; iC1 < bOptionData.Length; iC1++)
            {
                strDescription += bOptionData[iC1].ToString("x02") + " ";
            }
            return strDescription;
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
        /// Gets the option length
        /// </summary>
        public int OptionLength
        {
            get
            {
                if (!((iOptionNumber == IPOptionNumber.EndOfList || iOptionNumber == IPOptionNumber.NoOperation) && iOptionClass == IPOptionClass.Control))
                {
                    return bOptionData.Length + 2;
                }
                else
                {
                    return 1;
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the option number
        /// </summary>
        public IPOptionNumber OptionNumber
        {
            get { return iOptionNumber; }
            set { iOptionNumber = value; }
        }

        /// <summary>
        /// Gets or sets the option class
        /// </summary>
        public IPOptionClass OptionClass
        {
            get { return iOptionClass; }
            set { iOptionClass = value; }
        }

        /// <summary>
        /// Gets or sets a bool indicating whether the copy flag is set
        /// </summary>
        public bool CopyFlagSet
        {
            get { return bCopyFlag; }
            set { bCopyFlag = value; }
        }

        /// <summary>
        /// Gets the raw byte representation of this structure
        /// </summary>
        public byte[] Raw
        {
            get
            {
                byte[] bNewBytes = new byte[((iOptionNumber == IPOptionNumber.EndOfList || iOptionNumber == IPOptionNumber.NoOperation) && iOptionClass == IPOptionClass.Control) ? 1 : bOptionData.Length + 2];
                byte bInsert = Convert.ToByte(Convert.ToInt32(this.bCopyFlag) << 7);
                bInsert |= Convert.ToByte((int)this.iOptionClass << 5);
                bInsert |= Convert.ToByte((int)this.iOptionNumber);

                bNewBytes[0] = bInsert;

                if (bNewBytes.Length > 1)
                {
                    bNewBytes[1] = Convert.ToByte(bOptionData.Length + 2);

                    bOptionData.CopyTo(bNewBytes, 2);
                }

                return bNewBytes;
            }
        }

        #endregion

    }

    #region Enums

    /// <summary>
    /// An enumeration for IP option classes
    /// </summary>
    public enum IPOptionClass
    {
        /// <summary>
        /// Control class
        /// </summary>
        Control = 0,
        /// <summary>
        /// Debugging and measurement class
        /// </summary>
        DebuggingAndMeasurement = 2

    }

    /// <summary>
    /// An enumeration for IP option numbers
    /// </summary>
    public enum IPOptionNumber
    {
        /// <summary>
        /// Marks the end of an options list
        /// </summary>
        EndOfList = 0,
        /// <summary>
        /// No operation
        /// </summary>
        NoOperation = 1,
        /// <summary>
        /// Security
        /// </summary>
        Security = 2,
        /// <summary>
        /// Loose security routing
        /// </summary>
        LooseSecurityRouting = 3,
        /// <summary>
        /// Strict source routing
        /// </summary>
        StrictSourceRouting = 9,
        /// <summary>
        /// Record route
        /// </summary>
        RecordRoute = 7,
        /// <summary>
        /// Stream ID
        /// </summary>
        StreamID = 8,
        /// <summary>
        /// Internet timestamp
        /// </summary>
        InternetTimestamp = 4
    }

    #endregion
}
