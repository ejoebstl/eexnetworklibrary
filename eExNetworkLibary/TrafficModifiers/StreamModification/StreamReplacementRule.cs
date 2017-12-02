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

namespace eExNetworkLibrary.TrafficModifiers.StreamModification
{
    public class StreamReplacementRule
    {
        byte[] bDataToFind;
        byte[] bDataToReplace;

        public byte[] DataToFind
        {
            get { return bDataToFind; }
        }

        public byte[] DataToReplace
        {
            get { return bDataToReplace; }
        }

        public StreamReplacementRule(byte[] bDataToFind, byte[] bDataToReplace)
        {
            this.bDataToReplace = bDataToReplace;
            this.bDataToFind = bDataToFind;
        }
    }

}
