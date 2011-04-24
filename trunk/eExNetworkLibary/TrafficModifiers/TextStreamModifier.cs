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
using eExNetworkLibrary.Sockets;
using eExNetworkLibrary.TrafficModifiers.StreamModification;
using System.Net;

namespace eExNetworkLibrary.TrafficModifiers
{
    /// <summary>
    /// This class can be used to modify TCP stream contents in a specified encoding on the fly.
    /// </summary>
    public class TextStreamModifier : TCPStreamModifier
    {
        private int iPort;
        private string strDataToFind;
        private string strDataToReplace;
        private Encoding eEncoding;

        /// <summary>
        /// Gets or sets the port of the connections which should be modified. 
        /// Use a value of 0 to modify all connections. 
        /// </summary>
        public int Port
        {
            get { return iPort; }
            set
            {
                iPort = value; 
                InvokePropertyChanged();
            }
        }

        public Encoding Encoding
        {
            get { return eEncoding; }
            set
            {
                eEncoding = value;
                InvokePropertyChanged();
            }
        }

        public string DataToFind
        {
            get { return strDataToFind; }
            set
            {
                strDataToFind = value; 
                InvokePropertyChanged();
            }
        }
        public string DataToReplace
        {
            get { return strDataToReplace; }
            set
            {
                strDataToReplace = value;
                InvokePropertyChanged();
            }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public TextStreamModifier()
        {
            Port = 0;
            DataToFind = "";
            DataToReplace = "";
            eEncoding = System.Text.ASCIIEncoding.ASCII;
        }

        protected override NetworkStreamModifier[] CreateAndLinkStreamOperators(NetworkStream nsAlice, NetworkStream nsBob)
        {
            StreamReplacementOperator sroOperator = new StreamReplacementOperator(nsAlice, nsBob);
            sroOperator.Encoding = this.Encoding;
            if (!DataToFind.Equals(""))
            {
                sroOperator.ReplacementRule = new StreamReplacementRule(this.Encoding.GetBytes(strDataToFind), this.Encoding.GetBytes(strDataToReplace));
            }
            return new NetworkStreamModifier[] { sroOperator };
        }

        protected override bool ShouldIntercept(IPAddress ipaSource, IPAddress ipaDestination, int iSourcePort, int iDestinationPort)
        {
            return iSourcePort == iPort || iDestinationPort == iPort || iPort == 0;
        }
    }
}
