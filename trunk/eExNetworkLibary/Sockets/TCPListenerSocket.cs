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
using eExNetworkLibrary.TCP;

namespace eExNetworkLibrary.Sockets
{
    public class TCPListenerSocket : SocketBase
    {
        TCPSocket.TransmissionControlBlock tcb;
        List<TCPFrame> tcpFrameStore;
        TCPSocketState tcpState;
        IPseudoHeaderSource pseudoHeaderSource;
        object oTCBLock;

        public event EventHandler<TCPListenerSocketEventArgs> StateChange;

        public TCPSocketState TCPState
        {
            get { return tcpState; }
            private set 
            { 
                tcpState = value; 
                InvokeExternalAsync(StateChange, new TCPListenerSocketEventArgs(this));
            }
        }

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
            get
            {
                return TCPState == TCPSocketState.CloseWait ||
                TCPState == TCPSocketState.Established ||
                TCPState == TCPSocketState.Listen ||
                TCPState == TCPSocketState.SynReceived ||
                TCPState == TCPSocketState.SynSent;
            }
        }
        
        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="iSourcePort">The source (remote) port to bind this socket to</param>
        /// <param name="iDestinationPort">The destination (local) port to bind this socket to</param>
        /// <param name="bPseudoHeader">The layer 3 pseudeo header to calculate the checksum with</param>
        public TCPListenerSocket(int iSourcePort, int iDestinationPort, IPseudoHeaderSource pseudoHaaderSource)
        {
            oTCBLock = new object(); 
            RemoteBinding = iSourcePort;
            LocalBinding = iDestinationPort;
            this.pseudoHeaderSource = pseudoHaaderSource;
            TCPState = TCPSocketState.Closed;
            tcpFrameStore = new List<TCPFrame>();
            CreateTCB();
        }

        private void CreateTCB()
        {
            tcb = new eExNetworkLibrary.Sockets.TCPSocket.TransmissionControlBlock();
            tcb.SND_NXT = 0;
            tcb.SND_UNA = 0;
            tcb.SND_WND = 0;
            tcb.SND_UP = 0;
            tcb.SND_WL1 = 0;
            tcb.SND_WL2 = 0;
            tcb.ISS = 0;
            tcb.RCV_NXT = 0;
            tcb.RCV_WND = 0;
            tcb.RCV_UP = 0;
            tcb.IRS = 0;
        }

        /// <summary>
        /// Decapsulates the given TCP frame if the binding of this socket matches the frame and invokes the FrameDecapsulated event when finished.
        /// </summary>
        /// <param name="fFrame">The frame to process</param>
        /// <param name="bPush">The TCP Socket ignores this parameter, since TCP push flags can be set in the TCP frame directly.</param>
        /// <returns>A bool indicating whether the given frame is matching the binding of this socket</returns>
        public override bool PushUp(Frame fFrame, bool bPush)
        {
            if (fFrame.FrameType != FrameTypes.TCP)
            {
                fFrame = new TCPFrame(fFrame.FrameBytes);
            }

            TCPFrame tcpFrame = (TCPFrame)fFrame;

            //Is this socket the frame's destination?

            if (tcpFrame.DestinationPort != LocalBinding || tcpFrame.SourcePort != RemoteBinding)
            {
                return false;
            }

            //Check the checksum

            byte[] bMyChecksum = tcpFrame.CalculateChecksum(this.pseudoHeaderSource.GetPseudoHeader(tcpFrame));
            byte[] bReceivedChecksum = tcpFrame.Checksum;

            for (int iC1 = 0; iC1 < bMyChecksum.Length; iC1++)
            {
                if (bMyChecksum[iC1] != bReceivedChecksum[iC1])
                {
                    //If the checksum is different, return.
                    return true;
                }
            }
            
            //Handle the frame

            switch (TCPState)
            {
                case TCPSocketState.Established:
                    HandleEstablished(tcpFrame);
                    break;

                case TCPSocketState.Listen:
                    HandleListen(tcpFrame);
                    break;

                case TCPSocketState.SynSent:
                    HandleSynSent(tcpFrame);
                    break;
            }

            return true;
        }

