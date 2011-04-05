using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace eExNetworkLibrary.Monitoring
{
    /// <summary>
    /// This traffic handler is capable of measuring the throughput datarate
    /// <remarks>This traffic handler counts all data as it is on the medium (except ethernet preamble and checksum), not only layer 3 or 4 data</remarks>
    /// </summary>
    public class SpeedMeter : TrafficHandler
    {
        Timer t;
        Timer tRealSpeed;
        int iBytesPerSecond;
        int iByteCounter;
        int iRealSpeed;
        int iRealSpeedCounter;

        int iPeakDatarate;
        DateTime dPeakTime;

        /// <summary>
        /// Returns the peak datarate in bits per second
        /// </summary>
        public int PeakDatarate
        {
            get { return iPeakDatarate * 8; }
        }

        /// <summary>
        /// Returns exactly the count of bits which where transmitted in the last second
        /// </summary>
        public int RealSpeed
        {
            get { return iRealSpeed * 8; }
        }

        /// <summary>
        /// Returns the time when the peak data rate occoured
        /// </summary>
        public DateTime PeakTime
        {
            get { return dPeakTime; }
        }

        /// <summary>
        /// Returns the estamined measured datarate in bits per second. This value is updated every 200 milliseconds.
        /// </summary>
        public int Speed
        {
            get { return iByteCounter * 40; }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public SpeedMeter()
        {
            t = new Timer(200);
            t.AutoReset = true;
            t.Elapsed += new ElapsedEventHandler(t_Elapsed);
            tRealSpeed = new Timer(1000);
            tRealSpeed.AutoReset = true;
            tRealSpeed.Elapsed += new ElapsedEventHandler(tRealSpeed_Elapsed);
        }

        void tRealSpeed_Elapsed(object sender, ElapsedEventArgs e)
        {
            iRealSpeed = iRealSpeedCounter;
            iRealSpeedCounter = 0;
            if (iRealSpeed >= iPeakDatarate)
            {
                iPeakDatarate = iRealSpeed;
                dPeakTime = DateTime.Now;
            }
        }

        /// <summary>
        /// Starts this speed meter
        /// </summary>
        public override void Start()
        {
            t.Start();
            tRealSpeed.Start();
            base.Start();
        }

        /// <summary>
        /// Stops this speed meter
        /// </summary>
        public override void Stop()
        {
            t.Stop();
            tRealSpeed.Stop();
            base.Stop();
        } 

        void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            iByteCounter = iBytesPerSecond;
            iBytesPerSecond = 0;
        }

        /// <summary>
        /// Does nothing
        /// </summary>
        public override void Cleanup()
        {
            //Do nothing
        }

        /// <summary>
        /// Counts this frame's bytes
        /// </summary>
        /// <param name="fInputFrame">The frame to count</param>
        protected override void HandleTraffic(Frame fInputFrame)
        {
            int iLen = fInputFrame.Length;
            iBytesPerSecond += iLen;
            iRealSpeedCounter += iLen;
            NotifyNext(fInputFrame);
        }
    }
}
