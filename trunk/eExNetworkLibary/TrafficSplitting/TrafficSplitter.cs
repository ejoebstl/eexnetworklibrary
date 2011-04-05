using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.CommonTrafficAnalysis;

namespace eExNetworkLibrary.TrafficSplitting
{
    /// <summary>
    /// This class is capable of cloning frames for further forwarding and for analyzing
    /// </summary>
    public class TrafficSplitter : TrafficHandler
    {
        private List<TrafficAnalyzer> lTrafficAnalyzers;

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        public TrafficSplitter()
        {
            lTrafficAnalyzers = new List<TrafficAnalyzer>();
        }

        /// <summary>
        /// Checks whether a specific traffic analyzer is attached to this traffic splitter
        /// </summary>
        /// <param name="taAnalyzer">The traffic analyzer to search for</param>
        /// <returns>A bool indicating whether the given analyzer is attached to this handler</returns>
        public bool ContainsTrafficAnalyzer(TrafficAnalyzer taAnalyzer)
        {
            return lTrafficAnalyzers.Contains(taAnalyzer);
        }
        
        /// <summary>
        /// Attachs a specific traffic analyzer to this traffic splitter
        /// </summary>
        /// <param name="taAnalyzer">The traffic analyzer to attach</param>
        public void AddTrafficAnalyzer(TrafficAnalyzer taAnalyzer)
        {
            lTrafficAnalyzers.Add(taAnalyzer);
        }

        /// <summary>
        /// Detaches a specific traffic analyzer from this traffic splitter
        /// </summary>
        /// <param name="taAnalyzer">The traffic analyzer to detach</param>
        public void RemoveTrafficAnalyzer(TrafficAnalyzer taAnalyzer)
        {
            lTrafficAnalyzers.Remove(taAnalyzer);
        }

        /// <summary>
        /// Returns all attavhrf traffic analyzers
        /// </summary>
        /// <returns></returns>
        public TrafficAnalyzer[] GetTrafficAnalyzers()
        {
            return lTrafficAnalyzers.ToArray();
        }

        /// <summary>
        /// Forwards the given frame
        /// </summary>
        /// <param name="fInputFrame">The frame to forward</param>
        protected override void HandleTraffic(Frame fInputFrame)
        {
            NotifyNext(fInputFrame);
        }

        /// <summary>
        /// Forwards the given frame to all attached traffic analyzers and to the default output handler
        /// </summary>
        /// <param name="fInputFrame">The frame to forward</param>
        protected override void NotifyNext(Frame fInputFrame)
        {
            if (lTrafficAnalyzers.Count > 0)
            {
                Frame fClonedFrame = fInputFrame.Clone();
                foreach (TrafficAnalyzer ta in lTrafficAnalyzers)
                {
                    ta.PushTraffic(fClonedFrame);
                }
            }

            if (this.OutputHandler != null) //For the modify handler
            {
                this.OutputHandler.PushTraffic(fInputFrame);
            }

            InvokeFrameForwarded();
        }

        /// <summary>
        /// Does nothing
        /// </summary>
        public override void Cleanup()
        {
            //Nothing needed
        }
    }
}

