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
    public class TCPIPListenerStack : SocketBase
    {  
        TCPListenerSocket tcpSocket;
        IPSocket ipSocket;


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

        public TCPListenerSocket TCPSocket
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

        public TCPIPListenerStack(IPAddress ipaLocalAddress, IPAddress ipaRemoteAddress, int iLocalPort, int iRemotePort)
        {
            ipSocket = new IPSocket(ipaRemoteAddress, ipaLocalAddress, eExNetworkLibrary.IP.IPProtocol.TCP);
            tcpSocket = new TCPListenerSocket(iRemotePort, iLocalPort, ipSocket);

            tcpSocket.ChildSocket = ipSocket;
            ipSocket.ParentSocket = tcpSocket;
            tcpSocket.FrameDecapsulated += new FrameProcessedEventHandler(tcpSocket_FrameDecapsulated);
            ipSocket.FrameEncapsulated += new FrameProcessedEventHandler(ipSocket_FrameEncapsulated);
        }

        public void Listen()
        {
            tcpSocket.SimulateListen();
        }

        public void Connect()
        {
            tcpSocket.SimulateConnect();
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
            throw new InvalidOperationException("Writing data to a TCP/IP listening stack is not possible since it is read only.");
        }

        public override BindingInformation BindingInformation
        {
	        get { return new BindingInformation(this.LocalBinding, this.RemoteBinding); }
        }

        public override void Close()
        {
            tcpSocket.Close();
            ipSocket.Close();
            base.Close();
            tcpSocket.FrameDecapsulated -= new FrameProcessedEventHandler(tcpSocket_FrameDecapsulated);
            ipSocket.FrameEncapsulated -= new FrameProcessedEventHandler(ipSocket_FrameEncapsulated);
        }

        public override void Dispose()
        {
            TCPSocket.Dispose();
            IPSocket.Dispose();
        }

        public override void Flush()
        {
            throw new InvalidOperationException("Writing data to a TCP/IP listening stack is not possible since it is read only.");
        }
    }
}
