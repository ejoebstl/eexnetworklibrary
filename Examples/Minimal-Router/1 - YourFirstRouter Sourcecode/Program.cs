using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eExNetworkLibrary;
using eExNetworkLibrary.Utilities;
using eExNetworkLibrary.Routing;
using eExNetworkLibrary.TrafficSplitting;
using eExNetworkLibrary.CommonTrafficAnalysis;
using System.Net;

namespace YourFirstRouter
{
    public class Program
    {
        static void Main(string[] args)
        {
            //Query all interfaces
            WinPcapInterface[] arWpc = EthernetInterface.GetAllPcapInterfaces();

            //Create handler classes
            Router rRouter = new Router();
            TrafficSplitter tsSplitter = new TrafficSplitter();

            //Start handlers
            rRouter.Start();
            tsSplitter.Start();

            //Let the router forward traffic from the interfaces to the traffic splitter
            rRouter.OutputHandler = tsSplitter;
            //Let the traffic splitter forward received traffic back to the router
            tsSplitter.OutputHandler = rRouter;

            //Create the properties of the routing entry
            IPAddress ipaDestination = IPAddress.Parse("0.0.0.0");
            IPAddress ipaGateway = IPAddress.Parse("192.168.0.1");
            Subnetmask smMask = Subnetmask.Parse("0.0.0.0");
            int iMetric = 10;

            //Create the routing entry
            RoutingEntry rEntry = new RoutingEntry(ipaDestination, ipaGateway, iMetric, smMask, RoutingEntryOwner.UserStatic);

            //Add some event handlers
            rRouter.FrameDropped += new EventHandler(rRouter_FrameDropped);
            rRouter.FrameForwarded += new EventHandler(rRouter_FrameForwarded);
            rRouter.FrameReceived += new EventHandler(rRouter_FrameReceived);
            rRouter.ExceptionThrown += new TrafficHandler.ExceptionEventHandler(rRouter_ExceptionThrown);

            //Create a list for the interfaces
            List<EthernetInterface> wpcInterfaces = new List<EthernetInterface>();

            //Foreach WinPcapInterface of this host
            foreach (WinPcapInterface wpc in arWpc)
            {
                //Create a new interface handler and start it
                EthernetInterface ipInterface = new EthernetInterface(wpc);
                ipInterface.Start();

                //Then add it to the router and to our list
                wpcInterfaces.Add(ipInterface);
                rRouter.AddInterface(ipInterface);
            }

            Console.WriteLine("Loading complete...");

            //Run until 'x' is pressed
            while (Console.ReadKey().Key != ConsoleKey.X) ;

            //Start the cleanup process for all handlers
            rRouter.Cleanup();
            tsSplitter.Cleanup();

            //Start the cleanup process for all interfaces
            foreach (EthernetInterface ipInterface in wpcInterfaces)
            {
                ipInterface.Cleanup();
            }

            //Stop all handlers
            rRouter.Stop();
            tsSplitter.Stop();

            //Stop all interfaces
            foreach (EthernetInterface ipInterface in wpcInterfaces)
            {
                ipInterface.Stop();
            }
        }

        static void rRouter_ExceptionThrown(object sender, ExceptionEventArgs args)
        {
            Console.WriteLine("Router error: " + args.Exception.Message);
        }

        static void rRouter_FrameReceived(object sender, EventArgs e)
        {
            Console.WriteLine("Frame received!");
        }

        static void rRouter_FrameForwarded(object sender, EventArgs e)
        {
            Console.WriteLine("Frame forwarded!");
        }

        static void rRouter_FrameDropped(object sender, EventArgs e)
        {
            Console.WriteLine("Frame dropped!");
        }
    }
}
