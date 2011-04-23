using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.TrafficSplitting;
using eExNLML.Extensibility;
using eExNLML.IO;
using eExNLML.IO.HandlerConfigurationLoaders;
using eExNLML.IO.HandlerConfigurationWriters;
using eExNetworkLibrary;

namespace eExNLML.DefaultControllers
{
    public class ConditionalTrafficSplitterController : HandlerController
    {
        public TrafficHandlerPort OutPortA { get; protected set; }
        public TrafficHandlerPort OutPortB { get; protected set; }

        public ConditionalTrafficSplitterController(IHandlerDefinition hbDefinition, IEnvironment env)
            : base(hbDefinition, env, null)
        { }

        protected override eExNetworkLibrary.TrafficHandler Create(object param)
        {
            return new ConditionalTrafficSplitter();
        }

        protected override HandlerConfigurationLoader CreateConfigurationLoader(eExNetworkLibrary.TrafficHandler h, object param)
        {
            return new ConditionalSplitterConfigurationLoader(h);
        }

        protected override HandlerConfigurationWriter CreateConfigurationWriter(eExNetworkLibrary.TrafficHandler h, object param)
        {
            return new ConditionalSplitterConfigurationWriter(h);
        }

        protected override TrafficHandlerPort[] CreateTrafficHandlerPorts(TrafficHandler h, object param)
        {
            List<TrafficHandlerPort> lPorts = new List<TrafficHandlerPort>();

            lPorts.Add(CreateOutPortA(h));
            lPorts.Add(CreateOutPortB(h));
            lPorts.Add(CreateStandardInPort(h));
            lPorts.Add(CreateDroppedTrafficAnalyzerPort(h));

            return lPorts.ToArray();
        }

        #region Standard Out Port A Handling

        private TrafficHandlerPort CreateOutPortA(TrafficHandler h)
        {
            OutPortA = new TrafficHandlerPort(this, "Traffic Handler Out Port A", "A port which pushes traffic to another traffic handler", PortType.Output, "A");
            OutPortA.HandlerStatusCallback += new TrafficHandlerPort.PortQueryEventHandler(thOutPortA_HandlerStatusCallback);
            OutPortA.HandlerAttaching += new TrafficHandlerPort.PortActionEventHandler(OutPortA_HandlerAttaching);
            OutPortA.HandlerDetaching += new TrafficHandlerPort.PortActionEventHandler(OutPortA_HandlerDetaching);

            return OutPortA;
        }

        bool OutPortA_HandlerDetaching(TrafficHandlerPort sender, TrafficHandlerPort attacher)
        {
            if (sender != OutPortA)
                throw new InvalidOperationException("The Out Port A detach event was signalled by another sender than the Out Port A. This is a serious internal error.");

            ConditionalTrafficSplitter thSplitter = (ConditionalTrafficSplitter)TrafficHandler;

            if (attacher.PortType == PortType.Input)
            {
                if (thSplitter.OutputA == attacher.ParentHandler)
                {
                    thSplitter.OutputA = null;
                    return true;
                }
                else
                {
                    throw new InvalidOperationException("The specified ports are not connected.");
                }
            }

            throw new InvalidOperationException("This port can only be used with input ports.");
        }

        bool OutPortA_HandlerAttaching(TrafficHandlerPort sender, TrafficHandlerPort attacher)
        {
            if (sender != OutPortA)
                throw new InvalidOperationException("The Out Port A attach event was signalled by another sender than the Out Port A. This is a serious internal error.");

            ConditionalTrafficSplitter thSplitter = (ConditionalTrafficSplitter)TrafficHandler;

            if (attacher.PortType == PortType.Input)
            {
                if (thSplitter.OutputA == null)
                {
                    thSplitter.OutputA = attacher.ParentHandler;
                    return false;
                }
                else
                {
                    throw new InvalidOperationException("Another handler is already connected to this port.");
                }
            }

            throw new InvalidOperationException("This port can only be used with input ports.");

        }

        bool thOutPortA_HandlerStatusCallback(TrafficHandlerPort sender, TrafficHandlerPort attacher)
        {
            if (sender != OutPortA)
                throw new InvalidOperationException("The Out Port A query callback was called by another sender than the Out Port A. This is a serious internal error.");

            ConditionalTrafficSplitter thSplitter = (ConditionalTrafficSplitter)TrafficHandler;

            return thSplitter.OutputA == attacher.ParentHandler;
        }

        #endregion

        #region Standard Out Port B Handling

        /// <summary>
        /// Creates the Out Port B
        /// </summary>
        /// <param name="h">The traffic handler to create the port for</param>
        /// <returns>The created port</returns>
        protected TrafficHandlerPort CreateOutPortB(TrafficHandler h)
        {
            if (h != TrafficHandler)
                throw new InvalidOperationException("It's not allowed to create a port with a traffic handler which was not created by this definition");

            OutPortB = new TrafficHandlerPort(this, "Traffic Handler Out Port B", "A port which pushes traffic to another traffic handler", PortType.Output, "B");
            OutPortB.HandlerAttaching += new TrafficHandlerPort.PortActionEventHandler(thOutPortB_HandlerAttached);
            OutPortB.HandlerStatusCallback += new TrafficHandlerPort.PortQueryEventHandler(thOutPortB_HandlerStatusCallback);
            OutPortB.HandlerDetaching += new TrafficHandlerPort.PortActionEventHandler(thOutPortB_HandlerDetached);

            return OutPortB;
        }

        bool thOutPortB_HandlerStatusCallback(TrafficHandlerPort sender, TrafficHandlerPort attacher)
        {
            if (sender != OutPortB)
                throw new InvalidOperationException("The Out Port B query callback was called by another sender than the Out Port B. This is a serious internal error.");

            ConditionalTrafficSplitter thSplitter = (ConditionalTrafficSplitter)TrafficHandler;

            return thSplitter.OutputB == attacher.ParentHandler;
        }

        bool thOutPortB_HandlerDetached(TrafficHandlerPort sender, TrafficHandlerPort attacher)
        {
            if (sender != OutPortB)
                throw new InvalidOperationException("The Out Port B detach event was signalled by another sender than the Out Port B. This is a serious internal error.");

            ConditionalTrafficSplitter thSplitter = (ConditionalTrafficSplitter)TrafficHandler;

            if (attacher.PortType == PortType.Input)
            {
                if (thSplitter.OutputB == attacher.ParentHandler)
                {
                    thSplitter.OutputB = null;
                    return true;
                }
                else
                {
                    throw new InvalidOperationException("The specified ports are not connected.");
                }
            }

            throw new InvalidOperationException("This port can only be used with input ports.");

        }

        bool thOutPortB_HandlerAttached(TrafficHandlerPort sender, TrafficHandlerPort attacher)
        {
            if (sender != OutPortB)
                throw new InvalidOperationException("The Out Port B attach event was signalled by another sender than the Out Port B. This is a serious internal error.");

            ConditionalTrafficSplitter thSplitter = (ConditionalTrafficSplitter)TrafficHandler;

            if (attacher.PortType == PortType.Input)
            {
                if (thSplitter.OutputB == null)
                {
                    thSplitter.OutputB = attacher.ParentHandler;
                    return false;
                }
                else
                {
                    throw new InvalidOperationException("Another handler is already connected to this port.");
                }
            }

            throw new InvalidOperationException("This port can only be used with input ports.");

        }

        #endregion
    }
}
