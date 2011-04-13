using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using eExNetworkLibrary.TCP;
using eExNetworkLibrary.IP;

namespace eExNetworkLibrary.Sockets
{
    [Obsolete("This class is marked as an experimental preview and not fully functional at the moment", false)]
    public class TCPIPStack : SocketBase
    {
        TCPSocket tcpSocket;
        IPSocket ipSocket;

        private bool bClosing;
        private object oCloseLock;

        public override bool IsOpen
        {
            get { return tcpSocket.IsOpen && ipSocket.IsOpen; }
        }

        public TCPSocket TCPSocket
        {
            get { return tcpSocket; }
        }

        public IPSocket IPSocket
        {
            get { return ipSocket; }
        }

        public TCPIPEndPoint LocalBinding
        {
            get { return new TCPIPEndPoint(ipSocket.LocalBinding, tcpSocket.LocalBinding); }
        }

        public TCPIPEndPoint RemoteBinding
        {
            get { return new TCPIPEndPoint(ipSocket.RemoteBinding, tcpSocket.RemoteBinding); }
        }

        public TCPIPStack(IPAddress ipaLocalAddress, IPAddress ipaRemoteAddress, int iLocalPort, int iRemotePort)
        {
            ipSocket = new IPSocket(ipaRemoteAddress, ipaLocalAddress, eExNetworkLibrary.IP.IPProtocol.TCP);
            tcpSocket = new TCPSocket(iRemotePort, iLocalPort, ipSocket);

            bClosing = false;
            oCloseLock = new object();

            tcpSocket.ChildSocket = ipSocket;
            ipSocket.ParentSocket = tcpSocket;
            tcpSocket.FrameDecapsulated += new FrameProcessedEventHandler(tcpSocket_FrameDecapsulated);
            ipSocket.FrameEncapsulated += new FrameProcessedEventHandler(ipSocket_FrameEncapsulated);
        }

        public void Listen()
        {
            tcpSocket.Listen();
        }

        public void Connect()
        {
            tcpSocket.Connect();
        }

        void ipSocket_FrameEncapsulated(object sender, FrameProcessedEventArgs args)
        {
            InvokeFrameEncapsulated(args.ProcessedFrame, args.IsPush);
        }

        void tcpSocket_FrameDecapsulated(object sender, FrameProcessedEventArgs args)
        {
            InvokeFrameDecapsulated(args.ProcessedFrame, args.IsPush);
        }
    
        public override bool PushUp(Frame fFrame, bool bPush)
        {
            TCPFrame tcpFrame = null;
            IP.IPFrame ipFrame = null;

            Frame fEncapsulatedFrame = fFrame;

            do
            {
                if (fEncapsulatedFrame.FrameType == FrameTypes.TCP)
                    tcpFrame = (TCPFrame)fEncapsulatedFrame;
                if (FrameTypes.IsIP(fEncapsulatedFrame))
                    ipFrame = (IPFrame)fEncapsulatedFrame;
                fEncapsulatedFrame = fEncapsulatedFrame.EncapsulatedFrame;
            } while (fEncapsulatedFrame != null);

            if (ipFrame == null)
            {
                return false;
            }

            if (tcpFrame == null && ipFrame.Protocol == IPProtocol.TCP && ipFrame.EncapsulatedFrame != null)
            {
                tcpFrame = new TCPFrame(ipFrame.EncapsulatedFrame.FrameBytes);
                ipFrame.EncapsulatedFrame = tcpFrame;
            }

            if (tcpFrame == null)
            {
                return false;
            }

            if (ipFrame.SourceAddress.Equals(ipSocket.RemoteBinding) && ipFrame.DestinationAddress.Equals(ipSocket.LocalBinding) &&
                tcpFrame.SourcePort == tcpSocket.RemoteBinding && tcpFrame.DestinationPort == tcpSocket.LocalBinding)
            {
                return ipSocket.PushUp(ipFrame, bPush);
            }
            else
            {
                return false;
            }
        }

        public override void PushDown(Frame fFrame, bool bPush)
        {
            tcpSocket.PushDown(fFrame, bPush);
        }

        public override void PushDown(byte[] bData, bool bPush)
        {
            tcpSocket.PushDown(bData);
        }

        public override BindingInformation BindingInformation
        {
	        get { return new BindingInformation(this.LocalBinding, this.RemoteBinding); }
        }

        public override void Close()
        {
            lock (oCloseLock)
            {
                if (bClosing)
                {
                    throw new InvalidOperationException("A close is already in progress");
                }
                bClosing = true;

                tcpSocket.Close();
                ipSocket.Close();
                base.Close();
                tcpSocket.FrameDecapsulated -= new FrameProcessedEventHandler(tcpSocket_FrameDecapsulated);
                ipSocket.FrameEncapsulated -= new FrameProcessedEventHandler(ipSocket_FrameEncapsulated);
            }
        }

        public void CloseAsync()
        {
            lock (oCloseLock)
            {
                if (bClosing)
                {
                    throw new InvalidOperationException("A close is already in progress");
                }
                bClosing = true;
                tcpSocket.StateChange += new EventHandler<TCPSocketEventArgs>(tcpSocket_StateChange);
                tcpSocket.CloseAsync();
            }
        }

        void tcpSocket_StateChange(object sender, TCPSocketEventArgs e)
        {
            if (e.Sender.TCPState == TCPSocketState.Closed)
            {
                //When closed, detach events (in case of async close).
                tcpSocket.FrameDecapsulated -= new FrameProcessedEventHandler(tcpSocket_FrameDecapsulated);
                ipSocket.FrameEncapsulated -= new FrameProcessedEventHandler(ipSocket_FrameEncapsulated);
                e.Sender.StateChange -= new EventHandler<TCPSocketEventArgs>(tcpSocket_StateChange);

                //Then remove the IP socket and close the base socket
                ipSocket.Close();
                base.Close();
            }
        }

        internal void ConnectAsync()
        {
            tcpSocket.ConnectAsync();
        }

        public override void Dispose()
        {
            TCPSocket.Dispose();
            IPSocket.Dispose();
        }
        public override void Flush()
        {
            TCPSocket.Flush();
            IPSocket.Flush();
        }
    }

    public class TCPIPBindingInformation : BindingInformation
    {
        public TCPIPBindingInformation(TCPIPEndPoint epLocal, TCPIPEndPoint epRemote)
            : base(epLocal, epRemote)
        { }
    }

    public class TCPIPEndPoint : EndPoint
    {
        private IPAddress ipaAddress;
        private int iPort;

        public int Port
        {
            get { return iPort; }
        }

        public IPAddress Address
        {
            get { return ipaAddress; }
        }

        public TCPIPEndPoint(IPAddress ipaAddress, int iPort)
            : base(ipaAddress.ToString() + ":" + iPort)
        {
            this.ipaAddress = ipaAddress;
            this.iPort = iPort;
        }
    }
}
