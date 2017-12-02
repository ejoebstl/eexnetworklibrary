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

namespace eExNetworkLibrary
{
    /// <summary>
    /// This class represents a frame which carries several information about a captured frame, like capture time and the source interface.
    /// <remarks>
    /// Every frame should contain a traffic description frame. 
    /// The IP interface classes are responsible for creating and adding an instance of this class to each frame.
    /// This frames contents are ignored when converting a frame to bytes.
    /// </remarks>
    /// </summary>
    public class TrafficDescriptionFrame : Frame
    {
        public static string DefaultFrameType { get { return FrameTypes.TrafficDescriptionFrame; } }

        private IPInterface iSourceInterface;
        private DateTime dtCaptureTime;
        
        /// <summary>
        /// Ceates a new instance of this frame
        /// </summary>
        /// <param name="iSourceInterface">The interface which captured this frame</param>
        /// <param name="dtCaptureTime">The capture time of this frame</param>
        public TrafficDescriptionFrame(IPInterface iSourceInterface, DateTime dtCaptureTime)
        {
            this.iSourceInterface = iSourceInterface;
            this.dtCaptureTime = dtCaptureTime;
        }

        /// <summary>
        /// Clones this frame.
        /// </summary>
        /// <returns>An identic clone of this frame</returns>
        public override Frame Clone()
        {
            Frame f = new TrafficDescriptionFrame(iSourceInterface, new DateTime(dtCaptureTime.Ticks));
            if (this.EncapsulatedFrame != null)
            {
                f.EncapsulatedFrame = this.EncapsulatedFrame.Clone();
            }
            return f;
        }

        /// <summary>
        /// Gets the interface wich captured this frame.
        /// </summary>
        public IPInterface SourceInterface
        {
            get { return iSourceInterface; }
        }

        /// <summary>
        /// Gets the time when this frame was captured
        /// </summary>
        public DateTime CaptureTime
        {
            get { return dtCaptureTime; }
        }

        /// <summary>
        /// Gets the type of this frame.
        /// </summary>
        public override string FrameType
        {
            get { return TrafficDescriptionFrame.DefaultFrameType; }
        }

        /// <summary>
        /// Gets this frames converted to bytes.
        /// </summary>
        public override byte[] FrameBytes
        {
            get { return EncapsulatedFrame != null ? EncapsulatedFrame.FrameBytes : new byte[0]; }
        }

        /// <summary>
        /// Gets the length of the bytes of this frame.
        /// </summary>
        public override int Length
        {
            get { return EncapsulatedFrame != null ? EncapsulatedFrame.Length : 0; }
        }
    }
}
