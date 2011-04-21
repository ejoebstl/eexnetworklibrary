using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.Monitoring;
using System.Net;
using eExNetworkLibrary.Sockets;
using eExNetworkLibrary.Monitoring.StreamMonitoring;

namespace eExNetworkLibrary.Monitoring
{
    public abstract class TCPStreamMonitor : TrafficAnalyzer
    {
        List<TCPStreamMonitorStack> lStacks;
        bool bIsShuttingDown;

        public event EventHandler<TCPStreamMonitorEventArgs> StackCreated;
        public event EventHandler<TCPStreamMonitorEventArgs> StackDestroyed;

        public TCPStreamMonitor()
        {
            lStacks = new List<TCPStreamMonitorStack>();
        }

        protected override void HandleTraffic(Frame fInputFrame)
        {
            if (!bIsShuttingDown)
            {
                IP.IPFrame ipFrame = GetIPFrame(fInputFrame);
                TCP.TCPFrame tcpFrame = GetTCPFrame(fInputFrame); 
                
                if (ipFrame == null || tcpFrame == null)
                {
                    return;
                }
                
                if (!ShouldIntercept(ipFrame.SourceAddress, ipFrame.DestinationAddress, tcpFrame.SourcePort, tcpFrame.DestinationPort))
                {
                    return;
                }

                bool bFound = false;

                lock (lStacks)
                {

                    foreach (TCPStreamMonitorStack tcpStack in lStacks)
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

                        TCPIPListenerStack sAlice;
                        TCPIPListenerStack sBob;
                        NetworkStreamMonitor[] arMonitors;

                        sBob = new TCPIPListenerStack(ipFrame.DestinationAddress, ipFrame.SourceAddress, tcpFrame.DestinationPort, tcpFrame.SourcePort);
                        sBob.ProtocolParser = this.ProtocolParser;
                        sAlice = new TCPIPListenerStack(ipFrame.SourceAddress, ipFrame.DestinationAddress, tcpFrame.SourcePort, tcpFrame.DestinationPort);
                        sAlice.ProtocolParser = this.ProtocolParser;

                        sAlice.TCPSocket.StateChange += new EventHandler<TCPListenerSocketEventArgs>(TCPSocket_StateChange);
                        sBob.TCPSocket.StateChange += new EventHandler<TCPListenerSocketEventArgs>(TCPSocket_StateChange);

                        arMonitors = CreateAndLinkStreamMonitors(new SocketNetworkStream(sAlice), new SocketNetworkStream(sBob));

                        TCPStreamMonitorStack tsmsStack = new TCPStreamMonitorStack(sAlice, sBob, arMonitors);

                        foreach (NetworkStreamMonitor nsMonitor in tsmsStack.Monitors)
                        {
                            nsMonitor.Start();
                            nsMonitor.LoopError += new ExceptionEventHandler(nsMonitor_LoopError);
                            nsMonitor.LoopClosed += new EventHandler(nsMonitor_LoopClosed);
                        }

                        tsmsStack.StackBob.Listen();
                        tsmsStack.StackBob.PushUp(ipFrame, false);

                        tsmsStack.StackAlice.Connect();

                        lStacks.Add(tsmsStack);

                        //Notify Created
                        InvokeExternal(StackCreated, new TCPStreamMonitorEventArgs(tsmsStack));
                    }
                }
            }
        }

        void nsMonitor_LoopClosed(object sender, EventArgs e)
        {
            NetworkStreamMonitor nsMonitor = (NetworkStreamMonitor)sender;


            nsMonitor.LoopError -= new ExceptionEventHandler(nsMonitor_LoopError);
            nsMonitor.LoopClosed -= new EventHandler(nsMonitor_LoopClosed);

            TCPStreamMonitorStack tsmsStack = GetStackForMonitor(nsMonitor);

            if (tsmsStack != null && tsmsStack.IsClosed)
            {
                lock (lStacks)
                {
                    lStacks.Remove(tsmsStack);
                    InvokeExternal(StackDestroyed, new TCPStreamMonitorEventArgs(tsmsStack));
                }
            }
        }

        void TCPSocket_StateChange(object sender, TCPListenerSocketEventArgs e)
        {
            if (e.Sender.TCPState == TCPSocketState.Closed)
            {
                TCPStreamMonitorStack tsmsStack = GetStackForSocket(e.Sender);
                if (tsmsStack == null)
                {
                    throw new InvalidOperationException("TCP stream monitor caught an event of a socket which was not created by it. This should never happen.");
                }
                TCPListenerSocket sAlice = tsmsStack.StackAlice.TCPSocket;
                TCPListenerSocket sBob = tsmsStack.StackBob.TCPSocket;
                if (sAlice.TCPState == TCPSocketState.Closed && sBob.TCPState == TCPSocketState.Closed)
                {

                    ShutdownStack(tsmsStack);

                    if (tsmsStack.IsClosed)
                    {
                        lock (lStacks)
                        {
                            lStacks.Remove(tsmsStack);
                            InvokeExternal(StackDestroyed, new TCPStreamMonitorEventArgs(tsmsStack));
                        }
                    }
                }
            }
        }

