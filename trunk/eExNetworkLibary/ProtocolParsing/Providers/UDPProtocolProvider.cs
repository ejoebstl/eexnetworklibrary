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

namespace eExNetworkLibrary.ProtocolParsing.Providers
{
    public class UDPProtocolProvider : IProtocolProvider
    {
        Dictionary<int, string> dictPortsProtocols;

        public UDPProtocolProvider()
        {
            dictPortsProtocols = new Dictionary<int, string>();

            dictPortsProtocols.Add(67, FrameTypes.DHCP);
            dictPortsProtocols.Add(68, FrameTypes.DHCP);
            dictPortsProtocols.Add(53, FrameTypes.DNS);
        }

        public bool Contains(int iPort)
        {
            lock (dictPortsProtocols)
            {
                return dictPortsProtocols.ContainsKey(iPort);
            }
        }

        public bool Contains(string strProtocol)
        {
            lock (dictPortsProtocols)
            {
                return dictPortsProtocols.ContainsValue(strProtocol);
            }
        }

        public void Remove(int iPort)
        {
            lock (dictPortsProtocols)
            {
                dictPortsProtocols.Remove(iPort);
            }
        }

        public void Remove(string strProtocol)
        {
            lock (dictPortsProtocols)
            {
                List<int> lToRemove = new List<int>();

                foreach (KeyValuePair<int, string> kvp in dictPortsProtocols)
                {
                    if (kvp.Value == strProtocol)
                    {
                        lToRemove.Add(kvp.Key);
                    }
                }

                foreach (int iRemove in lToRemove)
                {
                    dictPortsProtocols.Remove(iRemove);
                }
            }
        }

        public void Add(int iPort, string strProtocol)
        {
            lock (dictPortsProtocols)
            {
                dictPortsProtocols.Add(iPort, strProtocol);
            }
        }

        public string Protocol
        {
            get { return FrameTypes.UDP; }
        }

        public string[] KnownPayloads
        {
            get
            {

                lock (dictPortsProtocols)
                {
                    string[] arProtocols = new string[dictPortsProtocols.Values.Count];
                    dictPortsProtocols.Values.CopyTo(arProtocols, 0);
                    return arProtocols;
                }
            }
        }

        public Frame Parse(Frame fFrame)
        {
            if (fFrame.FrameType == this.Protocol)
            {
                return fFrame;
            }

            return new UDP.UDPFrame(fFrame.FrameBytes);
        }

        public string PayloadType(Frame fFrame)
        {
            if (fFrame.FrameType != this.Protocol)
            {
                fFrame = Parse(fFrame);
            }

            lock (dictPortsProtocols)
            {
                if (dictPortsProtocols.ContainsKey(((UDP.UDPFrame)fFrame).SourcePort))
                {
                    return dictPortsProtocols[((UDP.UDPFrame)fFrame).SourcePort];
                }
                if (dictPortsProtocols.ContainsKey(((UDP.UDPFrame)fFrame).DestinationPort))
                {
                    return dictPortsProtocols[((UDP.UDPFrame)fFrame).DestinationPort];
                }
            }

            return "";
        }
    }
}
