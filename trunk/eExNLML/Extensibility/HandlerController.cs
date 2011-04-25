// This source file is part of the eEx Network Library Management Layer (NLML)
//
// Author: 	    Emanuel Jöbstl <emi@eex-dev.net>
// Weblink: 	http://network.eex-dev.net
//		        http://eex.codeplex.com
//
// Licensed under the GNU Library General Public License (LGPL) 
//
// (c) eex-dev 2007-2011

using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary;
using eExNLML.IO;
using eExNetworkLibrary.Monitoring;
using System.Xml;
using System.ComponentModel;

namespace eExNLML.Extensibility
{
    /// <summary>
    /// This class is a base class for all TrafficHandler controller classes, which define TrafficHandler ports, handle linking and the loading and saving of configuration.
    /// </summary>
    public abstract class HandlerController : eExNLML.Extensibility.IHandlerController
    {
        public TrafficHandlerPort InterfaceIOPort  {get; private set; }
        public TrafficHandlerPort StandardOutPort { get; private set; }
        public TrafficHandlerPort StandardInPort { get; private set; }
        public TrafficHandlerPort DroppedAnalyzerPort { get; private set; }
        public TrafficHandlerPort InterfacePort { get; private set; }

        string strName;

        /// <summary>
        /// Constructor which creates the TrafficHandler, the configuration reader and writer and the TrafficHandler ports.
        /// </summary>
        /// <param name="hdDefinition">The HandlerDefinition which is associated with this HandlerController.</param>
        /// <param name="env">The environment to associate this controller with</param>
        /// <param name="param">A parameter which is provided to all creation-methods of the subclass</param>
        protected HandlerController(IHandlerDefinition hdDefinition, IEnvironment env, object param)
        {
            Environment = env;
            TrafficHandler = Create(param);
            ConfigurationLoader = CreateConfigurationLoader(TrafficHandler, param);
            ConfigurationWriter = CreateConfigurationWriter(TrafficHandler, param);
            TrafficHandlerPorts = CreateTrafficHandlerPorts(TrafficHandler, param);
            BaseDefinition = hdDefinition;
            Name = BaseDefinition.Name;
            Properties = new Dictionary<string, object>();
        }

        #region Props

        /// <summary>
        /// Gets a dictionary where custom named properties of this controller can be stored.
        /// <remarks>The types which also support saving and loading are: int, string, float, double, IPAddress, Subnetmask, MACAddress. All other types will simply be converted to a string via the ToString() method before saving.</remarks>
        /// </summary>
        public Dictionary<string, object> Properties { get; private set; }

        /// <summary>
        /// Gets the environment associated with this controller.
        /// </summary>
        public IEnvironment Environment { get; private set; }

        /// <summary>
        /// Gets the traffic handler instance created by this definition.
        /// </summary>
        public TrafficHandler TrafficHandler { get; protected set; }

        /// <summary>
        /// Gets the definition for this handler.
        /// </summary>
        public IHandlerDefinition BaseDefinition { get; set; }

        /// <summary>
        /// Gets the configuration loader instance created by this definition.
        /// </summary>
        public HandlerConfigurationLoader ConfigurationLoader { get; protected set; }

        /// <summary>
        /// Gets the configuration writer instance created by this definition.
        /// </summary>
        public HandlerConfigurationWriter ConfigurationWriter { get; protected set; }

        /// <summary>
        /// Gets the traffic handler ports owned by this definition.
        /// </summary>
        public TrafficHandlerPort[] TrafficHandlerPorts { get; protected set; }

        /// <summary>
        /// Gets the port which is associated with the given name.
        /// </summary>
        /// <param name="strName">The name to get the port for.</param>
        /// <returns>The found port or null if not found.</returns>
        public TrafficHandlerPort GetPortForName(string strName)
        {
            foreach (TrafficHandlerPort thp in TrafficHandlerPorts)
            {
                if (thp.Name == strName)
                    return thp;
            }

            return null;
        }

        /// <summary>
        /// Gets or sets this controllers unique name. 
        /// </summary>
        public string Name { 
            get { return strName; }
            set
            {
                if (NameChanging != null)
                {
                    if (!NameChanging(value))
                    {
                        throw new ArgumentException("The given name is invalid. A name has to be unique for each traffic handler in a compilation");
                    }
                }

                strName = value;
                TrafficHandler.Name = strName;
            }
        }

