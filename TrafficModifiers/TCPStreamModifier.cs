using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.TCP;
using System.Net;
using System.Threading;
using eExNetworkLibrary.Sockets;
using eExNetworkLibrary.TrafficModifiers.StreamModification;
using eExNetworkLibrary.Utilities;

namespace eExNetworkLibrary.TrafficModifiers
{
    /// <summary>
    /// This class can be used for modifying TCP streams
    /// </summary>
    [Obsolete("This class is marked as an experimental preview and not fully functional at the moment. This class lacks clean exception handling", false)]
    public abstract class TCPStreamModifier : TrafficModifier
    {
        List<TCPStreamModifierStack> lStacks;
        List<IPAddress> lLocalAddresses;
        bool bIsShuttingDown;

        public event EventHandler<TCPStreamModifierEventArgs> StackCreated;
        public event EventHandler<TCPStreamModifierEventArgs> StackDestroyed;

        /// <summary>
        /// A bool indicating whether connections from or to addresses which 
        /// are assigned to this hosts network card by the operating system should be 
        /// intercepted or not. 
        /// 
        /// Local addresses are queried when this handler is created from the operating systems interface configuration.  
        /// </summary>
        public bool AutoExcludeLocalConnections { get; set; }

        public TCPStreamModifier()
        {
            lStacks = new List<TCPStreamModifierStack>();
            AutoExcludeLocalConnections = true;
            lLocalAddresses = new List<IPAddress>();

            foreach (string strInterface in InterfaceConfiguration.GetAllInterfaceNames())
            {
                lLocalAddresses.AddRange(InterfaceConfiguration.GetIPAddressesForInterface(strInterface));
            }
        }

        protected override Frame ModifyTraffic(Frame fInputFrame)
        {
            if (!bIsShuttingDown)
            {
                IP.IPv4Frame ipv4Frame = GetIPv4Frame(fInputFrame);
                TCP.TCPFrame tcpFrame = GetTCPFrame(fInputFrame);

                if (ipv4Frame == null || tcpFrame == null)
                {
                    return fInputFrame;
                }

                if (IsLocal(ipv4Frame))
                {
                    return fInputFrame;
                }

                if (!ShouldIntercept(ipv4Frame.SourceAddress, ipv4Frame.DestinationAddress, tcpFrame.SourcePort, tcpFrame.DestinationPort))
                {
                    return fInputFrame;
                }

                bool bFound = false;

                lock (lStacks)
                {

                    foreach (TCPStreamModifierStack tcpStack in lStacks)
                    {
                        if (tcpStack.StackAlice.PushUp(fInputFrame, false) || tcpStack.StackBob.PushUp(fInputFrame, false))
                        {
                            bFound = true;
                            break;
                        }
                    }

                    if (!bFound)
                    {
                        //We have to create a new stack...

                        TCPIPStack sAlice;
                        TCPIPStack sBob;
                        NetworkStreamModifier[] arMods;

                        sAlice = new TCPIPStack(ipv4Frame.SourceAddress, ipv4Frame.DestinationAddress, tcpFrame.SourcePort, tcpFrame.DestinationPort);
                        sBob = new TCPIPStack(ipv4Frame.DestinationAddress, ipv4Frame.SourceAddress, tcpFrame.DestinationPort, tcpFrame.SourcePort);
                        sAlice.FrameEncapsulated += new FrameProcessedEventHandler(Stack_FrameEncapsulated);
                        sBob.FrameEncapsulated += new FrameProcessedEventHandler(Stack_FrameEncapsulated);

                        sAlice.TCPSocket.StateChange += new EventHandler<TCPSocketEventArgs>(TCPSocket_StateChange);
                        sBob.TCPSocket.StateChange += new EventHandler<TCPSocketEventArgs>(TCPSocket_StateChange);

                        arMods = CreateAndLinkStreamOperators(new SocketNetworkStream(sAlice), new SocketNetworkStream(sBob));

                        TCPStreamModifierStack tsmsStack = new TCPStreamModifierStack(sAlice, sBob, arMods);

                        foreach (NetworkStreamModifier sModifier in tsmsStack.Modifiers)
                        {
                            sModifier.Start();
                            //sModifier.AliceLoopClosed += new EventHandler(sModifier_AliceLoopClosed);
                            //sModifier.BobLoopClosed += new EventHandler(sModifier_BobLoopClosed);
                            sModifier.AliceLoopError += new ExceptionEventHandler(sModifier_AliceLoopError);
                            sModifier.BobLoopError += new ExceptionEventHandler(sModifier_BobLoopError);
                        }

                        tsmsStack.StackAlice.Listen();
                        tsmsStack.StackAlice.PushUp(fInputFrame, false);

                        tsmsStack.StackBob.ConnectAsync();

                        lStacks.Add(tsmsStack);

                        //Notify Created
                        InvokeExternalAsync(StackCreated, new TCPStreamModifierEventArgs(tsmsStack));
                    }
                }

                return null;
            }

            return fInputFrame;
        }

