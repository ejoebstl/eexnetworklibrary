# Implementing a Configuration Loader

The configuration IO mechanism of the [NLML](eEx-NLML.ml) depends on a tree of configuration items, where each has a name and a value and any number of sub items. Configuration Loaders are responsible for loading the configuration of a certain handler from those tree.

To create a configuration loader, simply create a new class derived from `eExNLML.IO.HandlerConfigurationLoader`. All configuration loaders are passed their associated Traffic Handler via the constructor.

```csharp
private YourHandler thHandler;

public YourConfigurationLoader(TrafficHandler thHandler)
: base(thHandler)
{
    this.thHandler = (YourHandler)thHandler;
}
```

Then, override the `ParseConfiguration method` to parse the configuration. This method has two parameters. First, a dictionary which contains all configuration items at root level, and second, the environment.

```csharp         
/// <summary>
/// This method must be overriden by any derived class. It must configure the given traffic handler according to the given name value configuration items.
/// </summary>
/// <param name="strNameValues">A dictionary filled with name value items which store the configuration to apply to your traffic handler</param>
/// <param name="eEnviornment">The environment to associate with the traffic handler</param>
protected abstract void ParseConfiguration(Dictionary<string, NameValueItem[]()> strNameValues, IEnvironment eEnviornment);
```

When parsing, don't forget that each configuration item (NameValueItem) can have any number of sub-items. For parsing common data types, simply use the methods which are provided by the `eExNLML.IO.ConfigurationParser` class. Since the configuration can also contain many items with the same name, the parsing methods are written to work with arrays. If a property is not supposed to be contained more than once in the configuration, simply use the first value in the array.

```csharp
protected override void ParseConfiguration(Dictionary<string, NameValueItem[]()> strNameValues, IEnvironment eEnviornment)
{
    //Parsing root configuration

    if (strNameValues.ContainsKey("PropertyA"))
        thHandler.PropertyA = ConfigurationParser.ConvertToBools(strNameValues["PropertyA"](_PropertyA_))[0](0);

    if (strNameValues.ContainsKey("PropertyB"))
        thHandler.PropertyB = ConfigurationParser.ConvertToInt(strNameValues["PropertyB"](_PropertyB_))[0](0);

    //Parsing nested configuration
    
    if (strNameValues.ContainsKey("MultipleNestedProperty"))
    {
        //For each configuration item with the name "MultipleNestedProperty" 
        foreach (NameValueItem nvi in strNameValues["MultipleNestedProperty"](_MultipleNestedProperty_))
        {
            //Access sub items of the configuration item with the name "MultipleNestedProperty"
            IPAddress ipAddress= ConvertToIPAddress(nvi["Address"](_Address_))[0](0);
            Subnetmask smMask = ConvertToSubnetmask(nvi["Mask"](_Mask_))[0](0);
        
            thHandler.Add(ipaAddress, smMask);
        }
    }
}
```

The ParseConfiguration method is called by the NLML engine once when loading is done, after the initialization of the [HandlerController](HandlerController.md).