        /// <summary>
        /// Gets all traffic handler ports which are attachable at the moment
        /// </summary>
        public TrafficHandlerPort[] AvailableTrafficHandlerPorts
        {
            get
            {
                List<TrafficHandlerPort> lPortList = new List<TrafficHandlerPort>();

                foreach (TrafficHandlerPort thPort in TrafficHandlerPorts)
                {
                    if (thPort.CanAttach)
                        lPortList.Add(thPort);
                }

                return lPortList.ToArray();
            }
        }

        #endregion

        /// <summary>
        /// This delegate is used to handle NameChanging callbacks. 
        /// </summary>
        /// <param name="strNewName">The new name which will be assigned.</param>
        /// <returns>A bool indicating whether the new name is valid.</returns>
        public delegate bool NameChangingEventHandler(string strNewName);

        /// <summary>
        /// This event is fired whenever the name is changing and allows to cancle the name change.
        /// </summary>
        public event NameChangingEventHandler NameChanging;

        #region InterfaceIO Port Handling

        /// <summary>
        /// Creates an InterfaceIO port
        /// </summary>
        /// <param name="h">The DirectInterfaceIOHandler to create the port for</param>
        /// <returns>The created port</returns>
        protected TrafficHandlerPort CreateDirectInterfaceIOPort(DirectInterfaceIOHandler h)
        {
            if (h != TrafficHandler)
                throw new InvalidOperationException("It's not allowed to create a port with a traffic handler which was not created by this definition");

            InterfaceIOPort = new TrafficHandlerPort(this, "Interface IO Port", "A port which receives and transmits data from and to an inbound network interface", PortType.InterfaceIO, "I/O");
            InterfaceIOPort.HandlerAttaching += new TrafficHandlerPort.PortActionEventHandler(thInterfaceIO_HandlerAttached);
            InterfaceIOPort.HandlerStatusCallback += new TrafficHandlerPort.PortQueryEventHandler(thInterfaceIO_HandlerStatusCallback);
            InterfaceIOPort.HandlerDetaching += new TrafficHandlerPort.PortActionEventHandler(thInterfaceIO_HandlerDetached);

            return InterfaceIOPort;
        }

        bool thInterfaceIO_HandlerStatusCallback(TrafficHandlerPort sender, TrafficHandlerPort attacher)
        {
            CheckPorts(sender, InterfaceIOPort);

            DirectInterfaceIOHandler thLocalHandler = (DirectInterfaceIOHandler)TrafficHandler;

            return attacher.ParentHandler is IPInterface && thLocalHandler.ContainsInterface((IPInterface)attacher.ParentHandler);
        }
        bool thInterfaceIO_HandlerDetached(TrafficHandlerPort sender, TrafficHandlerPort attacher)
        {
            if (sender != InterfaceIOPort)
                throw new InvalidOperationException("The Interface IO Port detach event was signalled by another sender than the Interface IO Port. This is a serious internal error.");

            return DetachInterface(attacher);
        }

        private bool DetachInterface(TrafficHandlerPort h)
        {
            DirectInterfaceIOHandler thLocalHandler = (DirectInterfaceIOHandler)TrafficHandler;
            if (h.PortType == PortType.Interface)
            {
                if (h.ParentHandler is IPInterface)
                {
                    IPInterface ipi = (IPInterface)h.ParentHandler;
                    if (thLocalHandler.ContainsInterface(ipi))
                    {
                        thLocalHandler.RemoveInterface(ipi);
                    }
                    else
                    {
                        throw new InvalidOperationException("The ports " + h.Name + " and " + InterfaceIOPort.Name + " are not connected.");
                    }
                    return true;
                }
            }

            throw new InvalidOperationException("The " + InterfaceIOPort.Name + " cannot be used with other ports then interface ports or other devices then ip interfaces.");
        }

        bool thInterfaceIO_HandlerAttached(TrafficHandlerPort sender, TrafficHandlerPort attacher)
        {
            CheckPorts(sender, InterfaceIOPort);

            return AttachInterface(attacher);
        }

        private bool AttachInterface(TrafficHandlerPort h)
        {
            DirectInterfaceIOHandler thLocalHandler = (DirectInterfaceIOHandler)TrafficHandler;
            if (h.PortType == PortType.Interface)
            {
                if (h.ParentHandler is IPInterface)
                {
                    IPInterface ipi = (IPInterface)h.ParentHandler;
                    if (!thLocalHandler.ContainsInterface(ipi))
                    {
                        thLocalHandler.AddInterface(ipi);
                    }
                    else
                    {
                        throw new InvalidOperationException("The ports " + h.Name + " and " + InterfaceIOPort.Name + " are already connected.");
                    }
                    return true;
                }
            }
            throw new InvalidOperationException("The " + InterfaceIOPort.Name + " cannot be used with other ports then interface ports or other devices then ip interfaces.");
        }

