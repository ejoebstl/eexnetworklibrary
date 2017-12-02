# The Frame class 

The Frame classes are a main component of the network library. All implemented network protocols derive from the Frame super class which includes several methods which must be implemented. 

The frame parsing is done by the [Protocol Engine](Protocol-Engine.md). If you want to implement your own protocl add-on, you will have to provide a Frame class and a valid implementation of [IProtocolProvider](Protocol-Engine.md).

## General Operation

All frame classes in the Network Library derive from the superclass `Frame`. Only abstract frame objects are pushed from [Traffic Handler](Traffic-Handler) to Traffic Handler, but you can get the desired frame, for example IPv4, with ease since the traffic handler provides methods to access the protocol engine. 

The frame is a nested data structure. Each frame contains an encapsulated frame, which represents the frame payload. For example, an Ethernet Frame may contain an encapsulated IP frame and the IP frame may contain and encapsulated TCP frame, which finally contains some payload data. If you change the EncapsulatedFrame property of a frame, you can change it's payload. If you set it to null, the payload is removed.

When sending a frame, for example, the FrameBytes property is called, which converts the actual frames to bytes and calls the FrameBytes method for the encapsulated frame which again calls the FrameBytes method for the next encapsulated frame and so on. All frame types already contained in the Network Library have got a constructor which accepts a byte array, which will be parsed. 

Each frame also has an identifier, named the FrameType. This property returns a string which uniquely identifies the frame type. This is also used when searching for a [protocol provider](Protocol-Engine.md) for a given frame. 
If you want to implement frames, it is good practice to use the constants pre-defined for many protocols in the eExNetworkLibrary.FrameTypes object, and to define a static property in your frame class, which also returns the type of the frame. 

Each frame which was captured by an interface, should contain a TrafficDescriptionFrame as the first frame, which holds a reference to the capture interface.

## Overview of Frame Types

The network library implements the following classes which derive from frame:

* EthernetFrame
* IPFrame
	* IPv4Frame
	* IPv6Frame
* ARPFrame
* ICMPFrame
	* ICMPv4Frame
	* ICMPv6Frame
* RawDataFrame (For raw payload data)
* UDPFrame
* TCPFrame
* DNSFrame
* DHCPFrame
	* TLVItem
	* DHCPTLVItem
* NeighborSolicitation & NeighborAdvertisement
	* NeighborDiscoveryOption (For IVMPv6)
* OSPFCommonHeader
	* Various OSPF Messages
	* Various LSA Types
* TrafficDescriptionFrame
* RIPFrame
* ExtensionHeader (For IPv6)
	* FragmentationExtension
	* RoutingExtension

All these classes are supported by default by the [Protocol Engine](Protocol-Engine.md), and can be created from and converted to an array of bytes. 

## Creating own frames

Needless to say, it is possible to create new frames and set various parameters. For example, here is an example taken from the DHCPServer class:

```csharp
UDP.UDPFrame newUDPFrame = new eExNetworkLibrary.UDP.UDPFrame();
newUDPFrame.DestinationPort = iDHCPInPort;
newUDPFrame.SourcePort = iDHCPOutPort;
newUDPFrame.EncapsulatedFrame = newDHCPFrame;

IP.IPv4Frame newIPv4Frame = new eExNetworkLibrary.IP.IPv4Frame();
newIPv4Frame.Version = 4;
newIPv4Frame.DestinationAddress = IPAddress.Broadcast;
newIPv4Frame.SourceAddress = ipaServer;
newIPv4Frame.Protocol = eExNetworkLibrary.IP.IPProtocol.UDP;
newIPv4Frame.EncapsulatedFrame = newUDPFrame;
newIPv4Frame.Identification = (uint)IncrementIPIDCounter();
newIPv4Frame.TimeToLive = 128;
```

In this example, a new UDP frame and a new IP frame is created. Then, the UDP frame is set as the encapsulated frame of the IP frame, which means that now the UDP frame is the payload of the IP frame. 
As you can see, the encapsulate frame of the UDP frame ist set to a DHCP frame. The DHCP frame creation is a little bit large, so if you want to see it, head over into the DHCPServer class and see the method HandleDHCPFrame.

If you create own frames, it is not necessary to create Ethernet frames or other layer 2 protocols, as long as you use a router, since the router will tell the interface about the next hop of the frame, and the interface will automatically add layer 2 data when sending. 

You can push your frames any time by calling the NotifyNext() method of your [Traffic Handler](Traffic-Handler.md). 