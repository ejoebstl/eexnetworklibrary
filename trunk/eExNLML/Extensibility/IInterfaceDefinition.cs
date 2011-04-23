using System;
using System.Net.NetworkInformation;
namespace eExNLML.Extensibility
{
    /// <summary>
    /// This interface defines an network interface.
    /// </summary>
    public interface IInterfaceDefinition : IHandlerDefinition
    {
        /// <summary>
        /// Gets the unique GUID for this interface, which is diffrent for each NIC on each host. 
        /// </summary>
        string InterfaceGUID { get; }

        /// <summary>
        /// Gets the interface type for this interface.
        /// </summary>
        NetworkInterfaceType InterfaceType { get; }
    }
}