        #endregion

        #region Standard Out Port Handling

        /// <summary>
        /// Creates a standard Traffic Handler Out Port
        /// </summary>
        /// <param name="h">The traffic handler to create the port for</param>
        /// <returns>The created port</returns>
        protected TrafficHandlerPort CreateStandardOutPort(TrafficHandler h)
        {
            if (h != TrafficHandler)
                throw new InvalidOperationException("It's not allowed to create a port with a traffic handler which was not created by this definition");

            StandardOutPort = new TrafficHandlerPort(this, "Traffic Handler Out Port", "A port which pushes traffic to another traffic handler", PortType.Output, "tx");
            StandardOutPort.HandlerAttaching += new TrafficHandlerPort.PortActionEventHandler(thStandardTransmitPort_HandlerAttached);
            StandardOutPort.HandlerStatusCallback += new TrafficHandlerPort.PortQueryEventHandler(thStandardOutPort_HandlerStatusCallback);
            StandardOutPort.HandlerDetaching += new TrafficHandlerPort.PortActionEventHandler(thStandardTransmitPort_HandlerDetached);

            return StandardOutPort;
        }

        bool thStandardOutPort_HandlerStatusCallback(TrafficHandlerPort sender, TrafficHandlerPort attacher)
        {
            CheckPorts(sender, StandardOutPort);

            return TrafficHandler.OutputHandler == attacher.ParentHandler;
        }

        bool thStandardTransmitPort_HandlerDetached(TrafficHandlerPort sender, TrafficHandlerPort attacher)
        {
            CheckPorts(sender, StandardOutPort);

            if (attacher.PortType == PortType.Input)
            {
                if (this.TrafficHandler.OutputHandler == attacher.ParentHandler)
                {
                    this.TrafficHandler.OutputHandler = null;
                    return true;
                }
                else
                {
                    throw new InvalidOperationException("The ports " + sender.Name + " and " + attacher.Name + " are not connected.");
                }
            }

            throw new InvalidOperationException("The " + StandardOutPort.Name  + " can only be used with input ports.");

        }

        bool thStandardTransmitPort_HandlerAttached(TrafficHandlerPort sender, TrafficHandlerPort attacher)
        {
            CheckPorts(sender, StandardOutPort);

            if (attacher.PortType == PortType.Input)
            {
                if (this.TrafficHandler.OutputHandler == null)
                {
                    this.TrafficHandler.OutputHandler = attacher.ParentHandler;
                    return false;
                }
                else
                {
                    throw new InvalidOperationException("Another handler is already connected to the " + StandardOutPort.Name + ".");
                }
            }

            throw new InvalidOperationException("The " + StandardOutPort.Name + " can only be used with input ports.");
        }

        #endregion

        #region Standard In Port Handling

        /// <summary>
        /// Creates a standard Traffic Handler In Port
        /// </summary>
        /// <param name="h">The traffic handler to create the port for</param>
        /// <returns>The created port</returns>
        protected TrafficHandlerPort CreateStandardInPort(TrafficHandler h)
        {
            if (h != TrafficHandler)
                throw new InvalidOperationException("It's not allowed to create a port with a traffic handler which was not created by this definition");

            StandardInPort = new TrafficHandlerPort(this, "Traffic Handler In Port", "A port which receives traffic from another traffic handler", PortType.Input, "rx");
            StandardInPort.HandlerAttaching += new TrafficHandlerPort.PortActionEventHandler(thStandardInPort_HandlerAttached);
            StandardInPort.HandlerStatusCallback += new TrafficHandlerPort.PortQueryEventHandler(thStandardInPort_HandlerStatusCallback);
            StandardInPort.HandlerDetaching += new TrafficHandlerPort.PortActionEventHandler(thStandardInPort_HandlerDetached);

            return StandardInPort;
        }

        bool thStandardInPort_HandlerStatusCallback(TrafficHandlerPort sender, TrafficHandlerPort attacher)
        {
            CheckPorts(sender, StandardInPort);

            return false;
        }

        bool thStandardInPort_HandlerDetached(TrafficHandlerPort sender, TrafficHandlerPort attacher)
        {
            CheckPorts(sender, StandardInPort);

            if (attacher.PortType == PortType.Output)
            {
                return true;
            }

            throw new InvalidOperationException("The " + StandardInPort.Name + " can only interact with output ports.");
        }

