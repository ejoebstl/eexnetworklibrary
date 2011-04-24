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

namespace eExNetworkLibrary.IP.V6
{
    public abstract class ExtensionHeader : Frame, IIPHeader
    {
        public IPProtocol NextHeader { get; set; }
        public IPProtocol Protocol { get { return NextHeader; } set { NextHeader = value; } }

        protected ExtensionHeader(byte[] bData)
        {
            NextHeader = (IPProtocol)bData[0];
        }

        public override byte[] FrameBytes
        {
            get
            {
                byte[] bData = new byte[Length];
                bData[0] = (byte)NextHeader;
                return bData;
            }
        }
    }
}
