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
    /// This class represents methods for parsing encoded or compressed DNS names
    /// </summary>
    public class DNSNameEncoder
    {
        /// <summary>
        /// Deocdes a DNS compressed or encoded name from a given array of bytes
        /// </summary>
        /// <param name="bData">The byte array to parse</param>
        /// <param name="iIndex">The index at which the name to parse starts</param>
        /// <param name="iDataLen">A pointer to an integer where the data length is stored. This integer will be increased according to the number of bytes read</param>
        /// <returns>A decoded DNS name</returns>
        public static string DecodeDNSName(byte[] bData, int iIndex, ref int iDataLen)
        {
            int iCount = (int)bData[iIndex];
            int iDummy = 0;
            string strName = "";
            if ((iCount & 0xC0) == 0xC0)
            {
                iDataLen += 2;
                int iPointer = (((bData[iIndex] & 0x3F) << 8) + bData[iIndex + 1]);
                strName = DecodeDNSName(bData, iPointer, ref iDummy);
            }
            else if (iCount == 0)
            {
                //end;
                iDataLen += 1;
            }
            else
            {
                strName = "." + ASCIIEncoding.ASCII.GetString(bData, iIndex + 1, iCount) + DecodeDNSName(bData, iIndex + iCount + 1, ref iDataLen);
                iDataLen += iCount + 1;
            }

            if (strName == "")
            {
                strName = ".";
            }

            return strName;
        }

        /// <summary>
        /// Encodes a string to a DNS encoded name, but does not compress it
        /// </summary>
        /// <param name="strName">The string to encode</param>
        /// <returns>A DNS encoded string converted to bytes</returns>
        public static byte[] EncodeDNSName(string strName)
        {
            if (strName[0] != '.')
            {
                strName = "." + strName;
            }
            byte[] bData = ASCIIEncoding.ASCII.GetBytes(strName + " ");
            int iLastDot = 0;
            int iDotCounter = 0;

            for (int iC1 = 0; iC1 < bData.Length; iC1++)
            {
                if (bData[iC1] == (byte)'.' && iC1 != 0)
                {
                    bData[iLastDot] = (byte)iDotCounter;
                    iLastDot = iC1;
                    iDotCounter = 0;
                }
                iDotCounter++;
            }

            bData[bData.Length - 1] = 0;

            return bData;
        }

        /// <summary>
        /// Compresses a string to a DNS compressed name
        /// </summary>
        /// <param name="strName">The name to compress</param>
        /// <param name="dictCompressionIndices">A dictionary containing strings and their corresponding indexes from a DNS frame. If this is the first call to this function for a specific DNS frame, an empty instance of
        /// Dictionar&lt;string, int&gt; should be passed, which can be used in further calls of this method</param>
        /// <param name="iStartIndex">The index where this name is written into the corresponding DNS frame</param>
        /// <returns>The compressed DNS name converted to bytes</returns>
        public static byte[] CompressDNSName(string strName, Dictionary<string, int> dictCompressionIndices, int iStartIndex)
        {
            if (strName.Length == 0 || strName[0] != '.')
            {
                strName = "." + strName;
            }
            MemoryStream msMemoryStream = new MemoryStream();

            while (strName != "")
            {
                if (dictCompressionIndices.ContainsKey(strName))
                {
                    int iIndex = dictCompressionIndices[strName];
                    msMemoryStream.WriteByte((byte)(((iIndex >> 8) & 0x3F) | 0xC0));
                    msMemoryStream.WriteByte((byte)((iIndex) & 0xFF));
                    strName = "";
                }
                else
                {
                    dictCompressionIndices.Add(strName, iStartIndex);
                    int iIndexof = strName.IndexOf('.', 1);
                    string strValue;
                    if (iIndexof > 0)
                    {
                        strValue = strName.Substring(0, iIndexof);
                        strName = strName.Substring(iIndexof);
                    }
                    else
                    {
                        strValue = strName;
                        strName = "";
                    }
                    msMemoryStream.WriteByte((byte)(strValue.Length - 1));
                    msMemoryStream.Write(ASCIIEncoding.ASCII.GetBytes(strValue.Substring(1)), 0, strValue.Length - 1);
                    iStartIndex += strValue.Length;
                    //if (strName == "")
                    //{
                    //    msMemoryStream.WriteByte(0);
                    //    iStartIndex++;
                    //}
                }
            }

            return msMemoryStream.ToArray();
        }
        /// <summary>
        /// Compresses an array of strings to a DNS compressed names
        /// </summary>
        /// <param name="strNames">The names to compress</param>
        /// <param name="dictCompressionIndices">A dictionary containing strings and their corresponding indexes from a DNS frame. If this is the first call to this function for a specific DNS frame, an empty instance of
        /// Dictionar&lt;string, int&gt; should be passed, which can be used in further calls of this method</param>
        /// <param name="iDataStartIndex">The index where the names are written into the corresponding DNS frame</param>
        /// <returns>The compressed DNS names converted to bytes</returns>
        public static byte[] CompressDNSNames(string[] strNames, Dictionary<string, int> dictCompressionIndices, int iDataStartIndex)
        {
            MemoryStream msMemoryStream = new MemoryStream();
            byte[] bData;

            foreach (string strName in strNames)
            {
                bData = CompressDNSName(strName, dictCompressionIndices, iDataStartIndex);
                iDataStartIndex += bData.Length;
                msMemoryStream.Write(bData, 0, bData.Length);
            }

            return msMemoryStream.ToArray();
        }
    }
}
