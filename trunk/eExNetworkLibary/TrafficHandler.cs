using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using eExNetworkLibrary.IP;
using eExNetworkLibrary.CommonTrafficAnalysis;
using System.ComponentModel;
using eExNetworkLibrary.Threading;
using eExNetworkLibrary.IP.V6;
using eExNetworkLibrary.ProtocolParsing;

namespace eExNetworkLibrary
{
    /// <summary>
    /// This class represents a Traffic Handler, a basic component for traffic analyzing and modifying.
    /// All traffic analyzers, modifiers and interfaces must derive from this class.
    /// <remarks>
    /// <b>Threading issues</b>
    /// This class owns one worker thread which calls HandleTraffic and all methods which are invoked by calls to the ISynchronizeInvoke interface. 
    /// Thread safety for all objects which can be accessed from the outside has to be ensured. 
    /// Invoking methods over ISynchronizeInvoke to prevent cross thread operations is not forced by this class. 
    /// </remarks>
    /// </summary>
    public abstract class TrafficHandler : RunningObject, ISynchronizeInvoke
    {
        private Thread tWorker;
        private List<TrafficAnalyzer> lDroppedTrafficAnalyzer;
        private Queue<TrafficHandlerWorkItem> qwiWorkItems;
        private AutoResetEvent areWorkToDo;

        /// <summary>
        /// Gets or sets the protocol parser of this traffic handler. By changing it, it is possible to change the way the traffic handler parses protocols.
        /// </summary>
        public ProtocolParser ProtocolParser { get; set; }

        /// <summary>
        /// This traffic handlers default output handler. All forwarded frames will be pushed to this handlers queue. 
        /// </summary>
        protected TrafficHandler thNextHandler;

        /// <summary>
        /// This event will be fired whenever a property which is not associated to a special event is changed.
        /// </summary>
        public event EventHandler PropertyChanged;

        /// <summary>
        /// This event will be fired when a frame is forwarded to the next traffic handler.
        /// </summary>
        public event EventHandler FrameForwarded;
        /// <summary>
        /// This event will be fired when a frame is dropped.
        /// </summary>
        public event EventHandler FrameDropped;
        /// <summary>
        /// This event will be fired when a frame is received.
        /// </summary>
        public event EventHandler FrameReceived;
        /// <summary>
        /// This event will be fired when an internal processing error occours. 
        /// </summary>
        public event ExceptionEventHandler ExceptionThrown;

        /// <summary>
        /// Gets or sets this traffic handlers name. 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A delegate for exception handling.
        /// </summary>
        /// <param name="sender">The class which fired this event</param>
        /// <param name="args">The exception arguments</param>
        public delegate void ExceptionEventHandler(object sender, ExceptionEventArgs args);

        #region DropAnalyzer

        /// <summary>
        /// This method returns whether a traffic analyzer is contained in this traffic handlers drop analyzer list
        /// </summary>
        /// <param name="taAnalyzer">A traffic analyzer</param>
        /// <returns>A bool indicating whether a traffic analyzer is contained in this traffic handlers drop analyzer list</returns>
        public bool ContainsDroppedTrafficAnalyzer(TrafficAnalyzer taAnalyzer)
        {
            return lDroppedTrafficAnalyzer.Contains(taAnalyzer);
        }

        /// <summary>
        /// This method adds a traffic analyzer to this handlers drop analyzer list. All dropped frames will be forwarded to this traffic analyzer.
        /// </summary>
        /// <param name="taAnalyzer">The traffic analyzer to add</param>
        public void AddDroppedTrafficAnalyzer(TrafficAnalyzer taAnalyzer)
        {
            lDroppedTrafficAnalyzer.Add(taAnalyzer);
        }

        /// <summary>
        /// This method removes a traffic analyzer from this handlers drop analyzer list.
        /// </summary>
        /// <param name="taAnalyzer">The traffic analyzer to remove</param>
        public void RemoveDroppedTrafficAnalyzer(TrafficAnalyzer taAnalyzer)
        {
            lDroppedTrafficAnalyzer.Remove(taAnalyzer);
        }

        /// <summary>
        /// This method returns all drop analyzers associated with this traffic handler.
        /// </summary>
        /// <returns>An array containing ll drop analyzers associated with this traffic handler</returns>
        public TrafficAnalyzer[] GetDroppedTrafficAnalyzers()
        {
            return lDroppedTrafficAnalyzer.ToArray();
        }

        #endregion

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public TrafficHandler()
        {
            Name = this.GetType().Name;
            areWorkToDo = new AutoResetEvent(false);
            qwiWorkItems = new Queue<TrafficHandlerWorkItem>();
            lDroppedTrafficAnalyzer = new List<TrafficAnalyzer>();
            bIsRunning = false;
            ProtocolParser = new ProtocolParser();
        }