        bool thStandardInPort_HandlerAttached(TrafficHandlerPort sender, TrafficHandlerPort attacher)
        {
            CheckPorts(sender, StandardInPort);

            if (attacher.PortType == PortType.Output)
            {
                return true;
            }

            throw new InvalidOperationException("The " + StandardInPort.Name + " can only interact with output ports.");
        }

        #endregion

        #region Dropped Traffic Analyzer Port Handling

        /// <summary>
        /// Creates a Dropped Traffic Analyzer port
        /// </summary>
        /// <param name="h">The traffic handler to create the port for</param>
        /// <returns>The created port</returns>
        protected TrafficHandlerPort CreateDroppedTrafficAnalyzerPort(TrafficHandler h)
        {
            if (h != TrafficHandler)
                throw new InvalidOperationException("It's not allowed to create a port with a traffic handler which was not created by this definition");

            DroppedAnalyzerPort = new TrafficHandlerPort(this, "Dropped Traffic Analyzer Port", "This port provides the possibility to analyze dropped traffic", PortType.Output, "d+");
            DroppedAnalyzerPort.HandlerAttaching += new TrafficHandlerPort.PortActionEventHandler(thDroppedAnalyzerPort_HandlerAttached);
            DroppedAnalyzerPort.HandlerStatusCallback += new TrafficHandlerPort.PortQueryEventHandler(thDroppedAnalyzerPort_HandlerStatusCallback);
            DroppedAnalyzerPort.HandlerDetaching += new TrafficHandlerPort.PortActionEventHandler(thDroppedAnalyzerPort_HandlerDetached);
            return DroppedAnalyzerPort;
        }

        bool thDroppedAnalyzerPort_HandlerStatusCallback(TrafficHandlerPort sender, TrafficHandlerPort attacher)
        {
            CheckPorts(sender, DroppedAnalyzerPort);

            return attacher.ParentHandler is TrafficAnalyzer &&  TrafficHandler.ContainsDroppedTrafficAnalyzer((TrafficAnalyzer)attacher.ParentHandler);
        }

        bool thDroppedAnalyzerPort_HandlerDetached(TrafficHandlerPort sender, TrafficHandlerPort attacher)
        {
            CheckPorts(sender, DroppedAnalyzerPort);

            if (attacher.ParentHandler is TrafficAnalyzer)
            {
                if (TrafficHandler.ContainsDroppedTrafficAnalyzer((TrafficAnalyzer)attacher.ParentHandler))
                {
                    TrafficHandler.RemoveDroppedTrafficAnalyzer((TrafficAnalyzer)attacher.ParentHandler);

                    return true;
                }
                else
                {
                    throw new InvalidOperationException("The ports " + sender.Name + " and " + attacher.Name + " are not connected.");
                }
            }
            else
            {
                throw new InvalidOperationException("Only traffic analyzers can interact with the " + DroppedAnalyzerPort.Name);
            }
        }

        bool thDroppedAnalyzerPort_HandlerAttached(TrafficHandlerPort sender, TrafficHandlerPort attacher)
        {
            CheckPorts(sender, DroppedAnalyzerPort);

            if (attacher.ParentHandler is TrafficAnalyzer)
            {
                if (!TrafficHandler.ContainsDroppedTrafficAnalyzer((TrafficAnalyzer)attacher.ParentHandler))
                {
                    TrafficHandler.AddDroppedTrafficAnalyzer((TrafficAnalyzer)attacher.ParentHandler);

                    return true;
                }
                else
                {
                    throw new InvalidOperationException("The ports " + sender.Name + " and " + attacher.Name + " are already connected.");
                }
            }
            else
            {
                throw new InvalidOperationException("Only traffic analyzers can interact with the " + DroppedAnalyzerPort.Name);
            }
        }

        #endregion

        #region InterfaceIO Port Handling

        /// <summary>
        /// Creates an Interface port
        /// </summary>
        /// <param name="h">The IPInterface to create the port for</param>
        /// <returns>The created port</returns>
        protected TrafficHandlerPort CreateDirectInterfacePort(IPInterface h)
        {
            if (h != TrafficHandler)
                throw new InvalidOperationException("It's not allowed to create a port with a traffic handler which was not created by this definition");

            InterfacePort = new TrafficHandlerPort(this, "Interface Port", "A port which receives and transmits data from and to this network interface", PortType.Interface, "I/O");
            InterfacePort.HandlerAttaching += new TrafficHandlerPort.PortActionEventHandler(thInterfacePort_HandlerAttached);
            InterfacePort.HandlerStatusCallback += new TrafficHandlerPort.PortQueryEventHandler(thInterfacePort_HandlerStatusCallback);
            InterfacePort.HandlerDetaching += new TrafficHandlerPort.PortActionEventHandler(thInterfacePort_HandlerDetached);

            return InterfacePort;
        }

