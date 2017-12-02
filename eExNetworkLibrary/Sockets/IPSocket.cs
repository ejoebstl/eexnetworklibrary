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
using System.Net;
using eExNetworkLibrary.IP;
using System.IO;
using eExNetworkLibrary.IP.V6;
using System.Net.Sockets;

namespace eExNetworkLibrary.Sockets
{
    /// <summary>
    /// This class represents a network library IP socket implementation.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{BindingInformation.ToString()}")]
    public class IPSocket : SocketBase, IPseudoHeaderSource
    {
        private Dictionary<uint, MemoryStream> dictIDFragmentBuffer;
        private Random rRandom;

        private int iIPVersion;

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
            IPFrame ipFrame;

            if (iIPVersion == 4)
            {
                ipFrame = new IPv4Frame();
            }
            else if (iIPVersion == 6)
            {
                ipFrame = new IPv6Frame();
            }
            else
            {
                throw new ArgumentException("Only IPv4 or IPv6 addresses are supportet on the moment.");
            }

            ipFrame.DestinationAddress = RemoteBinding;
            ipFrame.SourceAddress = LocalBinding;

            ipFrame.Protocol = ProtocolBinding;

            ipFrame.EncapsulatedFrame = fFrame;

            return ipFrame.GetPseudoHeader();



        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="ipaRemoteBinding">The remote address to bind this socket to</param>
        /// <param name="ipaLocalBinding">The local address to bind this socket to</param>
        /// <param name="ipPotocol">The protocl this socket belongs to</param>
        public IPSocket(IPAddress ipaRemoteBinding, IPAddress ipaLocalBinding, IPProtocol ipPotocol)
        {
            if (ipaRemoteBinding.AddressFamily != ipaLocalBinding.AddressFamily)
            {
                throw new ArgumentException("It is not possible to mix up addresses of different types.");
            }

            if (ipaRemoteBinding.AddressFamily == AddressFamily.InterNetworkV6)
            {
                iIPVersion = 6;
            }
            else if (ipaRemoteBinding.AddressFamily == AddressFamily.InterNetwork)
            {
                iIPVersion = 4;
            }
            else
            {
                throw new ArgumentException("Only IPv4 and IPv6 addresses are supported at the moment.");
            }

            rRandom = new Random();

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
            if (!FrameTypes.IsIP(fFrame))
                fFrame = IPFrame.Create(fFrame.FrameBytes);

            IPFrame ipFrame = (IPFrame)fFrame;

            if (!ipFrame.SourceAddress.Equals(RemoteBinding)
                || !ipFrame.DestinationAddress.Equals(LocalBinding))
            {
                return false;
            }

            if (ipFrame.FrameType == FrameTypes.IPv4)
            {
                IPv4Frame ipv4Frame = (IPv4Frame)ipFrame;

                if (ipv4Frame.Protocol != this.ProtocolBinding)
                {
                    return false;
                }

                if (ipv4Frame.EncapsulatedFrame != null)
                {
                    if (ipv4Frame.PacketFlags.MoreFragments)
                    {
                        HandleFragment(bPush, ipv4Frame.PacketFlags.MoreFragments, ipv4Frame.Identification, ipv4Frame.FragmentOffset, ipv4Frame.EncapsulatedFrame.FrameBytes);
                    }
                    else
                    {
                        InvokeFrameDecapsulated(ipv4Frame.EncapsulatedFrame, bPush);
                    }
                }

                return true;
            }
            else if (ipFrame.FrameType == FrameTypes.IPv6)
            {
                ProtocolParser.ParseCompleteFrame(fFrame);

                Frame fPayload = null;
                Frame ipHeader = fFrame;

                while (ipHeader.EncapsulatedFrame != null && ipHeader.EncapsulatedFrame is IIPHeader)
                {
                    if (((IIPHeader)ipHeader).Protocol == this.ProtocolBinding)
                    {
                        fPayload = ipHeader.EncapsulatedFrame;
                        break;
                    }
                }

                if (fPayload == null)
                    return false; //Wrong payload type :(

                FragmentExtensionHeader fragHeader = (FragmentExtensionHeader)ProtocolParser.GetFrameByType(fFrame, FragmentExtensionHeader.DefaultFrameType);

                if (fragHeader != null)
                {
                    HandleFragment(bPush, fragHeader.MoreFragments, fragHeader.Identification, fragHeader.FragmentOffset, fPayload.FrameBytes);
                }
                else
                {
                    InvokeFrameDecapsulated(fPayload, bPush);
                }
            }

            throw new ArgumentException("Only IPv4 and IPv6 frames are supported at the moment.");

        }

        private void HandleFragment(bool bPush, bool bMoreFragments, uint iIdentification, int iFragmentOffset, byte[] bPayload)
        {
            // Fragmentation handling
            if (!dictIDFragmentBuffer.ContainsKey(iIdentification))
            {
                dictIDFragmentBuffer.Add(iIdentification, new MemoryStream());
            }

            if (dictIDFragmentBuffer.ContainsKey(iIdentification))
            {
                // Fragmentation handling
                dictIDFragmentBuffer[iIdentification].Seek(iFragmentOffset * 8, SeekOrigin.Begin);
                dictIDFragmentBuffer[iIdentification].Write(bPayload, 0, bPayload.Length);

                if (!bMoreFragments)
                {
                    byte[] bData = dictIDFragmentBuffer[iIdentification].ToArray();
                    dictIDFragmentBuffer.Remove(iIdentification);

                    InvokeFrameDecapsulated(new RawDataFrame(bData));
                }

            }
        }

        /// <summary>
        /// Encapsulates the given IP frame according to the binding of this socket and invokes the FrameEncapsulated event when finished.
        /// <remarks>This method also handles IP fragmentation</remarks>
        /// </summary>
        /// <param name="fFrame">The frame to process</param>
        /// <param name="bPush">A bool indicating whether the frame is delivered with a push flag</param>
        public override void PushDown(Frame fFrame, bool bPush)
        {
            if (iIPVersion == 4)
            {
                IPv4Frame ipv4Frame = new IPv4Frame();

                ipv4Frame.DestinationAddress = RemoteBinding;
                ipv4Frame.SourceAddress = LocalBinding;

                ipv4Frame.Protocol = ProtocolBinding;
                ipv4Frame.Identification = (uint)rRandom.Next(Int32.MaxValue);

                ipv4Frame.EncapsulatedFrame = fFrame;

                foreach (IPFrame fFragment in IPFragmenter.FragmentV4(ipv4Frame, MaximumTransmissionUnit))
                {
                    InvokeFrameEncapsulated(fFragment, bPush);
                }
            }
            else if (iIPVersion == 6)
            {
                IPv6Frame ipv6Frame = new IPv6Frame();

                ipv6Frame.DestinationAddress = RemoteBinding;
                ipv6Frame.SourceAddress = LocalBinding;

                ipv6Frame.Protocol = ProtocolBinding;

                ipv6Frame.EncapsulatedFrame = fFrame;

                foreach (IPFrame fFragment in IPFragmenter.FragmentV6(ipv6Frame, MaximumTransmissionUnit))
                {
                    InvokeFrameEncapsulated(fFragment, bPush);
                }
            }
            else
            {
                throw new ArgumentException("Only IPv4 and IPv6 addresses are supportet on the moment.");
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
