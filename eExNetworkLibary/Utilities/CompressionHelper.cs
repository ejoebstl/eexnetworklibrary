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
using System.IO.Compression;
using System.IO;

namespace eExNetworkLibrary.Utilities
{
    public static class CompressionHelper
    {
        public static byte[] DecompressDeflate(byte[] bData)
        {
            DeflateStream d = new DeflateStream(new MemoryStream(bData), CompressionMode.Decompress);

            return ReadStream(d);
        }

        public static byte[] DecompressGZip(byte[] bData)
        {
            GZipStream d = new GZipStream(new MemoryStream(bData), CompressionMode.Decompress);

            return ReadStream(d);
        }

        public static byte[] CompressDeflate(byte[] bData)
        {
            MemoryStream msOut = new MemoryStream();
            DeflateStream d = new DeflateStream(msOut, CompressionMode.Compress);
            d.Write(bData, 0, bData.Length);

            return msOut.ToArray();
        }

        public static byte[] CompressGZip(byte[] bData)
        {
            MemoryStream msOut = new MemoryStream();
            GZipStream d = new GZipStream(msOut, CompressionMode.Compress);
            d.Write(bData, 0, bData.Length);

            return msOut.ToArray();
        }

        public static byte[] DecompressChunked(byte[] bData)
        {
            MemoryStream msOut = new MemoryStream();
            int iChunkLength = 0;
            int iDataCounter = 0;
            string strLength;

            do
            {
                //Get the chunk length
                for (int iC1 = iDataCounter; iC1 < bData.Length - 1; iC1++)
                {
                    if (bData[iC1] == '\r' && bData[iC1 + 1] == '\n')
                    {
                        strLength = ASCIIEncoding.ASCII.GetString(bData, iDataCounter, iC1 - iDataCounter);
                        iChunkLength = Int32.Parse(strLength, System.Globalization.NumberStyles.HexNumber);
                        iDataCounter += (iC1 - iDataCounter) + 2;
                        break;
                    }
                }
                //Copy the data
                msOut.Write(bData, iDataCounter, iChunkLength);
                iDataCounter += iChunkLength + 2;

            } while (iChunkLength != 0 && iDataCounter < bData.Length);

            return msOut.ToArray();
        }

        private static byte[] ReadStream(Stream s)
        {
            MemoryStream msOutput = new MemoryStream();
            int iVar;

            while (true)
            {
                iVar = s.ReadByte();
                if (iVar == -1)
                {
                    break;
                }
                msOutput.WriteByte((byte)iVar);
            }

            return msOutput.ToArray();
        }

        /// <summary>
        /// Copies all bytes from SourceStream to DestinationStream, until the data sequence to search is found, including the data sequence to search.
        /// </summary>
        /// <param name="sSourceStream">The stream to copy from</param>
        /// <param name="sDestinationStream">The stream to copy to</param>
        /// <param name="bDataToSearch">The data to search for</param>
        /// <returns>A bool indicating wheter the searched data was found before the end of the stream was reached. If the end of the stream was reached first, this method returns false.</returns>
        public static bool CopyUntilFound(Stream sSourceStream, Stream sDestinationStream, byte[] bDataToSearch)
        {
            int iHitCount = 0;
            int iData;
            do
            {
                iData = sSourceStream.ReadByte();
                if (iData != -1)
                {
                    sDestinationStream.WriteByte((byte)iData);

                    if (iData == bDataToSearch[iHitCount])
                    {
                        iHitCount++;
                    }
                    else
                    {
                        iHitCount = 0;
                    }
                }
            } while (iData != -1 && iHitCount != bDataToSearch.Length);

            return iData != -1;
        }
    }
}
