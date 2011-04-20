using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.TCP;
using System.IO;
using eExNetworkLibrary.Utilities;
using System.Threading;

namespace eExNetworkLibrary.Sockets
{
    public static class TCPParameters
    {
        public static int MslTimer { get { return 30000; } }
        public static int Timeout {get { return 2000; } }
        public static int MaximumRetries { get { return 3; } }
    }

    [Obsolete("This class is marked as an experimental preview and not fully functional at the moment. Things which are not implemented at the moment are: Window size adjustment, optional parameters (MSS etc.)", false)]
    public class TCPSocket : SocketBase
    {
        TransmissionControlBlock tcb;
        IPseudoHeaderSource pseudoHeaderSource;
        Random r;
        TCPSocketState tcpState;
        TCPSocketState lastTcpState;
        RingBuffer rbSendBuffer;
        List<TCPFrame> tcpFrameStore;
        TCPRetransmissionQueue tcpRetransmissionQueue;

        bool bFlushWhenEstablished;

        public event EventHandler<TCPSocketEventArgs> StateChange;

        AutoResetEvent areClosed;
        AutoResetEvent areConnected;
        Timer tTimeWaitTimer;

        object oTCBLock;

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

        public TCPSocketState TCPState
        {
            get { return tcpState; }
            private set
            {
                lastTcpState = tcpState;

                tcpState = value;

                if (tcpState == TCPSocketState.Closed && lastTcpState != TCPSocketState.TimeWait)
                {
                    tcpRetransmissionQueue.Clear();
                    areClosed.Set();
                }

                if (tcpState == TCPSocketState.Established)
                {
                    areConnected.Set();
                    if (bFlushWhenEstablished)
                    {
                        SegmentAndSend();
                    }
                }

                if (tcpState == TCPSocketState.TimeWait)
                {
                    StartMSLTimer();
                }

                if (tcpState == TCPSocketState.Closed)
                {
                    Dispose();
                }

                InvokeExternalAsync(StateChange, new TCPSocketEventArgs(this));
            }
        }

        private void StartMSLTimer()
        {
            tTimeWaitTimer.Change(TCPParameters.MslTimer, Timeout.Infinite);
        }

        /// <summary>
        /// Gets the local port to which this socket is bound
        /// </summary>
        public int LocalBinding { get; private set; }
        /// <summary>
        /// Gets the remote port to which this socket is bound
        /// </summary>
        public int RemoteBinding { get; private set; }

        /// <summary>
        /// Gets or sets the MSS for this socket. 
        /// </summary>
        public int MaximumSegmentSize { get; set; }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="iSourcePort">The source (remote) port to bind this socket to</param>
        /// <param name="iDestinationPort">The destination (local) port to bind this socket to</param>
        /// <param name="bPseudoHeader">The layer 3 pseudeo header to calculate the checksum with</param>
        public TCPSocket(int iSourcePort, int iDestinationPort, IPseudoHeaderSource pseudoHaaderSource)
        {
            areClosed = new AutoResetEvent(false);
            areConnected = new AutoResetEvent(false);

            bFlushWhenEstablished = false;
            oTCBLock = new object();

            tTimeWaitTimer = new Timer(new TimerCallback(HandleTimeWaitElapsed), null, Timeout.Infinite, Timeout.Infinite);

            RemoteBinding = iSourcePort;
            LocalBinding = iDestinationPort;
            this.pseudoHeaderSource = pseudoHaaderSource;
            rbSendBuffer = new RingBuffer(65565);
            r = new Random();
            lastTcpState = TCPSocketState.Closed;
            tcpState = TCPSocketState.Closed;
            MaximumSegmentSize = 1400;
            tcpFrameStore = new List<TCPFrame>();
            tcpRetransmissionQueue = new TCPRetransmissionQueue(Transmit, HandleTransmissionFailure);
        }
        private void Transmit(TCPFrame tcpFrame)
        {
            InvokeFrameEncapsulated(tcpFrame);
        }

        private void TransmitAssured(TCPFrame tcpFrame)
        {
            tcpRetransmissionQueue.Enqueue(tcpFrame);
            InvokeFrameEncapsulated(tcpFrame);
        }

        private void HandleTransmissionFailure(TCPFrame tcpFrame)
        {
            this.TCPState = TCPSocketState.Closed;
        }

        private void HandleTimeWaitElapsed(object param)
        {
            TCPState = TCPSocketState.Closed;
        }
        public void ConnectAsync()
        {
            if (TCPState == TCPSocketState.Closed || TCPState == TCPSocketState.Listen)
            {
                if (TCPState == TCPSocketState.Closed)
                {
                    CreateTCB();
                }

                TCPFrame tcpFrame = new TCPFrame();
                tcpFrame.SynchronizeFlagSet = true;
                tcpFrame.SequenceNumber = tcb.ISS;
                tcpFrame.SourcePort = this.LocalBinding;
                tcpFrame.DestinationPort = this.RemoteBinding;
                tcpFrame.Window = tcb.RCV_WND;
                tcpFrame.EncapsulatedFrame = new RawDataFrame(new byte[0]);
                tcpFrame.Checksum = tcpFrame.CalculateChecksum(pseudoHeaderSource.GetPseudoHeader(tcpFrame));

                TransmitAssured(tcpFrame);

                tcb.SND_UNA = tcb.ISS;
                tcb.SND_NXT = (tcb.ISS + 1) % uint.MaxValue;

                this.TCPState = TCPSocketState.SynSent;
            }
            else
            {
                throw new InvalidOperationException("Cannot call Connect() on a socket which is being used.");
            }
        }