        bool thInterfacePort_HandlerStatusCallback(TrafficHandlerPort sender, TrafficHandlerPort attacher)
        {
            CheckPorts(sender, InterfacePort);

            return false;
        }

        bool thInterfacePort_HandlerDetached(TrafficHandlerPort sender, TrafficHandlerPort attacher)
        {
            CheckPorts(sender, InterfacePort);

            if (attacher.PortType == PortType.InterfaceIO)
            {
                return true;
            }
            else
            {
                throw new InvalidOperationException("The " + InterfacePort.Name + " can only be used with Interface IO ports.");
            }
        }

        bool thInterfacePort_HandlerAttached(TrafficHandlerPort sender, TrafficHandlerPort attacher)
        {
            CheckPorts(sender, InterfacePort);

            if (attacher.PortType == PortType.InterfaceIO)
            {
                return true;
            }
            else
            {
                throw new InvalidOperationException("The " + InterfacePort.Name + " can only be used with Interface IO ports.");
            }
        }

        #endregion

        protected void CheckPorts(TrafficHandlerPort port, TrafficHandlerPort check)
        {
            if (check == null)
                throw new InvalidOperationException(port.Name + " called an event for a port which was not even created. This is a serious internal error.");
            if (port != check)
                throw new InvalidOperationException("A " + check.Name + " event was called by another sender (" + port.Name + ") than the " + check.Name + ". This is a serious internal error.");
        }
        
        #region Creations

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
        protected abstract TrafficHandlerPort[] CreateTrafficHandlerPorts(TrafficHandler h, object param);

        #endregion

        /// <summary>
        /// Automatically creates the default traffic handler ports.
        /// </summary>
        /// <param name="h">The traffic handler to create the ports with</param>
        /// <param name="bCreateStandardInPort">A bool which defines whether to create a Traffic Handler In Port</param>
        /// <param name="bCreateStandardOutPort">A bool which defines whether to create a Traffic Handler Out Port</param>
        /// <param name="bCreateInterfaceIOPort">A bool which defines whether to create an Interface IO Port</param>
        /// <param name="bCreateDroppedTrafficAnalyzerPort">A bool which defines whether to create a Dropped Traffic Analyzer Port</param>
        /// <param name="bCreateInterfacePort">A bool which defines whether to create an Interface Port</param>
        /// <returns></returns>
        protected TrafficHandlerPort[] CreateDefaultPorts(TrafficHandler h, bool bCreateStandardInPort,
            bool bCreateStandardOutPort,
            bool bCreateInterfaceIOPort,
            bool bCreateDroppedTrafficAnalyzerPort,
            bool bCreateInterfacePort)
        {
            if (h != TrafficHandler)
                throw new InvalidOperationException("It's not allowed to create a port with a traffic handler which was not created by this definition");

            List<TrafficHandlerPort> lPorts = new List<TrafficHandlerPort>();

            if (bCreateStandardInPort)
                lPorts.Add(CreateStandardInPort(h));
            if (bCreateStandardOutPort)
                lPorts.Add(CreateStandardOutPort(h));
            if (bCreateInterfaceIOPort)
                lPorts.Add(CreateDirectInterfaceIOPort((DirectInterfaceIOHandler)h));
            if (bCreateDroppedTrafficAnalyzerPort)
                lPorts.Add(CreateDroppedTrafficAnalyzerPort(h));
            if (bCreateInterfacePort)
                lPorts.Add(CreateDirectInterfacePort((IPInterface)h));

            return lPorts.ToArray();
        }

        /// <summary>
        /// Writes the configuration of this handler in the given environment to the given XmlWriter.
        /// </summary>
        /// <param name="xmlWriter">The XmlWriter to writes the configuration to.</param>
        public void SaveConfiguration(XmlWriter xmlWriter)
        {
            if (ConfigurationWriter != null)
            {
                ConfigurationWriter.SaveConfiguration(xmlWriter, Environment);
            }
        }

        /// <summary>
        /// Reads the configuration of this handler from the given XmlWriter.
        /// </summary>
        /// <param name="xmlReader">The XmlWriter to read the configuration from.</param>
        public void LoadConfiguration(XmlReader xmlReader)
        {
            if (ConfigurationLoader != null)
            {
                ConfigurationLoader.LoadConfiguration(xmlReader, Environment);
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
