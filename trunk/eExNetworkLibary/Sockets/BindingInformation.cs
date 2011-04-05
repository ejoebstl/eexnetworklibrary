using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary.Sockets
{
    /// <summary>
    /// This class represents binding information of a socket
    /// </summary>
    public class BindingInformation
    {
        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="localBinding">The local binding information</param>
        /// <param name="remoteBinding">The remote binding Information</param>
        public BindingInformation(EndPoint localBinding, EndPoint remoteBinding)
        {
            LocalBinding = localBinding;
            RemoteBinding = remoteBinding;
        }

        /// <summary>
        /// The local endpoint of the socket.
        /// </summary>
        public EndPoint LocalBinding { get; protected set; }
        /// <summary>
        /// The remote endpoint of the socket.
        /// </summary>
        public EndPoint RemoteBinding { get; protected set; }

        /// <summary>
        /// Gets the description of this endpoint
        /// </summary>
        /// <returns>The description of this endpoint</returns>
        public override string ToString()
        {
            return "Local: " + LocalBinding.ToString() + ", Remote: " + RemoteBinding.ToString();
        }
    }

    /// <summary>
    /// This class represents an EndPoint of a socket conversation
    /// </summary>
    public class EndPoint
    {
        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="strDescription">The description of this EndPoint</param>
        public EndPoint(string strDescription)
        {
            this.Description = strDescription;
        }

        /// <summary>
        /// Gets the description of this EndPoint
        /// </summary>
        public string Description { get; protected set; }

        /// <summary>
        /// Gets the description of this EndPoint
        /// </summary>
        /// <returns>The description of this EndPoint</returns>
        public override string ToString()
        {
            return Description;
        }
    }
}
