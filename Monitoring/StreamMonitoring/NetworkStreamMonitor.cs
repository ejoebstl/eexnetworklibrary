using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using eExNetworkLibrary.Sockets;

namespace eExNetworkLibrary.Monitoring.StreamMonitoring
{
    public abstract class NetworkStreamMonitor : RunningObject
    {
        Thread tWorker;
        NetworkStream nsInput;

        public event eExNetworkLibrary.TrafficHandler.ExceptionEventHandler LoopError;
        public event EventHandler LoopClosed;

        protected NetworkStream InputStream
        {
            get { return nsInput; }
        }

        public abstract string Description { get; }

        public NetworkStreamMonitor(NetworkStream nsInput)
        {
            this.nsInput = nsInput;
        }

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
            InvokeExternal(LoopClosed);
            bIsRunning = false;
        }


        /// <summary>
        /// When overriden by a derived class, this method should read from the input stream 
        /// and parse the data.
        /// </summary>
        protected abstract void Run();

        public override void Pause()
        {
            throw new InvalidOperationException("Pausing a stream monitor is not supported.");
        }

        public override void Dispose()
        {
            Stop();
        }

        public override void Stop()
        {
            if (bSouldRun)
            {
                bSouldRun = false;
                nsInput.Close();
                tWorker.Join();
                nsInput = null;
            }
        }

        public void StopAsync()
        {
            if (bSouldRun)
            {
                nsInput.Close();
            }
        }

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
