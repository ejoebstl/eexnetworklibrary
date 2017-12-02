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
using System.Threading;
using System.IO;

namespace eExNetworkLibrary.Utilities
{
    /// <summary>
    /// Represents a thread safe buffer, which can be written to and read simultaneously. 
    /// </summary>
    public class RingBuffer : Stream
    {
        byte[] arBuffer;

        int iReaderPosition;
        int iWriterPosition;

        int iCount;
        object oSync;

        bool bOpen;

        AutoResetEvent areDataWritten;
        AutoResetEvent areDataRead;

        /// <summary>
        /// Gets the count of bytes in this buffer
        /// </summary>
        public int Count
        {
            get { return iCount; }
        }

        /// <summary>
        /// Gets the size of this buffer
        /// </summary>
        public int Size
        {
            get { return this.arBuffer.Length; }
        }

        /// <summary>
        /// Gets a bool indicating whether this buffer is closed.
        /// </summary>
        public bool Closed
        {
            get { return !bOpen; }
        }

        /// <summary>
        /// Creates a new instance of this class with the given capacity
        /// </summary>
        /// <param name="iSize">The initial capacity, in bytes, of this ring buffer</param>
        public RingBuffer(int iSize)
        {
            arBuffer = new byte[iSize];
            bOpen = true;
            oSync = new object();
            areDataRead = new AutoResetEvent(true);
            areDataWritten = new AutoResetEvent(true);
        }

        /// <summary>
        /// Reads a byte from the buffer.
        /// This method blocks until data for reading is available.
        /// </summary>
        /// <returns>A value between 0 and 255 and -1 if the buffer was forcibly closed while reading.</returns>
        public int Read()
        {
            while (iCount <= 0)
            {
                if (!bOpen)
                    return -1;
                areDataWritten.WaitOne();
                if (arBuffer == null)
                    return -1;
            }

            byte bValue;

            lock (oSync)
            {
                bValue = arBuffer[iReaderPosition];

                iReaderPosition = (iReaderPosition + 1) % arBuffer.Length;
                iCount--;

            }

            areDataRead.Set();

            return bValue;
        }

        /// <summary>
        /// Writes a byte to the buffer.
        /// This method blocks until space for writing is available
        /// </summary>
        /// <param name="b">The byte to write</param>
        /// <exception cref="InvalidOperationExceptoin">Is thrown if the buffer is full.</exception>
        public void Write(byte b)
        {
            if (!bOpen)
                throw new ObjectDisposedException("RingBuffer");

            while (iCount >= arBuffer.Length)
            {
                Reallocate();
            }

            lock (oSync)
            {

                arBuffer[iWriterPosition] = b;

                iWriterPosition = (iWriterPosition + 1) % arBuffer.Length;
                iCount++;
            }

            areDataWritten.Set();
        }

        private void Reallocate()
        {
            lock (oSync)
            {
                byte[] arOldBuffer = arBuffer;
                arBuffer = new byte[arOldBuffer.Length * 2];

                if (iReaderPosition > iWriterPosition)
                {
                    Array.Copy(arOldBuffer, iReaderPosition, arBuffer, 0, arOldBuffer.Length - iReaderPosition);
                    Array.Copy(arOldBuffer, 0, arBuffer, arOldBuffer.Length - iReaderPosition, iWriterPosition);

                    iReaderPosition = 0;
                    iWriterPosition = iCount;
                }
                else
                {
                    Array.Copy(arOldBuffer, iReaderPosition, arBuffer, 0, iCount);

                    iReaderPosition = 0;
                    iWriterPosition = iCount;
                }
            }
        }

        /// <summary>
        /// Reads a number of bytes from this buffer and stores them in <paramref name="arBuffer"/>.
        /// This method blocks until all the bytes to read are available. 
        /// </summary>
        /// <param name="arBuffer">The array which is filled with the data read from the buffer.</param>
        /// <param name="iOffset">The offset in <paramref name="arBuffer"/> at which to begin</param>
        /// <param name="iCount">The count of bytes to read</param>
        /// <returns>The number of bytes written into <paramref name="arBuffer"/></returns>
        public override int Read(byte[] arBuffer, int iOffset, int iCount)
        {
            if (iOffset + iCount > arBuffer.Length)
            {
                throw new ArgumentException("With the specified offset and count values, the operation would have resulted in an overflow.");
            }

            while (iCount > this.iCount)
            {
                if (!bOpen)
                    return 0;

                areDataWritten.WaitOne();
                if (!bOpen)
                    return 0;
            }

            lock (oSync)
            {

                for (int iC1 = 0; iC1 < iCount; iC1++)
                {
                    arBuffer[iC1 + iOffset] = this.arBuffer[iReaderPosition];
                    iReaderPosition = (iReaderPosition + 1) % this.arBuffer.Length;
                }

                this.iCount -= iCount;

            }

            areDataRead.Set();

            return iCount;
        }

        /// <summary>
        /// Writes a number of bytes into this buffer.
        /// This method blocks until enough free space to write all the bytes is available. 
        /// </summary>
        /// <param name="arBuffer">The data to write</param>
        /// <param name="iStartIndex">The index at where writing should begin</param>
        /// <param name="iCount">The number of bytes to write</param>
        public override void Write(byte[] arBuffer, int iStartIndex, int iCount)
        {
            if (!bOpen)
                throw new ObjectDisposedException("RingBuffer");

            if (iStartIndex + iCount > arBuffer.Length)
            {
                throw new ArgumentException("With the specified start index and count values, the operation would have resulted in an overflow.");
            }

            while (iCount > (this.arBuffer.Length - this.iCount))
            {
                Reallocate();
            }

            lock (oSync)
            {
                for (int iC1 = 0; iC1 < iCount; iC1++)
                {
                    this.arBuffer[iWriterPosition] = arBuffer[iC1 + iStartIndex];
                    iWriterPosition = (iWriterPosition + 1) % this.arBuffer.Length;
                }

                this.iCount += iCount;
            }

            areDataWritten.Set();
        }

        /// <summary>
        /// Closes this buffer and frees all associated resources.
        /// </summary>
        public override void Close()
        {
            lock (oSync)
            {
                if (bOpen)
                {
                    bOpen = false;
                    areDataWritten.Set();
                    areDataRead.Set();

                    areDataRead.Close();
                    areDataWritten.Close();
                }
            }
        }

        /// <summary>
        /// Destroys and closes this ring buffer
        /// </summary>
        ~RingBuffer()
        {
            Close();
            arBuffer = null;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Length
        {
            get {
                if (arBuffer == null)
                    throw new ObjectDisposedException("RingBuffer");
                
                return arBuffer.Length; 
            }
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }
    }
}