        public void Connect()
        {
            ConnectAsync();
            areConnected.WaitOne();
        }

        private void CreateTCB()
        {
            tcb = new TransmissionControlBlock();
            tcb.SND_NXT = 0;
            tcb.SND_UNA = 0;
            tcb.SND_WND = 0;
            tcb.SND_UP = 0;
            tcb.SND_WL1 = 0;
            tcb.SND_WL2 = 0;
            tcb.ISS = (uint)r.Next();
            tcb.RCV_NXT = 0;
            tcb.RCV_WND = 13000;
            tcb.RCV_UP = 0;
            tcb.IRS = 0;
        }

        public void Listen()
        {
            if (TCPState == TCPSocketState.Closed)
            {
                CreateTCB();
                this.TCPState = TCPSocketState.Listen;
            }
            else
            {
                throw new InvalidOperationException("Cannot call Listen() on a socket which is being used.");
            }
        }

        public override void Close()
        {
            BeginClose(); 
            EndClose();
        }

        public void EndClose()
        {
            if (tcpState != TCPSocketState.TimeWait && tcpState != TCPSocketState.Closed)
            {
                areClosed.WaitOne();
            }
        }

        public void BeginClose()
        {
            if (tcpState != TCPSocketState.TimeWait && tcpState != TCPSocketState.Closed)
            {
                if (rbSendBuffer.Length > 0)
                {
                    Flush();
                }
                if (tcpState == TCPSocketState.Established)
                {
                    TCPFrame tcpResponseFrame = new TCPFrame();
                    tcpResponseFrame.FinishFlagSet = true;
                    tcpResponseFrame.AcknowledgementFlagSet = true;
                    tcpResponseFrame.SequenceNumber = tcb.SND_NXT;
                    tcpResponseFrame.AcknowledgementNumber = tcb.RCV_NXT;
                    tcpResponseFrame.SourcePort = this.LocalBinding;
                    tcpResponseFrame.DestinationPort = this.RemoteBinding;
                    tcpResponseFrame.Window = tcb.RCV_WND;
                    tcpResponseFrame.Checksum = tcpResponseFrame.CalculateChecksum(pseudoHeaderSource.GetPseudoHeader(tcpResponseFrame));

                    tcb.SND_NXT = tcb.SND_NXT + 1;

                    TransmitAssured(tcpResponseFrame);
                    this.TCPState = TCPSocketState.FinWait1;
                }
                else if (tcpState == TCPSocketState.Listen)
                {
                    ClearBuffers();
                    this.TCPState = TCPSocketState.Closed;
                }
                else
                {
                    this.TCPState = TCPSocketState.TimeWait;
                }
            }
            else
            {
                throw new InvalidOperationException("Cannot close a socket which has no connection established.");
            }
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
                    //If the checksum is diffrent, return.
                    return true;
                }
            }

            //Handle the frame

            switch (TCPState)
            {
                case TCPSocketState.CloseWait:
                    HandleCloseWait(tcpFrame);
                    break;

                case TCPSocketState.Closing:
                    HandleClosing(tcpFrame);

                    break;

                case TCPSocketState.Established:
                    HandleEstablished(tcpFrame);
                    break;

                case TCPSocketState.FinWait1:
                    HandleFinWait1(tcpFrame);
                    break;

                case TCPSocketState.FinWait2:
                    HandleFinWait2(tcpFrame);
                    break;

                case TCPSocketState.LastAck:
                    HandleLastAck(tcpFrame);
                    break;

                case TCPSocketState.Listen:
                    if (tcpFrame.ResetFlagSet)
                    {
                        return true;
                    }
                    HandleListen(tcpFrame);
                    break;

                case TCPSocketState.SynReceived:
                    HandleSynReceived(tcpFrame);
                    break;

                case TCPSocketState.SynSent:
                    HandleSynSent(tcpFrame);
                    break;

                case TCPSocketState.TimeWait:
                    HandleTimeWait(tcpFrame);
                    break;

                case TCPSocketState.Closed:
                    RespondWithReset(tcpFrame);
                    break;
            }

