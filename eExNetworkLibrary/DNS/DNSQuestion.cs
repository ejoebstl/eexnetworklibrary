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
using System.IO;

namespace eExNetworkLibrary.DNS
{
    /// <summary>
    /// Represents a DNS question encapsulated in a DNS frame
    /// </summary>
    public class DNSQuestion : HelperStructure
    {
        private string strQuestion;
        private DNSResourceType qType;
        private DNSResourceClass qClass;
        private DNSNameEncoder dnsEnc;

        /// <summary>
        /// Gets or sets the DNS resource class
        /// </summary>
        public DNSResourceClass Class
        {
            get { return qClass; }
            set { qClass = value; }
        }

        /// <summary>
        /// Gets or sets the DNS resource type
        /// </summary>
        public DNSResourceType Type
        {
            get { return qType; }
            set { qType = value; }
        }

        /// <summary>
        /// Gets or sets the query string
        /// </summary>
        public string Query
        {
            get { return strQuestion; }
            set { strQuestion = value; }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public DNSQuestion()
        {
            dnsEnc = new DNSNameEncoder();
            strQuestion = "";
            qType = DNSResourceType.All;
            qClass = DNSResourceClass.Any;
        }

        /// <summary>
        /// Creates a new instance of this class by parsing the given data
        /// <example><code>
        /// // For parsing DNS frames, set the index variable to 12, because the question section starts there
        /// int iIndex = 12;
        ///
        /// // Parse all questions
        /// while (lQuestions.Count &lt; iQCount)
        /// {
        ///     // Create a new DNS question from the data and pass the index as pointer to the constructor.
        ///     // The index will be increased during parsing so that it will point to the beginning of the next record.
        ///     DNSQuestion qLast = new DNSQuestion(bData, ref iIndex);
        ///     lQuestions.Add(qLast);
        /// }
        /// </code></example>
        /// </summary>
        /// <param name="bData">The data to parse</param>
        /// <param name="iParserIndex">The index where parsing begins. This index must be passed as pointer for it will be increased during parsing.</param>
        public DNSQuestion(byte[] bData, ref int iParserIndex)
        {
            dnsEnc = new DNSNameEncoder();
            int iStringLen = 0;
            strQuestion = DNSNameEncoder.DecodeDNSName(bData, iParserIndex, ref iStringLen).Substring(1);
            iParserIndex += iStringLen;
            qType = (DNSResourceType)(((bData[iParserIndex]) << 8) + bData[iParserIndex + 1]);
            qClass = (DNSResourceClass)(((bData[iParserIndex + 2]) << 8) + bData[iParserIndex + 3]);
            iParserIndex += 4;
        }

        /// <summary>
        /// Returns the length of this structure in bytes
        /// </summary>
        public override int Length
        {
            get
            {
                return 4 + (strQuestion.Length > 0 && strQuestion[0] == '.' ? strQuestion.Length + 1 : strQuestion.Length + 2);
            }
        }


        /// <summary>
        /// Returns the raw byte representation of this structure
        /// </summary>
        public override byte[] Bytes
        {
            get
            {
                byte[] bData = new byte[this.Length];
                byte[] bName = DNSNameEncoder.EncodeDNSName(strQuestion);
                bName.CopyTo(bData, 0);
                int iIndex = bName.Length;
                bData[iIndex] = (byte)(((int)qType >> 8) & 0xFF);
                bData[iIndex + 1] = (byte)(((int)qType) & 0xFF);
                bData[iIndex + 2] = (byte)(((int)qClass >> 8) & 0xFF);
                bData[iIndex + 3] = (byte)(((int)qClass) & 0xFF);

                return bData;
            }
        }

        /// <summary>
        /// Returns the compressed bytes of this DNS question
        /// <example><code>
        /// // For constucting DNS frames, set the index variable to 12, because the question section starts there
        /// int iIndex = 12;
        /// // Create a new, empty dictionary
        /// Dictionary&lt;string, int&gt; dictCompression = new Dictionary&lt;string, int&gt;();
        ///
        /// // For all questions...
        /// foreach (DNSQuestion q in lQuestions)
        /// {
        ///     // Get the compressed bytes by passing the index at which this record will be inserted in the DNS frame and the dictionary to the corresponding method.
        ///     bData = q.GetCompressedBytes(dictCompression, iIndex);
        ///     
        ///     // Increase the index value
        ///     iIndex += bData.Length;
        ///     
        ///     // ... Do something with the data ...
        /// }
        /// 
        /// // For a maximum compression factor re-use the same dictionary in the answer, authorotive and additional section of this frame. 
        /// </code></example>
        /// </summary>
        /// <param name="dictCompression">A dictionary containing strings and their corresponding indexes from a DNS frame. If this is the first call to this function for a specific DNS frame, an empty instance of
        /// Dictionar&lt;string, int&gt; should be passed, which can be reused in further calls of this method</param>
        /// <param name="iStartIndex">The index at which this record will be inseted</param>
        /// <returns>This DNS question compressed as byte array</returns>
        public byte[] GetCompressedBytes(Dictionary<string, int> dictCompression, int iStartIndex)
        {
            MemoryStream msStream = new MemoryStream();

            byte[] bName = DNSNameEncoder.CompressDNSName(strQuestion, dictCompression, iStartIndex);
            msStream.Write(bName, 0, bName.Length);
            byte[] bData = new byte[4];
            bData[0] = (byte)(((int)qType >> 8) & 0xFF);
            bData[1] = (byte)(((int)qType) & 0xFF);
            bData[2] = (byte)(((int)qClass >> 8) & 0xFF);
            bData[3] = (byte)(((int)qClass) & 0xFF);
            msStream.Write(bData, 0, bData.Length);

            return msStream.ToArray();
        }
    }
}