        void sModifier_BobLoopError(object sender, ExceptionEventArgs args)
        {
            HandleOperatorException(sender, args);
        }

        private void HandleOperatorException(object sender, ExceptionEventArgs args)
        {
            TCPStreamModifierStack tsmsStack = GetStackForOperator((NetworkStreamModifier)sender);
            if (tsmsStack == null)
            {
                throw new InvalidOperationException("TCP stream modifier caught an event of a socket which was not created by it. This should never happen.");
            }
            InvokeExceptionThrown(new TCPStreamModifierException("An exception was thrown by the TCP modifier stack with the binding " + tsmsStack.ToString(), args.Exception));
        }

        void sModifier_AliceLoopError(object sender, ExceptionEventArgs args)
        {
            HandleOperatorException(sender, args);
        }

        //void sModifier_BobLoopClosed(object sender, EventArgs e)
        //{
        //    TCPStreamModifierStack tsmsStack = GetStackForOperator((NetworkStreamModifier)sender);
        //    if (tsmsStack == null)
        //    {
        //        throw new InvalidOperationException("TCP stream modifier caught an event of a stream modifier which was not created by it. This should never happen.");
        //    }

        //    for (int iC1 = 0; iC1 < tsmsStack.Modifiers.Length; iC1++)
        //    {
        //        if (tsmsStack.Modifiers[iC1] == sender)
        //        {
        //            if (iC1 - 1 < 0)
        //            {
        //                if (tsmsStack.StackAlice.TCPSocket.TCPState != TCPSocketState.Closed && tsmsStack.StackAlice.TCPSocket.TCPState != TCPSocketState.TimeWait)
        //                {
        //                    tsmsStack.StackAlice.Close();
        //                }
        //            }
        //            else
        //            {
        //                if (tsmsStack.Modifiers[iC1 - 1].IsRunning)
        //                {
        //                    NetworkStreamModifier nsModifier = tsmsStack.Modifiers[iC1 - 1];
        //                    nsModifier.Stop();
        //                }
        //            }

        //            tsmsStack.Modifiers[iC1].BobLoopClosed -= new EventHandler(sModifier_BobLoopClosed);
        //            tsmsStack.Modifiers[iC1].BobLoopError -= new ExceptionEventHandler(sModifier_BobLoopError);

        //            RemoveIfClosed(tsmsStack);

        //            break;
        //        }
        //    }
        //}

        //void sModifier_AliceLoopClosed(object sender, EventArgs e)
        //{
        //    TCPStreamModifierStack tsmsStack = GetStackForOperator((NetworkStreamModifier)sender);
        //    if (tsmsStack == null)
        //    {
        //        throw new InvalidOperationException("TCP stream modifier caught an event of a stream modifier which was not created by it. This should never happen.");
        //    }

        //    for (int iC1 = 0; iC1 < tsmsStack.Modifiers.Length; iC1++)
        //    {
        //        if (tsmsStack.Modifiers[iC1] == sender)
        //        {
        //            if (iC1 + 1 >= tsmsStack.Modifiers.Length)
        //            {
        //                if (tsmsStack.StackBob.TCPSocket.TCPState != TCPSocketState.Closed && tsmsStack.StackBob.TCPSocket.TCPState != TCPSocketState.TimeWait)
        //                {
        //                    tsmsStack.StackBob.Close();
        //                }
        //            }
        //            else
        //            {
        //                if (tsmsStack.Modifiers[iC1 + 1].IsRunning)
        //                {
        //                    NetworkStreamModifier nsModifier = tsmsStack.Modifiers[iC1 + 1];
        //                    nsModifier.Stop();
        //                }
        //            }
        //            tsmsStack.Modifiers[iC1].AliceLoopClosed -= new EventHandler(sModifier_AliceLoopClosed);
        //            tsmsStack.Modifiers[iC1].AliceLoopError -= new ExceptionEventHandler(sModifier_AliceLoopError);

        //            RemoveIfClosed(tsmsStack);

        //            break;
        //        }
        //    }
        //}

        private bool IsLocal(eExNetworkLibrary.IP.IPv4Frame ipv4Frame)
        {
            return lLocalAddresses.Contains(ipv4Frame.SourceAddress) || lLocalAddresses.Contains(ipv4Frame.DestinationAddress);
        }

