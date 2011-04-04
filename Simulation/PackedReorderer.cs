using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace eExNetworkLibrary.Simulation
{
    /// <summary>
    /// This simulator item class is capable of randomizing the sequence of frames.
    /// </summary>
    public class PacketReorderer : TrafficSimulatorModificationItem
    {
        private Thread tWorker;
        private Random rRandom;
        private bool bRun;
        private List<Frame> lFrames;
        private int iAccumulationTime;
        private AutoResetEvent areAccumulationTimerSet;

        /// <summary>
        /// Gets or sets the accumulation time. This value describes how many milliseconds 
        /// this instance should wait for packets before shuffling them.
        /// </summary>
        public int AccumulationTime
        {
            get { return iAccumulationTime; }
            set 
            {
                iAccumulationTime = value;
                if (iAccumulationTime < 10)
                {
                    if (iAccumulationTime <= 0)
                    {
                        iAccumulationTime = 0;
                    }
                    else
                    {
                        iAccumulationTime = 10;
                    }
                }
                areAccumulationTimerSet.Set();
                InvokePropertyChanged();
            }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public PacketReorderer()
        {
            iAccumulationTime = 0;
            lFrames = new List<Frame>();
            rRandom = new Random();
            areAccumulationTimerSet = new AutoResetEvent(false);
        }

        /// <summary>
        /// Applies the effect of this simulator chain item to the given frame.
        /// </summary>
        /// <param name="f">The input frame</param>
        public override void Push(Frame f)
        {
            if (iAccumulationTime != 0)
            {
                lock (lFrames)
                {
                    lFrames.Add(f);
                }
            }
            else
            {
                this.Next.Push(f);
            }
        }

        private void Run()
        {
            while (bRun)
            {
                if (iAccumulationTime == 0 && lFrames.Count < 1)
                {
                    areAccumulationTimerSet.WaitOne();
                }
                Thread.Sleep(iAccumulationTime);
                DoFrameShuffle();
            }
        }

        private void DoFrameShuffle()
        {
            int iIndex;
            lock (lFrames)
            {
                while (lFrames.Count > 0)
                {
                    iIndex = rRandom.Next(0, lFrames.Count);
                    this.Next.Push(lFrames[iIndex]);
                    lFrames.RemoveAt(iIndex);
                }
            }
        }

        /// <summary>
        /// Starts this packet reorderer
        /// </summary>
        public override void Start()
        {
            if (tWorker == null)
            {
                bRun = true;
                tWorker = new Thread(Run);
                tWorker.Start();
            }
        }

        /// <summary>
        /// Stops this packet reorderer
        /// </summary>
        public override void Stop()
        {
            if (tWorker != null)
            {
                bRun = false;
                areAccumulationTimerSet.Set();
                tWorker.Join();
                tWorker = null;
                lFrames.Clear();
            }
        }
    }
}