        void nsMonitor_LoopError(object sender, ExceptionEventArgs args)
        {
            TCPStreamMonitorStack tsmsStack = GetStackForMonitor((NetworkStreamMonitor)sender);

            if (tsmsStack != null)
            {
                InvokeExceptionThrown(new TCPStreamMonitorException("An exception was thrown by a TCP stream monitor with the binding " + tsmsStack.ToString(), args.Exception));
            }
            else
            {
                InvokeExceptionThrown(new TCPStreamMonitorException("An exception was thrown by a TCP stream monitor which was already destroyed.", args.Exception));
            }
        }

        protected TCPStreamMonitorStack GetStackForSocket(TCPListenerSocket sock)
        {
            lock (lStacks)
            {
                foreach (TCPStreamMonitorStack sStack in lStacks)
                {
                    if (sStack.StackAlice.TCPSocket == sock || sStack.StackBob.TCPSocket == sock)
                    {
                        return sStack;
                    }
                }

                return null;
            }
        }

        public override void Cleanup()
        {
            bIsShuttingDown = true;
        }

        public override void Stop()
        {
            bIsShuttingDown = true;

            foreach (TCPStreamMonitorStack tsmsStack in lStacks)
            {
                ShutdownStack(tsmsStack);
            }
            lock (lStacks)
            {
                lStacks.Clear();
            }

            base.Stop();
        }

        private void ShutdownStack(TCPStreamMonitorStack tsmsStack)
        {

            tsmsStack.StackAlice.TCPSocket.StateChange -= new EventHandler<TCPListenerSocketEventArgs>(TCPSocket_StateChange);
            tsmsStack.StackBob.TCPSocket.StateChange -= new EventHandler<TCPListenerSocketEventArgs>(TCPSocket_StateChange);
        
            if (tsmsStack.StackAlice.TCPSocket.TCPState != TCPSocketState.Closed)
            {
                tsmsStack.StackAlice.Close();
            }

            if (tsmsStack.StackBob.TCPSocket.TCPState != TCPSocketState.Closed)
            {
                tsmsStack.StackBob.Close();
            }

            foreach (NetworkStreamMonitor nsMonitor in tsmsStack.Monitors)
            {
                if (nsMonitor.IsRunning)
                {
                    nsMonitor.LoopError -= new ExceptionEventHandler(nsMonitor_LoopError);
                    nsMonitor.LoopClosed -= new EventHandler(nsMonitor_LoopClosed);

                    nsMonitor.StopAsync();
                }
            }
        }

        protected TCPStreamMonitorStack GetStackForMonitor(NetworkStreamMonitor nsToFind)
        {
            lock (lStacks)
            {
                foreach (TCPStreamMonitorStack sStack in lStacks)
                {
                    foreach (NetworkStreamMonitor nsModifier in sStack.Monitors)
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

        protected abstract NetworkStreamMonitor[] CreateAndLinkStreamMonitors(NetworkStream nsAlice, NetworkStream nsBob);

        protected abstract bool ShouldIntercept(IPAddress ipaSource, IPAddress ipaDestination, int iSourcePort, int iDestinationPort);
    }

    public class TCPStreamMonitorStack
    {
        public TCPIPListenerStack StackAlice { get; private set; }
        public TCPIPListenerStack StackBob { get; private set; }
        public NetworkStreamMonitor[] Monitors { get; private set; }


        public bool SocketsClosed
        {        
            get { return StackAlice.TCPSocket.TCPState == TCPSocketState.Closed && StackBob.TCPSocket.TCPState == TCPSocketState.Closed; }
        }

        public bool AllMonitorsClosed
        {
            get
            {
                foreach (NetworkStreamMonitor nsMonitor in Monitors)
                {
                    if (nsMonitor.IsRunning)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public bool IsClosed
        {
            get { return SocketsClosed && AllMonitorsClosed; }
        }

        public TCPStreamMonitorStack(TCPIPListenerStack stAlice, TCPIPListenerStack stBob, NetworkStreamMonitor[] mods)
        {
            StackAlice = stAlice;
            StackBob = stBob;
            Monitors = mods;
        }

        public override string ToString()
        {
            return "R: " + StackAlice.RemoteBinding.ToString() + " L: " + StackAlice.LocalBinding.ToString()
                + " <> " + "R: " + StackBob.RemoteBinding.ToString() + " L: " + StackBob.LocalBinding.ToString();
        }
    }

    public class TCPStreamMonitorEventArgs : EventArgs
    {
        public TCPStreamMonitorStack Stack { get; private set; }

        public TCPStreamMonitorEventArgs(TCPStreamMonitorStack tsmStack)
        {
            Stack = tsmStack;
        }
    }

    public class TCPStreamMonitorException : Exception
    {
        public TCPStreamMonitorException(string strMessage, Exception exInnerException) : base(strMessage, exInnerException) { }
    }
}
