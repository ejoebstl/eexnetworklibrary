using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.Utilities;

namespace eExNetworkLibrary.Sockets
{
    /// <summary>
    /// This class represents a network stream which handles communications
    /// with a socket.
    /// </summary>
    public class SocketNetworkStream : eExNetworkLibrary.Sockets.NetworkStream
    {
        public ISocket sSocket;
        public RingBuffer rfBuffer;
        volatile bool bIsPush;
        public Queue<long> qPushIndex;
        long iNextPush;
        long iWriteCount;
        long iReadCount;
        object oPushSync;

        public SocketNetworkStream(ISocket sSocket) : this(sSocket, 65535) { }

        public SocketNetworkStream(ISocket sSocket, int iBufferLength)
        {
            if (iBufferLength < 1)
                throw new ArgumentException("Initial buffer length cannot be zero or less.");
            this.sSocket = sSocket;
            this.rfBuffer = new RingBuffer(iBufferLength);
            this.bIsPush = false;
            this.iWriteCount = 0;
            this.iReadCount = 0;
            this.iNextPush = -1;
            this.oPushSync = new object();
            this.qPushIndex = new Queue<long>();

            sSocket.FrameDecapsulated += new FrameProcessedEventHandler(sSocket_FrameDecapsulated);
        }

        void sSocket_FrameDecapsulated(object sender, FrameProcessedEventArgs args)
        {
            byte[] bData = args.ProcessedFrame.FrameBytes; 
            
            lock (oPushSync)
            {
                iWriteCount += bData.Length;
                iWriteCount %= rfBuffer.Length;

                if (args.IsPush)
                {
                    qPushIndex.Enqueue(iWriteCount);
                    if (qPushIndex.Count == 1)
                    {
                        iNextPush = iWriteCount;
                    }
                }
            }

            rfBuffer.Write(bData, 0, bData.Length);
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
            if (!sSocket.IsOpen)
                throw new ObjectDisposedException("Socket [" + sSocket.BindingInformation.ToString() + "]");
            sSocket.Flush();
        }

        public override long Length
        {
            get { return rfBuffer.Count; }
        }

        public override long Position
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// A bool indicating whether the last bytes which were read last were written with a push flag set.
        /// </summary>
        public override bool IsPush
        {
            get { return bIsPush; }
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

        private bool IsPushInRange(long iReadStart, long iReadEnd, long iNextPush)
        {
            return ((iReadEnd < iReadStart && ((iNextPush >= iReadStart && iNextPush <= rfBuffer.Length) || (iNextPush >= 0 && iNextPush <= iReadEnd))) ||
                (iReadEnd > iReadStart && iNextPush >= iReadStart && iNextPush <= iReadEnd));
        }

        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Writes the given bytes to the underlying socket.
        /// </summary>
        /// <param name="buffer">The buffer to write.</param>
        /// <param name="offset">The offset in buffer where writing starts.</param>
        /// <param name="count">The count of bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            Write(buffer, offset, count, false);
        }

        /// <summary>
        /// Writes the given bytes to the underlying socket.
        /// </summary>
        /// <param name="buffer">The buffer to write.</param>
        /// <param name="offset">The offset in buffer where writing starts.</param>
        /// <param name="count">The count of bytes to write.</param>
        /// <param name="bPush">A bool indicating whether a push flag should be set for the bites written.</param>
        public override void Write(byte[] buffer, int offset, int count, bool bPush)
        {
            if (!sSocket.IsOpen)
                throw new ObjectDisposedException("Socket [" + sSocket.BindingInformation.ToString() + "]");

            byte[] bData = new byte[count];
            for (int iC1 = 0; iC1 < count; iC1++)
            {
                bData[iC1] = buffer[iC1 + offset];
            }

            sSocket.PushDown(bData, bPush);
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

        public override void Close()
        {
            sSocket.FrameDecapsulated -= new FrameProcessedEventHandler(sSocket_FrameDecapsulated);
            rfBuffer.Close();
            base.Close();
        }
    }
}
