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
using System.Net.Sockets;
using System.Net;
using eExNetworkLibrary.IP;

namespace eExNetworkLibrary.Utilities
{
    [Obsolete("This class is not Network Libary 2.0 compliant. Further, monitoring sockets are not supported any more.", true)]
    class PacketMonitor
    {
        private Socket sMySocket;
        private IPAddress ipaListenTo;
        public delegate void PacketCaptured(IPFrame ipfCaptured);
        public event PacketCaptured OnPacketCaptured;
        private byte[] bBuffer;

        public PacketMonitor(IPAddress ipaListenTo)
        {
            this.ipaListenTo = ipaListenTo;
        }

        public void Start()
        {
            bBuffer = new byte[65535];
            sMySocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
            sMySocket.Bind(new IPEndPoint(ipaListenTo, 0));
            byte[] bByte = new byte[1];
            bByte[0] = (byte)1;
            try
            {
                sMySocket.IOControl(IOControlCode.ReceiveAll, BitConverter.GetBytes(1), null);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
            sMySocket.BeginReceive(bBuffer, 0, bBuffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), null); 
        }

        public void Stop()
        {
            if (this.sMySocket != null)
            {
                sMySocket.Close();
            }
        }

        ~PacketMonitor()
        {
            Stop();
        }

        private void OnReceive(IAsyncResult iarResult)
        {
            try
            {
                IPFrame ipfPacket;
                int iRecivedBytes = sMySocket.EndReceive(iarResult);
                byte[] bCapturedFrame = new byte[iRecivedBytes];
                Array.Copy(bBuffer, bCapturedFrame, iRecivedBytes);
                if (OnPacketCaptured != null)
                {
                    ipfPacket = IPFrame.Create(bCapturedFrame);
                    OnPacketCaptured(ipfPacket);
                }
                sMySocket.BeginReceive(bBuffer, 0, bBuffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);
            }
            catch (SocketException)
            {
                Stop();
            }
            catch (ObjectDisposedException)
            {
                
            }
        }


    }
}
