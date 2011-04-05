using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace eExNetworkLibrary.DNS
{
    /// <summary>
    /// This class represens an DNS resource record encapsulated in a DNS frame
    /// </summary>
    public class DNSResourceRecord : HelperStructure
    {
        private string strName;
        private DNSResourceType dnsType;
        private DNSResourceClass dnsClass;
        private int iTTL;
        private byte[] bResourceData;
        private DNSNameEncoder dnsEnc;

        /// <summary>
        /// Gets or sets the resource data
        /// </summary>
        public byte[] ResourceData
        {
            get { return bResourceData; }
            set { bResourceData = value; }
        }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name
        {
            get { return strName; }
            set { strName = value; }
        }

        /// <summary>
        /// Gets or sets the resource type
        /// </summary>
        public DNSResourceType Type
        {
            get { return dnsType; }
            set { dnsType = value; }
        }

        /// <summary>
        /// Gets or sets the resource type
        /// </summary>
        public DNSResourceClass Class
        {
            get { return dnsClass; }
            set { dnsClass = value; }
        }

        /// <summary>
        /// Gets or sets the TTL
        /// </summary>
        public int TTL
        {
            get { return iTTL; }
            set { iTTL = value; }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public DNSResourceRecord()
        {
            dnsEnc = new DNSNameEncoder();
            strName = "";
            dnsType = DNSResourceType.All;
            dnsClass = DNSResourceClass.Any;
        }


        /// <summary>
        /// Creates a new instance of this class by parsing the given data
        /// <seealso cref="DNSQuestion"/>
        /// <example><code>
        /// // For parsing DNS frames, set the index variable to the index where parsing should begin. 
        /// // This is in case of DNS frames 12 + the length of all records before this record
        /// int iIndex = 12;
        ///
        /// // Parse all records
        /// while (lRecords.Count &lt; iCount)
        /// {
        ///     // Create a new DNS records from the data and pass the index as pointer to the constructor.
        ///     // The index will be increased during parsing so that it will point to the beginning of the next record.
        ///     DNSResourceRecord qLast = new DNSResourceRecord(bData, ref iIndex);
        ///     lRecords.Add(qLast);
        /// }
        /// </code></example>
        /// </summary>
        /// <param name="bData">The data to parse</param>
        /// <param name="iParserIndex">The index where parsing starts. This index will be incremented automatically during parsing</param>
        public DNSResourceRecord(byte[] bData, ref int iParserIndex)
        {
            dnsEnc = new DNSNameEncoder();
            int iStringLen = 0;
            strName = dnsEnc.DecodeDNSName(bData, iParserIndex, ref iStringLen).Substring(1);
            iParserIndex += iStringLen;
            dnsType = (DNSResourceType)(((bData[iParserIndex]) << 8) + bData[iParserIndex + 1]);
            dnsClass = (DNSResourceClass)(((bData[iParserIndex + 2]) << 8) + bData[iParserIndex + 3]);
            iTTL = ((int)bData[iParserIndex + 4] << 24) + ((int)bData[iParserIndex + 5] << 16) + ((int)bData[iParserIndex + 6] << 8) + bData[iParserIndex + 7];
            int iRDLen = ((int)bData[iParserIndex + 8] << 8) + bData[iParserIndex + 9];

            iParserIndex += 10;
            if (dnsType != DNSResourceType.CNAME)
            {
                bResourceData = new byte[iRDLen];

                for (int iC1 = 0; iC1 < iRDLen; iC1++)
                {
                    bResourceData[iC1] = bData[iParserIndex + iC1];
                }
            }
            else
            {
                int iDummy = 0;
                bResourceData = ASCIIEncoding.ASCII.GetBytes(dnsEnc.DecodeDNSName(bData, iParserIndex, ref iDummy).Substring(1));
            }

            iParserIndex += iRDLen;
        }

        /// <summary>
        /// Returns the length of this structure in bytes
        /// </summary>
        public override int Length
        {
            get
            {
                return 10 + (strName.Length > 0 && strName[0] == '.' ? strName.Length + 1 : strName.Length + 2) + bResourceData.Length;
            }
        }

        /// <summary>
        /// Returns the compressed bytes of this DNS record
        /// <example><code>
        /// // For parsing DNS frames, set the index variable to the index where parsing should begin. 
        /// // This is in case of DNS frames 12 + the length of all records before this record
        /// int iIndex = 12;
        /// // If available, you should use the Dictionary created when compression the DNS questions. Else create a new, empty dictionary
        /// Dictionary&lt;string, int&gt; dictCompression = new Dictionary&lt;string, int&gt;();
        ///
        /// // For all recirds...
        /// foreach (DNSResourceRecord r in lRecords)
        /// {
        ///     // Get the compressed bytes by passing the index at which this record will be inserted in the DNS frame and the dictionary to the corresponding method.
        ///     bData = r.GetCompressedBytes(dictCompression, iIndex);
        ///     
        ///     // Increase the index value
        ///     iIndex += bData.Length;
        ///     
        ///     // ... Do something with the data ...
        /// }
        /// 
        /// // For a maximum compression factor re-use the same dictionary in the other resource sections of the frame. 
        /// </code></example>
        /// </summary>
        /// <param name="dictCompression">A dictionary containing strings and their corresponding indexes from a DNS frame. If this is the first call to this function for a specific DNS frame, an empty instance of
        /// Dictionar&lt;string, int&gt; should be passed, which can be reused in further calls of this method</param>
        /// <param name="iStartIndex">The index at which this record will be inseted</param>
        /// <returns>This DNS question compressed as byte array</returns>
        public byte[] GetCompressedBytes(Dictionary<string, int> dictCompression, int iStartIndex)
        {
            MemoryStream msStream = new MemoryStream();
            byte[] bName = dnsEnc.CompressDNSName(strName, dictCompression, iStartIndex);
            msStream.Write(bName, 0, bName.Length);
            byte[] bData = new byte[10];
            bData[0] = (byte)(((int)dnsType >> 8) & 0xFF);
            bData[1] = (byte)(((int)dnsType) & 0xFF);
            bData[2] = (byte)(((int)dnsClass >> 8) & 0xFF);
            bData[3] = (byte)(((int)dnsClass) & 0xFF);

            bData[4] = (byte)((iTTL >> 24) & 0xFF);
            bData[5] = (byte)((iTTL >> 16) & 0xFF);
            bData[6] = (byte)((iTTL >> 8) & 0xFF);
            bData[7] = (byte)((iTTL) & 0xFF);

            bData[8] = (byte)((bResourceData.Length >> 8) & 0xFF);
            bData[9] = (byte)((bResourceData.Length) & 0xFF);
            msStream.Write(bData, 0, bData.Length);
            msStream.Write(bResourceData, 0, bResourceData.Length);

            return msStream.ToArray();
        }

        /// <summary>
        /// Returns the byte representation of this structure
        /// </summary>
        public override byte[] Bytes
        {
            get
            {
                byte[] bData = new byte[this.Length];
                byte[] bName = dnsEnc.EncodeDNSName(strName);
                bName.CopyTo(bData, 0);
                int iIndex = bName.Length;
                bData[iIndex] = (byte)(((int)dnsType >> 8) & 0xFF);
                bData[iIndex + 1] = (byte)(((int)dnsType) & 0xFF);
                bData[iIndex + 2] = (byte)(((int)dnsClass >> 8) & 0xFF);
                bData[iIndex + 3] = (byte)(((int)dnsClass) & 0xFF);

                bData[iIndex + 4] = (byte)((iTTL >> 24) & 0xFF);
                bData[iIndex + 5] = (byte)((iTTL >> 16) & 0xFF);
                bData[iIndex + 6] = (byte)((iTTL >> 8) & 0xFF);
                bData[iIndex + 7] = (byte)((iTTL) & 0xFF);

                bData[iIndex + 8] = (byte)((bResourceData.Length >> 8) & 0xFF);
                bData[iIndex + 9] = (byte)((bResourceData.Length) & 0xFF);

                bResourceData.CopyTo(bData, iIndex + 10);

                return bData;
            }
        }
    }
}