        /// <summary>
        /// Gets or sets this traffic handlers default output handler. All forwarded frames will be pushed to this handlers queue. 
        /// </summary>
        public virtual TrafficHandler OutputHandler
        {
            get { return thNextHandler; }
            set { thNextHandler = value; }
        }

        /// <summary>
        /// This method starts this traffic handlers worker threads.
        /// This method must be called to make this traffic handler do its work.
        /// </summary>
        public override void Start()
        {
            if (!bSouldRun)
            {
                bSouldRun = true;
                tWorker = new Thread(MainWorkingLoop);
                tWorker.Start();
                tWorker.Name = "Traffic handler worker thread (" + Name + ")";
                bIsRunning = true;
            }
        }

        /// <summary>
        /// Gets the IPv4 frame from an abstract frame or returns null in case no IPv4 frame exists.
        /// </summary>
        /// <param name="fInputFrame">The abstract input frame</param>
        /// <returns>An IPv4 frame</returns>
        protected IP.IPv4Frame GetIPv4Frame(Frame fInputFrame)
        {
            return (IPv4Frame)GetFrameByType(fInputFrame, FrameTypes.IPv4);
        }

        /// <summary>
        /// Gets the IPv6 frame from an abstract frame or returns null in case no IPv4 frame exists.
        /// </summary>
        /// <param name="fInputFrame">The abstract input frame</param>
        /// <returns>An IPv6 frame</returns>
        protected IPv6Frame GetIPv6Frame(Frame fInputFrame)
        {
            return (IPv6Frame)GetFrameByType(fInputFrame, FrameTypes.IPv6);
        }    
        
        /// <summary>
        /// Gets the IP frame from an abstract frame or returns null in case no IPv4 frame exists.
        /// </summary>
        /// <param name="fInputFrame">The abstract input frame</param>
        /// <returns>An IP frame</returns>
        protected IP.IPFrame GetIPFrame(Frame fInputFrame)
        {
            IPFrame fFrame = GetIPv4Frame(fInputFrame);
            if (fFrame == null)
                fFrame = GetIPv6Frame(fInputFrame);
            return fFrame;
        }

        /// <summary>
        /// Gets the a frame specified by its type from an abstract frame or returns null in case no frame with this type exists.
        /// </summary>
        /// <param name="fInputFrame">The abstract input frame</param>
        /// <param name="fFrameType">The frame type to search for</param>
        /// <returns>The parsed frame or null, if the frame did not contain a frame with the specified type.</returns>
        protected Frame GetFrameByType(Frame fInputFrame, string strFrameType)
        {
            return ProtocolParser.GetFrameByType(fInputFrame, strFrameType);
        }


        /// <summary>
        /// Gets the a frame specified by its type from an abstract frame or returns null in case no frame with this type exists.
        /// </summary>
        /// <param name="fInputFrame">The abstract input frame</param>
        /// <param name="fFrameType">The frame type to search for</param>
        /// <param name="bReturnRawData">A bool indicating whether raw data frames can be returned, if the protocol is known but no protocol provider is available.</param>
        /// <returns>The parsed frame, a raw data frame with the searched frame's data or null, if the frame did not contain a frame with the specified type.</returns>
        protected Frame GetFrameByType(Frame fInputFrame, string strFrameType, bool bReturnRawData)
        {
            return ProtocolParser.GetFrameByType(fInputFrame, strFrameType, bReturnRawData);
        }

        /// <summary>
        /// This method starts the handlers cleanup process which will release network resources or remote allocated resources. It should be called before stopping the handler to ensure a clean shutdown.
        /// </summary>
        public abstract void Cleanup();

        /// <summary>
        /// Gets a bool indicating whether the input queue of this handler is empty.
        /// </summary>
        public bool QueueEmpty
        {
            get { lock (qwiWorkItems) { return qwiWorkItems.Count == 0; } }
        }

        /// <summary>
        /// Gets the arp frame from an abstract frame or returns null in case no arp frame exists.
        /// </summary>
        /// <param name="fInputFrame">The abstract input frame</param>
        /// <returns>An arp frame</returns>
        protected ARP.ARPFrame GetARPFrame(Frame fInputFrame)
        {
            return (ARP.ARPFrame)GetFrameByType(fInputFrame, FrameTypes.ARP);
        }

        /// <summary>
        /// Gets the ethernet frame from an abstract frame or returns null in case no ethernet frame exists.
        /// </summary>
        /// <param name="fInputFrame">The abstract input frame</param>
        /// <returns>An ethernet frame</returns>
        protected Ethernet.EthernetFrame GetEthernetFrame(Frame fInputFrame)
        {
            return (Ethernet.EthernetFrame)GetFrameByType(fInputFrame, FrameTypes.Ethernet);
        }

