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
using eExNetworkLibrary.ProtocolParsing;

namespace eExNetworkLibrary.Sockets
{
    /// <summary>
    /// This class represents the base for socket implementations. 
    /// This class is designed to be modular and stackable. 
    /// 
    /// Considerations:
    /// 
    /// 
    ///                 [....]         
    ///         *                    *
    ///        \ /                  / \
    ///   **********************************
    ///   * PushDown()    FrameDecapsulated*
    ///   *                                *
    ///   *                                *
    ///   *                                *
    ///   *          Parent Socket         *
    ///   *                                *
    ///   *                                *
    ///   *                                *
    ///   *FrameEncapsulated      PushUp() *
    ///   **********************************
    ///        \ /                  / \
    ///         *                    *
    ///         *                    *
    ///         *                    *
    ///        \ /                  / \
    ///   **********************************
    ///   * PushDown()    FrameDecapsulated*
    ///   *                                *
    ///   *                                *
    ///   *                                *
    ///   *          Child  Socket         *
    ///   *                                *
    ///   *                                *
    ///   *                                *
    ///   *FrameEncapsulated      PushUp() *
    ///   **********************************
    ///        \ /                  / \
    ///         *                    *
    ///                 [....]        
    ///                 
    /// If you use the ChildSocket and ParentSocket properties to assign to sockets to be child and parent, the connections
    /// of event handlers will be done automatically. 
    /// </summary>
    public abstract class SocketBase : ISocket
    {
        ISocket sUnderlyingSocket;
        ISocket sParentSocket;

        ProtocolParser ipProtocolParser;

        /// <summary>
        /// Gets or sets the protocol parser of this socket. By changing it, it is possible to change the way the socket parses protocols.
        /// </summary>
        public virtual ProtocolParser ProtocolParser
        {
            get
            {
                if (ipProtocolParser == null)
                    ipProtocolParser = new ProtocolParser();
                return ipProtocolParser;
            }
            set { ipProtocolParser = value; }
        }

        /// <summary>
        /// A bool indicating whether the socken can receive or send data.
        /// </summary>
        public abstract bool IsOpen { get; }

        /// <summary>
        /// Invokes a delegate on any external object with the given params and waits for the invoke's completion.
        /// This method automatically determines whether dynamic invoking is possible or a invoke over the ISynchronizeInvoke interface is required.
        /// </summary>
        /// <param name="d">The delgate to invoke</param>
        /// <param name="param">The params for the invocation</param>
        protected void InvokeExternal(Delegate d, object param)
        {
            Threading.InvocationHelper.InvokeExternal(d, this, param);
        }

        /// <summary>
        /// Gets or sets the socket which is located under this sockets instance in the socket-stack and automatically registers the FrameDecapsulated event handler to ensure stack functionality. 
        /// </summary>
        public ISocket ChildSocket
        {
            get
            {
                return sUnderlyingSocket;
            }
            set
            {
                if (sUnderlyingSocket != null)
                {
                    sUnderlyingSocket.FrameDecapsulated -= new FrameProcessedEventHandler(sUnderlyingSocket_FrameDecapsulated);
                }
                sUnderlyingSocket = value;
                if (sUnderlyingSocket != null)
                {
                    sUnderlyingSocket.FrameDecapsulated += new FrameProcessedEventHandler(sUnderlyingSocket_FrameDecapsulated);
                }
            }
        }

        void sUnderlyingSocket_FrameProcessed(object sender, FrameProcessedEventArgs args)
        {
            PushUp(args.ProcessedFrame, args.IsPush);
        }


        /// <summary>
        /// Gets or sets the socket which is located over this sockets instance in the socket-stack and automatically registers the FrameEncapsulated event handler to ensure stack functionality. 
        /// </summary>
        public ISocket ParentSocket
        {
            get
            {
                return sParentSocket;
            }
            set
            {
                if (sParentSocket != null)
                {
                    sParentSocket.FrameEncapsulated -= new FrameProcessedEventHandler(sParentSocket_FrameEncapsulated);
                }
                sParentSocket = value; 
                if (sParentSocket != null)
                {
                    sParentSocket.FrameEncapsulated += new FrameProcessedEventHandler(sParentSocket_FrameEncapsulated);
                }
            }
        }

        void sParentSocket_FrameEncapsulated(object sender, FrameProcessedEventArgs args)
        {
            PushDown(args.ProcessedFrame, args.IsPush);
        }

        void sUnderlyingSocket_FrameDecapsulated(object sender, FrameProcessedEventArgs args)
        {
            InputBytes += args.ProcessedFrame.Length;
            PushUp(args.ProcessedFrame, args.IsPush);
        }

