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
    /// This class is capable of constraining the speed available on a virtual link. 
    /// </summary>
    public class SpeedConstrainer : TrafficSimulatorModificationItem
    {
        private Thread tWorker;
        private bool bRun;
        private int iTrafficCredit;
        private int iSpeed;
        private int iBufferSize;
        private int iMaxBufferSize; 

        private Queue<Frame> qFrames;

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        public SpeedConstrainer()
        {
            iSpeed = 4096;
            iMaxBufferSize = Int32.MaxValue;
            iBufferSize = 0;
            qFrames = new Queue<Frame>();
        }

        /// <summary>
        /// Gets or sets the maximum speed in kilobytes per second
        /// </summary>
        public int Speed
        {
            get { return iSpeed; }
            set
            {
                iSpeed = value;
                InvokePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the maximum buffer size in byte. 
        /// If the buffer is full, tail dropping will occour.
        /// </summary>
        public int BufferSize
        {
            get { return iMaxBufferSize; }
            set
            {
                iMaxBufferSize = value;
                InvokePropertyChanged();
            }
        }

        /// <summary>
        /// Applies the effect of this simulator chain item to the given frame.
        /// </summary>
        /// <param name="f">The input frame</param>
        public override void Push(Frame f)
        {
            lock (qFrames)
            {
                if (iMaxBufferSize > iBufferSize)
                {
                    qFrames.Enqueue(f);
                    iBufferSize += f.Length;
                }
            }

            DoQueueWork();
        }

        private void Run()
        {
            while (bRun)
            {
                lock (qFrames)
                {
                    if (qFrames.Count > 0)
                    {
                        iTrafficCredit += iSpeed;
                    }
                    else
                    {
                        iTrafficCredit = iSpeed;
                    }
                }
                DoQueueWork();
                Thread.Sleep(10);
            }
        }

        private void DoQueueWork()
        {
            lock (qFrames)
            {
                while (qFrames.Count > 0 && iTrafficCredit > qFrames.Peek().Length)
                {
                    Frame fDequeue = qFrames.Dequeue();
                    this.Next.Push(fDequeue);
                    int iLen = fDequeue.Length;
                    iTrafficCredit -= iLen;
                    iBufferSize -= iLen;
                }
            }
        }

        /// <summary>
        /// Starts this simulator item.
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
        /// Stops this simulator item.
        /// </summary>
        public override void Stop()
        {
            if (tWorker != null)
            {
                bRun = false;
                tWorker.Join();
                qFrames.Clear();
            }
        }
    }
}