        private void HandleEstablished(TCPFrame tcpFrame)
        {
            if (tcpFrame.SequenceNumber < tcb.RCV_NXT)
            {
                //Frame belongs to this socket but is out of date
                return;
            }

            if (tcpFrame.ResetFlagSet || tcpFrame.SynchronizeFlagSet)
            {
                ClearBuffers();
                TCPState = TCPSocketState.Closed;
            }

            ProcessFramePayload(tcpFrame);

            if (tcpFrame.FinishFlagSet)
            {
                ClearBuffers();
                TCPState = TCPSocketState.Closed;
            }
        }

        private void HandleSynSent(TCPFrame tcpFrame)
        {
            if (tcpFrame.ResetFlagSet)
            {
                ClearBuffers();
                TCPState = TCPSocketState.Closed;
            }

            if (tcpFrame.SynchronizeFlagSet)
            {
                tcb.RCV_NXT = tcpFrame.SequenceNumber + 1;
                tcb.IRS = tcpFrame.SequenceNumber;
                TCPState = TCPSocketState.Established;
            }
        }

        private void HandleListen(TCPFrame tcpFrame)
        {
            if (tcpFrame.SynchronizeFlagSet)
            {
                tcb.RCV_NXT = tcpFrame.SequenceNumber + 1;
                tcb.IRS = tcpFrame.SequenceNumber;
                TCPState = TCPSocketState.Established;
            }
        }

        private void ProcessFramePayload(TCPFrame tcpFrame)
        {
            if (tcpFrame.EncapsulatedFrame.Length == 0)
            {
                return;
            }

            foreach (TCPFrame fFrame in tcpFrameStore)
            {
                if (tcpFrame.SequenceNumber == fFrame.SequenceNumber)
                {
                    //Frame already present - duplicate
                    return;
                }
            }

            this.tcpFrameStore.Add(tcpFrame);

            tcpFrameStore.Sort(new TCPFrameSequenceComparer());

            while (tcpFrameStore.Count > 0)
            {
                if (tcpFrameStore[0].SequenceNumber == tcb.RCV_NXT)
                {
                    tcb.RCV_NXT += (uint)tcpFrameStore[0].EncapsulatedFrame.Length;
                    InvokeFrameDecapsulated(tcpFrameStore[0].EncapsulatedFrame, tcpFrame.PushFlagSet || tcpFrame.FinishFlagSet);
                    System.Diagnostics.Debug.WriteLine(tcpFrameStore[0].EncapsulatedFrame.Length + "bytes of data pushed. (From " + this.RemoteBinding + " to "+  this.LocalBinding + " at socket " + this.ChildSocket.BindingInformation.ToString());
                    tcpFrameStore.RemoveAt(0);
                }
                else
                {
                    break;
                }
            }
        }

        private void ClearBuffers()
        {
            tcpFrameStore.Clear();
        }

        public override void Flush()
        {
            throw new InvalidOperationException("Writing data to a TCP listener socket is not possible since it is read only.");
        }

        public override void PushDown(Frame fFrame, bool bPush)
        {
            throw new InvalidOperationException("Writing data to a TCP listener socket is not possible since it is read only.");
        }

        public override BindingInformation BindingInformation
        {
            get { throw new NotImplementedException(); }
        }

        public override void Dispose()
        {
            ClearBuffers();
        }

        public void SimulateConnect()
        {
            TCPState = TCPSocketState.SynSent;
        }

        public void SimulateListen()
        {
            TCPState = TCPSocketState.Listen;
        }

        public void SimulateClose()
        {
            ClearBuffers();
            TCPState = TCPSocketState.Closed;
        }
    }


    public class TCPListenerSocketEventArgs : EventArgs
    {
        public TCPListenerSocket Sender { get; private set; }

        public TCPListenerSocketEventArgs(TCPListenerSocket sSender)
        {
            this.Sender = sSender;
        }
    }
}
