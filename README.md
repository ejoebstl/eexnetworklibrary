# What is this? 

The [eEx Network Library](Documentation/eEx-Network-Library.md) is a implementation several computer network protocols in C#. The library is object oriented and allows fine-grained access to network packets and algorithms with a graph-based approch. It also takes care of concurrency and resource management, which can be tricky tasks when working with networks. 

The Network Library is the core of the [eEx NetLab](Documentation/eEx-NetLab.md). The tools [Traffic Watch](http://network.eex-dev.net/index.php?id=68) and [Network Symphony](http://network.eex-dev.net/index.php?id=51) were also built using this library. 

**Notice**: This project is discontinued since 2011. However, the code is stable and well documented, and I will happily answer your questions. 

## Features

* **Injection and Sniffing** - Send and receive to and from your pysical network card, including packet filters (winpcap or libpcap required)
* **Protocol Parsing** - Ethernet, IPv4, IPv6, TCP, UDP, DHCP, OSPF, RIP, DNS, HTTP, ICMPv4, ICMPv6
* **Userland Sockets** - Bind on foreign addresses and hijack TCP/IP connections
* **Network Algorithms** - DHCP Server, Loggers, Routing, NAT and much more
* **WAN Emulation** - WAN simulation with frame loss, duplication of frames, bit errors and so on between to interfaces
* **Attacks** - Proof-of-Concept - ARP poisoning, DHCP spoofing, DNS on-the-fly spoofing
* **Scanning** - Passive scanning, ARP scanning, IP scanning
* **Extensibility** - Framework architecture allows you to implement your own [Traffic Handlers](Documentation/Traffic-Handler.md) with ease
* **HTTP interception** - Monitor, decode and modify HTTP payloads and headers on the fly. 

## Key Principles

In the Network Library, there are three main concepts, which provide the basics of the Network Library framework:

* The first one is the [Traffic Handler](Documentation/Traffic-Handler.md) class. Building on top of this class enables developers to modify frames on a callback-like basis, without worrying about asynchronous operation. Most applications on top of this library are built by constructing a graph of Traffic Handlers. 
* The second one is the [Frame](Documentation/Frame.md) class. A frame represents any network packet. Frames can be parsed or casted to IP frames, Ethernet frames, TCP frames and so on. A frame also provides methods to parse from raw data or to convert the frame object back to raw data. 
* The third one is the [Layer Architecture](Documentation/Layer-Architecture), which provides support for larger, more dynamic applications which can also support plug-ins, like the [eEx NetLab](Documentation/eEx-NetLab.md). The benefit is automated resource collection and serializeable state. 

## Documentation

All classes are documented using C#-Style comments, compatible with intelliSense. A good place to get started is to look at the implementation of existing Traffic Handlers and at the tutorials linked below. 

## Further Resources and Tutorials

* [Writing your own applications](Documentation/Application-Development.md) based on the network library
* [Writing plug-ins](Documentation/Plug-in-Development.md) for the Network Library [Management Layer](Documentation/Management.md) or the NetLab
* [Documentation of all existing Traffic Handlers](http://network.eex-dev.net/index.php?id=64) used in NetLab. This documentation focuses on NetLab's UI, but the same principles apply. To see class member documentation, please have a look at the specific C#-files. 