            return true;
        }

        private void HandleCloseWait(TCPFrame tcpFrame)
        {
            if (CheckSequenceNumber(tcpFrame))
            {
                if (tcpFrame.ResetFlagSet)
                {
                    ClearBuffers();
                    TCPState = TCPSocketState.Closed;
                }
                else
                {
                    if (tcpFrame.SynchronizeFlagSet)
                    {
                        RespondWithReset(tcpFrame);
                        ClearBuffers();
                        TCPState = TCPSocketState.Closed;
                    }
                    else
                    {
                        if (tcpFrame.AcknowledgementFlagSet)
                        {
                            if (tcb.SND_UNA < tcpFrame.AcknowledgementNumber && tcpFrame.AcknowledgementNumber <= tcb.SND_NXT)
                            {
                                tcb.SND_UNA = tcpFrame.AcknowledgementNumber;
                                SendAndUpdateRetransmissionQueue();
                            }
                            if (tcb.SND_NXT < tcpFrame.AcknowledgementNumber)
                            {
                                RespondWithAck();
                                return;
                            }
                        }

                        if (tcb.SND_UNA < tcpFrame.AcknowledgementNumber && tcpFrame.AcknowledgementNumber <= tcb.SND_NXT)
                        {
                            //Upadate send window
                            if (tcb.SND_WL1 < tcpFrame.SequenceNumber || (tcb.SND_WL1 == tcpFrame.SequenceNumber && tcb.SND_WL2 <= tcpFrame.AcknowledgementNumber))
                            {
                                tcb.SND_WND = tcpFrame.Window;
                                tcb.SND_WL2 = tcpFrame.AcknowledgementNumber;
                                tcb.SND_WL1 = tcpFrame.SequenceNumber;
                            }
                        }

                        HandleCommonFin(tcpFrame);
                    }
                }
            }
            else
            {
                RespondWithAck();
            }
        }

        private void HandleCommonFin(TCPFrame tcpFrame)
        {
            if (tcpFrame.FinishFlagSet)
            {
                SignalConnectionClosing();
                RespondWithAck();
            }
        }

        private void HandleFinWait2(TCPFrame tcpFrame)
        {
            if (CheckSequenceNumber(tcpFrame))
            {
                if (tcpFrame.ResetFlagSet)
                {
                    ClearBuffers();
                    TCPState = TCPSocketState.Closed;
                }
                else
                {
                    if (tcpFrame.SynchronizeFlagSet)
                    {
                        RespondWithReset(tcpFrame);
                        ClearBuffers();
                        TCPState = TCPSocketState.Closed;
                    }
                    else
                    {
                        if (tcpFrame.AcknowledgementFlagSet)
                        {
                            if (tcb.SND_UNA < tcpFrame.AcknowledgementNumber && tcpFrame.AcknowledgementNumber <= tcb.SND_NXT)
                            {
                                tcb.SND_UNA = tcpFrame.AcknowledgementNumber;
                                SendAndUpdateRetransmissionQueue();
                            }
                            if (tcb.SND_UNA < tcpFrame.AcknowledgementNumber && tcpFrame.AcknowledgementNumber <= tcb.SND_NXT)
                            {
                                //Upadate send window
                                if (tcb.SND_WL1 < tcpFrame.SequenceNumber || (tcb.SND_WL1 == tcpFrame.SequenceNumber && tcb.SND_WL2 <= tcpFrame.AcknowledgementNumber))
                                {
                                    tcb.SND_WND = tcpFrame.Window;
                                    tcb.SND_WL2 = tcpFrame.AcknowledgementNumber;
                                    tcb.SND_WL1 = tcpFrame.SequenceNumber;
                                }
                            }
                        }

                        ProcessFramePayload(tcpFrame);

                        if (tcpRetransmissionQueue.IsEmpty && tcpFrame.FinishFlagSet)
                        {
                            HandleCommonFin(tcpFrame);
                            TCPState = TCPSocketState.TimeWait;
                            return;
                        }

                        RespondWithAck();
                    }

                }
            }
            else
            {
                RespondWithAck();
            }
        }

        private void HandleTimeWait(TCPFrame tcpFrame)
        {
            if (CheckSequenceNumber(tcpFrame))
            {
                if (tcpFrame.ResetFlagSet)
                {
                    TCPState = TCPSocketState.Closed;
                }
                else
                {
                    if (tcpFrame.SynchronizeFlagSet)
                    {
                        RespondWithReset(tcpFrame);
                        ClearBuffers();
                        TCPState = TCPSocketState.Closed;
                    }
                    else
                    {
                        if (tcpFrame.FinishFlagSet)
                        {
                            RespondWithAck();
                            StartMSLTimer();
                        }
                    }
                }
            }
            else
            {
                RespondWithAck();
            }
        }

        private void HandleLastAck(TCPFrame tcpFrame)
        {
            if (CheckSequenceNumber(tcpFrame))
            {
                if (tcpFrame.ResetFlagSet)
                {
                    TCPState = TCPSocketState.Closed;
                }
                else
                {
                    if (tcpFrame.SynchronizeFlagSet)
                    {
                        RespondWithReset(tcpFrame);
                        ClearBuffers();
                        TCPState = TCPSocketState.Closed;
                    }
                    else
                    {
                        if (tcpFrame.AcknowledgementFlagSet)
                        {
                            tcb.SND_UNA = tcpFrame.AcknowledgementNumber;
                            SendAndUpdateRetransmissionQueue();
                            if (IsFinAcknowledged())
                            {
                                TCPState = TCPSocketState.Closed;
                            }
                        }
                    }
                }
            }
            else
            {
                RespondWithAck();
            }
        }

        private void HandleFinWait1(TCPFrame tcpFrame)
        {
            if (CheckSequenceNumber(tcpFrame))
            {
                if (tcpFrame.ResetFlagSet)
                {
                    ClearBuffers();
                    TCPState = TCPSocketState.Closed;
                }
                else
                {
                    if (tcpFrame.SynchronizeFlagSet)
                    {
                        RespondWithReset(tcpFrame);
                        ClearBuffers();
                        TCPState = TCPSocketState.Closed;
                    }
                    else
                    {
                        if (tcpFrame.AcknowledgementFlagSet)
                        {
                            if (tcb.SND_UNA < tcpFrame.AcknowledgementNumber && tcpFrame.AcknowledgementNumber <= tcb.SND_NXT)
                            {
                                tcb.SND_UNA = tcpFrame.AcknowledgementNumber;
                                SendAndUpdateRetransmissionQueue();
                            }
                            if (tcb.SND_NXT < tcpFrame.AcknowledgementNumber)
                            {
                                RespondWithAck();
                                return;
                            }

                            if (tcb.SND_UNA < tcpFrame.AcknowledgementNumber && tcpFrame.AcknowledgementNumber <= tcb.SND_NXT)
                            {
                                //Upadate send window
                                if (tcb.SND_WL1 < tcpFrame.SequenceNumber || (tcb.SND_WL1 == tcpFrame.SequenceNumber && tcb.SND_WL2 <= tcpFrame.AcknowledgementNumber))
                                {
                                    tcb.SND_WND = tcpFrame.Window;
                                    tcb.SND_WL2 = tcpFrame.AcknowledgementNumber;
                                    tcb.SND_WL1 = tcpFrame.SequenceNumber;
                                }
                            }

                        }

                        ProcessFramePayload(tcpFrame);

                        if (IsFinAcknowledged())
                        {
                            TCPState = TCPSocketState.FinWait2;
                            if (tcpFrame.FinishFlagSet)
                            {
                                HandleCommonFin(tcpFrame);
                                TCPState = TCPSocketState.TimeWait;
                            }
                            return;
                        }
                        else if (tcpFrame.FinishFlagSet)
                        {
                            HandleCommonFin(tcpFrame);
                            TCPState = TCPSocketState.Closing;
                            return;
                        }

                        RespondWithAck();
                    }
                }
            }
            else
            {
                RespondWithAck();
            }
        }

        private void HandleClosing(TCPFrame tcpFrame)
        {
            if (CheckSequenceNumber(tcpFrame))
            {
                if (tcpFrame.ResetFlagSet)
                {
                    TCPState = TCPSocketState.Closed;
                }
                else
                {
                    if (tcpFrame.SynchronizeFlagSet)
                    {
                        RespondWithReset(tcpFrame);
                        ClearBuffers();
                        TCPState = TCPSocketState.Closed;
                    }
                    else
                    {
                        if (tcpFrame.AcknowledgementFlagSet)
                        {
                            if (tcb.SND_UNA < tcpFrame.AcknowledgementNumber && tcpFrame.AcknowledgementNumber <= tcb.SND_NXT)
                            {
                                tcb.SND_UNA = tcpFrame.AcknowledgementNumber;
                                SendAndUpdateRetransmissionQueue();
                            }
                            if (tcb.SND_NXT < tcpFrame.AcknowledgementNumber)
                            {
                                RespondWithAck();
                                return;
                            }
                            if (tcb.SND_UNA < tcpFrame.AcknowledgementNumber && tcpFrame.AcknowledgementNumber <= tcb.SND_NXT)
                            {
                                //Upadate send window
                                if (tcb.SND_WL1 < tcpFrame.SequenceNumber || (tcb.SND_WL1 == tcpFrame.SequenceNumber && tcb.SND_WL2 <= tcpFrame.AcknowledgementNumber))
                                {
                                    tcb.SND_WND = tcpFrame.Window;
                                    tcb.SND_WL2 = tcpFrame.AcknowledgementNumber;
                                    tcb.SND_WL1 = tcpFrame.SequenceNumber;
                                }
                            }
                        }

                        if (tcpFrame.FinishFlagSet)
                        {
                            ProcessFramePayload(tcpFrame);
                            RespondWithAck();

                            if (IsFinAcknowledged())
                            {
                                TCPState = TCPSocketState.TimeWait;
                            }
                        }
                    }
                }
            }
            else
            {
                RespondWithAck();
            }
        }

        private void HandleEstablished(TCPFrame tcpFrame)
        {
            if (CheckSequenceNumber(tcpFrame))
            {
                if (tcpFrame.ResetFlagSet)
                {
                    ClearBuffers();
                    TCPState = TCPSocketState.Closed;
                }
                else
                {
                    if (tcpFrame.SynchronizeFlagSet)
                    {
                        RespondWithReset(tcpFrame);
                        ClearBuffers();
                        TCPState = TCPSocketState.Closed;
                    }
                    else
                    {
                        if (tcpFrame.AcknowledgementFlagSet)
                        {
                            if (tcb.SND_UNA < tcpFrame.AcknowledgementNumber && tcpFrame.AcknowledgementNumber <= tcb.SND_NXT)
                            {
                                tcb.SND_UNA = tcpFrame.AcknowledgementNumber;
                                SendAndUpdateRetransmissionQueue();
                            }
                            if (tcb.SND_NXT < tcpFrame.AcknowledgementNumber && tcpFrame.Length != 0)
                            {
                                RespondWithAck();
                                return;
                            }
                            if (tcb.SND_UNA <= tcpFrame.AcknowledgementNumber && tcpFrame.AcknowledgementNumber <= tcb.SND_NXT)
                            {
                                //Upadate send window
                                if (tcb.SND_WL1 < tcpFrame.SequenceNumber || (tcb.SND_WL1 == tcpFrame.SequenceNumber && tcb.SND_WL2 <= tcpFrame.AcknowledgementNumber))
                                {
                                    tcb.SND_WND = tcpFrame.Window;
                                    tcb.SND_WL2 = tcpFrame.AcknowledgementNumber;
                                    tcb.SND_WL1 = tcpFrame.SequenceNumber;
                                }
                            }
                        }

                        ProcessFramePayload(tcpFrame);

                        if (tcpFrame.FinishFlagSet)
                        {
                            HandleCommonFin(tcpFrame);
                            TCPState = TCPSocketState.CloseWait;

                            TCPFrame tcpResponseFrame = new TCPFrame();
                            tcpResponseFrame.FinishFlagSet = true;
                            tcpResponseFrame.AcknowledgementFlagSet = true;
                            tcpResponseFrame.SequenceNumber = tcb.SND_NXT;
                            tcpResponseFrame.AcknowledgementNumber = tcb.RCV_NXT;
                            tcpResponseFrame.SourcePort = this.LocalBinding;
                            tcpResponseFrame.DestinationPort = this.RemoteBinding;
                            tcpResponseFrame.Window = tcb.RCV_WND;
                            tcpResponseFrame.Checksum = tcpResponseFrame.CalculateChecksum(pseudoHeaderSource.GetPseudoHeader(tcpResponseFrame));

                            TransmitAssured(tcpResponseFrame);

                            TCPState = TCPSocketState.LastAck;
                        }
                        else
                        {
                            if (tcpFrame.EncapsulatedFrame != null && tcpFrame.EncapsulatedFrame.Length > 0)
                            {
                                RespondWithAck();
                            }
                        }
                    }
                }
            }
            else
            {
                RespondWithAck();
            }
        }

        private void SignalConnectionClosing()
        {
            //throw new NotImplementedException();
        }

        private bool IsFinAcknowledged()
        {
            return !tcpRetransmissionQueue.ContainsFin();
        }

        private void SendAndUpdateRetransmissionQueue()
        {
            tcpRetransmissionQueue.Acknowledge(tcb.SND_UNA);

            SegmentAndSend();
        }

        private void RespondWithAck()
        {
            TCPFrame tcpResponseFrame = new TCPFrame();
            tcpResponseFrame.AcknowledgementFlagSet = true;
            tcpResponseFrame.SequenceNumber = tcb.SND_NXT;
            tcpResponseFrame.AcknowledgementNumber = tcb.RCV_NXT;
            tcpResponseFrame.SourcePort = this.LocalBinding;
            tcpResponseFrame.DestinationPort = this.RemoteBinding;
            tcpResponseFrame.Window = tcb.RCV_WND;
            tcpResponseFrame.Checksum = tcpResponseFrame.CalculateChecksum(pseudoHeaderSource.GetPseudoHeader(tcpResponseFrame));

            InvokeFrameEncapsulated(tcpResponseFrame);
        }

        private bool CheckSequenceNumber(TCPFrame tcpFrame)
        {
            if (tcpFrame.EncapsulatedFrame.Length == 0 && tcb.RCV_WND == 0)
            {
                return tcpFrame.SequenceNumber == tcb.RCV_NXT;
            }
            else if (tcpFrame.EncapsulatedFrame.Length == 0 && tcb.RCV_WND > 0)
            {
                return tcb.RCV_NXT <= tcpFrame.SequenceNumber && tcpFrame.SequenceNumber < tcb.RCV_NXT + tcb.RCV_WND;
            } 
            else if (tcpFrame.EncapsulatedFrame.Length > 0 && tcb.RCV_WND == 0)
            {
                return false;
            }
            else
            {
                return (tcb.RCV_NXT <= tcpFrame.SequenceNumber && tcpFrame.SequenceNumber < tcb.RCV_NXT + tcb.RCV_WND) ||
                    (tcb.RCV_NXT <= tcpFrame.SequenceNumber && tcpFrame.SequenceNumber + tcpFrame.EncapsulatedFrame.Length - 1 < tcb.RCV_NXT + tcb.RCV_WND);
            
            }
        }

        private void HandleSynSent(TCPFrame tcpFrame)
        {
            if (tcpFrame.AcknowledgementFlagSet)
            {
                if (tcpFrame.AcknowledgementNumber <= tcb.ISS || tcpFrame.AcknowledgementNumber > tcb.SND_NXT)
                {
                    RespondWithReset(tcpFrame);
                    return;
                }
                if (!(tcb.SND_UNA <= tcpFrame.AcknowledgementNumber && tcpFrame.AcknowledgementNumber <= tcb.SND_NXT))
                {
                    return;
                }
            }
            if (tcpFrame.ResetFlagSet)
            {
                TCPState = TCPSocketState.Closed;
                return;
            }
            if (tcpFrame.SynchronizeFlagSet)
            {
                tcb.RCV_NXT = tcpFrame.SequenceNumber + 1;

                tcb.IRS = tcpFrame.SequenceNumber;
                tcb.SND_WND = tcpFrame.Window;

                if (tcpFrame.AcknowledgementFlagSet)
                {
                    tcb.SND_UNA = tcpFrame.AcknowledgementNumber;
                    tcpRetransmissionQueue.Acknowledge(tcb.SND_UNA);
                }

                if (tcb.SND_UNA > tcb.ISS)
                {
                    TCPState = TCPSocketState.Established;

                    TCPFrame tcpResponseFrame = new TCPFrame();
                    tcpResponseFrame.SynchronizeFlagSet = false;
                    tcpResponseFrame.AcknowledgementFlagSet = true;
                    tcpResponseFrame.SequenceNumber = tcb.SND_NXT;
                    tcpResponseFrame.SourcePort = this.LocalBinding;
                    tcpResponseFrame.DestinationPort = this.RemoteBinding;
                    tcpResponseFrame.AcknowledgementNumber = tcb.RCV_NXT;
                    tcpResponseFrame.Window = tcb.RCV_WND;
                    tcpResponseFrame.Checksum = tcpResponseFrame.CalculateChecksum(pseudoHeaderSource.GetPseudoHeader(tcpResponseFrame));

                    Transmit(tcpResponseFrame);
                }
                else
                {
                    TCPState = TCPSocketState.SynReceived;

                    TCPFrame tcpResponseFrame = new TCPFrame();
                    tcpResponseFrame.SynchronizeFlagSet = true;
                    tcpResponseFrame.AcknowledgementFlagSet = true;
                    tcpResponseFrame.SequenceNumber = tcb.ISS;
                    tcpResponseFrame.SourcePort = this.LocalBinding;
                    tcpResponseFrame.DestinationPort = this.RemoteBinding;
                    tcpResponseFrame.AcknowledgementNumber = tcb.RCV_NXT;
                    tcpResponseFrame.Window = tcb.RCV_WND;
                    tcpResponseFrame.Checksum = tcpResponseFrame.CalculateChecksum(pseudoHeaderSource.GetPseudoHeader(tcpResponseFrame));

                    TransmitAssured(tcpFrame);
                }
            }
        }

        private void HandleSynReceived(TCPFrame tcpFrame)
        {
            if (CheckSequenceNumber(tcpFrame))
            {
                if (tcpFrame.ResetFlagSet)
                {
                    ClearBuffers();
                    if (lastTcpState == TCPSocketState.Listen)
                    {
                        TCPState = TCPSocketState.Listen;
                    }
                    else
                    {
                        TCPState = TCPSocketState.Closed;
                    }
                }
                else
                {
                    if (tcpFrame.SynchronizeFlagSet)
                    {
                        RespondWithReset(tcpFrame);
                        ClearBuffers();
                        TCPState = TCPSocketState.Closed;
                    }
                    else
                    {
                        if (tcpFrame.AcknowledgementFlagSet)
                        {
                            if (tcb.SND_UNA <= tcpFrame.AcknowledgementNumber && tcpFrame.AcknowledgementNumber <= tcb.SND_NXT)
                            {
                                TCPState = TCPSocketState.Established;
                            }
                            else
                            {
                                RespondWithReset(tcpFrame);
                                TCPState = TCPSocketState.Closed;
                            }
                        }
                    }

                    if (tcpFrame.FinishFlagSet)
                    {
                        ProcessFramePayload(tcpFrame);
                        HandleCommonFin(tcpFrame);
                        TCPState = TCPSocketState.CloseWait;
                    }
                }
            }
            else
            {
                RespondWithAck();
            }
        }

        private void ClearBuffers()
        {
            tcpRetransmissionQueue.Clear();
            tcpFrameStore.Clear();
        }

        private void HandleListen(TCPFrame tcpFrame)
        {
            if (tcpFrame.AcknowledgementFlagSet)
            {
                RespondWithReset(tcpFrame);
            }
            if (tcpFrame.SynchronizeFlagSet)
            {
                TCPState = TCPSocketState.SynReceived;

                tcb.RCV_NXT = tcpFrame.SequenceNumber + 1;

                tcb.IRS = tcpFrame.SequenceNumber;
                tcb.SND_WND = tcpFrame.Window;

                if (tcpFrame.EncapsulatedFrame != null && tcpFrame.EncapsulatedFrame.Length > 0)
                {
                    ProcessFramePayload(tcpFrame);
                }

                TCPFrame tcpResponseFrame = new TCPFrame();
                tcpResponseFrame.SynchronizeFlagSet = true;
                tcpResponseFrame.AcknowledgementFlagSet = true;
                tcpResponseFrame.SourcePort = this.LocalBinding;
                tcpResponseFrame.DestinationPort = this.RemoteBinding;
                tcpResponseFrame.SequenceNumber = tcb.ISS;
                tcpResponseFrame.AcknowledgementNumber = tcb.RCV_NXT;
                tcpResponseFrame.Window = tcb.RCV_WND;
                tcpResponseFrame.Checksum = tcpResponseFrame.CalculateChecksum(pseudoHeaderSource.GetPseudoHeader(tcpResponseFrame));

                TransmitAssured(tcpResponseFrame);

                tcb.SND_NXT = tcb.ISS + 1;
                tcb.SND_UNA = tcb.ISS;
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
                    tcpFrameStore.RemoveAt(0);
                }
                else
                {
                    break;
                }
            }

            //foreach (TCPFrame f in tcpDecapsulator.DecapsulateFrame(tcpFrame))
            //{
            //    InvokeFrameDecapsulated(f.EncapsulatedFrame, f.PushFlagSet || f.FinishFlagSet);
            //}
        }

        private void RespondWithReset(TCPFrame tcpFrame)
        {
            if (!tcpFrame.ResetFlagSet)
            {
                TCPFrame tcpResponseFrame = new TCPFrame();
                tcpResponseFrame.SourcePort = LocalBinding;
                tcpResponseFrame.DestinationPort = RemoteBinding;
                tcpResponseFrame.ResetFlagSet = true;
                if (!tcpFrame.AcknowledgementFlagSet)
                {
                    tcpResponseFrame.AcknowledgementFlagSet = true;
                    tcpResponseFrame.AcknowledgementNumber = tcpFrame.SequenceNumber + (uint)tcpFrame.Length;
                    tcpResponseFrame.SequenceNumber = 0;
                }
                else
                {
                    tcpResponseFrame.SequenceNumber = tcpFrame.AcknowledgementNumber;
                }
                tcpResponseFrame.Checksum = tcpResponseFrame.CalculateChecksum(pseudoHeaderSource.GetPseudoHeader(tcpResponseFrame));

                Transmit(tcpResponseFrame);
            }
        }

        public override void PushDown(Frame fFrame, bool bPush)
        {
            PushDown(fFrame.FrameBytes, bPush);
        }

        public void PushDown(Frame fFrame)
        {
            PushDown(fFrame.FrameBytes, false);
        }


        public void PushDown(byte[] bData)
        {
            PushDown(bData, false);
        }

        public override void Flush()
        {
            SegmentAndSend();
        }

        public override void PushDown(byte[] bData, bool bPush)
        {
            lock(rbSendBuffer)
            {
            //rbSendBuffer.Write(bData, 0, bData.Length);
                switch (TCPState)
                {
                    case TCPSocketState.CloseWait:
                    case TCPSocketState.Established:

                        rbSendBuffer.Write(bData, 0, bData.Length);
                        if (bPush || rbSendBuffer.Count >= tcb.SND_WND)
                        {
                            Flush();
                        }
                        break;

                    case TCPSocketState.Listen:

                        rbSendBuffer.Write(bData, 0, bData.Length);
                        Connect();
                        break;

                    case TCPSocketState.SynReceived:
                    case TCPSocketState.SynSent:

                        rbSendBuffer.Write(bData, 0, bData.Length);
                        break;

                    case TCPSocketState.Closing:
                    case TCPSocketState.TimeWait:
                    case TCPSocketState.Closed:
                    case TCPSocketState.FinWait1:
                    case TCPSocketState.FinWait2:
                    case TCPSocketState.LastAck:
                        throw new InvalidOperationException("Trying to send data is not possible while no connection has been established");
                }

            }
        }

        public override BindingInformation BindingInformation
        {
            get { return new TCPBindingInformation(new TCPEndPoint(LocalBinding), new TCPEndPoint(RemoteBinding)); }
        }

        private void SegmentAndSend()
        {
            lock (oTCBLock)
            {
                if (tcpState != TCPSocketState.Established)
                {
                    bFlushWhenEstablished = true;
                    return;
                }

                byte[] bSegment = new byte[Math.Min(rbSendBuffer.Count, (tcb.SND_UNA + tcb.SND_WND - tcb.SND_NXT))];
                rbSendBuffer.Read(bSegment, 0, bSegment.Length);

                for (int iC1 = 0; iC1 < bSegment.Length; iC1 += MaximumSegmentSize)
                {
                    byte[] bSegmentedSegment = new byte[Math.Min(bSegment.Length - iC1, MaximumSegmentSize)];

                    for (int iC2 = 0; iC2 < bSegmentedSegment.Length; iC2++)
                    {
                        bSegmentedSegment[iC2] = bSegment[iC2 + (iC1)];
                    }

                    TCPFrame tcpFrame = new TCPFrame();

                    tcpFrame.SequenceNumber = tcb.SND_NXT;
                    tcpFrame.AcknowledgementNumber = tcb.RCV_NXT;
                    tcpFrame.Window = tcb.RCV_WND;
                    tcpFrame.AcknowledgementFlagSet = true;
                    tcpFrame.SourcePort = this.LocalBinding;
                    tcpFrame.DestinationPort = this.RemoteBinding;
                    tcpFrame.EncapsulatedFrame = new RawDataFrame(bSegmentedSegment);
                    tcpFrame.PushFlagSet = true;
                    tcb.SND_NXT += (uint)bSegmentedSegment.Length;
                    tcpFrame.Checksum = tcpFrame.CalculateChecksum(pseudoHeaderSource.GetPseudoHeader(tcpFrame));

                    TransmitAssured(tcpFrame);
                }
            }
        }

       

        #region Structs

        public struct TransmissionControlBlock
        {
            public uint SND_UNA;
            public uint SND_NXT;
            public uint SND_WND;
            public uint SND_UP;
            public uint SND_WL1;
            public uint SND_WL2;
            public uint ISS;
            public uint RCV_NXT;
            public uint RCV_WND;
            public uint RCV_UP;
            public uint IRS;
        }

        #endregion

        public override void Dispose()
        {
            tcpRetransmissionQueue.Dispose();
            rbSendBuffer.Close();
            areClosed.Set();
            base.Close();
        }
    }

    /// <summary>
    /// This enum defines some TCP socket states
    /// </summary>
    public enum TCPSocketState
    {
        Listen, 
        SynSent,
        SynReceived,
        Established, 
        FinWait1,
        FinWait2,
        CloseWait,
        Closing,
        LastAck,
        TimeWait,
        Closed
    }
    
    public class TCPBindingInformation : BindingInformation
    {
        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="localBinding">The local binding information</param>
        /// <param name="remoteBinding">The remote binding Information</param>
        public TCPBindingInformation(TCPEndPoint localBinding, TCPEndPoint remoteBinding)
            : base(localBinding, remoteBinding)
        { }

        /// <summary>
        /// Gets the description of this EndPoint
        /// </summary>
        /// <returns>The description of this EndPoint</returns>
        public override string ToString()
        {
            return "(TCP) " + base.ToString();
        }
    }

    public class TCPEndPoint : EndPoint
    {
        /// <summary>
        /// Creates a new TCP endpoint
        /// </summary>
        /// <param name="iPort">The port this TCP endpoint belongs to</param>
        public TCPEndPoint(int iPort) :
            base(iPort.ToString())
        {
            Port = iPort;
        }

        /// <summary>
        /// Returns the port of the TCP endpoint
        /// </summary>
        public int Port { get; private set; }
    }

    class TCPFrameSequenceComparer : Comparer<TCPFrame>
    {
        public override int Compare(TCPFrame x, TCPFrame y)
        {
            return x.SequenceNumber.CompareTo(y.SequenceNumber);
        }
    }

    class TCPRetransmissionQueue : IDisposable
    {
        public delegate void RetransmitDelegate(TCPFrame tcpFrame);

        private RetransmitDelegate Retransmit;
        private RetransmitDelegate NotifyFailure;

        private List<TCPRetransmissionEntry> lRetransmissionEntries;

        private Timer tTimer;

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="rtDelegate">A delegate invoked when a TCP frame needs to be retransmitted</param>
        /// <param name="rtFailureDelegate">A delegate invoked when a retransmission fails multiple times</param>
        public TCPRetransmissionQueue(RetransmitDelegate rtDelegate, RetransmitDelegate rtFailureDelegate)
        {
            this.Retransmit = rtDelegate;
            this.NotifyFailure = rtFailureDelegate;
            lRetransmissionEntries = new List<TCPRetransmissionEntry>();
            tTimer = new Timer(CheckQueue, null, 10, 10);
        }

        public bool IsEmpty
        {
            get
            {
                lock (lRetransmissionEntries) { return lRetransmissionEntries.Count == 0; }
            }
        }

        private void CheckQueue(object param)
        {
            lock (lRetransmissionEntries)
            {
                foreach (TCPRetransmissionEntry tcpEntry in lRetransmissionEntries)
                {
                    tcpEntry.DecreaseCounter();
                    if (tcpEntry.Counter < 0)
                    {
                        if (tcpEntry.RetransmissionCounter < TCPParameters.MaximumRetries)
                        {
                            Retransmit(tcpEntry.Frame);
                            tcpEntry.ResetCounter();
                        }
                        else
                        {
                            NotifyFailure(tcpEntry.Frame);
                            //This is used to handle the case of a complete failure which closes the socket and clears the retransmission queue. 
                            break;
                        }
                    }
                }
            }
        }

        public bool ContainsFin()
        {
            lock (lRetransmissionEntries)
            {
                foreach (TCPRetransmissionEntry tcpEntry in lRetransmissionEntries)
                {
                    if (tcpEntry.Frame.FinishFlagSet)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void Acknowledge(uint iAckNumber)
        {
            lock (lRetransmissionEntries)
            {
                for (int iC1 = lRetransmissionEntries.Count - 1; iC1 >= 0; iC1--)
                {
                    if (lRetransmissionEntries[iC1].Frame.SequenceNumber < iAckNumber)
                    {
                        lRetransmissionEntries.RemoveAt(iC1);
                    }
                }
            }
        }

        public void Enqueue(TCPFrame tcpFrame)
        {
            lock (lRetransmissionEntries)
            {
                lRetransmissionEntries.Add(new TCPRetransmissionEntry(tcpFrame));
            }
        }

        public void Clear()
        {
            lock (lRetransmissionEntries)
            {
                lRetransmissionEntries.Clear();
            }
        }

        public void Dispose()
        {
            Clear();
            tTimer.Dispose();
        }

        ~TCPRetransmissionQueue()
        {
            Dispose();
        }
    }

    class TCPRetransmissionEntry
    {
        public int Counter { get; private set; }
        public int RetransmissionCounter { get; private set; }
        public TCPFrame Frame { get; private set; }

        public void ResetCounter()
        {
            Counter = TCPParameters.Timeout;
            RetransmissionCounter++;
        }

        public void DecreaseCounter()
        {
            Counter -= 10;
        }

        public TCPRetransmissionEntry(TCPFrame fFrame)
        {
            ResetCounter();
            Frame = fFrame;
        }
    }

    public class TCPSocketEventArgs : EventArgs
    {
        public TCPSocket Sender { get; private set; }

        public TCPSocketEventArgs(TCPSocket sSender)
        {
            this.Sender = sSender;
        }
    }
}