        void TCPSocket_StateChange(object sender, TCPSocketEventArgs e)
        {
            if (e.Sender.TCPState == TCPSocketState.Closed || e.Sender.TCPState == TCPSocketState.TimeWait)
            {
                TCPStreamModifierStack tsmsStack = GetStackForSocket(e.Sender);
                if (tsmsStack == null)
                {
                    throw new InvalidOperationException("TCP stream modifier caught an event of a socket which was not created by it. This should never happen.");
                }
                TCPSocket sAlice = tsmsStack.StackAlice.TCPSocket;
                TCPSocket sBob = tsmsStack.StackBob.TCPSocket;

                if (sAlice.TCPState == TCPSocketState.Closed && sBob.TCPState == TCPSocketState.Closed)
                {
                    ShutdownStack(tsmsStack);

                    RemoveIfClosed(tsmsStack);
                }
                else if (sAlice.TCPState != TCPSocketState.Closed && sAlice.TCPState != TCPSocketState.TimeWait)
                {
                    //for (int iC1 = 0; iC1 < tsmsStack.Modifiers.Length; iC1++)
                    //{
                    //    tsmsStack.Modifiers[iC1].Stop();
                    //    tsmsStack.Modifiers[iC1].AliceLoopError -= new ExceptionEventHandler(sModifier_AliceLoopError);
                    //    tsmsStack.Modifiers[iC1].BobLoopError -= new ExceptionEventHandler(sModifier_BobLoopError);
                    //}
                    //tsmsStack.StackAlice.CloseAsync();
                }
                else if (sBob.TCPState != TCPSocketState.Closed && sBob.TCPState != TCPSocketState.TimeWait)
                {
                    //for (int iC1 = tsmsStack.Modifiers.Length - 1; iC1 >= 0; iC1--)
                    //{
                    //    tsmsStack.Modifiers[iC1].Stop();
                    //    tsmsStack.Modifiers[iC1].AliceLoopError -= new ExceptionEventHandler(sModifier_AliceLoopError);
                    //    tsmsStack.Modifiers[iC1].BobLoopError -= new ExceptionEventHandler(sModifier_BobLoopError);
                    //}
                    //tsmsStack.StackBob.CloseAsync();
                }
            }
        }

        /// <summary>
        /// Checks whether all worker threads exited and all resources have been disposed. If true, removes the stack. 
        /// </summary>
        /// <param name="tsmsStack">The stack to check</param>
        /// <returns>A bool indicating whether all worker threads exited and all resources have been disposed.</returns>
        private bool RemoveIfClosed(TCPStreamModifierStack tsmsStack)
        {
            if (tsmsStack.IsClosed)
            {
                lock (lStacks)
                {
                    lStacks.Remove(tsmsStack);
                }

                InvokeExternalAsync(StackDestroyed, new TCPStreamModifierEventArgs(tsmsStack));

                return true;
            }

            return false;
        }

        protected TCPStreamModifierStack GetStackForSocket(TCPSocket sock)
        {
            lock (lStacks)
            {
                foreach (TCPStreamModifierStack sStack in lStacks)
                {
                    if (sStack.StackAlice.TCPSocket == sock || sStack.StackBob.TCPSocket == sock)
                    {
                        return sStack;
                    }
                }

                return null;
            }
        }

        protected TCPStreamModifierStack GetStackForOperator(NetworkStreamModifier nsToFind)
        {
            lock (lStacks)
            {
                foreach (TCPStreamModifierStack sStack in lStacks)
                {
                    foreach (NetworkStreamModifier nsModifier in sStack.Modifiers)
                    {
                        if (nsModifier == nsToFind)
                        {
                            return sStack;
                        }
                    }
                }

                return null;
            }
        }

        void Stack_FrameEncapsulated(object sender, FrameProcessedEventArgs args)
        {
            NotifyNext(args.ProcessedFrame);
        }

        public override void Cleanup()
        {
            bIsShuttingDown = true;


            foreach (TCPStreamModifierStack tsmsStack in lStacks)
            {
                ShutdownStack(tsmsStack);
            }
            lock (lStacks)
            {
                lStacks.Clear();
            }
        }