        /// <summary>
        /// Gets the TCP frame from an abstract frame or returns null in case no TCP frame exists.
        /// </summary>
        /// <param name="fInputFrame">The abstract input frame</param>
        /// <returns>An TCP frame</returns>
        protected TCP.TCPFrame GetTCPFrame(Frame fInputFrame)
        {
            return (TCP.TCPFrame)GetFrameByType(fInputFrame, FrameTypes.TCP);
        }

        /// <summary>
        /// Gets the UDP frame from an abstract frame or returns null in case no UDP frame exists.
        /// </summary>
        /// <param name="fInputFrame">The abstract input frame</param>
        /// <returns>An UDP frame</returns>
        protected UDP.UDPFrame GetUDPFrame(Frame fInputFrame)
        {
            return (UDP.UDPFrame)GetFrameByType(fInputFrame, FrameTypes.UDP);
        }


        /// <summary>
        /// Pushes a frame in this handler input-queue. 
        /// </summary>
        /// <param name="fInputFrame">The frame to handle</param>
        public void PushTraffic(Frame fInputFrame)
        {
            if (!bSouldRun)
            {
                throw new InvalidOperationException("Cannot receive pushed traffic. Worker thread is currently stopped.");
            }
            lock (qwiWorkItems)
            {
                qwiWorkItems.Enqueue(new TrafficHandlerWorkItem(fInputFrame));
                areWorkToDo.Set();
            }
            InvokeFrameReceived();
        }

        /// <summary>
        /// Stops this handlers worker threads.
        /// </summary>
        public override void Stop()
        {
            if (bSouldRun)
            {
                bSouldRun = false;
                areWorkToDo.Set();
                tWorker.Join();
                tWorker = null;
                bIsRunning = false;
            }
        }

        /// <summary>
        /// This task will be executed for every frame in the input queue
        /// Per default, it simply calls the HandleTraffic method and does some exception handling.
        /// </summary>
        /// <param name="fInputFrame">The frame to process</param>
        protected void MainWorkingLoopTask(Frame fInputFrame)
        {
            try
            {
                HandleTraffic(fInputFrame);
            }
            catch (Exception ex)
            {
                if (bSouldRun)
                {
                    if (ex.InnerException != null)
                    {
                        InvokeExternalAsync(ExceptionThrown, new ExceptionEventArgs(ex.InnerException, DateTime.Now));
                    }
                    else
                    {
                        InvokeExternalAsync(ExceptionThrown, new ExceptionEventArgs(ex, DateTime.Now));
                    }
                }
            }
        }

        /// <summary>
        /// This method is called for every frame in the input queue per default. It should be used to process received traffic.
        /// </summary>
        /// <param name="fInputFrame">The frame to process</param>
        protected abstract void HandleTraffic(Frame fInputFrame);

        /// <summary>
        /// This method can be used to pause the traffic handler.
        /// <remarks>Per default this method simply calls stop.s</remarks>
        /// </summary>
        public override void Pause()
        {
            Stop();
        }

        /// <summary>
        /// This method is used to forward a frame to the output handler of this traffic handler and invokes the FrameForwarded event.
        /// </summary>
        /// <param name="fInputFrame">The frame to forward</param>
        protected virtual void NotifyNext(Frame fInputFrame)
        {
            if (thNextHandler != null)
            {
                thNextHandler.PushTraffic(fInputFrame);
                InvokeFrameForwarded();
            }
        }

        /// <summary>
        /// Raises the FrameDropped event.
        /// </summary>
        protected void InvokeFrameDropped()
        {
            InvokeExternalAsync(FrameDropped);
        }

        /// <summary>
        /// Rises the ExceptionThrown event with the given params
        /// </summary>
        /// <param name="ex">The exception which occoured</param>
        protected void InvokeExceptionThrown(Exception ex)
        {
            InvokeExternalAsync(ExceptionThrown, new ExceptionEventArgs(ex, DateTime.Now));
        }

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        protected void InvokePropertyChanged()
        {
            InvokeExternalAsync(PropertyChanged);
        }

        /// <summary>
        /// Raises the FrameForwarded event.
        /// </summary>
        protected void InvokeFrameForwarded()
        {
            InvokeExternalAsync(FrameForwarded);
        }

        /// <summary>
        /// Raises the FrameReceived event.
        /// </summary>
        protected void InvokeFrameReceived()
        {
            InvokeExternalAsync(FrameReceived);
        }

        /// <summary>
        /// Forwards a dropped frame to all connected drop analyzers and invokes the FrameDropped event.
        /// </summary>
        /// <param name="fFrame">The frame to forward.</param>
        protected void PushDroppedFrame(Frame fFrame)
        {
            foreach (TrafficAnalyzer ta in lDroppedTrafficAnalyzer)
            {
                ta.PushTraffic(fFrame);
            }
            InvokeFrameDropped();
        }

        #region ISynchronizeInvoke Members

