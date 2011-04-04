using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.TrafficModifiers;

namespace eExNetworkLibrary
{
    [Obsolete("Do not use this. Each traffic handler has to parse layer 7 data for itself", true)]
    class PortMapper : TrafficModifier
    {
        private List<ProtocolInformation> pInformation;

        public PortMapper()
        {
            pInformation = new List<ProtocolInformation>();

            CreateStandardMappings();
        }

        private void CreateStandardMappings()
        {
            pInformation.Add(new ProtocolInformation(ConnectionType.UDP, 67, typeof(DHCP.DHCPFrame), "DHCP"));
            pInformation.Add(new ProtocolInformation(ConnectionType.UDP, 520, typeof(Routing.RIP.RIPFrame), "RIP"));
        }

        public void AddPortMapping(ProtocolInformation p)
        {
            pInformation.Add(p);
        }

        public bool ContainsPortMapping(ProtocolInformation p)
        {
            return pInformation.Contains(p);
        }

        public void RemovePortMapping(ProtocolInformation p)
        {
            pInformation.Remove(p);
        }

        public ProtocolInformation[] GetPortMappings()
        {
            return pInformation.ToArray();
        }

        public Frame ParseTraffic(Frame fParentFrame)
        {
            TCP.TCPFrame tcpFrame = GetTCPFrame(fParentFrame);
            UDP.UDPFrame udpFrame = GetUDPFrame(fParentFrame);
            Frame rawDataFrame = GetFrameByType(fParentFrame, FrameType.ByteData);
            ProtocolInformation p;

            if (tcpFrame != null && rawDataFrame != null)
            {
                p = GetProtocol(ConnectionType.TCP, tcpFrame.SourcePort, tcpFrame.DestinationPort);
                if (p != null)
                {
                    tcpFrame.EncapsulatedFrame = p.CreateFrame(((RawDataFrame)rawDataFrame).Data);
                }

            }
            else if (udpFrame != null && rawDataFrame != null)
            {
                p = GetProtocol(ConnectionType.UDP, udpFrame.SourcePort, udpFrame.DestinationPort);
                if (p != null)
                {
                    udpFrame.EncapsulatedFrame = p.CreateFrame(((RawDataFrame)rawDataFrame).Data);
                }
            }

            return fParentFrame;
        }

        public ProtocolInformation GetProtocol(ConnectionType cType, int iSourcePort, int iDestinationPort)
        {
            foreach (ProtocolInformation pi in pInformation)
            {
                if (pi.ConnectionType == cType && (pi.Port == iSourcePort || pi.Port == iDestinationPort))
                {
                    return pi;
                }
            }

            return null;
        }

        protected override Frame ModifyTraffic(Frame fInputFrame)
        {
            ParseTraffic(fInputFrame);
            return fInputFrame;
        }

        public override void Cleanup()
        {
            //Don't need to do anything on init shutdown. 
        }
    }
}
