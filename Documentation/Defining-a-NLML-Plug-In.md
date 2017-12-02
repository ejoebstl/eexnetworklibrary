# Defining a Plug-In at NLML Layer

Plug-ins at the [NLML Layer](Layer-Architecture.md) consist of a [HandlerDefinition](HandlerDefinition.md) which spawns a [HandlerController](HandlerController.md), which handles management tasks, spawns the Traffic Handler and provides information about the Traffic Handler's [ports](Ports.md). Plug-ins at this layer do not include any UI, in contrast to NetLab plug-ins.

## Creating a Controller

To create a controller for your handler, simply create a new class with an appropriate name and derive from eExNLML.Extensibility.HandlerController. Create a constructor which calls the constructor of HandlerController, the base class:

```csharp
public PluginController(IHandlerDefinition hbDefinition, IEnvironment env)
   : base(hbDefinition, env, null) { } //Simply call the base constructor.
```

The third parameter can be used to pass information through to the Trafic Handler. 

Additional to this, four abstract methods have to be implemented:

```csharp
/// <summary>
/// Must create the traffic handler which is defined in this plug-in.
/// This method is called once on plugin creation.
/// </summary>
/// <param name="param">A param associated with the controller to create</param>
/// <returns>The traffic handler which is defined in this plug-in</returns>
protected abstract TrafficHandler Create(object param);

/// <summary>
/// Must create a configuration loader for the given traffic handler, which loads the configuration or null to do not support loading. 
/// This method is called once on plugin creation.
/// </summary>
/// <param name="h">The traffic handler to create the configuration loader for</param>
/// <param name="param">A param associated with the controller to create</param>
/// <returns>The configuration loader for the given traffic handler or null, if no configuration loader should be used</returns>
protected abstract HandlerConfigurationLoader CreateConfigurationLoader(TrafficHandler h, object param);

/// <summary>
/// Must create a configuration writer for the given traffic handler, which writes the configuration or null to do not support saving. 
/// This method is called once on plugin creation.
/// </summary>
/// <param name="h">The traffic handler to create the configuration writer for</param>
/// <param name="param">A param associated with the controller to create</param>
/// <returns>The configuration writer for the given traffic handler or null, if no configuration writer should be used</returns>
protected abstract HandlerConfigurationWriter CreateConfigurationWriter(TrafficHandler h, object param);

/// <summary>
/// Must create all traffic handler ports owned by this handler and also connect the appropriate event handlers.
/// This method is called once on plugin creation.
/// </summary>
/// <param name="param">A param associated with the controller to create</param>
/// <returns>All traffic handler ports owned by this handler</returns>
protected abstract TrafficHandlerPort[]() CreateTrafficHandlerPorts(TrafficHandler h, object param);
```

These methods are responsible for creating the traffic handler and management objects which provide linking and file IO. All four of these methods are called by the constructor of HandlerController when the controller is created.

The `Create()` method must return a new instance of the Traffic Handler associated with this controller, like this:

```csharp
protected override eExNetworkLibrary.TrafficHandler Create(object param)
{
      return new HandlerTemplate(); //Create and return your handler/analyzer here.
}
```

The `CreateConfigurationLoader()` and `CreateConfigurationWriter()` methods have to return [Configuration Loaders](Implementing-a-ConfigurationLoader.md) and [Configuration Writers](Implementing-a-ConfigurationWriter.md), or null, if the Traffic Handler has no configuration to load or save. 

```csharp
protected override eExNLML.IO.HandlerConfigurationLoader CreateConfigurationLoader(eExNetworkLibrary.TrafficHandler h, object param)
{
    return null; //Create your configuration loader here, return null for no configuration loader
}

protected override eExNLML.IO.HandlerConfigurationWriter CreateConfigurationWriter(eExNetworkLibrary.TrafficHandler h, object param)
{
    return null; //Create your configuration writer here, return null for no configuration writer
}
```

The ´CreateTrafficHandlerPorts()` method is responsible for creating [ports](Ports.md). It can be tricky to create ports, so the HandlerController provides a method which creates default ports (Input, Output, DropAnalyzer, Interface, InterfaceIO), whereas the boolean parameters define which ports should be created. If your plug-in is a normal Traffic Handler with one input and one output port, you would call the following code:

```csharp
protected override eExNLML.TrafficHandlerPort[]() CreateTrafficHandlerPorts(eExNetworkLibrary.TrafficHandler h, object param)
{
    return CreateDefaultPorts(h, true, true, false, false, false); //Create default ports
}
```

If you create a TrafficAnalyzer, for example, you would call `CreateDefaultPorts(h, true, false, false, false, false)`, since analyzers do not have any output ports. 
If your plug-in is an Interface and inherits from IPInterface, you would call `CreateDefaultPorts(h, false, false, false, false, true)`, to create an Interface port. 
If your plug-in needs to directly interfere with network interfaces and inherits from DirectInterfaceIO, you would call `CreateDefaultPorts(h, false, false, true, false, false)`, to create an InterfaceIO port. 

Of course, you can combine different port settings. The router which is provided by the Network Library, uses all default ports except the Interface port.

## Creating a Definition

The [HandlerDefinition](HandlerDefinition.ml) of a handler is loaded from an assembly by the NLML and is used to provide information about the plug-in.

To create a HandlerDefinition, simply derive from eExNLML.Extensibility.HandlerDefinition. You will have to implement only one method: 

```csharp
/// <summary>
/// This method must create a new HandlerController instance associated with this class.
/// </summary>
/// <param name="env">The environment of the controller</param>
/// <returns>The created HandlerController instance</returns>
public abstract IHandlerController Create(IEnvironment env);
```

This method is called whenever a new handler is created, since each handler has its own handler controller. 

You can simply implement the method as follows, so that it returns a new instance of your controller defined before:

```csharp
public override eExNLML.Extensibility.IHandlerController Create(IEnvironment env)
{
    return new PluginController(this, env); //Instantiate a handler controller with the given environment.
}
```

Further, you should create a constructor which sets some properties about the plug-in, which are already filled with defaults in the base class, but should be re-defined by you. 

```
public PluginDefinition()
{
    //Set plug-in parameters
    Name = "NetLab and NLML Test Template";
    Description = "This is for tests only.";
    Author = "Emanuel Jöbstl";
    WebLink = "http://www.eex-dev.net";

    //Plug-In key - has to be unique
    PluginKey = "eex_nlml_netlab_plugin_template";

    //Plug-In type - defines what your plug-in is. (For example, it could also be an interface). 
    PluginType = PluginTypes.TrafficHandler;
}
```

While Name, Description, Author and WebLink are only for informational purposes, the [PluginKey](PluginKey.md) and the PluginType interfere with the NLML engine. The key is used to identify the plug-in when loading and saving and has to be unique. The PluginType contains a string which defines what this plug-in is. There are some pre-defined types, but the only ones which make sense for HandlerDefinitions are `PluginTypes.TrafficHandler` and `PluginType.Interface`. 

## Putting it all together

The plug-in definition has to be marked as public. If the project is compiled to an assembly (`dll` or `exe`), the Traffic Handler can now be dynamically loaded and used by the NLML. 

You can find a complete example for NLML plug-ins [here](../Examples/HandlerPlugInTemplate/PluginTemplate.cs).
