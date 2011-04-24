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
using System.IO;
using eExNetworkLibrary.Sockets;
using eExNetworkLibrary.HTTP;

namespace eExNetworkLibrary.TrafficModifiers.StreamModification.HTTP
{
    public abstract class HTTPStreamOperator : StreamModification.NetworkStreamModifier
    {
        public HTTPStreamOperator(NetworkStream sAlice, NetworkStream sBob)
            : base(sAlice, sBob)
        { }

        protected override void RunAlice()
        {
            MemoryStream msStream = new MemoryStream();
            NetworkStream sIn = StreamAlice;
            NetworkStream sOut = StreamBob;

            try
            {
                while (bSouldRun)
                {
                    int iLen = 0;
                    HTTPRequest htreq = new eExNetworkLibrary.HTTP.HTTPRequest(sIn, out iLen);
                    HTTPMessage htreqForward = ModifyRequest(htreq);
                    if (htreqForward != null)
                    {
                        byte[] bData = htreqForward.RawBytes;
                        sOut.Write(bData, 0, bData.Length);
                        sOut.Flush();
                    }
                }
            }
            catch (HTTPParserStreamEndedException ex)
            { }
            catch (ObjectDisposedException ex)
            { throw new NetworkStreamModifierException("A network stream was forcibly closed, but " + this.ToString() + " still had data to write.", ex); }
        }

        protected abstract HTTPMessage ModifyRequest(HTTPRequest htreqRequest);

        protected override void RunBob()
        {
            MemoryStream msStream = new MemoryStream();
            NetworkStream sIn = StreamBob;
            NetworkStream sOut = StreamAlice;

            try
            {
                while (bSouldRun)
                {
                    HTTPResponse htrsp = new HTTPResponse(sIn);
                    HTTPMessage htrspForward = ModifyResponse(htrsp);
                    if (htrspForward != null)
                    {
                        byte[] bData = htrspForward.RawBytes;
                        sOut.Write(bData, 0, bData.Length);
                        sOut.Flush();
                    }
                }
            }
            catch (HTTPParserStreamEndedException ex)
            { }
            catch (ObjectDisposedException ex)
            { throw new NetworkStreamModifierException("A network stream was forcibly closed, but " + this.ToString() + " still had data to write.", ex); }
        }

        protected abstract HTTPMessage ModifyResponse(HTTPResponse htrspResponse);

        public override string ToString()
        {
            return "HTTP stream operator";
        }
    }
}
