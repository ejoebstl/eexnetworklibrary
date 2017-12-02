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
using System.Threading;
using eExNetworkLibrary.Sockets;

namespace eExNetworkLibrary.Monitoring.StreamMonitoring
{
    /// <summary>
    /// This class provides a base for network stream monitoring.<br /> 
    /// If you implement your own base class, you have to also implement a class which inherits
    /// eExNetworkLibrary.Monitoring.TCPStreamMonitor and to submit your base class in the 
    /// CreateAndLinkStreamMonitors(NetworkStream nsAlice, NetworkStream nsBob) method.
    /// </summary>
    public abstract class NetworkStreamMonitor : RunningObject
    {
        Thread tWorker;
        NetworkStream nsInput;

        /// <summary>
        /// This event is fired when a loop terminates due to an error.
        /// </summary>
        public event eExNetworkLibrary.TrafficHandler.ExceptionEventHandler LoopError;
        /// <summary>
        /// This event is fired when a loop terminates.
        /// </summary>
        public event EventHandler LoopClosed;

        /// <summary>
        /// Gets the input stream of this stream monitor.
        /// </summary>
        protected NetworkStream InputStream
        {
            get { return nsInput; }
        }

        /// <summary>
        /// When overriden by a derived class, must return a description of the stream monitor. 
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// Initializes this class.
        /// </summary>
        /// <param name="nsInput">The input stream of this monitor.</param>
        protected NetworkStreamMonitor(NetworkStream nsInput)
        {
            this.nsInput = nsInput;
        }

        /// <summary>
        /// Starts the monitor's working thread.
        /// </summary>
        public override void Start()
        {
            CheckDisposed();
            if (!bSouldRun)
            {
                bSouldRun = true;
                tWorker = new Thread(RunWrapper);
                tWorker.Name = "Network Stream Monitor Worker (" + this.GetType().Name + ")";
                tWorker.Start();
                bIsRunning = true;
            }
        }

        private void RunWrapper()
        {
            try
            {
                Run();
            }
            catch (Exception ex)
            {
                InvokeExternal(LoopError, new ExceptionEventArgs(ex, DateTime.Now));
            }
            bIsRunning = false;
            InvokeExternal(LoopClosed);
        }


        /// <summary>
        /// When overriden by a derived class, this method should read from the input stream 
        /// and parse the data.
        /// </summary>
        protected abstract void Run();

        /// <summary>
        /// Throws an InvalidOperationException.
        /// </summary>
        public override void Pause()
        {
            throw new InvalidOperationException("Pausing a stream monitor is not supported.");
        }

        /// <summary>
        /// Calls Stop();
        /// </summary>
        public override void Dispose()
        {
            Stop();
        }

        /// <summary>
        /// Closes the input stream and terminates the worker thread.
        /// </summary>
        public override void Stop()
        {
            if (bSouldRun)
            {
                bSouldRun = false;
                nsInput.Close();
                tWorker.Join();
            }
        }

        /// <summary>
        /// Closes the input stream and terminates the worker thread asynchronously. 
        /// </summary>
        public void StopAsync()
        {
            if (bSouldRun)
            {
                bSouldRun = false;
                nsInput.Close();
            }
        }

        /// <summary>
        /// Closes the input stream and terminates the worker thread.
        /// </summary>
        ~NetworkStreamMonitor()
        {
            Dispose();
        }

        private void CheckDisposed()
        {
            if (nsInput == null)
            {
                throw new ObjectDisposedException("NetworkStreamModifier");
            }
        }
    }
}