        /// <summary>
        /// Invokes a delegate asyncronously in this handlers thread context.
        /// </summary>
        /// <param name="method">The method to invoke</param>
        /// <param name="args">The params for the invokation</param>
        /// <returns>A IAsyncResult associated with the invocation process</returns>
        public IAsyncResult BeginInvoke(Delegate method, object[] args)
        {
            if (!bSouldRun)
            {
                throw new InvalidOperationException("Cannot begin an invoke. Worker thread is currently stopped.");
            }
            lock (qwiWorkItems)
            {
                WorkItem wiItem = new WorkItem(null, method, args);
                qwiWorkItems.Enqueue(new TrafficHandlerWorkItem(wiItem));
                areWorkToDo.Set();
                return wiItem;
            }
        }

        /// <summary>
        /// Waits for an invocation process to finish
        /// </summary>
        /// <param name="result">The IAsyncResult associated with the invocation process to wait for</param>
        /// <returns>The result of the operation</returns>
        public object EndInvoke(IAsyncResult result)
        {
            if (!bSouldRun)
            {
                throw new InvalidOperationException("Cannot end an invoke. Worker thread is currently stopped.");
            }
            result.AsyncWaitHandle.WaitOne();
            WorkItem wiItem = (WorkItem)result;
            return wiItem.MethodReturnValue;
        }
        /// <summary>
        /// Invokes a delegate syncronously in this handlers thread context.
        /// </summary>
        /// <param name="method">The method to invoke</param>
        /// <param name="args">The params for the invokation</param>
        /// <returns>The result of the operation</returns>
        public object Invoke(Delegate method, object[] args)
        {
            if (!bSouldRun)
            {
                throw new InvalidOperationException("Cannot invoke. Worker thread is currently stopped.");
            }
            return EndInvoke(BeginInvoke(method, args));
        }

        /// <summary>
        /// Determines whether an invoke is required. 
        /// Invoking synchronously if this property returns true is a recommendation.
        /// Simply calling a method without invoking it could lead to unexpected errors.
        /// </summary>
        public bool InvokeRequired
        {
            get { return Thread.CurrentThread != this.tWorker; }
        }

        #endregion

        private void MainWorkingLoop()
        {
            TrafficHandlerWorkItem wiCurrent;
            try
            {
                while (bSouldRun) //Main working loop
                {
                    while (qwiWorkItems.Count > 0) //Work for everything in the queue.
                    {
                        lock (qwiWorkItems)
                        {
                            wiCurrent = qwiWorkItems.Dequeue();
                        }
                        if (wiCurrent != null)
                        {
                            if (wiCurrent.Frame != null)
                            {
                                MainWorkingLoopTask(wiCurrent.Frame);
                            }

                            try
                            {
                                if (wiCurrent.Callback != null)
                                {
                                    wiCurrent.Callback.CallBack();
                                }
                            }
                            catch (Exception ex)
                            {
                                if (bSouldRun)
                                {
                                    if (ex.InnerException != null)
                                    {
                                        InvokeExternalAsync(ExceptionThrown, new ExceptionEventArgs(ex.InnerException, DateTime.Now));
                                    }
                                    else
                                    {
                                        InvokeExternalAsync(ExceptionThrown, new ExceptionEventArgs(ex, DateTime.Now));
                                    }
                                }
                            }
                        }
                    }
                    if (bSouldRun)
                    {
                        areWorkToDo.WaitOne(); //If the queue is finished, wait for new work.
                    }
                }
            }
            finally
            {
                qwiWorkItems.Clear();
            }
        }
    }

    class TrafficHandlerWorkItem
    {
        public WorkItem Callback { get; private set; }
        public Frame Frame { get; private set; }

        public TrafficHandlerWorkItem(WorkItem cCallback, Frame fFrame)
        {
            this.Callback = cCallback;
            this.Frame = fFrame;
        }

        public TrafficHandlerWorkItem(Frame fFrame)
        {
            this.Frame = fFrame;
        }

        public TrafficHandlerWorkItem(WorkItem cCallback)
        {
            this.Callback = cCallback;
        }
    }

    /// <summary>
    /// This class represents EventArgs for exception handling
    /// </summary>
    public class ExceptionEventArgs : EventArgs
    {
        private Exception ex;
        private DateTime dtTime;

        /// <summary>
        /// An exception which was thrown
        /// </summary>
        public Exception Exception
        {
            get { return ex; }
        }

        /// <summary>
        /// The time of the exception happening
        /// </summary>
        public DateTime Time
        {
            get { return dtTime; }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="ex">An exception which was thrown</param>
        /// <param name="dtTime">The time of the excpetion happening</param>
        public ExceptionEventArgs(Exception ex, DateTime dtTime)
        {
            this.ex = ex;
            this.dtTime = dtTime;
        }
    }
}
