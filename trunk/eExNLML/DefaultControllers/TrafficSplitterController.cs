using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary;
using eExNetworkLibrary.TrafficSplitting;
using eExNLML.IO;
using eExNLML.Extensibility;
using eExNetworkLibrary.Monitoring;

namespace eExNLML.DefaultControllers
{
    public class TrafficSplitterController : HandlerController
    {
        public TrafficHandlerPort ClonePort { get; private set; }

        public TrafficSplitterController(IHandlerDefinition hbDefinition, IEnvironment env)
            : base(hbDefinition, env, null)
        { }

        protected override TrafficHandler Create(object param)
        {
            return new TrafficSplitter();
        }

        protected override HandlerConfigurationLoader CreateConfigurationLoader(TrafficHandler h, object param)
        {
            return null;
        }

        protected override HandlerConfigurationWriter CreateConfigurationWriter(TrafficHandler h, object param)
        {
            return null;
        }

        protected override TrafficHandlerPort[] CreateTrafficHandlerPorts(TrafficHandler h, object param)
        {
            List<TrafficHandlerPort> lPorts = new List<TrafficHandlerPort>();

            lPorts.Add(CreateStandardInPort(h));
            lPorts.Add(CreateStandardOutPort(h));
            lPorts.Add(CreateClonePort(h));

            return lPorts.ToArray();
        }

        #region Clone Port Handling

        /// <summary>
        /// Creates a Clone Port
        /// </summary>
        /// <param name="h">The traffic handler to create the port for</param>
        /// <returns>The created port</returns>
        protected TrafficHandlerPort CreateClonePort(TrafficHandler h)
        {
            if (h != TrafficHandler)
                throw new InvalidOperationException("It's not allowed to create a port with a traffic handler which was not created by this definition");

            ClonePort = new TrafficHandlerPort(this, "Clone Port", "This port provides the possibility to analyze traffic asynchronously.", PortType.Output, "c");
            ClonePort.HandlerAttaching += new TrafficHandlerPort.PortActionEventHandler(thClonePort_HandlerAttached);
            ClonePort.HandlerStatusCallback += new TrafficHandlerPort.PortQueryEventHandler(thClonePort_HandlerStatusCallback);
            ClonePort.HandlerDetaching += new TrafficHandlerPort.PortActionEventHandler(thClonePort_HandlerDetached);
            return ClonePort;
        }

        bool thClonePort_HandlerStatusCallback(TrafficHandlerPort sender, TrafficHandlerPort attacher)
        {
            if (sender != ClonePort)
                throw new InvalidOperationException("The Clone Port query callback was called by another sender than the Clone Port. This is a serious internal error.");

            TrafficSplitter s = (TrafficSplitter)TrafficHandler;

            return attacher.ParentHandler is TrafficAnalyzer && s.ContainsTrafficAnalyzer((TrafficAnalyzer)attacher.ParentHandler);
        }

        bool thClonePort_HandlerDetached(TrafficHandlerPort sender, TrafficHandlerPort attacher)
        {
            if (sender != ClonePort)
                throw new InvalidOperationException("The Clone Port detach event was signalled by another sender than the Clone Port. This is a serious internal error.");

            TrafficSplitter s = (TrafficSplitter)TrafficHandler;

            if (attacher.ParentHandler is TrafficAnalyzer)
            {
                if (s.ContainsTrafficAnalyzer((TrafficAnalyzer)attacher.ParentHandler))
                {
                    s.RemoveTrafficAnalyzer((TrafficAnalyzer)attacher.ParentHandler);

                    return true;
                }
                else
                {
                    throw new InvalidOperationException("The ports " + sender.Name + " and " + attacher.Name + " are not connected.");
                }
            }
            else
            {
                throw new InvalidOperationException("Only traffic analyzers can connect the " + ClonePort.Name + ".");
            }
        }

        bool thClonePort_HandlerAttached(TrafficHandlerPort sender, TrafficHandlerPort attacher)
        {
            if (sender != ClonePort)
                throw new InvalidOperationException("The Clone Port attach event was signalled by another sender than the Clone Port. This is a serious internal error.");

            TrafficSplitter s = (TrafficSplitter)TrafficHandler;

            if (attacher.ParentHandler is TrafficAnalyzer)
            {
                if (!s.ContainsTrafficAnalyzer((TrafficAnalyzer)attacher.ParentHandler))
                {
                    s.AddTrafficAnalyzer((TrafficAnalyzer)attacher.ParentHandler);

                    return true;
                }
                else
                {
                    throw new InvalidOperationException("The ports " + sender.Name + " and " + attacher.Name + " are already connected.");
                }
            }
            else
            {
                throw new InvalidOperationException("Only traffic analyzers can connect the " + ClonePort.Name + ".");
            }
        }

        #endregion
    }
}
