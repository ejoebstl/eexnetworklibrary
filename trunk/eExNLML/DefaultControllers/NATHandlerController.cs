using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.Routing;
using eExNLML.IO.HandlerConfigurationLoaders;
using eExNLML.IO.HandlerConfigurationWriters;
using eExNetworkLibrary;
using eExNLML.IO;
using eExNLML.Extensibility;

namespace eExNLML.DefaultControllers
{
    public class NATHandlerController : HandlerController
    {
        public TrafficHandlerPort ExternalInPort { get; protected set; }
        public TrafficHandlerPort ExternalOutPort { get; protected set; }

        public TrafficHandlerPort InternalInPort { get; protected set; }
        public TrafficHandlerPort InternalOutPort { get; protected set; }

        private NetworkAddressTranslationHandler NATHandler { get { return ((NetworkAddressTranslationHandler)TrafficHandler); } }

        public NATHandlerController(IHandlerDefinition hbDefinition, IEnvironment env)
            : base(hbDefinition, env, null)
        {
            StandardOutPort.Name = "Internal NAT Out Port";
            StandardOutPort.Description = "All frames send out this port are translated to internal addresses to provide consistend addressing. This port is meant to be linked with traffic handlers doing analysis and modification.";
            StandardOutPort.Abbreviation = "itx";
            InternalOutPort = StandardOutPort;
            StandardInPort.Name = "Internal NAT In Port";
            StandardInPort.Description = "Traffic pushed into this port will be translated to external addresses and pushed out to the External Output Port. This port is meant to be linked with traffic handlers doing analysis and modification.";
            StandardInPort.Abbreviation = "irx";
            InternalInPort = StandardInPort;
        }

        #region ExternalInPortHandling

        protected TrafficHandlerPort CreateExternalInPort()
        {
            ExternalInPort = new TrafficHandlerPort(this, NATHandler.ExternalInputHandler, "External NAT In Port", "All frames pushed into this port will be translated to the internal addresses and pushed out to the Internal Output Port. This port is meant to be linked with the Router or DirectInterfaceIO.", PortType.Input, "erx");
            ExternalInPort.HandlerAttaching += new TrafficHandlerPort.PortActionEventHandler(ExternalInPort_HandlerAttaching);
            ExternalInPort.HandlerStatusCallback += new TrafficHandlerPort.PortQueryEventHandler(ExternalInPort_HandlerStatusCallback);
            ExternalInPort.HandlerDetaching += new TrafficHandlerPort.PortActionEventHandler(ExternalInPort_HandlerDetaching);

            return ExternalInPort;
        }

        bool ExternalInPort_HandlerDetaching(TrafficHandlerPort sender, TrafficHandlerPort attacher)
        {
            CheckPorts(sender, ExternalInPort); 
            
            if (attacher.PortType == PortType.Output)
            {
                return true;
            }

            throw new InvalidOperationException("The " + ExternalInPort.Name + " can only interact with output ports.");

        }

        bool ExternalInPort_HandlerStatusCallback(TrafficHandlerPort sender, TrafficHandlerPort attacher)
        {
            CheckPorts(sender, ExternalInPort);

            return false;
        }

        bool ExternalInPort_HandlerAttaching(TrafficHandlerPort sender, TrafficHandlerPort attacher)
        {
            CheckPorts(sender, ExternalInPort); 
            
            if (attacher.PortType == PortType.Output)
            {
                return true;
            }

            throw new InvalidOperationException("The " + ExternalInPort.Name + " can only interact with output ports.");
        }

        #endregion

        #region ExternalOutPortHandling

        protected TrafficHandlerPort CreateExternalOutPort()
        {
            ExternalOutPort = new TrafficHandlerPort(this, "External NAT Out Port", "All frames send out this port are translated to their external addresses, ready for sending. This port is meant to be linked with the Router or DirectInterfaceIO.", PortType.Output, "etx");
            ExternalOutPort.HandlerAttaching += new TrafficHandlerPort.PortActionEventHandler(ExternalOutPort_HandlerAttached);
            ExternalOutPort.HandlerStatusCallback += new TrafficHandlerPort.PortQueryEventHandler(ExternalOutPort_HandlerStatusCallback);
            ExternalOutPort.HandlerDetaching += new TrafficHandlerPort.PortActionEventHandler(ExternalOutPort_HandlerDetached);

            return ExternalOutPort;
        }

        bool ExternalOutPort_HandlerStatusCallback(TrafficHandlerPort sender, TrafficHandlerPort attacher)
        {
            CheckPorts(sender, ExternalOutPort);

            return NATHandler.ExternalOutputHandler == attacher.ParentHandler;
        }

        bool ExternalOutPort_HandlerDetached(TrafficHandlerPort sender, TrafficHandlerPort attacher)
        {
            CheckPorts(sender, ExternalOutPort);

            if (attacher.PortType == PortType.Input)
            {
                if (NATHandler.ExternalOutputHandler == attacher.ParentHandler)
                {
                    NATHandler.ExternalOutputHandler = null;
                    return true;
                }
                else
                {
                    throw new InvalidOperationException("The ports " + sender.Name + " and " + attacher.Name + " are not connected.");
                }
            }

            throw new InvalidOperationException("The " + sender.Name + " can only be used with input ports.");

        }

        bool ExternalOutPort_HandlerAttached(TrafficHandlerPort sender, TrafficHandlerPort attacher)
        {
            CheckPorts(sender, ExternalOutPort);

            if (attacher.PortType == PortType.Input)
            {
                if (NATHandler.ExternalOutputHandler == null)
                {
                    NATHandler.ExternalOutputHandler = attacher.ParentHandler;
                    return false;
                }
                else
                {
                    throw new InvalidOperationException("Another handler is already connected to the " + sender.Name + ".");
                }
            }

            throw new InvalidOperationException("The " + sender.Name + " can only be used with input ports.");
        }

        #endregion

        protected override eExNetworkLibrary.TrafficHandler Create(object param)
        {
            return new NetworkAddressTranslationHandler();
        }

        protected override HandlerConfigurationLoader CreateConfigurationLoader(TrafficHandler h, object param)
        {
            return new NATHandlerConfigurationLoader((NetworkAddressTranslationHandler)h);
        }

        protected override HandlerConfigurationWriter CreateConfigurationWriter(TrafficHandler h, object param)
        {
            return new NATHandlerConfigurationWriter((NetworkAddressTranslationHandler)h);
        }

        protected override TrafficHandlerPort[] CreateTrafficHandlerPorts(TrafficHandler h, object param)
        {
            List<TrafficHandlerPort> lPorts = new List<TrafficHandlerPort>();
            lPorts.AddRange(CreateDefaultPorts(h, true, true, false, true, false));
            lPorts.Add(CreateExternalInPort());
            lPorts.Add(CreateExternalOutPort());

            return lPorts.ToArray();
        }
    }
}
