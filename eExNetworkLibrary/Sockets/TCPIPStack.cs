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
using eExNetworkLibrary.TCP;
using eExNetworkLibrary.IP;
using eExNetworkLibrary.IP.V6;

namespace eExNetworkLibrary.Sockets
{
    [Obsolete("This class is marked as an experimental preview and not fully functional at the moment", false)]
    public class TCPIPStack : SocketBase
    {
        TCPSocket tcpSocket;
        IPSocket ipSocket;

        private bool bClosing;
        private object oCloseLock;

        public override eExNetworkLibrary.ProtocolParsing.ProtocolParser ProtocolParser
        {
            get
            {
                return base.ProtocolParser;
            }
            set
            {
                base.ProtocolParser = value;
                tcpSocket.ProtocolParser = value;
                ipSocket.ProtocolParser = value;
            }
        }

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
            TCPFrame tcpFrame = (TCPFrame)ProtocolParser.GetFrameByType(fFrame, TCPFrame.DefaultFrameType);
            IP.IPFrame ipFrame = (IPFrame)ProtocolParser.GetFrameByType(fFrame, IPv4Frame.DefaultFrameType);
            if (ipFrame == null)
            {
                ipFrame = (IPFrame)ProtocolParser.GetFrameByType(fFrame, IPv6Frame.DefaultFrameType);
            }

            if (ipFrame == null || tcpFrame == null)
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

        public void BeginClose()
        {
            lock (oCloseLock)
            {
                if (bClosing)
                {
                    throw new InvalidOperationException("A close is already in progress");
                }
                bClosing = true;
                tcpSocket.StateChange += new EventHandler<TCPSocketEventArgs>(tcpSocket_StateChange);
                tcpSocket.BeginClose();
            }
        }

        public void EndClose()
        {
            tcpSocket.EndClose();
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

        public void ConnectAsync()
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
