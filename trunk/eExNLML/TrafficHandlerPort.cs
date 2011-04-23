using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary;
using eExNLML.Extensibility;

namespace eExNLML
{    
    /// <summary>
    /// This class represents a traffic handler port which is used in the management layer to 
    /// represent ports of traffic handlers which can be connected together. 
    /// </summary>
    public class TrafficHandlerPort
    {
        /// <summary>
        /// Gets the name of this port.
        /// </summary>
        public string Name { get; set; }       
        /// <summary>
        /// Gets a bool indicating whether handlers can attach to this port.
        /// </summary>
        public bool CanAttach { get; private set; }
        /// <summary>
        /// Gets the description of this port.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Gets the abbreviation of this traffic handler port.
        /// </summary>
        public string Abbreviation { get; set; }
        /// <summary>
        /// Gets the type of this port.
        /// </summary>
        public PortType PortType { get; private set; }

        /// <summary>
        /// Gets the Traffic Handler this port belongs to.
        /// </summary>
        public TrafficHandler ParentHandler { get; private set; }

        /// <summary>
        /// Gets the Handler Controller this port belongs to.
        /// </summary>
        public HandlerController ParentController { get; private set; }

        /// <summary>
        /// This delegate describes a method which is used to handle port attach or detach events.
        /// </summary>
        /// <param name="sender">The traffic handler port which rised the event</param>
        /// <param name="attacher">The traffic handler port which wants to attach to sender</param>
        /// <returns>Bool indicating if after the attach/detach process another port can still be attached to this port.</returns>
        public delegate bool PortActionEventHandler(TrafficHandlerPort sender, TrafficHandlerPort attacher); //Return value is saved in bCanAttach. Sucessfull attaches must therefore return false. 
        
        /// <summary>
        /// This delegate describes a method which is used to query the port connection status.
        /// </summary>
        /// <param name="sender">The first traffic handler port.</param>
        /// <param name="attacher">The second traffic handler port.</param>
        /// <returns>Bool indicating whether the given ports are connected or not.</returns>
        public delegate bool PortQueryEventHandler(TrafficHandlerPort sender, TrafficHandlerPort attacher); 

        /// <summary>
        /// This delegate describes a method which is used to handle port attached or detached events.
        /// </summary>
        /// <param name="sender">The traffic handler port which rised the event</param>
        /// <param name="attacher">The traffic handler port which wants to attach to sender</param>
        public delegate void PortNotificationEventHandler(TrafficHandlerPort sender, TrafficHandlerPort attacher);

        /// <summary>
        /// This event is fired when a handler tries to attach to this port.
        /// </summary>
        public event PortActionEventHandler HandlerAttaching;
        /// <summary>
        /// This event is fired when a handler tries to detach from this port.
        /// </summary>
        public event PortActionEventHandler HandlerDetaching;

        /// <summary>
        /// This event is fired when a handler successfully attached to this port.
        /// </summary>
        public event PortNotificationEventHandler HandlerAttached;
        /// <summary>
        /// This event is fired when a handler successfully detached to this port.
        /// </summary>
        public event PortNotificationEventHandler HandlerDetached;

        /// <summary>
        /// This event is fired when the status of a port is queried.
        /// The connected delegate is responsible for delivering the port status.
        /// </summary>
        public event PortQueryEventHandler HandlerStatusCallback;
        
        /// <summary>
        /// Creates a new instance of this class with the given parameters
        /// </summary>
        /// <param name="hcController">The parent Handler Controller</param>
        /// <param name="strName">The name of this port</param>
        /// <param name="strDescription">The description of this port</param>
        /// <param name="pType">The type of this port</param>
        /// <param name="strAbbreviation">The abbreviation of this port's name</param>
        public TrafficHandlerPort(HandlerController hcController, string strName, string strDescription, PortType pType, string strAbbreviation)
            :this(hcController, hcController.TrafficHandler, strName, strDescription, pType, strAbbreviation)
        {
        }
        
        /// <summary>
        /// Creates a new instance of this class with the given parameters
        /// </summary>
        /// <param name="hcController">The parent Handler Controller</param>
        /// <param name="thHandler">The TrafficHandler controlled by this port</param>
        /// <param name="strName">The name of this port</param>
        /// <param name="strDescription">The description of this port</param>
        /// <param name="pType">The type of this port</param>
        /// <param name="strAbbreviation">The abbreviation of this port's name</param>
        public TrafficHandlerPort(HandlerController hcController, TrafficHandler thHandler, string strName, string strDescription, PortType pType, string strAbbreviation)
        {
            ParentHandler = thHandler;
            ParentController = hcController;
            this.Description = strDescription;
            this.Name = strName;
            this.CanAttach = true;
            this.PortType = pType;
            this.Abbreviation = strAbbreviation;
        }


        /// <summary>
        /// Tries to attach the given handler
        /// </summary>
        /// <param name="th">The handler to attach</param>
        public void AttachHandler(TrafficHandlerPort th)
        {
            if (HandlerAttaching != null)
            {
                CanAttach = HandlerAttaching(this, th);
                HandlerAttached(this, th);
            }
        }

        /// <summary>
        /// Tries to detach the given handler
        /// </summary>
        /// <param name="th">The handler to detach</param>
        public void DetachHandler(TrafficHandlerPort th)
        {
            if (HandlerDetaching != null)
            {
                CanAttach = HandlerDetaching(this, th);
                HandlerDetached(this, th);
            }
        }

        /// <summary>
        /// Checks whether this port is connected to the given port.
        /// </summary>
        /// <param name="th">The port to check the connection with.</param>
        /// <returns>A bool indicating whether this port is connected to the given port.</returns>
        public bool IsConnectedTo(TrafficHandlerPort th)
        {
            if (HandlerStatusCallback != null)
            {
                return HandlerStatusCallback(this, th);
            }
            else
            {
                throw new NotImplementedException("Ports status query failed. The HandlerStatusCallback event was not overloaded by the HandlerController which created this port");
            }
        }
    }

    /// <summary>
    /// Describes the type of an traffic handler port
    /// </summary>
    public enum PortType
    {
        /// <summary>
        /// An input port
        /// </summary>
        Input = 0,
        /// <summary>
        /// An output port
        /// </summary>
        Output = 1,
        /// <summary>
        /// An interfaceIO port for attaching interfaces
        /// </summary>
        InterfaceIO = 2,
        /// <summary>
        /// An interface port for attaching interface I/O ports
        /// </summary>
        Interface = 3
    }
}
