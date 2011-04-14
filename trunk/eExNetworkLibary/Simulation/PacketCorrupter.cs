using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary.Simulation
{
    /// <summary>
    /// This class represents an abstract implementation of a packet corrupting simulator chain item where corruption occours based on a given probability and on a random error count between two given bounds.
    /// </summary>
    public abstract class PacketCorrupter : RandomEventTrafficSimulatorItem
    {
        /// <summary>
        /// This varaible represents the minimum error count for the random chosen frame.
        /// </summary>
        protected int iMinErrorCount;
        /// <summary>
        /// This varaible represents the maximal error count for the random chosen frame.
        /// </summary>
        protected int iMaxErrorCount;
        private Random rRandom;

        /// <summary>
        /// Sets the minimum errors for the random chosen frame.
        /// </summary>
        public int MinErrorCount
        {
            get { return iMinErrorCount; }
            set
            {
                if (value > iMaxErrorCount)
                {
                    throw new ArgumentException("Value must be less or equal than maximal error count.");
                }
                iMinErrorCount = value;
                InvokePropertyChanged();
            }
        }

        /// <summary>
        /// Sets the maximum errors for the random chosen frame.
        /// </summary>
        public int MaxErrorCount
        {
            get { return iMaxErrorCount; }
            set 
            {
                if (value < iMinErrorCount)
                {
                    throw new ArgumentException("Value must be greater or equal than minimal error count.");
                }
                iMaxErrorCount = value;
                InvokePropertyChanged();
            }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public PacketCorrupter()
        {
            rRandom = new Random();
        }

        /// <summary>
        /// Corrupts the frame
        /// </summary>
        /// <param name="f">The frame to corrupt</param>
        protected override void CaseHappening(Frame f)
        {
            if (iMaxErrorCount > 0)
            {
                while (f != null && FrameTypes.IsIP(f))
                {
                    f = f.EncapsulatedFrame;
                }

                if (f != null && FrameTypes.IsIP(f))
                {
                    f.EncapsulatedFrame = new RawDataFrame(DoErrors(f.EncapsulatedFrame.FrameBytes));
                }
            }

            this.Next.Push(f);
        }

        /// <summary>
        /// This method is called to do the errors
        /// </summary>
        /// <param name="bData">The data to corrupt</param>
        /// <returns>The corrupted data</returns>
        protected abstract byte[] DoErrors(byte[] bData);

        /// <summary>
        /// Forwards the frame
        /// </summary>
        /// <param name="f">The frame to forward</param>
        protected override void CaseNotHappening(Frame f)
        {
            // Simply forward
            this.Next.Push(f);
        }
    }
}
