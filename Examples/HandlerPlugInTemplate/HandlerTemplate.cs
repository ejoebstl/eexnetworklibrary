using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary;

namespace PluginTemplate
{
    // The traffic handler is the core of your plug-in. It is the part which
    // does traffic engeneering and accomplishes networking tasks. 
    class HandlerTemplate : TrafficHandler
    {
        // This method is called when the traffic handler is supposed to be
        // shut down soon.
        // This method is supposed to do cleanup tasks which can involve the network,
        // e.g. closing connections, releasing DHCP leases and so on. 
        public override void Cleanup()
        {
            return; //Place cleanup stuff here.
        }

        // This method is called when the traffic handler is removed from 
        // a compilation and should stop down.
        // This method should in first place cleanup memory and stop threads, but
        // do no time-consuming recovery operations which involve the network.
        public override void Stop()
        {
            //Place stop stuff here.
            base.Stop(); //Don't forget to stop the engines
        }

        // This method is called when the traffic handler is inserted into a
        // network compilation and should start up
        public override void Start()
        {
            //Place start stuff here.
            base.Start(); //Don't forget to start the engines
        }

        // This method is called for each frame which is sent to the traffic handler.
        // It's the core method where all the work should be done.
        protected override void HandleTraffic(Frame fInputFrame)
        {
            //Each handler has some support methods

            // Gets the IP component of the input frame, or null, if no IP component is present.
            eExNetworkLibrary.IP.IPFrame ipFrame = GetIPFrame(fInputFrame);
            eExNetworkLibrary.IP.IPv4Frame ipv4Frame = GetIPv4Frame(fInputFrame);
            eExNetworkLibrary.IP.V6.IPv6Frame ipv6Frame = GetIPv6Frame(fInputFrame); 

            //This is also possible for TCP-Frames and so on
            eExNetworkLibrary.TCP.TCPFrame tcpFrame = GetTCPFrame(fInputFrame);

            //There are also some helper classes
            eExNetworkLibrary.IP.IPAddressAnalysis.GetIPRange(
                new System.Net.IPAddress(new byte[] { 192, 168, 1, 1 }),
                new System.Net.IPAddress(new byte[] { 192, 168, 1, 254 }));

            if (tcpFrame != null)
            {
                //Now, do something with the frame.
                tcpFrame.DestinationPort = 8080;

                //The payload of a frame is set in the EncapsulatedFrame property.
                //For example, an Ethernet frame has an encapsulated IPFrame, the IPFrame has
                //an encapsulated TCP frame, and the encapsulated frame of the TCP frame is
                //the payload data.
                tcpFrame.EncapsulatedFrame = null; //Delete the payload of the TCP frame

                //But don't forget to adjust checksums if you modify traffic.
                //Also you have to use the pseudo-header of the right frame 
                tcpFrame.Checksum = tcpFrame.CalculateChecksum(ipFrame.GetPseudoHeader());
            }

            //Push the frame to the next handler. 
            //Omit this call, if you want to discard the frame. 
            NotifyNext(fInputFrame);
        }
    }
}
