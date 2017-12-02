## Writing a router

A router is a very basic component found in many computer networks. It reads frames from its network interfaces, looks up the destination in its routing table, and forwards the frame to the according interface. Let’s start to write a very simple routing implementation without a GUI. 

To do so, create a new command line application and add the [eEx Network Library](Documentation/eEx-Network-Library) to the references. We will only work in the main method for this basic example.
The first step for building a router is to query all interfaces from the local host. We will use the class `EthernetInterfaces`, so [WinPcap](http://www.winpcap.org) or LibPcap must be installed on the computer to run the program. 

At first, we will get all available interfaces and save them into an array: 

```csharp
WinPcapInterface[]() arWpc = EthernetInterface.GetAllPcapInterfaces();
```

The next step is to instantiate the [Traffic Handlers](Traffic-Handler) we will need: The router and a traffic splitter. A traffic spliter copies all packets once and forwards them to an arbitary number of Traffic Handlers. This needs to be done to avoid concurrent modification of a frame. 

```csharp
Router rRouter = new Router(); 
TrafficSplitter tsSplitter = new TrafficSplitter();
```

The next step is to start the Traffic Handler's working threads. Also the handlers must be linked together, so they forward the received packets to each other. 

```csharp
//Start handlers
rRouter.Start();
tsSplitter.Start();

//Let the router forward traffic from the interfaces to the traffic splitter
rRouter.OutputHandler = tsSplitter;
//Let the traffic splitter forward received traffic back to the router
tsSplitter.OutputHandler = rRouter;
```

This code segment starts all handlers and links them together. The way handlers are linked does affect the whole behavior of this program. In this scenario, the graph which is created is simple: The router sends all traffic from the interfaces to the traffic splitter, and the traffic splitter sends it back to the router which will forward it to the interfaces. 

Next, we have to add a default route, if we need one. To do so, let’s create a new routing entry and add it to the router. 

```csharp
//Create the properties of the routing entry
IPAddress ipaDestination = IPAddress.Parse("0.0.0.0");
IPAddress ipaGateway = IPAddress.Parse("192.168.1.1");
Subnetmask mask = Subnetmask.Parse("0.0.0.0");
int metric = 10;

//Create the routing entry
RoutingEntry rEntry = new RoutingEntry(ipaDestination, ipaGateway, 
metric, mask, RoutingEntryOwner.UserStatic);
```

This code segment creates the IP addresses and the subnet mask for the routing entry. Then it creates the routing entry with the given metric. Setting the owner of self added entries to UserStatic is important when working with routing protocols, since it tells the router that this route is a static route from the user and should not be overwritten by routes from routing protocols like RIP and OSPF.

Before we add the network interfaces to the router, there is still one task to go: Let’s register some event handlers to see what the router is doing. 

```csharp
//Add some event handlers
rRouter.FrameDropped += new EventHandler(rRouter_FrameDropped);
rRouter.FrameForwarded += new EventHandler(rRouter_FrameForwarded);
rRouter.FrameReceived += new EventHandler(rRouter_FrameReceived);
rRouter.ExceptionThrown += new TrafficHandler.ExceptionEventHandler(rRouter_ExceptionThrown);
```

The following event handlers will output some messages, so you can see what your program is doing. 

```csharp
static void rRouter_FrameForwarded(object sender, EventArgs e)
{
    Console.WriteLine("Frame forwarded!");
}

static void rRouter_FrameDropped(object sender, EventArgs e)
{
    Console.WriteLine("Frame dropped!");
}

static void rRouter_ExceptionThrown(object sender, ExceptionEventArgs args)
{
    Console.WriteLine("Router error: " + args.Exception.Message);
} 

static void rRouter_FrameReceived(object sender, EventArgs e)
{
    Console.WriteLine("Frame received!");
}
```

The next step is to add the interfaces. Here we have to create an EthernetInterface from each WinPcapInterface we want to add, start it, and add it to the router. We also have to remember the interfaces because we need to close them when we shut down the program. 

```csharp
//Create a list for the interfaces
List wpcInterfaces = new List();

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
``` 

Here we create a new interface handler for each WinPcapInterface and add all known interfaces to the router. The router now inserts routes for each interface's subnet automatically to the routing table. 

If you would start this programm now, it would start to route packets. But if you would then close it, there could be errors because none of the interfaces would be disposed and none of the threads would be stopped. Resource disposal is very important when working with networks. 

Traffic handlers should be stopped in the same order as they were started and interfaces should be stopped last. Also, the cleanup method needs to be called in the same order as just described before initiating the shutdown process. A call to the cleanup method will stop, for example, the interfaces from receiving traffic or lets attacks restore conditions as they were before the attack happened. So let’s implement that the programm will shut down cleanly when we press the ‘x’ key. 

```csharp
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
```

Congratulations! Your simple routing implementation is now finished. 

**Summary**

Writing a small program with the network library can be split up into the following steps:

* Create, link and configure all Traffic Handlers
* Create, link and configure all Interfaces
* Let the program run and handle events from event handlers
* Cleanup all Traffic Handlers
* Cleanup all Interfaces
* Stop all Traffic Handlers
* Stop all Interfaces

The boilerplate work of starting, linking and stopping can also be done by using the [Management Layer](Layer-Architecture.md). 

The next step is [Extending the Router](Extending-the-Router.md).
