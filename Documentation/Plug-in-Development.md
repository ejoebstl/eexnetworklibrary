# Developing Plug-Ins

Plug-Ins enable you to bundle Traffic Handlers, so that they can be loaded dynamically by all applications which use the [NetLab](eExNetLab) or the [NLML](eEx-NLML). The NLML provides a lot of base classes for different types of Plug-Ins, which aim to make writing plug-ins as comfortable as possible.

This is not necassary for using TrafficHandlers from C# code. 

Developing a Handler Plug-In can be split up in the following tasks:

* [Implementing a Traffic Handler](Implementing-custom-Traffic-Handlers.md)
* [Defining a NLML Plug-In](Defining-a-NLML-Plug-In.md) - don't forget to choose a [PluginKey](PluginKey)
* [Defining a NetLab Plug-In](Defining-a-NetLab-Plug-In.md)

**Notice**: If you want to implement non-handler plug-ins or need more examples, have a look at the classes in the NLML, especially in the folders `Extensibility`, `DefaultControllers`, `DefaultDefinitions` and `SubPlugInDefinitions`.