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

namespace eExNetworkLibrary.HTTP
{
    /// <summary>
    /// This class is capable of parsing a captured HTTP conversation
    /// </summary>
    public class HTTPConversationParser
    {
        /// <summary>
        /// This method parses a captured HTTP conversation and returns the parsed requests and responses
        /// </summary>
        /// <param name="bData">The captured data to parse</param>
        /// <returns>An array of respones, each containing the associated requests</returns>
        public HTTPRequest[] ParseConversation(byte[] bData)
        {
            List<HTTPRequest> httpRequests = new List<HTTPRequest>();
            int iLastLength;

            while (bData.Length > 0)
            {
                if (NextIsResponse(bData, 0))
                {
                    if (httpRequests.Count != 0 && httpRequests[httpRequests.Count - 1].Response == null)
                    {
                        httpRequests[httpRequests.Count - 1].Response = new HTTPResponse(bData, out iLastLength);
                        byte[] bNewData = new byte[bData.Length - iLastLength];
                        Array.Copy(bData, iLastLength, bNewData, 0, bNewData.Length);
                        bData = bNewData;
                    }
                    else
                    {
                        throw new ArgumentException("A client request is missing during this conversation.");
                    }
                }
                else
                {
                    if (httpRequests.Count == 0 || httpRequests[httpRequests.Count - 1].Response != null)
                    {
                        httpRequests.Add(new HTTPRequest(bData, out iLastLength));
                        byte[] bNewData = new byte[bData.Length - iLastLength];
                        Array.Copy(bData, iLastLength, bNewData, 0, bNewData.Length);
                        bData = bNewData;
                    }
                    else
                    {
                        throw new ArgumentException("A server response is missing during this conversation.");
                    }
                }
            }

            return httpRequests.ToArray();
        }

        private bool NextIsResponse(byte[] bData, int iStartIndex)
        {
            if (bData.Length <= iStartIndex + 4)
            {
                throw new ArgumentException("The HTTP-transmission has ended unexpectly.");
            }
            string strFirstFour = Encoding.ASCII.GetString(bData, iStartIndex, 4).ToUpper();
            return strFirstFour.Equals("HTTP");
        }
    }
}
