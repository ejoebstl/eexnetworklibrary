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

namespace eExNetworkLibrary.Sockets
{
    /// <summary>
    /// This interface represents a socket. 
    /// </summary>
    public interface ISocket : IDisposable
    {
        #region Debug

        long OutputBytes { get; }
        long InputBytes { get; }

        #endregion

        /// <summary>
        /// This method has to accept any type of frames. If the frame pushed to the socket is matching the socket's binding, 
        /// the frame should be <b>decapsulated</b> and true should be returned. Otherwise the frame should be discarded by this instance 
        /// and false should be returned.
        /// After successfully processing the frame, the FrameDecapsulated event must be called with the decapsulated frame as paramater. 
        /// </summary>
        /// <param name="fFrame">The frame to process. <b>This frame instance should only be read, not edited.</b></param>
        /// <param name="bPush">A bool indicating whether the frame was delivered with a push flag.</param>
        /// <returns>A bool indicating whether the submitted frame matches this socket's binding.</returns>
        bool PushUp(Frame fFrame, bool bPush);        
        
        /// <summary>
        /// This method has to accept any type of frames. 
        /// The given frame should be <b>encapsulated</b> according to this socket's binding.
        /// After successfully processing the frame, the FrameEncapsulated event must be called with the encapsulated frame as paramater. 
        /// </summary>
        /// <param name="fFrame">The frame to process. <b>This frame instance should only be read, not edited.</b></param>
        /// <param name="bPush">A bool indicating whether the frame was delivered with a push flag.</param>
        void PushDown(Frame fFrame, bool bPush);

        /// <summary>
        /// Forces the socket to send out all data waiting to be send immedeately, if possible.
        /// </summary>
        void Flush();

        /// <summary>
        /// This accessor has to return a BindingInformation object containing information about this socket's binding. 
        /// </summary>
        BindingInformation BindingInformation { get; }

        /// <summary>
        /// This event should be fired whenever frame processing and decapsulation finished and the decapsulated frame can be pushed upwards the stack. 
        /// </summary>
        event FrameProcessedEventHandler FrameDecapsulated;
        /// <summary>
        /// This event should be fired whenever frame processing and encapsulation finished and the encapsulated frame can be pushed downwards the stack. 
        /// </summary>
        event FrameProcessedEventHandler FrameEncapsulated;

        /// <summary>
        /// This method accepts an array of bytes. 
        /// The given bytes are <b>encapsulated</b> according to this socket's binding.
        /// After successfully processing the frame, the FrameEncapsulated event is called with the encapsulated frame as paramater. 
        /// </summary>
        /// <param name="bPush">A bool indicating whether the frame was delivered with a push flag.</param>
        /// <param name="bData">The bytes to process</param>
        void PushDown(byte[] bData, bool bPush);

        /// <summary>
        /// A bool indicating whether the socken can receive or send data.
        /// </summary>
        bool IsOpen { get; }
    }

    /// <summary>
    /// This delegate represents the method used to handle FrameProcessed events.
    /// </summary>
    /// <param name="sender">The object which invoked the event.</param>
    /// <param name="args">The event args.</param>
    public delegate void FrameProcessedEventHandler(object sender, FrameProcessedEventArgs args);

    /// <summary>
    /// This class represents the event arguments the FrameProcessed event handler of a socket. 
    /// </summary>
    public class FrameProcessedEventArgs
    {
        /// <summary>
        /// The frame which was processed by the socket instance. 
        /// </summary>
        public Frame ProcessedFrame { get; set; }

        /// <summary>
        /// A bool indicating whether this frame is being delivered with a push flag. 
        /// </summary>
        public bool IsPush { get; set; }

        /// <summary>
        /// Creates a new instance of this class. 
        /// </summary>
        /// <param name="fProcessedFrame">The frame which was processed by the socket instance.</param>
        /// <param name="bPush">A bool indicating whether this frame is being delivered with a push flag.</param>
        public FrameProcessedEventArgs(Frame fProcessedFrame, bool bPush)
        {
            this.ProcessedFrame = fProcessedFrame;
            this.IsPush = bPush;
        }

        /// <summary>
        /// Creates a new instance of this class. 
        /// </summary>
        /// <param name="fProcessedFrame">The frame which was processed by the socket instance.</param>
        public FrameProcessedEventArgs(Frame fProcessedFrame)
            : this(fProcessedFrame, false)
        { }
    }
}
