using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary;
using eExNetworkLibrary.Utilities;
using eExNetworkLibrary.Routing;
using eExNetworkLibrary.TrafficSplitting;
using eExNetworkLibrary.CommonTrafficAnalysis;
using System.Net;
using eExNetworkLibrary.Monitoring;
using eExNetworkLibrary.Simulation;
using System.IO;

namespace ExtendingTheRouter
{
    class Program
    {
        static void Main(string[] args)
        {
            //Query all interfaces
            WinPcapInterface[] arWpc = EthernetInterface.GetAllPcapInterfaces();

            //Create handler classes
            Router rRouter = new Router();
            TrafficSplitter tsSplitter = new TrafficSplitter();
            LibCapDumper lcpDumper = new LibCapDumper();
            NetMap nmMap = new NetMap();
            WANEmulator wanEmulator = new WANEmulator();

            //Start handlers
            rRouter.Start();
            tsSplitter.Start();
            lcpDumper.Start();
            nmMap.Start();
            wanEmulator.Start();

            //Let the router forward traffic from the interfaces to the traffic splitter
            rRouter.OutputHandler = tsSplitter;
            //Let the traffic splitter forward received traffic to the WAN emulator
            tsSplitter.OutputHandler = wanEmulator;
            //Let the WAN emulator forward received traffic back to the router
            wanEmulator.OutputHandler = rRouter;
            //Let the traffic splitter clone each frame and send it to the traffic dumper and the NetMap
            tsSplitter.AddTrafficAnalyzer(nmMap);
            tsSplitter.AddTrafficAnalyzer(lcpDumper);


            //Create the properties of the routing entry
            IPAddress ipaDestination = IPAddress.Parse("0.0.0.0");
            IPAddress ipaGateway = IPAddress.Parse("192.168.0.1");
            Subnetmask smMask = Subnetmask.Parse("0.0.0.0");
            int iMetric = 10;

            //Create the routing entry
            RoutingEntry rEntry = new RoutingEntry(ipaDestination, ipaGateway, iMetric, smMask, RoutingEntryOwner.UserStatic);

            //Set traffic dumper properties
            lcpDumper.StartLogging(Path.Combine(System.Environment.CurrentDirectory, "Dump " + DateTime.Now.ToLongDateString()), false);

            //Add some event handlers
            rRouter.FrameDropped += new EventHandler(rRouter_FrameDropped);
            rRouter.FrameForwarded += new EventHandler(rRouter_FrameForwarded);
            rRouter.FrameReceived += new EventHandler(rRouter_FrameReceived);
            rRouter.ExceptionThrown += new TrafficHandler.ExceptionEventHandler(rRouter_ExceptionThrown);
            nmMap.HostInformationChanged += new NetMap.HostChangedEventHandler(nmMap_HostInformationChanged);

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
            lcpDumper.Cleanup();
            nmMap.Cleanup();
            wanEmulator.Cleanup();

            //Start the cleanup process for all interfaces
            foreach (EthernetInterface ipInterface in wpcInterfaces)
            {
                ipInterface.Cleanup();
            }

            //Stop all handlers
            rRouter.Stop();
            tsSplitter.Stop();
            lcpDumper.Stop();
            nmMap.Stop();
            wanEmulator.Stop();

            //Stop all interfaces
            foreach (EthernetInterface ipInterface in wpcInterfaces)
            {
                ipInterface.Stop();
            }
        }

        static void nmMap_HostInformationChanged(HostInformationChangedEventArgs args, object sender)
        {
            Console.Write("Host found: " + args.Host.Name + " ");
            if (args.Host.IPAddresses.Count > 0)
            {
                Console.Write(args.Host.IPAddresses[0].ToString() + " ");
            }
            Console.WriteLine(args.Host.Type.ToString());
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
