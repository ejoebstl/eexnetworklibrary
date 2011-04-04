using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using eExNetworkLibrary.UDP;

namespace eExNetworkLibrary.Sockets
{
    /// <summary>
    /// This class represents an UDP socket
    /// </summary>
    public class UDPSocket : SocketBase
    {
        /// <summary>
        /// Gets the local port to which this socket is bound
        /// </summary>
        public int LocalBinding { get; private set; }
        /// <summary>
        /// Gets the remote port to which this socket is bound
        /// </summary>
        public int RemoteBinding { get; private set; }

        public override bool IsOpen
        {
            get { return true; }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="iSourcePort">The source (remote) port to bind this socket to</param>
        /// <param name="iDestinationPort">The destination (local) port to bind this socket to</param>
        public UDPSocket(int iSourcePort, int iDestinationPort)
        {
            RemoteBinding = iSourcePort;
            LocalBinding = iDestinationPort;
        }

        /// <summary>
        /// Decapsulates the given UDP frame if the binding of this socket matches the frame and invokes the FrameDecapsulated event when finished.
        /// </summary>
        /// <param name="fFrame">The frame to process</param>
        /// <param name="bPush">A bool indicating whether the frame is delivered with a push flag</param>
        /// <returns>A bool indicating whether the given frame matched the binding of this socket</returns>
        public override bool PushUp(Frame fFrame, bool bPush)
        {
            if (fFrame.FrameType != FrameType.UDP)
            {
                try
                {
                    fFrame = new UDPFrame(fFrame.FrameBytes);
                }
                catch
                {
                    return false;
                }
            }

            UDPFrame fUDPFrame = (UDPFrame)fFrame;

            if (fUDPFrame.SourcePort != this.RemoteBinding || fUDPFrame.DestinationPort != this.LocalBinding)
            {
                return false;
            }

            InvokeFrameDecapsulated(fUDPFrame.EncapsulatedFrame, bPush);

            return true;
        }

        /// <summary>
        /// Encapsulates the given UDP frame according to the binding of this socket and invokes the FrameEncapsulated event when finished.
        /// </summary>
        /// <param name="fFrame">The frame to process</param>
        /// <param name="bPush">A bool indicating whether the frame is delivered with a push flag</param>
        public override void PushDown(Frame fFrame, bool bPush)
        {
            UDPFrame fUDPFrame = new UDPFrame();
            fUDPFrame.DestinationPort = RemoteBinding;
            fUDPFrame.SourcePort = LocalBinding;

            fUDPFrame.EncapsulatedFrame = fFrame;

            InvokeFrameEncapsulated(fUDPFrame, bPush);
        }

        /// <summary>
        /// Returns the BindingInformation of this socket as UDPBindingInformation
        /// </summary>
        public override BindingInformation BindingInformation
        {
            get { return new UDPBindingInformation(new UDPEndPoint(LocalBinding), new UDPEndPoint(RemoteBinding)); }
        }

        public override void Dispose()
        {
            //Nothing to do.
        }

        public override void Flush()
        {
            //Nothing to do.
        }
    }

    public class UDPBindingInformation : BindingInformation
    {

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="localBinding">The local binding information</param>
        /// <param name="remoteBinding">The remote binding Information</param>
        public UDPBindingInformation(UDPEndPoint localBinding, UDPEndPoint remoteBinding)
            : base(localBinding, remoteBinding)
        { }

        /// <summary>
        /// Gets the description of this EndPoint
        /// </summary>
        /// <returns>The description of this EndPoint</returns>
        public override string ToString()
        {
            return "(UDP) " + base.ToString();
        }
    }

    public class UDPEndPoint : EndPoint 
    {
        /// <summary>
        /// Creates a new UDP endpoint
        /// </summary>
        /// <param name="iPort">The port this UDP endpoint belongs to</param>
        public UDPEndPoint(int iPort) :
            base(iPort.ToString())
        {
            Port = iPort;
        }

        /// <summary>
        /// Returns the port of the UDP endpoint
        /// </summary>
        public int Port { get; private set; }
    }
}
