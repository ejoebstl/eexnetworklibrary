using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary
{
    /// <summary>
    /// Represents a small, frame-like helper structure
    /// </summary>
    public abstract class HelperStructure
    {
        /// <summary>
        /// Gets the bytes of this helper structure
        /// </summary>
        public abstract byte[] Bytes { get; }
        /// <summary>
        /// Gets the length of this helper structure
        /// </summary>
        public abstract int Length { get; }
    }
}
