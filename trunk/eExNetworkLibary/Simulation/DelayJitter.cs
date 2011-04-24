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

namespace eExNetworkLibrary.Simulation
{
    /// <summary>
    /// This simulator item class is capable of randomizing the delay of frames.
    /// </summary>
    public class DelayJitter : TrafficSimulatorModificationItem
    {
        private int iMaxDelay;
        private int iMinDelay;
        private Thread tWorker;
        private Random rRandom;
        private bool bRun;

        private List<TimeJitterItem> lJitterItem;

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        public DelayJitter()
        {
            rRandom = new Random();
            lJitterItem = new List<TimeJitterItem>();
        }

        /// <summary>
        /// The maximum frame delay in milliseconds
        /// </summary>
        public int MaxDelay
        {
            get { return iMaxDelay * 10; }
            set 
            {
                if (value / 10 < iMinDelay || value < 0)
                {
                    throw new ArgumentException("Value must be greater or equal than minimal delay and greater then -1.");
                }
                iMaxDelay = (value / 10);
                InvokePropertyChanged();
            }
        }

        /// <summary>
        /// The minimum frame delay in milliseconds
        /// </summary>
        public int MinDelay
        {
            get { return iMinDelay * 10; }
            set 
            {

                if (value / 10 > iMaxDelay || value < 0)
                {
                    throw new ArgumentException("Value must be smaller or equal than maximal delay and greater then -1.");
                }
                iMinDelay = (value / 10);
                InvokePropertyChanged();
            }
        }

        private void Run()
        {
            while (bRun)
            {
                Thread.Sleep(10);
                TimeJitterItem[] artji;
                lock (lJitterItem)
                {
                    artji = lJitterItem.ToArray();
                }
                foreach (TimeJitterItem tji in artji)
                {
                    if (tji != null)
                    {
                        tji.Time--;
                        if (tji.Time <= 0)
                        {
                            lJitterItem.Remove(tji);
                            this.Next.Push(tji.CarrierFrame);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Applies the effect of this simulator chain item to the given frame.
        /// </summary>
        /// <param name="f">The input frame</param>
        public override void Push(Frame f)
        {
            if (iMaxDelay >= 0)
            {
                TimeJitterItem tji = new TimeJitterItem(f, rRandom.Next(iMinDelay, iMaxDelay + 1));
                if (tji.Time >= 0)
                {
                    lock (lJitterItem)
                    {
                        lJitterItem.Add(tji);
                    }
                }
                else
                {
                    this.Next.Push(f);
                }
            }
            else
            {
                this.Next.Push(f);
            }
        }

        /// <summary>
        /// Starts this delay jitter
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
        /// Stops this delay jitter
        /// </summary>
        public override void Stop()
        {
            if (tWorker != null)
            {
                bRun = false;
                tWorker.Join();
                lJitterItem.Clear();
            }
        }
    }

    class TimeJitterItem
    {
        private Frame fCarrierFrame;
        private int iTime;

        public TimeJitterItem(Frame fFrame, int iTime)
        {
            this.fCarrierFrame = fFrame;
            this.iTime = iTime;
        }

        public Frame CarrierFrame
        {
            get { return fCarrierFrame; }
        }

        public int Time
        {
            get { return iTime; }
            set { iTime = value; }
        }
    }
}
