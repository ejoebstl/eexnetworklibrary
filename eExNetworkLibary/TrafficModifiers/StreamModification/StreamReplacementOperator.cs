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

namespace eExNetworkLibrary.TrafficModifiers.StreamModification
{
    public class StreamReplacementOperator : NetworkStreamModifier
    {
        //Clean Rule change?
        public StreamReplacementRule ReplacementRule { get; set; }
        public Encoding Encoding { get; set; }

        public StreamReplacementOperator(NetworkStream sAlice, NetworkStream sBob)
            : base(sAlice, sBob)
        {
            EnableBuffering = false;
            Encoding = System.Text.ASCIIEncoding.ASCII;
        }

        /// <summary>
        /// Gets a bool which indicates whether streams should be buffered for comparison with 
        /// the data to find. This is especially useful for telnet connections, but can lead to 
        /// connection lagging due to data not being flushed immediately. 
        /// </summary>
        public bool EnableBuffering { get; set; }

        protected override void RunAlice()
        {
            NetworkStream sAlice = StreamAlice;
            NetworkStream sBob = StreamBob;

            CommonRun(sAlice, sBob);
        }

        protected override void RunBob()
        {
            NetworkStream sAlice = StreamAlice;
            NetworkStream sBob = StreamBob;

            CommonRun(sBob, sAlice);
        }

        private void CommonRun(NetworkStream sIn, NetworkStream sOut)
        {
            int iHitCounter = 0;
            MemoryStream msBuffer = new MemoryStream();

            int iData;
            while (bSouldRun)
            {
                iData = sIn.ReadByte();
                if (iData != -1)
                {
                    if (ReplacementRule != null && ReplacementRule.DataToFind.Length != 0 && iData == ReplacementRule.DataToFind[iHitCounter] && (EnableBuffering || !sIn.IsPush))
                    {
                        msBuffer.WriteByte((byte)iData);
                        iHitCounter++;
                        if (iHitCounter >= ReplacementRule.DataToFind.Length)
                        {
                            iHitCounter = 0;
                            msBuffer.SetLength(0);
                            msBuffer.Position = 0;
                            sOut.Write(ReplacementRule.DataToReplace, 0, ReplacementRule.DataToReplace.Length);
                        }
                    }
                    else
                    {
                        if (msBuffer.Length > 0)
                        {
                            sOut.Write(msBuffer.ToArray(), 0, (int)msBuffer.Length);
                            msBuffer.SetLength(0);
                            msBuffer.Position = 0;
                            iHitCounter = 0;
                        }
                        sOut.WriteByte((byte)iData);
                        if (sIn.IsPush)
                        {
                            sOut.Flush(); //Flush. Ok?
                        }
                    }
                }
                else
                {
                    break;
                }
            }
        }

        public override string Description
        {
            get 
            {
                if (ReplacementRule != null)
                {
                    return "Replace \"" + this.Encoding.GetString(ReplacementRule.DataToFind) + "\" with \"" + this.Encoding.GetString(ReplacementRule.DataToReplace) + "\" (" + this.Encoding.EncodingName + ")";
                }
                else
                {
                    return "";
                }
            }
        }
    }
}
