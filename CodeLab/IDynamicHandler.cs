using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary.CodeLab
{
    /// <summary>
    /// This interface provides an interface for dynamic handlers which are used by the dynamic function compiler
    /// </summary>
    public interface IDynamicHandler
    {
        /// <summary>
        /// Modifies or analyzes the given frame
        /// </summary>
        /// <param name="fInputFrame">The frame to analyze or modify</param>
        /// <returns>The modified frame, or null if the frame should be dropped</returns>
        Frame ModifyTraffic(Frame fInputFrame);

        /// <summary>
        /// Starts the cleanup process
        /// </summary>
        void Cleanup();
        
        /// <summary>
        /// Stops this dynamic handler 
        /// </summary>
        void Stop();

        /// <summary>
        /// Starts this dynamic handler
        /// </summary>
        void Start();
    }
}
