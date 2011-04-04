using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.TrafficModifiers.StreamModification;
using System.Net;

namespace eExNetworkLibrary.TrafficModifiers
{
    public class HTTPStreamModifier : TCPStreamModifier
    {
        protected override NetworkStreamModifier[] CreateAndLinkStreamOperators(eExNetworkLibrary.Sockets.NetworkStream nsAlice, eExNetworkLibrary.Sockets.NetworkStream nsBob)
        {
            HTTPStreamReplacementOperator sroOperator = new HTTPStreamReplacementOperator(nsAlice, nsBob);
            return new NetworkStreamModifier[] { sroOperator };
        }

        protected override bool ShouldIntercept(IPAddress ipaSource, IPAddress ipaDestination, int iSourcePort, int iDestinationPort)
        {
            return iSourcePort == 80 || iDestinationPort == 80;
        }
    }
}
