using System;

namespace eExNetworkLibrary.Sockets
{
    /// <summary>
    /// This class represents the base for a network stream which is capable of saving 
    /// push flags for bytes which are written. 
    /// </summary>
    public abstract class NetworkStream : System.IO.Stream
    {
        /// <summary>
        /// A bool indicating whether the last bytes which were read last were written with a push flag set.
        /// </summary>
        public abstract bool IsPush { get; }

        /// <summary>
        /// Writes the given bytes to the network stream.
        /// </summary>
        /// <param name="buffer">The buffer to write.</param>
        /// <param name="offset">The offset in buffer where writing starts.</param>
        /// <param name="count">The count of bytes to write.</param>
        /// <param name="bPush">A bool indicating whether a push flag should be set for the bites written.</param>
        public abstract void Write(byte[] buffer, int offset, int count, bool bPush);
    }
}