        /// <summary>
        /// Invokes a delegate asyncronously on any external object with the given params.
        /// This method automatically determines whether dynamic invoking is possible or a invoke over the ISynchronizeInvoke interface is required.
        /// </summary>
        /// <param name="d">The delgate to invoke</param>
        /// <param name="param">The params for the invocation</param>
        protected void InvokeExternalAsync(Delegate d, object param)
        {
            Threading.InvocationHelper.InvokeExternalAsync(d, param, this);
        }

        /// <summary>
        /// Fires the FrameDecapsulated event asynchronously. 
        /// </summary>
        /// <param name="fFrame">The frame associated with this event.</param>
        protected void InvokeFrameDecapsulated(Frame fFrame)
        {
            InvokeFrameDecapsulated(fFrame, false);
        }

        /// <summary>
        /// Fires the FrameDecapsulated event asynchronously.
        /// </summary>
        /// <param name="fFrame">The frame associated with this event.</param>
        /// <param name="fFrame">A bool indicating whether this frame was delivered with a push flag.</param>
        protected void InvokeFrameDecapsulated(Frame fFrame, bool bPush)
        {
            InvokeExternalAsync(FrameDecapsulated, new FrameProcessedEventArgs(fFrame, bPush));
        }     
        
        /// <summary>
        /// Fires the FrameEncapsulated event asynchronously. 
        /// </summary>
        /// <param name="fFrame">The frame associated with this event.</param>
        protected void InvokeFrameEncapsulated(Frame fFrame)
        {
            InvokeFrameEncapsulated(fFrame, false);
        }

        /// <summary>
        /// Fires the FrameEncapsulated event asynchronously.
        /// </summary>
        /// <param name="fFrame">The frame associated with this event.</param>
        /// <param name="fFrame">A bool indicating whether this frame was delivered with a push flag.</param>
        protected void InvokeFrameEncapsulated(Frame fFrame, bool bPush)
        {
            OutputBytes += fFrame.Length;
            InvokeExternalAsync(FrameEncapsulated, new FrameProcessedEventArgs(fFrame, bPush));
        }     

        /// <summary>
        /// This event is fired whenever frame processing and decapsulation finished and the decapsulated frame can be pushed upwards the stack. 
        /// </summary>
        public event FrameProcessedEventHandler FrameDecapsulated;
        
        /// <summary>
        /// This event should be fired whenever frame processing and encapsulation finished and the encapsulated frame can be pushed downwards the stack. 
        /// </summary>
        public event FrameProcessedEventHandler FrameEncapsulated;

        /// <summary>
        /// This method has to accept any type of frames. If the frame pushed to the socket is matching the socket's binding, 
        /// the frame should be <b>decapsulated</b> and true should be returned. Otherwise the frame should be discarded by this instance 
        /// and false should be returned.
        /// After successfully processing the frame, the FrameDecapsulated event must be called with the decapsulated frame as paramater. 
        /// </summary>
        /// <param name="fFrame">The frame to process. <b>This frame instance should only be read, not edited.</b></param>
        /// <param name="bPush">A bool indicating whether the frame was delivered with a push flag.</param>
        /// <returns>A bool indicating whether the submitted frame matches this socket's binding.</returns>
        public abstract bool PushUp(Frame fFrame, bool bPush);
        
        /// <summary>
        /// Forces the socket to send out all data waiting to be send immedeately, if possible.
        /// </summary>
        public abstract void Flush();

        /// <summary>
        /// Closes this socket and frees all used resources.
        /// </summary>
        public virtual void Close()
        {
            this.ParentSocket = null;
            this.ChildSocket = null;
        }

        /// <summary>
        /// This method has to accept any type of frames. 
        /// The given frame should be <b>encapsulated</b> according to this socket's binding.
        /// After successfully processing the frame, the FrameEncapsulated event must be called with the encapsulated frame as paramater. 
        /// </summary>
        /// <param name="fFrame">The frame to process. <b>This frame instance should only be read, not edited.</b></param>
        /// <param name="bPush">A bool indicating whether the frame was delivered with a push flag.</param>
        public abstract void PushDown(Frame fFrame, bool bPush);

        /// <summary>
        /// This method accepts an array of bytes. 
        /// The given bytes are <b>encapsulated</b> according to this socket's binding.
        /// After successfully processing the frame, the FrameEncapsulated event is called with the encapsulated frame as paramater. 
        /// </summary>
        /// <param name="bPush">A bool indicating whether the frame was delivered with a push flag.</param>
        /// <param name="bData">The bytes to process</param>
        public virtual void PushDown(byte[] bData, bool bPush)
        {
            PushDown(new RawDataFrame(bData), bPush);
        }

        /// <summary>
        /// This accessor has to return a BindingInformation object containing information about this socket's binding. 
        /// </summary>
        public abstract BindingInformation BindingInformation {get; }

        #region Debug

        public long OutputBytes { get; protected set; }
        public long InputBytes { get; protected set; }

        #endregion

        #region IDisposable Members

        public abstract void Dispose();

        #endregion
    }
}