        private void ShutdownStack(TCPStreamModifierStack tsmsStack)
        {
            tsmsStack.StackAlice.TCPSocket.StateChange -= new EventHandler<TCPSocketEventArgs>(TCPSocket_StateChange);
            tsmsStack.StackBob.TCPSocket.StateChange -= new EventHandler<TCPSocketEventArgs>(TCPSocket_StateChange);
            tsmsStack.StackAlice.FrameEncapsulated -= new FrameProcessedEventHandler(Stack_FrameEncapsulated);
            tsmsStack.StackBob.FrameEncapsulated -= new FrameProcessedEventHandler(Stack_FrameEncapsulated);

            if (tsmsStack.StackAlice.TCPSocket.TCPState != TCPSocketState.Closed && tsmsStack.StackAlice.TCPSocket.TCPState != TCPSocketState.TimeWait)
            {
                tsmsStack.StackAlice.Close();
            }

            if (tsmsStack.StackBob.TCPSocket.TCPState != TCPSocketState.Closed && tsmsStack.StackBob.TCPSocket.TCPState != TCPSocketState.TimeWait)
            {
                tsmsStack.StackBob.Close();
            }

            foreach (NetworkStreamModifier sModifier in tsmsStack.Modifiers)
            {
                if (sModifier.IsRunning)
                {
                    //sModifier.AliceLoopClosed -= new EventHandler(sModifier_AliceLoopClosed);
                    //sModifier.BobLoopClosed -= new EventHandler(sModifier_BobLoopClosed);
                    sModifier.AliceLoopError -= new ExceptionEventHandler(sModifier_AliceLoopError);
                    sModifier.BobLoopError -= new ExceptionEventHandler(sModifier_BobLoopError);
                    sModifier.Stop();
                }
            }
        }

        protected abstract NetworkStreamModifier[] CreateAndLinkStreamOperators(NetworkStream nsAlice, NetworkStream nsBob);

        protected abstract bool ShouldIntercept(IPAddress ipaSource, IPAddress ipaDestination, int iSourcePort, int iDestinationPort);
    }

    public class TCPStreamModifierStack
    {
        public TCPIPStack StackAlice { get; private set; }
        public TCPIPStack StackBob { get; private set; }
        public NetworkStreamModifier[] Modifiers { get; private set; }

        private Dictionary<NetworkStreamModifier, int> dictIsDisposed;

        public bool IsModifierDisposed(NetworkStreamModifier nsModifier)
        {
            return dictIsDisposed[nsModifier] == 0;
        }

        public bool SocketsClosed
        {
            get { return StackAlice.TCPSocket.TCPState == TCPSocketState.Closed && StackBob.TCPSocket.TCPState == TCPSocketState.Closed; }
        }

        public bool IsClosed
        {
            get { return SocketsClosed && AllModifiersClosed; }
        }

        public bool AllModifiersClosed
        {
            get
            {
                foreach (int iInt in dictIsDisposed.Values)
                {
                    if (iInt > 0)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public TCPStreamModifierStack(TCPIPStack stAlice, TCPIPStack stBob, NetworkStreamModifier[] mods)
        {
            dictIsDisposed = new Dictionary<NetworkStreamModifier, int>();
            StackAlice = stAlice;
            StackBob = stBob;
            Modifiers = mods;
            foreach (NetworkStreamModifier nsModifier in mods)
            {
                dictIsDisposed.Add(nsModifier, 2);
                nsModifier.AliceLoopClosed += new EventHandler(nsModifier_AliceLoopClosed);
                nsModifier.BobLoopClosed += new EventHandler(nsModifier_BobLoopClosed);
            }
        }

        void nsModifier_BobLoopClosed(object sender, EventArgs e)
        {
            lock (dictIsDisposed)
            {
                NetworkStreamModifier nsModifier = (NetworkStreamModifier)sender;
                dictIsDisposed[nsModifier]--;
                if (dictIsDisposed[nsModifier] < 0)
                {
                    throw new InvalidOperationException("A loop stopped event was received more than two times from an strem modifier. Since a stream modifier contains only two loops, this could not be possible.");
                }
                nsModifier.BobLoopClosed -= new EventHandler(nsModifier_BobLoopClosed);

            }
        }

        void nsModifier_AliceLoopClosed(object sender, EventArgs e)
        {
            lock (dictIsDisposed)
            {
                NetworkStreamModifier nsModifier = (NetworkStreamModifier)sender;
                dictIsDisposed[nsModifier]--;
                if (dictIsDisposed[nsModifier] < 0)
                {
                    throw new InvalidOperationException("A loop stopped event was received more than two times from an strem modifier. Since a stream modifier contains only two loops, this could not be possible.");
                }
                nsModifier.AliceLoopClosed -= new EventHandler(nsModifier_AliceLoopClosed);
            }
        }

        public override string ToString()
        {
            return "R: " + StackAlice.RemoteBinding.ToString() + " L: " + StackAlice.LocalBinding.ToString() 
                + " <> " + "R: " + StackBob.RemoteBinding.ToString() + " L: " + StackBob.LocalBinding.ToString();
        }
    }

    public class TCPStreamModifierEventArgs : EventArgs
    {
        public TCPStreamModifierStack Stack { get; private set; }

        public TCPStreamModifierEventArgs(TCPStreamModifierStack tsmStack)
        {
            Stack = tsmStack;
        }
    }

    public class TCPStreamModifierException : Exception
    {
        public TCPStreamModifierException(string strMessage, Exception exInnerException) : base(strMessage, exInnerException) { }
    }
}
