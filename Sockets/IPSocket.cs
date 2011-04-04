using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using eExNetworkLibrary.IP;
using System.IO;

namespace eExNetworkLibrary.Sockets
{
    /// <summary>
    /// This class represents a network library IP socket implementation.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{BindingInformation.ToString()}")]
    public class IPSocket : SocketBase, IPseudoHeaderSource
    {
        private Dictionary<uint, MemoryStream> dictIDFragmentBuffer;

        public override bool IsOpen
        {
            get { return true; }
        }

        /// <summary>
        /// Gets or sets the MTU for this socket. 
        /// </summary>
        public int MaximumTransmissionUnit { get; set; }

        /// <summary>
        /// Gets the local IPAddress to which this socket is bound.
        /// </summary>
        public IPAddress LocalBinding { get; private set; }
        
        /// <summary>
        /// Gets the remote IPAddress to which this socket is bound.
        /// </summary>
        public IPAddress RemoteBinding { get; private set; }

        /// <summary>
        /// Gets the protocl this socket belongs to
        /// </summary>
        public IPProtocol ProtocolBinding { get; private set; }

        /// <summary>
        /// Returns the IP pseudo header for the given frame.
        /// </summary>
        /// <param name="fFrame">The frame to calculate the pseudo-header for.</param>
        /// <returns>The pseudo header of the given frame.</returns>
        public byte[] GetPseudoHeader(Frame fFrame)
        {
            IPv4Frame ipv4Frame = new IPv4Frame();

            ipv4Frame.DestinationAddress = RemoteBinding;
            ipv4Frame.SourceAddress = LocalBinding;

            ipv4Frame.Protocol = ProtocolBinding;

            ipv4Frame.EncapsulatedFrame = fFrame;

            return ipv4Frame.GetPseudoHeader();

        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="ipaRemoteBinding">The remote address to bind this socket to</param>
        /// <param name="ipaLocalBinding">The local address to bind this socket to</param>
        /// <param name="ipPotocol">The protocl this socket belongs to</param>
        public IPSocket(IPAddress ipaRemoteBinding, IPAddress ipaLocalBinding, IPProtocol ipPotocol)
        {
            if (ipaRemoteBinding.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork || ipaLocalBinding.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
            {
                throw new ArgumentException("Only IPv4 addresses are supportet on the moment.");
            }
            LocalBinding = ipaLocalBinding;
            ProtocolBinding = ipPotocol;
            RemoteBinding = ipaRemoteBinding;
            MaximumTransmissionUnit = 1500;
            dictIDFragmentBuffer = new Dictionary<uint, MemoryStream>();
        }

        /// <summary>
        /// Decapsulates the given IP frame if the binding of this socket matches the frame and invokes the FrameDecapsulated event when finished.
        /// <remarks>This mehtod also handles IP fragmentation</remarks>
        /// </summary>
        /// <param name="fFrame">The frame to process</param>
        /// <param name="bPush">A bool indicating whether the frame is delivered with a push flag</param>
        /// <returns>A bool indicating whether the given frame matched the binding of this socket</returns>
        public override bool PushUp(Frame fFrame, bool bPush)
        {
            if (fFrame.FrameType != FrameType.IP)
            {
                fFrame = new IPv4Frame(fFrame.FrameBytes);
            }

            IPv4Frame ipv4Frame = (IPv4Frame)fFrame;

            if (!ipv4Frame.Protocol.Equals(ProtocolBinding) 
                || !ipv4Frame.SourceAddress.Equals(RemoteBinding) 
                || !ipv4Frame.DestinationAddress.Equals(LocalBinding))
            {
                return false;
            }


            if (ipv4Frame.PacketFlags.MoreFragments)
            {
                // Fragmentation handling
                if (!dictIDFragmentBuffer.ContainsKey(ipv4Frame.Identification))
                {
                    dictIDFragmentBuffer.Add(ipv4Frame.Identification, new MemoryStream());
                }
            }

            if (dictIDFragmentBuffer.ContainsKey(ipv4Frame.Identification))
            {
                // Fragmentation handling
                dictIDFragmentBuffer[ipv4Frame.Identification].Seek(ipv4Frame.FragmentOffset * 8, SeekOrigin.Begin);
                dictIDFragmentBuffer[ipv4Frame.Identification].Write(ipv4Frame.EncapsulatedFrame.FrameBytes, 0, ipv4Frame.EncapsulatedFrame.Length);

                if (!ipv4Frame.PacketFlags.MoreFragments)
                {
                    byte[] bData = dictIDFragmentBuffer[ipv4Frame.Identification].ToArray();
                    dictIDFragmentBuffer.Remove(ipv4Frame.Identification);

                    InvokeFrameDecapsulated(new RawDataFrame(bData));
                }
            }
            else
            {
                InvokeFrameDecapsulated(ipv4Frame.EncapsulatedFrame, bPush);
            }

            return true;
        }

        /// <summary>
        /// Encapsulates the given IP frame according to the binding of this socket and invokes the FrameEncapsulated event when finished.
        /// <remarks>This method also handles IP fragmentation</remarks>
        /// </summary>
        /// <param name="fFrame">The frame to process</param>
        /// <param name="bPush">A bool indicating whether the frame is delivered with a push flag</param>
        public override void PushDown(Frame fFrame, bool bPush)
        {
            IPv4Frame ipv4Frame = new IPv4Frame();

            ipv4Frame.DestinationAddress = RemoteBinding;
            ipv4Frame.SourceAddress = LocalBinding;

            ipv4Frame.Protocol = ProtocolBinding;


            if (fFrame.Length + (ipv4Frame.InternetHeaderLength * 4) > MaximumTransmissionUnit)
            {
                byte[] bBuffer = fFrame.FrameBytes;

                int iChunkSize = MaximumTransmissionUnit - (ipv4Frame.InternetHeaderLength * 4);
                iChunkSize -= iChunkSize % 8;

                for (int iC1 = 0; iC1 < bBuffer.Length; iC1 += iChunkSize)
                {
                    IPv4Frame ipv4Clone = (IPv4Frame)ipv4Frame.Clone();

                    byte[] bChunk = new byte[Math.Min(iC1 + iChunkSize, bBuffer.Length) - iC1];

                    for (int iC2 = iC1; iC2 < iC1 + iChunkSize && iC2 < bBuffer.Length; iC1++ )
                    {
                        bChunk[iC2 - iC1] = bBuffer[iC2];
                    }

                    if (iC1 + iChunkSize < bBuffer.Length)
                    {
                        ipv4Clone.PacketFlags.MoreFragments = true;
                    }

                    ipv4Clone.FragmentOffset = (ushort)(iC1 / 8);

                    ipv4Clone.EncapsulatedFrame = new RawDataFrame(bChunk);

                    InvokeFrameEncapsulated(ipv4Frame, bPush);
                }
            }
            else
            {
                ipv4Frame.EncapsulatedFrame = fFrame;
                InvokeFrameEncapsulated(ipv4Frame, bPush);
            }
        }

        /// <summary>
        /// Returns the BindingInformation of this socket as IPBindingInformation
        /// </summary>
        public override BindingInformation BindingInformation
        {
            get { return new IPBindingInformation(new IPEndPoint(LocalBinding), new IPEndPoint(RemoteBinding), ProtocolBinding); }
        }

        public override void Close()
        {
            dictIDFragmentBuffer.Clear();
            base.Close();
        }

        public override void Dispose()
        {
            dictIDFragmentBuffer.Clear();
        }

        public override void Flush()
        {
            //Nothing to do
        }
    }

    /// <summary>
    /// This class represents IP socket binding information
    /// </summary>
    public class IPBindingInformation : BindingInformation
    {       
        /// <summary>
        /// Gets the protocl the socket belongs to
        /// </summary>
        /// 
        public IPProtocol ProtocolBinding { get; private set; }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="localBinding">The local binding information</param>
        /// <param name="remoteBinding">The remote binding Information</param>
        /// <param name="protocolBinding">The protocol binding Information</param>
        public IPBindingInformation(IPEndPoint localBinding, IPEndPoint remoteBinding, IPProtocol protocolBinding)
            : base(localBinding, remoteBinding)
        {
            ProtocolBinding = protocolBinding;
        }

        /// <summary>
        /// Gets the description of this EndPoint
        /// </summary>
        /// <returns>The description of this EndPoint</returns>
        public override string ToString()
        {
            return base.ToString() + ", " + ProtocolBinding.ToString() ;
        }
    }

    /// <summary>
    /// This class represents an IP endpoint
    /// </summary>
    public class IPEndPoint : EndPoint
    {
        /// <summary>
        /// Creates a new IP endpoint
        /// </summary>
        /// <param name="ipaBinding">The address this IP endpoint belongs to</param>
        public IPEndPoint(IPAddress ipaBinding) :
            base(ipaBinding.ToString())
        {
            Address = ipaBinding;
        }

        /// <summary>
        /// Returns the address of the IP endpoint
        /// </summary>
        public IPAddress Address { get; private set; }
    }

    /// <summary>
    /// Represents an interface for classes which support pseudo-header generation for checksum calculation.
    /// </summary>
    public interface IPseudoHeaderSource
    {
        /// <summary>
        /// Has to return a pseudo-header to calculate the TCP checksum from for the given frame.
        /// </summary>
        /// <param name="fFrame">The frame to calculate the pseudo-header for.</param>
        /// <returns>The pseudo-header for the given frame.</returns>
        byte[] GetPseudoHeader(Frame fFrame);
    }
}
