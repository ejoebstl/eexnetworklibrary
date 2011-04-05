using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using eExNetworkLibrary.Sockets;

namespace eExNetworkLibrary.TrafficModifiers.StreamModification
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
                    HTTP.HTTPRequest htreq = new eExNetworkLibrary.HTTP.HTTPRequest(sIn, out iLen);
                    AliceInputBytes += iLen;
                    HTTP.HTTPRequest htreqForward = ModifyRequest(htreq);
                    if (htreqForward != null)
                    {
                        byte[] bData = htreqForward.RawBytes;
                        AliceOutputBytes += bData.Length;
                        sOut.Write(bData, 0, bData.Length);
                        sOut.Flush();
                    }
                }
            }
            catch (HTTP.HTTPParserStreamEndedException ex)
            { }
            catch (ObjectDisposedException ex)
            { throw new NetworkStreamModifierException("A network stream was forcibly closed, but " + this.ToString() + " still had data to write.", ex); }
        }

        protected abstract HTTP.HTTPRequest ModifyRequest(HTTP.HTTPRequest htreqRequest);

        protected override void RunBob()
        {
            MemoryStream msStream = new MemoryStream();
            NetworkStream sIn = StreamBob;
            NetworkStream sOut = StreamAlice;

            try
            {
                while (bSouldRun)
                {
                    HTTP.HTTPResponse htrsp = new eExNetworkLibrary.HTTP.HTTPResponse(sIn);
                    BobInputBytes += htrsp.Length;
                    HTTP.HTTPResponse htrspForward = ModifyResponse(htrsp);
                    if (htrspForward != null)
                    {
                        byte[] bData = htrspForward.RawBytes;
                        BobOutputBytes += bData.Length;
                        sOut.Write(bData, 0, bData.Length);
                        sOut.Flush();
                    }
                }
            }
            catch (HTTP.HTTPParserStreamEndedException ex)
            { }
            catch (ObjectDisposedException ex)
            { throw new NetworkStreamModifierException("A network stream was forcibly closed, but " + this.ToString() + " still had data to write.", ex); }
        }

        protected abstract HTTP.HTTPResponse ModifyResponse(HTTP.HTTPResponse htrspResponse);

        public override string ToString()
        {
            return "HTTP stream operator";
        }
    }
}
