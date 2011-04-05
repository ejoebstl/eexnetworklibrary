using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.Utilities;

namespace eExNetworkLibrary.Sockets
{
    /// <summary>
    /// This class represents a network stream which is capable of
    /// writing and reading data to and from a buffer. 
    /// This class is supposed to be used to connect network stream modifiers. 
    /// </summary>
    public class BufferNetworkStream : NetworkStream
    {
        public RingBuffer rfBuffer;
        volatile bool bIsPush;
        public Queue<long> qPushIndex;
        long iNextPush;
        long iWriteCount;
        long iReadCount;
        object oPushSync;

        public BufferNetworkStream() : this(65535) { }

        public BufferNetworkStream(int iInitialBufferSize)
        {
            this.rfBuffer = new RingBuffer(iInitialBufferSize);
            this.bIsPush = false;
            this.iWriteCount = 0;
            this.iReadCount = 0;
            this.iNextPush = -1;

        }

        /// <summary>
        /// A bool indicating whether the last bytes which were read last were written with a push flag set.
        /// </summary>
        public override bool IsPush
        {
            get { return bIsPush; }
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
            qPushIndex.Enqueue(iWriteCount);
            if (qPushIndex.Count == 1)
            {
                iNextPush = iWriteCount;
            }
        }

        public override long Length
        {
            get { return rfBuffer.Count; }
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

        public override int Read(byte[] buffer, int offset, int count)
        {
            int iValue = rfBuffer.Read(buffer, offset, count);
            lock (oPushSync)
            {
                bIsPush = false;

                while (iNextPush != -1 && IsPushInRange(iReadCount, (iReadCount + count) % rfBuffer.Length, iNextPush))
                {
                    bIsPush = true;
                    qPushIndex.Dequeue();
                    if (qPushIndex.Count > 0)
                    {
                        iNextPush = qPushIndex.Peek();
                    }
                    else
                    {
                        iNextPush = -1;
                    }
                }

                iReadCount += count;
                iReadCount %= rfBuffer.Length;
            }
            return iValue;
        }

        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        private bool IsPushInRange(long iReadStart, long iReadEnd, long iNextPush)
        {
            return ((iReadEnd < iReadStart && ((iNextPush >= iReadStart && iNextPush <= rfBuffer.Length) || (iNextPush >= 0 && iNextPush <= iReadEnd))) ||
                (iReadEnd > iReadStart && iNextPush >= iReadStart && iNextPush <= iReadEnd));
        }

        /// <summary>
        /// Reads a single byte from the stream.
        /// </summary>
        /// <returns>The byte read.</returns>
        public override int ReadByte()
        {
            int iValue = rfBuffer.ReadByte();
            lock (oPushSync)
            {
                bIsPush = false;
                iReadCount++;
                iReadCount %= rfBuffer.Length;
                if (iReadCount == iNextPush)
                {
                    bIsPush = true;
                    qPushIndex.Dequeue();
                    if (qPushIndex.Count > 0)
                    {
                        iNextPush = qPushIndex.Peek();
                    }
                    else
                    {
                        iNextPush = -1;
                    }
                }
            }
            return iValue;
        }
        /// <summary>
        /// Writes the given bytes to the network stream.
        /// </summary>
        /// <param name="buffer">The buffer to write.</param>
        /// <param name="offset">The offset in buffer where writing starts.</param>
        /// <param name="count">The count of bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            Write(buffer, offset, count, false);
        }
        /// <summary>
        /// Writes the given bytes to the network stream.
        /// </summary>
        /// <param name="buffer">The buffer to write.</param>
        /// <param name="offset">The offset in buffer where writing starts.</param>
        /// <param name="count">The count of bytes to write.</param>
        /// <param name="bPush">A bool indicating whether a push flag should be set for the bites written.</param>
        public override void Write(byte[] buffer, int offset, int count, bool bPush)
        {
            lock (oPushSync)
            {
                iWriteCount += count;
                iWriteCount %= rfBuffer.Length;

                if (bPush)
                {
                    Flush();
                }
            }
            rfBuffer.Write(buffer, offset, count);
       
        }
    }
}
