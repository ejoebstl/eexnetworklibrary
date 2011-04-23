using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.Routing;
using eExNLML.Extensibility;
using eExNLML.IO.HandlerConfigurationLoaders;
using eExNLML.IO.HandlerConfigurationWriters;
using eExNLML.IO;
using eExNetworkLibrary;
using eExNetworkLibrary.Monitoring;

namespace eExNLML.DefaultControllers
{
    public class RouterController : HandlerController
    {
        TrafficHandlerPort thRoutedTrafficAnalyzerPort;

        public TrafficHandlerPort RoutedTrafficAnalyzerPort { get { return thRoutedTrafficAnalyzerPort; } }

        public RouterController(IHandlerDefinition hbDefinition, IEnvironment env)
            : base(hbDefinition, env, null)
        { }

        protected override TrafficHandler Create(object param)
        {
            return new Router(false);
        }

        protected override HandlerConfigurationLoader CreateConfigurationLoader(TrafficHandler h, object param)
        {
            return new RouterConfigurationLoader(h);
        }

        protected override HandlerConfigurationWriter CreateConfigurationWriter(TrafficHandler h, object param)
        {
            return new RouterConfigurationWriter(h);
        }

        protected override TrafficHandlerPort[] CreateTrafficHandlerPorts(TrafficHandler h, object param)
        {
            List<TrafficHandlerPort> lPorts = new List<TrafficHandlerPort>();

            lPorts.Add(CreateDirectInterfaceIOPort((DirectInterfaceIOHandler)h));
            lPorts.Add(CreateDroppedTrafficAnalyzerPort(h));
            lPorts.Add(CreateStandardInPort(h));
            lPorts.Add(CreateStandardOutPort(h));
            lPorts.Add(CreateRoutedTrafficAnalyzerPort(h));

            return lPorts.ToArray();
        }       
        
        #region Routed Traffic Analyzer Port Handling

        /// <summary>
        /// Creates a Routed Traffic Analyzer port
        /// </summary>
        /// <param name="h">The traffic handler to create the port for</param>
        /// <returns>The created port</returns>
        protected TrafficHandlerPort CreateRoutedTrafficAnalyzerPort(TrafficHandler h)
        {
            if (h != TrafficHandler)
                throw new InvalidOperationException("It's not allowed to create a port with a traffic handler which was not created by this definition");

            thRoutedTrafficAnalyzerPort = new TrafficHandlerPort(this, "Routed Traffic Analyzer Port", "This port provides the possibility to analyze routed traffic", PortType.Output, "r+");
            thRoutedTrafficAnalyzerPort.HandlerAttaching += new TrafficHandlerPort.PortActionEventHandler(thRoutedTrafficAnalyzerPort_HandlerAttached);
            thRoutedTrafficAnalyzerPort.HandlerStatusCallback += new TrafficHandlerPort.PortQueryEventHandler(thRoutedTrafficAnalyzerPort_HandlerStatusCallback);
            thRoutedTrafficAnalyzerPort.HandlerDetaching += new TrafficHandlerPort.PortActionEventHandler(thRoutedTrafficAnalyzerPort_HandlerDetached);
            return thRoutedTrafficAnalyzerPort;
        }

        bool thRoutedTrafficAnalyzerPort_HandlerStatusCallback(TrafficHandlerPort sender, TrafficHandlerPort attacher)
        {
            if (sender != thRoutedTrafficAnalyzerPort)
                throw new InvalidOperationException("The Routed Traffic Analyzer Port query callback was called by another sender than the Routed Traffic Analyzer Port. This is a serious internal error.");

            Router rt = (Router)TrafficHandler;

            return attacher.ParentHandler is TrafficAnalyzer && rt.ContainsRoutedTrafficAnalyzer((TrafficAnalyzer)attacher.ParentHandler);
        }

        bool thRoutedTrafficAnalyzerPort_HandlerDetached(TrafficHandlerPort sender, TrafficHandlerPort attacher)
        {
            if (sender != thRoutedTrafficAnalyzerPort)
                throw new InvalidOperationException("The Routed Traffic Analyzer Port detach event was signalled by another sender than the Routed Traffic Analyzer Port. This is a serious internal error.");

            Router rt = (Router)TrafficHandler;

            if (attacher.ParentHandler is TrafficAnalyzer)
            {
                if (rt.ContainsRoutedTrafficAnalyzer((TrafficAnalyzer)attacher.ParentHandler))
                {
                    rt.RemoveRoutingTrafficAnalyzer((TrafficAnalyzer)attacher.ParentHandler);

                    return true;
                }
                else
                {
                    throw new InvalidOperationException("The ports " + sender.Name + " and " + attacher.Name + " are not connected.");
                }
            }
            else
            {
                throw new InvalidOperationException("Only traffic analyzers can connect the " + thRoutedTrafficAnalyzerPort.Name + ".");
            }
        }

        bool thRoutedTrafficAnalyzerPort_HandlerAttached(TrafficHandlerPort sender, TrafficHandlerPort attacher)
        {
            if (sender != thRoutedTrafficAnalyzerPort)
                throw new InvalidOperationException("The Routed Traffic Analyzer Port attach event was signalled by another sender than the Routed Traffic Analyzer Port. This is a serious internal error.");

            Router rt = (Router)TrafficHandler;

            if (attacher.ParentHandler is TrafficAnalyzer)
            {
                if (!rt.ContainsRoutedTrafficAnalyzer((TrafficAnalyzer)attacher.ParentHandler))
                {
                    rt.AddRoutingTrafficAnalyzer((TrafficAnalyzer)attacher.ParentHandler);

                    return true;
                }
                else
                {
                    throw new InvalidOperationException("The ports " + sender.Name + " and " + attacher.Name + " are already connected.");
                }
            }
            else
            {
                throw new InvalidOperationException("Only traffic analyzers can connect the " + thRoutedTrafficAnalyzerPort.Name + ".");
            }
        }

        #endregion
    }
}
