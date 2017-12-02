# Network Library Management Layer

Also known as *eEx NLML*.

The [Network Library Management Layer](eEx-NLML.md) encapsulates objects of the Network Library so that they can be dynamically linked together and provides classes to load and save [network compilations](Network-Compilation.md) to [xml-based files](XNL-File.md), which can be shared to other machines. The NLML also provides plug-in capability. Generally speaking, the NLML is the layer which provides flexibility for adding various dynamic UIs at the top of the network library [(Read more about the layer model)](Relations). The NLML uses [HandlerControllers](HandlerController) and [HandlerDefinitions](HandlerDefinition) for wrapping the handlers provided by the network library.