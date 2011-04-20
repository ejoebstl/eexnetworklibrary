using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using eExNetworkLibrary.Sockets;

namespace eExNetworkLibrary.TrafficModifiers.StreamModification
{
    public abstract class NetworkStreamModifier : RunningObject
    {
        Thread tWorkerThreadAlice;
        Thread tWorkerThreadBob;
        NetworkStream nsStreamAlice;
        NetworkStream nsStreamBob;

        public event eExNetworkLibrary.TrafficHandler.ExceptionEventHandler AliceLoopError;
        public event EventHandler AliceLoopClosed;
        public event eExNetworkLibrary.TrafficHandler.ExceptionEventHandler BobLoopError;
        public event EventHandler BobLoopClosed;

        bool bAliceStopped;
        bool bBobStopped;

        protected NetworkStream StreamAlice
        {
            get { return nsStreamAlice; }
        }

        protected NetworkStream StreamBob
        {
            get { return nsStreamBob; }
        }

        public abstract string Description { get; }

        public NetworkStreamModifier(NetworkStream nsStreamAlice, NetworkStream nsStreamBob)
        {
            this.nsStreamAlice = nsStreamAlice;
            this.nsStreamBob = nsStreamBob;
        }

        public override void Start()
        {
            CheckDisposed();
            if (!bSouldRun)
            {
                bSouldRun = true;
                tWorkerThreadAlice = new Thread(RunAliceWrapper);
                tWorkerThreadAlice.Name = "Network Stream Modifier Alice Worker (" + this.GetType().Name + ")";
                tWorkerThreadBob = new Thread(RunBobWrapper);
                tWorkerThreadBob.Name = "Network Stream Modifier Bob Worker (" + this.GetType().Name + ")";

                tWorkerThreadAlice.Start();
                tWorkerThreadBob.Start();
                bIsRunning = true;
            }
        }

        private void RunAliceWrapper()
        {
            bAliceStopped = false;
            try
            {
                RunAlice();
            }
            catch (Exception ex)
            {
                InvokeExternalAsync(AliceLoopError, new ExceptionEventArgs(ex, DateTime.Now));
            }
            bAliceStopped = true;
            bIsRunning = bBobStopped || bAliceStopped;
            InvokeExternalAsync(AliceLoopClosed);
        }

        private void RunBobWrapper()
        {
            bBobStopped = false;
            try
            {
                RunBob();
            }
            catch (Exception ex)
            {
                InvokeExternalAsync(BobLoopError, new ExceptionEventArgs(ex, DateTime.Now));
            }
            bBobStopped = true;
            bIsRunning = bBobStopped || bAliceStopped;
            InvokeExternalAsync(BobLoopClosed);
        }

        /// <summary>
        /// When overriden by a derived class, this method should read from alice's stream and write to bob's stream. 
        /// the data on the stream can safely be modified. 
        /// </summary>
        protected abstract void RunAlice();

        /// <summary>
        /// When overriden by a derived class, this method should read from bob's stream and write to alice's stream. 
        /// the data on the stream can safely be modified. 
        /// </summary>
        protected abstract void RunBob();

        public override void Stop()
        {
            if (bSouldRun)
            {
                bSouldRun = false;
                nsStreamAlice.Close();
                nsStreamBob.Close();
                tWorkerThreadAlice.Join();
                tWorkerThreadBob.Join();
            }
        }

        public void StopAsync()
        {
            if (bSouldRun)
            {
                bSouldRun = false;
                nsStreamAlice.Close();
                nsStreamBob.Close();
            }
        }

        public override void Pause()
        {
            throw new InvalidOperationException("Pausing a stream operator is not supported.");
        }

        public override void Dispose()
        {
            Stop();
        }

        private void CheckDisposed()
        {
            if (nsStreamBob == null || nsStreamAlice == null)
            {
                throw new ObjectDisposedException("NetworkStreamModifier");
            }
        }

        ~NetworkStreamModifier()
        {
            Dispose();
        }

        #region Debug

        public long AliceOutputBytes { get; protected set; }
        public long AliceInputBytes { get; protected set; }
        public long BobOutputBytes { get; protected set; }
        public long BobInputBytes { get; protected set; }

        #endregion
    }

    public class NetworkStreamModifierException : Exception
    {
        public NetworkStreamModifierException(string strMessage) : base(strMessage) { }
        public NetworkStreamModifierException(string strMessage, Exception exInnerException) : base(strMessage, exInnerException) { }
    }
}
