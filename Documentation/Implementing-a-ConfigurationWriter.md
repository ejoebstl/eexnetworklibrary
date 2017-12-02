# Implementing a Configuration Writer

The configuration IO mechanism of the [NLML](eEx-NLML.md) depends on a tree of configuration items, where each has a name and a value and any number of sub items. Configuration Writers are responsible for converting the configuration of a certain handler to such a tree.

To create a configuration writer, simply create a new class derived from `eExNLML.IO.HandlerConfigurationWriter`.

All configuration loaders are passed their associated Traffic Handler via the constructor.
  
```csharp
private YourHandler thHandler;

public YourConfigurationWriter(TrafficHandler thHandler)
: base(thHandler)
{
    this.thHandler = (YourHandler)thHandler;
}
```

The constructor of this class is called once when a new instance of the according [HandlerController](HandlerController.md) is created. 
Then, implement the AddConfiguration method. This method has two parameters. First, a list to add your configuration too, second, the environment. 

```csharp
/// <summary>
/// This method must be overriden by all derived classes. It has to add it's own configuration items to the given list.
/// </summary>
/// <param name="lNameValueItems">The list to add all configuration items to</param>
/// <param name="eEnviornment">The environment</param>
protected abstract void AddConfiguration(List<NameValueItem> lNameValueItems, IEnvironment eEnviornment);
```

When creating the configuration, don't forget that you can add any number of sub items to each of your configuration items. For converting common data types and even arrays of common data types, use the `eExNLML.IO.ConfigurationParser` class. You can create many items with the same name. Don't worry about the name of your configuration items to be unique among the configurations of other handlers, since the NLML engine stores the configuration isolated from other handlers.

```csharp
protected override void AddConfiguration(List<NameValueItem> lNameValueItems, IEnvironment eEnviornment)
{
    //Creation of root items
    lNameValueItems.AddRange(ConvertToNameValueItems("PropertyA", thHandler.PropertyA));
    lNameValueItems.AddRange(ConvertToNameValueItems("PropertyB", thHandler.PropertyB));
    
    //Creation of multiple, nested items
    IPAddress[]() ipa = thHandler.IpAddresses;
    Subnetmask[]() smMasks = thHandler.Subnetmasks;

    for (int iC1 = 0; iC1 < ipa.Length; iC1++)
    {
        NameValueItem nviNestedItem = new NameValueItem("MultipleNestedProperty", "");
        
        //Adding child items to a nested item
        nviNestedItem.AddChildRange(ConvertToNameValueItems("Address", ipa[iC1](iC1)));
        nviNestedItem.AddChildRange(ConvertToNameValueItems("Mask", smMasks[iC1](iC1)));
        
        lNameValueItems.Add(nviNestedItem);
    }
}
```

The `AddConfiguration` method is called by the engine whenever the the command to save the compilation is received.