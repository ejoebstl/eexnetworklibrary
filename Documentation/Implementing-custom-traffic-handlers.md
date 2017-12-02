# Implementing custom Traffic Handlers

The [Traffic Handler](Traffic-Handler.md) class is one of the base classes in the Network Library. It allows access frame properties in a callback-based manner, abstracts parallelism away. Traffic Handlers can be composed into a Graph.

For a class derived from Traffic Handler, two methods need to be implemented: 

```csharp
/// <summary>
/// This method starts the handler's cleanup process which will release network resources or
/// remote allocated resources. It must be called before stopping the handler to ensure 
/// a clean shutdown.
/// </summary>
public abstract void Cleanup();

/// <summary>
/// This method is called for every frame in the input queue per default. 
// It should be used to process received traffic.
/// </summary>
///<param name="fInputFrame">The frame to process</param>
protected abstract void HandleTraffic(Frame fInputFrame);
```

The cleanup method provides you with the opportunity to perform cleanup tasks before the handler is shuts down. If you, for example, wrote a handler which allocates addresses from a DHCP server, the cleanup method would be the right place to release the addresses again before shutdown. 

The HandleTraffic method is responsible for performing the handler's operation, regardless of analysis or modification. The [Frame](Frame.md) `fInputFrame` is the frame which was received from the previous handler. It is the core method where all the work should be done. 

Furthermore, you can override the Start() and Stop() methods, but do not forget to call base.Start() and base.Stop() to start and stop the handler's worker thread.

The [Traffic Handler](Traffic-Handler.md) base class provides you with some methods to make working with traffic easier. This especially includes methods for [protocol parsing](Protocol-Engine.md): 

```csharp
// Gets the IP component of the input frame, or null, if no IP component is present.
eExNetworkLibrary.IP.IPFrame ipFrame = GetIPFrame(fInputFrame);
eExNetworkLibrary.IP.IPv4Frame ipv4Frame = GetIPv4Frame(fInputFrame);
eExNetworkLibrary.IP.V6.IPv6Frame ipv6Frame = GetIPv6Frame(fInputFrame); 

//This is also possible for TCP-Frames and so on
eExNetworkLibrary.TCP.TCPFrame tcpFrame = GetTCPFrame(fInputFrame);
``` 

If a frame, for example the TCP frame is present, you can edit the frame.

```
if (tcpFrame != null)
{
    //Now, do something with the frame.
    tcpFrame.DestinationPort = 8080;

    //But don't forget to adjust checksums if you modify traffic.
    //For TCP-Checksums, you will have to take the IP pseudo header into account.
    tcpFrame.Checksum = tcpFrame.CalculateChecksum(ipFrame.GetPseudoHeader());
}
```

Each frame which uses checksums provides you with a method to do that. 
Needless to say, you can also edit the payload of each frame. The payload of a [Frame](Frame) is accessed via the `Frame.EncapsulatedFrame` property. If you directly want to edit abstracted TCP stream data, it may be more easy to derive from the [TCPStreamModifier](TCPStreamModifier.md) class. 

Finally, you can push your frame to the [OutputHandler](TrafficHandler.md). To forward it:

```csharp
//Push the frame to the next handler. 
//Omit this call, if you want to discard the frame. 
NotifyNext(fInputFrame);
```

If you omit this call, the frame is discarded. You can also call the `NotifyNext` method, for example, each five seconds by using a timer or by user interaction to send self-crafted [frames](Frame.md). 

You can find a complete template [here](../Examples/HandlerPlugInTemplate/HandlerTempate.cs).

There are also special subclasses of Traffic Handler, that can be derived from. They can be found [here](Traffic-Handler.md).