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
using eExNetworkLibrary.Attacks.Scanning;
using eExNetworkLibrary.Attacks;
using eExNetworkLibrary.Routing;
using eExNLML.Extensibility;
using eExNetworkLibrary;
using System.Reflection;
using System.Xml;
using eExNLML.IO;
using System.Net;
using System.Xml.Schema;
using System.Diagnostics;
using System.IO;
using eExNLML.DefaultDefinitions;
using eExNLML.SubPlugInDefinitions;
using eExNetworkLibrary.ProtocolParsing;

namespace eExNLML
{
    /// <summary>
    /// This class presents a network compilation based on traffic handlers and the eEx Network Library.
    /// This compilation can be used to build diffrent UIs upon the network library. Also it supports saving and loading. 
    /// </summary>
    public class NetworkCompilation : IEnvironment
    {
        string XMLComment;
        const string XMLNameSpace = "NLMLCompilation.xsd";
        const string XMLNetLabFile = "nlmlCompilation";
        const string XMLInterface = "interface";
        const string XMLHandler = "handler";
        const string XMLLink = "link";
        const string XMLPort = "port";
        const string XMLConfiguration = "config";
        const string XMLCustomProperty = "customProperty";

        List<IHandlerController> lActiveHandlers;
        List<Link> lLinks;
        List<IPlugin> lPlugins;

        public static IPlugin[] DefaultPlugins
        {
            get
            {
                List<IPlugin> lDefinitions = new List<IPlugin>();

                lDefinitions.AddRange(new IPlugin[]{
                new RouterControlDefinition(), 
                new TrafficDumperControlDefinition(), 
                new TrafficSplitterControlDefinition(), 
                new DirectInterfaceIOControlDefinition(), 
                new APRAttackControlDefinition(), 
                new ARPScannerControlDefinition(), 
                new NetMapControlDefinition(),
                new DHCPSpooferControlDefinition(),
                new DHCPServerControlDefinition(),
                new DNSQueryLoggerControlDefinition(),
                new SpeedMeterControlDefinition(),
                new RIPRouterControlDefinition(),
                new WANEmulatorControlDefinition(),
                new CodeLabControlDefinition(),
                new DNSOTFSpooferControlDefinition(),
                new ConditionalTrafficSplitterControlDefinition(),
                new NATHandlerControlDefinition(),
                new TextStreamModifierControlDefinition(),
                new HTTPMonitorControlDefinition(),
                new HTTPStreamModifierControlDefinition(),

                new PortRuleDefinition(), 
                new IPAddressRuleDefinition(),
                
                new ImageFlipperDefinition(),
                new RegexHeaderConditionDefinition()});

                lDefinitions.AddRange(new NetworkLibraryInterfaceExtensionFactory().Create());

                return lDefinitions.ToArray();
            }
        }

        /// <summary>
        /// Gets or sets the environment of this compilation. The default value of this property is <b>this</b>, but it can be used to override the environment in special cases. 
        /// </summary>
        public IEnvironment Environment { get; set; }

        /// <summary>
        /// Defines a callback to handle the case of a missing interface.
        /// This case happens very often, for example if you save a network compilation and load it on another computer, since the GUIDs of the network interfaces
        /// will not be the same.
        /// </summary>
        /// <param name="sender">The class which called the callback</param>
        /// <param name="args">The arguments of the callback</param>
        /// <returns>The callback must return the appropriate handler definition for the interface or null</returns>
        public delegate IHandlerDefinition InterfaceNotFoundCallback(object sender, InterfaceNotFoundEventArgs args);
        /// <summary>
        /// Defines a callback to handle the case of a missing extension.
        /// </summary>
        /// <param name="sender">The class which called the callback</param>
        /// <param name="args">The arguments of the callback</param>
        /// <returns>The callback must return the appropriate handler definition which can be used as replacement for the missing extension</returns>
        public delegate IHandlerDefinition PluginNotFoundCallback(object sender, PluginNotFoundArgs args);

        /// <summary>
        /// Creates a new network compilation.
        /// </summary>
        public NetworkCompilation()
        {
            XMLComment = @"eEx NLML Compilation File, saved by NLML Version " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
            lActiveHandlers = new List<IHandlerController>();
            Environment = this;
            lLinks = new List<Link>();
            lPlugins = new List<IPlugin>();
        }

        #region Plug-In Handling
        
        /// <summary>
        /// Adds the given plug-ins to this compilation.<br />
        /// Please see AddPlugin(Plugin) for more details.
        /// </summary>
        /// <param name="enumPlugin">The plug-ins to add</param>
        public void AddPlugins(IEnumerable<IPlugin> enumPlugin)
        {
            foreach (IPlugin pPlugin in enumPlugin)
            {
                AddPlugin(pPlugin);
            }
        }

        /// <summary>
        /// Adds the given plug-in to this compilation. Adding all plug-ins you need should be the first step before loading or creating a compilation. 
        /// <remarks>
        /// If you add a handler or interface (IHandlerDefinition or IInterfaceDefinition) definition, the compilation will become aware of the definition, so that files which use the plug-in will be loadable. <br />
        /// If you add a protocol definition (IProtocolDefinition), the protocol will added to the parsing engine, for all handlers which are created after the protocol definition was added. <br />
        /// Other plugins (like HTTP modifier actions or traffic splitter rules) will become available to the engine as soon as they are loaded. 
        /// </remarks>
        /// </summary>
        /// <param name="pPlugin">The plug-in to add</param>
        public void AddPlugin(IPlugin pPlugin)
        {
            if (pPlugin.PluginType == PluginTypes.TrafficHandler && !(pPlugin is IHandlerDefinition))
            {
                throw new ArgumentException("The plugin " + pPlugin.Name + " (" + pPlugin.PluginKey + ") declares itself as TrafficHandler, but does not implement the interface IHandlerDefinition");
            }
            if (pPlugin.PluginType == PluginTypes.Interface && !(pPlugin is IInterfaceDefinition))
            {
                throw new ArgumentException("The plugin " + pPlugin.Name + " (" + pPlugin.PluginKey + ") declares itself as Interface, but does not implement the interface IInterfaceDefinition");
            }
            if (pPlugin.PluginType == PluginTypes.ProtocolProvider && !(pPlugin is IProtocolDefinition))
            {
                throw new ArgumentException("The plugin " + pPlugin.Name + " (" + pPlugin.PluginKey + ") declares itself as ProtocolDefinition, but does not implement the interface IProtocolDefinition");
            }

            if (ContainsPlugIn(pPlugin))
            {
                RemovePlugIn(pPlugin);
            }
            lPlugins.Add(pPlugin);
        }

        /// <summary>
        /// Removes the given plug-in from this compilation.
        /// </summary>
        /// <param name="pPluginToRemove">The definition to remove.</param>
        public void RemovePlugIn(IPlugin pPluginToRemove)
        {
            if (pPluginToRemove.PluginType == PluginTypes.Interface)
            {
                RemovePlugIn(pPluginToRemove.PluginKey, ((IInterfaceDefinition)pPluginToRemove).InterfaceGUID);
            }
            else
            {
                RemovePlugIn(pPluginToRemove.PluginKey);
            }
        }

        /// <summary>
        /// Removes the given plug-in from this compilation.
        /// </summary>
        /// <param name="strPluginKey">The key of the plug-in to remove.</param>
        public void RemovePlugIn(string strPluginKey)
        {
            RemovePlugIn(strPluginKey, null);
        }

        /// <summary>
        /// Removes the given interface plug-in from this compilation.
        /// </summary>
        /// <param name="strPluginKey">The key of the interface plug-in to remove.</param>
        public void RemovePlugIn(string strPluginKey, string strInterfaceGUID)
        {
            foreach (IPlugin pPlugin in lPlugins)
            {
                if (pPlugin.PluginKey == strPluginKey && 
                    (strInterfaceGUID == null || 
                        (pPlugin.PluginType == PluginTypes.Interface && ((IInterfaceDefinition)pPlugin).InterfaceGUID == strInterfaceGUID)))
                {
                    lPlugins.Remove(pPlugin);
                    break;
                }
            }
        }

        /// <summary>
        /// Checks whether a specific plugin is known within this compilation.
        /// </summary>
        /// <param name="pPluginToCheck">The plug-in to check for.</param>
        /// <returns>A bool indicating whether a specific plug-in is known within this compilation.</returns>
        public bool ContainsPlugIn(IPlugin pPluginToCheck)
        {
            if (pPluginToCheck.PluginType == PluginTypes.Interface)
            {
                return ContainsPlugIn(pPluginToCheck.PluginKey, ((IInterfaceDefinition)pPluginToCheck).InterfaceGUID);
            }
            else
            {
                return ContainsPlugIn(pPluginToCheck.PluginKey);
            }
        }

        /// <summary>
        /// Checks whether a specific plugin is known within this compilation.
        /// </summary>
        /// <param name="strPluginKey">The plug-in key to check for.</param>
        /// <returns>A bool indicating whether a specific plug-in is known within this compilation.</returns>
        public bool ContainsPlugIn(string strPluginKey)
        {
            return ContainsPlugIn(strPluginKey, null);
        }

        /// <summary>
        /// Checks whether a specific interface plugin is known within this compilation.
        /// </summary>
        /// <param name="strPluginKey">The plug-in key to check for.</param>
        /// <param name="strInterfaceGUID">The interface GUID to check for.</param>
        /// <returns>A bool indicating whether a specific interface plug-in is known within this compilation.</returns>
        public bool ContainsPlugIn(string strPluginKey, string strInterfaceGUID)
        {
            foreach (IPlugin pPlugin in lPlugins)
            {
                if (pPlugin.PluginKey == strPluginKey && 
                    (strInterfaceGUID == null || 
                        (pPlugin.PluginType == PluginTypes.Interface && ((IInterfaceDefinition)pPlugin).InterfaceGUID == strInterfaceGUID)))
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Returns all known plug-ins with the given type.
        /// </summary>
        /// <param name="strType">The type to search plug-ins for.</param>
        /// <returns>All known plug-ins with the given type.</returns>
        public IPlugin[] GetPluginsByType(string strType)
        {
            List<IPlugin> lFoundPlugins = new List<IPlugin>();

            foreach (IPlugin pPlugin in lPlugins)
            {
                if (pPlugin.PluginType == strType)
                {
                    lFoundPlugins.Add(pPlugin);
                }
            }

            return lFoundPlugins.ToArray();
        }

        /// <summary>
        /// Returns the plug-in with the given plug-in key.
        /// </summary>
        /// <param name="strName">The key to search a plug-in for.</param>
        /// <returns>The plug-in with the given plug-in key or null, if no plug-in was found.</returns>
        public IPlugin GetPlugInByKey(string strPluginKey)
        {
            return GetPlugInByKey(strPluginKey, null);
        }

        /// <summary>
        /// Returns the interface plug-in with the given plug-in key.
        /// </summary>
        /// <param name="strName">The key to search a plug-in for.</param>
        /// <param name="strInterfaceGUID">The interface GUID of the plug-in to find.</param>
        /// <returns>The interface plug-in with the given plug-in key and the given GUID or null, if no plug-in was found.</returns>
        public IPlugin GetPlugInByKey(string strPluginKey, string strInterfaceGUID)
        {
            foreach (IPlugin pPlugin in lPlugins)
            {
                if (pPlugin.PluginKey == strPluginKey &&
                    (strInterfaceGUID == null ||
                        (pPlugin.PluginType == PluginTypes.Interface && ((IInterfaceDefinition)pPlugin).InterfaceGUID == strInterfaceGUID)))
                {
                    return pPlugin;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets all plug-ins known by this compilation.
        /// </summary>
        /// <returns>All plug-ins known by this compilation.</returns>
        public IPlugin[] GetPlugins()
        {
            return lPlugins.ToArray();
        }

        /// <summary>
        /// Removes all plug-ins from this compilation.
        /// </summary>
        public void ClearPlugIns()
        {
            lPlugins.Clear();
        }

        /// <summary>
        /// Removes all plug-ins with the given type from this compilation.
        /// <param name="strPluginType">The plug-in type to remove all plug-ins for.</param>
        /// </summary>
        public void RemovePlugInsByType(string strPluginType)
        {
            for (int iC1 = lPlugins.Count - 1; iC1 >= 0; iC1--)
            {
                if (lPlugins[iC1].PluginType == strPluginType)
                {
                    lPlugins.RemoveAt(iC1);
                }
            }
        }

        #endregion

        /// <summary>
        /// Gets a valid name for the given handler controller.
        /// </summary>
        /// <param name="hcToAdd">The controller to get the valid name for.</param>
        /// <returns>A suggested, valid and unique name for the given controller.</returns>
        public string GetValidName(IHandlerController hcToAdd)
        {
            int iCounter = 1;
            string strName = hcToAdd.Name;

            bool bValid = false;

            while (!bValid)
            {
                if (GetHandlerForName(strName) != null)
                {
                    strName = hcToAdd.Name + " #" + iCounter.ToString();
                    iCounter++;
                }
                else
                {
                    bValid = true;
                }
            }

            return strName;
        }

        /// <summary>
        /// Adds a controller which is created from the given handler definition to this compilation and automatically generates a valid name.
        /// </summary>
        /// <param name="hdToAdd">The definition to create the controller to add from.</param>
        public IHandlerController Add(IHandlerDefinition hdToAdd)
        {
            IHandlerController hcToAdd = hdToAdd.Create(Environment);
            hcToAdd.Name = GetValidName(hcToAdd);
            this.Add(hcToAdd);
            return hcToAdd;
        }

        /// <summary>
        /// Adds a controller to this compilation. The name of the controller must be unique and it has to be associated with this compilation. 
        /// </summary>
        /// <param name="hcToAdd">The controller to add.</param>
        public void Add(IHandlerController hcToAdd)
        {
            if (GetHandlerForName(hcToAdd.Name) != null)
            {
                throw new InvalidOperationException("The name of the handler to add is invaild since there is already a handler with the same name defined in this compilation.");
            }
            if (hcToAdd.Environment != Environment)
            {
                throw new InvalidOperationException("Cannot add a controller which is associated with an diffrent environment than this compilation.");
            }

               
            hcToAdd.TrafficHandler.ProtocolParser = new ProtocolParser(GetKnownProtocols(), true);

            if (!hcToAdd.TrafficHandler.IsRunning)
            {
                hcToAdd.TrafficHandler.Start();
            }

            this.lActiveHandlers.Add(hcToAdd);
            foreach(TrafficHandlerPort thp in hcToAdd.TrafficHandlerPorts)
            {
                thp.HandlerAttached += new TrafficHandlerPort.PortNotificationEventHandler(thp_HandlerAttached);
                thp.HandlerDetached += new TrafficHandlerPort.PortNotificationEventHandler(thp_HandlerDetached);
            }
        }

        private IProtocolProvider[] GetKnownProtocols()
        {
            List<IProtocolProvider> ipiProvider = new List<IProtocolProvider>();

            foreach (IProtocolDefinition ipiDefinition in GetPluginsByType(PluginTypes.ProtocolProvider))
            {
                ipiProvider.Add(ipiDefinition.Create());
            }

            return ipiProvider.ToArray();
        }

        /// <summary>
        /// Removes a controller from this compilation. 
        /// </summary>
        /// <param name="hcToRemove">The controller to remove.</param>
        public void Remove(IHandlerController hcToRemove)
        {
            this.lActiveHandlers.Remove(hcToRemove);

            Unlink(hcToRemove);

            foreach (TrafficHandlerPort thp in hcToRemove.TrafficHandlerPorts)
            {
                thp.HandlerAttached -= new TrafficHandlerPort.PortNotificationEventHandler(thp_HandlerAttached);
                thp.HandlerDetached -= new TrafficHandlerPort.PortNotificationEventHandler(thp_HandlerDetached);
            }
        }

        /// <summary>
        /// Unlinks the given controller.
        /// </summary>
        /// <param name="hcToRemove">The controller to remove all links from</param>
        public void Unlink(IHandlerController hcToRemove)
        {
            for (int iC1 = lLinks.Count - 1; iC1 >= 0; iC1--)
            {
                //In loop counter adjustment
                if (lLinks.Count <= iC1)
                {
                    iC1 = lLinks.Count - 1;
                }
                if (iC1 < 0)
                {
                    break;
                }

                if (lLinks[iC1].Alice.ParentController.Name == hcToRemove.Name)
                {
                    Link l = lLinks[iC1];
                    l.Alice.DetachHandler(l.Bob);
                    l.Bob.DetachHandler(l.Alice);
                }
            }
        }
        /// <summary>
        /// Removes a controller from this compilation and shuts it down.
        /// </summary>
        /// <param name="hcToRemove">The controller to remove and shut down.</param>
        public void RemoveAndShutdown(IHandlerController hcToRemove)
        {
            RemoveAndShutdown(hcToRemove, null);
        }

        /// <summary>
        /// Removes a controller from this compilation and shuts it down.
        /// </summary>
        /// <param name="hcToRemove">The controller to remove and shut down.</param>
        /// <param name="tncCallback">A callback used for notification of task state changes.</param>
        public void RemoveAndShutdown(IHandlerController hcToRemove, TaskNotificationCallback tncCallback)
        {
            Task tCleanup = new Task(CleanupHandler, tncCallback, hcToRemove, "Cleanup " + hcToRemove.Name);
            Task tUnlink = new Task(UnlinkHandler, tncCallback, hcToRemove, "Unlink " + hcToRemove.Name);
            Task tStop = new Task(StopHandler, tncCallback, hcToRemove, "Stop " + hcToRemove.Name);

            tCleanup.Execute();
            tUnlink.Execute();
            tStop.Execute();

            Remove(hcToRemove);
        }

        /// <summary>
        /// Calls Cleanup on the TrafficHandler which is associated with the HandlerController which is the Tag of the given Task
        /// </summary>
        /// <param name="t">The calling task</param>
        protected virtual void CleanupHandler(Task t)
        {
            if (!(t.Tag is IHandlerController))
            {
                throw new InvalidOperationException("Cannot call cleanup with a tag which is not of type HandlerController");
            }
            ((IHandlerController)(t.Tag)).TrafficHandler.Cleanup();
        }

        /// <summary>
        /// Calls Unlink on the HandlerController which is the Tag of the given Task
        /// </summary>
        /// <param name="t">The calling task</param>
        protected virtual void UnlinkHandler(Task t)
        {
            if (!(t.Tag is IHandlerController))
            {
                throw new InvalidOperationException("Cannot call unlink with a tag which is not of type HandlerController");
            }
            Unlink((IHandlerController)(t.Tag));
        }

        /// <summary>
        /// Calls Stop on the TrafficHandler which is associated with the HandlerController which is the Tag of the given Task
        /// </summary>
        /// <param name="t">The calling task</param>
        protected virtual void StopHandler(Task t)
        {
            if (!(t.Tag is IHandlerController))
            {
                throw new InvalidOperationException("Cannot call stop with a tag which is not of type HandlerController");
            }
            ((IHandlerController)(t.Tag)).TrafficHandler.Stop();
        }

        /// <summary>
        /// Checks whether the given controller is contained in this compilation.
        /// </summary>
        /// <param name="hcToCheck">The controller to check for</param>
        /// <returns>A bool indicating whether the given controller is contained in this compilation.</returns>
        public bool Contains(IHandlerController hcToCheck)
        {
            return lActiveHandlers.Contains(hcToCheck);
        }

        /// <summary>
        /// Clears this compilation without shutting down the handlers and controllers.
        /// </summary>
        public void Clear()
        {
            while (lActiveHandlers.Count > 0)
            {
                Remove(lActiveHandlers[0]);
            }
        }
        
        /// <summary>
        /// Clears this compilation and shuts down all the handlers and controllers.
        /// </summary>
        public void Shutdown(TaskNotificationCallback tncCallback)
        {
            List<Task> lCleanup = new List<Task>();
            List<Task> lShutdown = new List<Task>();
            List<Task> lStop = new List<Task>();

            foreach (IHandlerController hcToRemove in lActiveHandlers)
            {
                Task tCleanup = new Task(CleanupHandler, tncCallback, hcToRemove, "Cleanup " + hcToRemove.Name);
                Task tUnlink = new Task(UnlinkHandler, tncCallback, hcToRemove, "Unlink " + hcToRemove.Name);
                Task tStop = new Task(StopHandler, tncCallback, hcToRemove, "Stop " + hcToRemove.Name);

                lCleanup.Add(tCleanup);
                lShutdown.Add(tUnlink);
                lStop.Add(tStop);
            }

            foreach (Task t in lCleanup)
            {
                t.Execute();
            }
            foreach (Task t in lShutdown)
            {
                t.Execute();
            }
            foreach (Task t in lStop)
            {
                t.Execute();
            }

            Clear();
        }

        #region Connection Handling

        void thp_HandlerDetached(TrafficHandlerPort sender, TrafficHandlerPort attacher)
        {
            RemoveLink(sender, attacher);
        }

        /// <summary>
        /// Removes a link with the given ports from the link list.
        /// </summary>
        /// <param name="sender">The first port of the link.</param>
        /// <param name="attacher">The second port of the link.</param>
        private void RemoveLink(TrafficHandlerPort sender, TrafficHandlerPort attacher)
        {
            for (int iC1 = lLinks.Count - 1; iC1 >= 0; iC1--)
            {
                if (lLinks[iC1].Alice == sender && lLinks[iC1].Bob == attacher)
                {
                    lLinks.RemoveAt(iC1);
                }
            }
        }

        void thp_HandlerAttached(TrafficHandlerPort sender, TrafficHandlerPort attacher)
        {
            AddLink(sender, attacher);
        }

        /// <summary>
        /// Adds a link with the given ports to the link list
        /// </summary>
        /// <param name="sender">The first port of the link.</param>
        /// <param name="attacher">The second port of the link.</param>
        protected virtual void AddLink(TrafficHandlerPort sender, TrafficHandlerPort attacher)
        {
            lLinks.Add(new Link(sender, attacher));
        }

        /// <summary>
        /// Checks whether the two given ports are connected.
        /// </summary>
        /// <param name="thpAlice">The first port to check.</param>
        /// <param name="thpBob">The second port to check.</param>
        /// <returns>A bool indicating whether the two given ports are connected.</returns>
        public bool IsConnected(TrafficHandlerPort thpAlice, TrafficHandlerPort thpBob)
        {
            return thpAlice.IsConnectedTo(thpBob) || thpBob.IsConnectedTo(thpAlice);
        }

        /// <summary>
        /// Connects two traffic handler ports.
        /// </summary>
        /// <param name="thpAlice">The first port to connect.</param>
        /// <param name="thpBob">The second port to connect.</param>
        public void Connect(TrafficHandlerPort thpAlice, TrafficHandlerPort thpBob)
        {
            thpAlice.AttachHandler(thpBob);
            thpBob.AttachHandler(thpAlice);
        }

        /// <summary>
        /// Disconnects two traffic handler ports.
        /// </summary>
        /// <param name="thpAlice">The first port to disconnect.</param>
        /// <param name="thpBob">The second port to disconnect.</param>
        public void Disconnect(TrafficHandlerPort thpAlice, TrafficHandlerPort thpBob)
        {
            thpAlice.DetachHandler(thpBob);
            thpBob.DetachHandler(thpAlice);
        }

        #endregion

        #region Getters

        /// <summary>
        /// Gets all active scanners in the current compilation.
        /// </summary>
        /// <returns>All active scanners in the current compilation</returns>
        public IScanner[] GetScanners()
        {
            return GetAllOfType<IScanner>();
        }

        /// <summary>
        /// Gets all active MITM attacks in the current compilation.
        /// </summary>
        /// <returns>All active MITM attacks in the current compilation</returns>
        public IMITMAttack[] GetMITMAttacks()
        {
            return GetAllOfType<IMITMAttack>();
        }

        /// <summary>
        /// Gets all active attacks in the current compilation.
        /// </summary>
        /// <returns>All active attacks in the current compilation</returns>
        public ISingleHostAttack[] GetAttacks()
        {
            return GetAllOfType<ISingleHostAttack>();
        }

        /// <summary>
        /// Gets all active routers in the current compilation.
        /// </summary>
        /// <returns>All active routers in the current compilation</returns>
        public IRouter[] GetRouters()
        {
            return GetAllOfType<IRouter>();
        }

        /// <summary>
        /// Gets all active traffic handlers in the current compilation.
        /// </summary>
        public TrafficHandler[] ActiveHandlers
        {
            get
            {
                List<TrafficHandler> lList = new List<TrafficHandler>();

                foreach (HandlerController hc in lActiveHandlers)
                {
                        lList.Add(hc.TrafficHandler);
                }

                return lList.ToArray();
            }
        }
 
        /// <summary>
        /// Gets all links of this compilation.
        /// </summary>
        public Link[] Links
        {
            get 
            {
                return lLinks.ToArray();
            }
        }

        private T[] GetAllOfType<T>()
        {
            List<T> lList = new List<T>();

            foreach (IHandlerController hc in lActiveHandlers)
            {
                if (hc.TrafficHandler is T)
                {
                    lList.Add((T)((object)hc.TrafficHandler));
                }
            }

            return lList.ToArray();
        }

        /// <summary>
        /// Gets a controller by its name.
        /// </summary>
        /// <param name="strName">The name to get the controller for.</param>
        /// <returns>The controller with the given name or null if no controller was found.</returns>
        public IHandlerController GetControllerForName(string strName)
        {
            foreach (IHandlerController hc in lActiveHandlers)
            {
                if (hc.Name == strName)
                {
                    return hc;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a traffic handler by its name.
        /// </summary>
        /// <param name="strName">The name to get the traffic handler for.</param>
        /// <returns>The traffic handler with the given name, or null if no traffic handler was found.</returns>
        public TrafficHandler GetHandlerForName(string strName)
        {
            IHandlerController hc = GetControllerForName(strName);

            if (hc != null)
            {
                return hc.TrafficHandler;
            }

            return null;
        }

        /// <summary>
        /// Gets the controller for the given traffic handler.
        /// </summary>
        /// <param name="thHandler">The handler to get the controller for</param>
        /// <returns>The controller which belongs to the given handler, or null if no controller was found.</returns>
        public IHandlerController GetControllerForHandler(TrafficHandler thHandler)
        {
            foreach (IHandlerController hc in lActiveHandlers)
            {
                if (hc.TrafficHandler == thHandler)
                {
                    return hc;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets all active controllers.
        /// </summary>
        public IHandlerController[] Controllers
        {
            get { return lActiveHandlers.ToArray(); }
        }

        #endregion

        #region File IO
        
        /// <summary>
        /// Writes this compilation to the file with the given name.
        /// </summary>
        /// <param name="strFilename">The name of the file to write this compilation to.</param>
        public void SaveToFile(string strFilename)
        {
            XmlWriterSettings xmlSettings = new XmlWriterSettings();
            xmlSettings.CloseOutput = true;
            xmlSettings.Indent = true;
            xmlSettings.ConformanceLevel = ConformanceLevel.Document;
            xmlSettings.Encoding = System.Text.UnicodeEncoding.Unicode;
            xmlSettings.OmitXmlDeclaration = false;
            XmlWriter wWriter = XmlWriter.Create(strFilename, xmlSettings);

            wWriter.WriteStartDocument();
            wWriter.WriteComment(XMLComment);
            WriteDocument(wWriter);
            wWriter.WriteEndDocument();
            wWriter.Close();
        }

        private void WriteDocument(XmlWriter wWriter)
        {
            wWriter.WriteStartElement(XMLNetLabFile, XMLNameSpace);

            //Write Interfaces

            foreach (IHandlerController hc in lActiveHandlers)
            {
                if (hc.BaseDefinition.PluginType == PluginTypes.Interface)
                {
                    IInterfaceDefinition ipiInterface = (IInterfaceDefinition)hc.BaseDefinition;
                    wWriter.WriteStartElement(XMLInterface);

                    wWriter.WriteStartAttribute("systemName");
                    wWriter.WriteString(ipiInterface.Name);
                    wWriter.WriteEndAttribute();

                    wWriter.WriteStartAttribute("systemGUID");
                    wWriter.WriteString(ToDeviceGUID(ipiInterface.InterfaceGUID));
                    wWriter.WriteEndAttribute();

                    wWriter.WriteStartAttribute("name");
                    wWriter.WriteString(hc.Name);
                    wWriter.WriteEndAttribute();

                    wWriter.WriteStartAttribute("key");
                    wWriter.WriteString(hc.BaseDefinition.PluginKey);
                    wWriter.WriteEndAttribute();

                    wWriter.WriteEndElement();
                }
            }

            //Write Handlers

            foreach (IHandlerController hc in lActiveHandlers)
            {
                if (hc.BaseDefinition.PluginType == PluginTypes.TrafficHandler)
                {
                    wWriter.WriteStartElement(XMLHandler);

                    wWriter.WriteStartAttribute("name");
                    wWriter.WriteString(hc.Name);
                    wWriter.WriteEndAttribute();

                    wWriter.WriteStartAttribute("key");
                    wWriter.WriteString(hc.BaseDefinition.PluginKey);
                    wWriter.WriteEndAttribute();

                    wWriter.WriteEndElement();
                }
            }

            // Write Links

            foreach (Link l in lLinks)
            {
                wWriter.WriteStartElement(XMLLink);

                wWriter.WriteStartElement(XMLPort);
                wWriter.WriteStartAttribute("handlerName");
                wWriter.WriteString(l.Alice.ParentController.Name);
                wWriter.WriteEndAttribute();
                wWriter.WriteString(l.Alice.Name);
                wWriter.WriteEndElement();

                wWriter.WriteStartElement(XMLPort);
                wWriter.WriteStartAttribute("handlerName");
                wWriter.WriteString(l.Bob.ParentController.Name);
                wWriter.WriteEndAttribute();
                wWriter.WriteString(l.Bob.Name);
                wWriter.WriteEndElement();

                wWriter.WriteEndElement();
            }

            //Write Interface Configuration

            foreach (IHandlerController hc in lActiveHandlers)
            {
                if (hc.BaseDefinition.PluginType == PluginTypes.Interface)
                {
                    wWriter.WriteStartElement(XMLConfiguration);

                    wWriter.WriteStartAttribute("name");
                    wWriter.WriteString(hc.Name);
                    wWriter.WriteEndAttribute();

                    hc.SaveConfiguration(wWriter);

                    WriteCustomProperties(wWriter, hc);

                    wWriter.WriteEndElement();
                }
            }

            //Write Handler Configuration

            foreach (IHandlerController hc in lActiveHandlers)
            {
                if (hc.BaseDefinition.PluginType == PluginTypes.TrafficHandler)
                {
                    wWriter.WriteStartElement(XMLConfiguration);

                    wWriter.WriteStartAttribute("name");
                    wWriter.WriteString(hc.Name);
                    wWriter.WriteEndAttribute();

                    hc.SaveConfiguration(wWriter);

                    WriteCustomProperties(wWriter, hc);

                    wWriter.WriteEndElement();
                }
            }

            wWriter.WriteEndElement();
        }

        private void WriteCustomProperties(XmlWriter wWriter, IHandlerController hc)
        {
            foreach (KeyValuePair<string, object> nvi in hc.Properties)
            {
                wWriter.WriteStartElement(XMLCustomProperty);
                wWriter.WriteStartAttribute("name");
                wWriter.WriteString(nvi.Key);
                wWriter.WriteEndAttribute();

                wWriter.WriteStartAttribute("type");
                if (nvi.Value is int) { wWriter.WriteString("int"); }
                else if (nvi.Value is string) { wWriter.WriteString("string"); }
                else if (nvi.Value is double) { wWriter.WriteString("double"); }
                else if (nvi.Value is float) { wWriter.WriteString("float"); }
                else if (nvi.Value is IPAddress) { wWriter.WriteString("IPAddress"); }
                else if (nvi.Value is Subnetmask) { wWriter.WriteString("Subnetmask"); }
                else if (nvi.Value is MACAddress) { wWriter.WriteString("MACAddress"); }
                else { wWriter.WriteString("Unknown"); }
                wWriter.WriteEndAttribute();

                wWriter.WriteString(nvi.Value.ToString());

                wWriter.WriteEndElement();
            }
        }

        private string ToDeviceGUID(string strInterfaceName)
        {
            string strDevice = strInterfaceName;
            if (strInterfaceName.StartsWith("\\Device\\NPF_"))
            {
                strDevice = strInterfaceName.Substring(12);
            }
            return strDevice;
        }

        /// <summary>
        /// Loads a compilation from a file. 
        /// </summary>
        /// <param name="strFilename">The file to load the compilation from</param>
        public void LoadFromFile(string strFilename)
        {
            LoadFromFile(strFilename, null, null);
        }

        /// <summary>
        /// Loads a compilation from a file. 
        /// </summary>
        /// <param name="strFilename">The file to load the compilation from</param>
        /// <param name="arKnownDefinitions">An array containing all known handler definitions</param>
        /// <param name="pluginNotFoundCallback">A callback to handle the case of missing plug-ins</param>
        public void LoadFromFile(string strFilename, InterfaceNotFoundCallback ifaceNotFouncCallback, PluginNotFoundCallback pluginNotFoundCallback)
        {
            IPlugin[] arPlugins = GetPlugins();

            XmlReaderSettings xmlSettings = new XmlReaderSettings();
            xmlSettings.CloseInput = true;
            xmlSettings.ConformanceLevel = ConformanceLevel.Document;
            xmlSettings.IgnoreComments = true;
            xmlSettings.IgnoreWhitespace = true;
            xmlSettings.ProhibitDtd = true;
           
            Stream sSchema = File.Open(Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath), "NLMLCompilation.xsd"), FileMode.Open);
            XmlSchema xmlSchema = XmlSchema.Read(sSchema, new ValidationEventHandler(XmlSchema_Validate));
            sSchema.Close();

            xmlSettings.Schemas.Add(xmlSchema);
            xmlSettings.ValidationType = ValidationType.Schema;

            XmlReader xmlReader = XmlReader.Create(strFilename, xmlSettings);

            try
            {
                while (!xmlReader.EOF)
                {
                    if (xmlReader.NodeType == XmlNodeType.Element)
                    {
                        if (xmlReader.Name == XMLHandler)
                        {
                            string strName = xmlReader.GetAttribute("name");
                            string strKey = xmlReader.GetAttribute("key");

                            IHandlerController hc = CreateHandlerInstance(arPlugins, strKey, strName, pluginNotFoundCallback);

                            if (hc != null)
                            {
                                hc.Name = strName;
                                Add(hc);
                            }
                            xmlReader.Read();
                        }
                        else if (xmlReader.Name == XMLConfiguration)
                        {

                            string strName = xmlReader.GetAttribute("name");
                            IHandlerController hc = GetControllerForName(strName);

                            xmlReader.Read();

                            hc.LoadConfiguration(xmlReader);

                            ReadCustomProperties(xmlReader, hc);

                        }
                        else if (xmlReader.Name == XMLInterface)
                        {
                            string strName = xmlReader.GetAttribute("name");
                            string strKey = xmlReader.GetAttribute("key");
                            string strSystemGUID = xmlReader.GetAttribute("systemGUID");
                            string strInterfaceName = xmlReader.GetAttribute("systemName");

                            IHandlerController hc = CreateInterfaceInstance(arPlugins, strKey, strName, strSystemGUID, strInterfaceName, ifaceNotFouncCallback);

                            if (hc != null)
                            {
                                hc.Name = strName;
                                Add(hc);
                            }
                            xmlReader.Read();
                        }
                        else if (xmlReader.Name == XMLLink)
                        {
                            xmlReader.Read();
                            string strAliceKey = xmlReader.GetAttribute("handlerName");
                            xmlReader.Read();
                            string strAlicePort = xmlReader.Value;
                            xmlReader.Read();
                            xmlReader.Read();
                            string strBobKey = xmlReader.GetAttribute("handlerName");
                            xmlReader.Read();
                            string strBobPort = xmlReader.Value;

                            IHandlerController hcAlice = GetControllerForName(strAliceKey);
                            IHandlerController hcBob = GetControllerForName(strBobKey);

                            if (hcAlice != null && hcBob != null)
                            {
                                TrafficHandlerPort pAlice = hcAlice.GetPortForName(strAlicePort);
                                TrafficHandlerPort pBob = hcBob.GetPortForName(strBobPort);

                                if (pAlice != null && pBob != null)
                                {
                                    pAlice.AttachHandler(pBob);
                                }
                                else
                                {
                                    throw new InvalidOperationException("Could not find one of the traffic handler ports to link.");
                                }
                            }
                            else
                            {
                                throw new InvalidOperationException("Could not find one of the traffic handlers to link together.");
                            }
                        }
                        else
                        {
                            xmlReader.Read();
                        }
                    }
                    else
                    {
                        xmlReader.Read();
                    }
                }
            }
            catch(Exception ex)
            {
                QuickShutdown();
                throw ex;
            }
            finally
            {
                xmlReader.Close();
            }
        }

        private IHandlerController CreateInterfaceInstance(IPlugin[] arKnownDefinitions, string strKey, string strName, string strSystemGUID, string strInterfaceName, InterfaceNotFoundCallback ifaceNotFouncCallback)
        {
            IHandlerController hc = null;

            foreach (IPlugin hd in arKnownDefinitions)
            {
                if (hd.PluginType == PluginTypes.Interface && hd.PluginKey == strKey
                    && ToDeviceGUID(((IInterfaceDefinition)hd).InterfaceGUID) == ToDeviceGUID(strSystemGUID))
                {
                    hc = ((IInterfaceDefinition)hd).Create(Environment);
                }
            } 
            
            if (hc == null && ifaceNotFouncCallback != null)
            {
                IHandlerDefinition hdUserDef = ifaceNotFouncCallback(this, new InterfaceNotFoundEventArgs(strName, strSystemGUID, strKey));
                if (hdUserDef != null) { hc = hdUserDef.Create(Environment); }
            }

            if (hc != null)
            {
                hc.Name = strName;
            }

            return hc;
        }

        private void ReadCustomProperties(XmlReader xmlReader, IHandlerController hc)
        {
            while (xmlReader.Name == XMLCustomProperty)
            {
                string strPropertyName = xmlReader.GetAttribute("name");
                string strType = xmlReader.GetAttribute("type");
                object oValue;

                xmlReader.Read();
                string strProperty = xmlReader.Value;
                xmlReader.Read();

                if (strType == "int") { oValue = Int32.Parse(strProperty); }
                else if (strType == "string") { oValue = strProperty; }
                else if (strType == "double") { oValue = Double.Parse(strProperty); }
                else if (strType == "float") { oValue = float.Parse(strProperty); }
                else if (strType == "IPAddress") { oValue = IPAddress.Parse(strProperty); }
                else if (strType == "Subnetmask") { oValue = Subnetmask.Parse(strProperty); }
                else if (strType == "MACAddress") { oValue = MACAddress.Parse(strProperty); }
                else { oValue = strProperty; }

                if (hc.Properties.ContainsKey(strPropertyName))
                {
                    hc.Properties[strPropertyName] = oValue;
                }
                else
                {
                    hc.Properties.Add(strPropertyName, oValue);
                }
                xmlReader.Read();
            }
        }

        private IHandlerController CreateHandlerInstance(IPlugin[] arKnownDefinitions, string strKey, string strName, PluginNotFoundCallback pluginNotFoundCallback)
        {
            IHandlerController hc = null;

            foreach (IPlugin hd in arKnownDefinitions)
            {
                if (hd.PluginType == PluginTypes.TrafficHandler && hd.PluginKey == strKey)
                {
                    hc = ((IHandlerDefinition)hd).Create(Environment);
                }
            }

            if (hc == null && pluginNotFoundCallback != null)
            {
                IHandlerDefinition hdUserDef = pluginNotFoundCallback(this, new PluginNotFoundArgs(strName, strKey));
                if (hdUserDef != null) { hc = hdUserDef.Create(Environment); }
            }

            if (hc != null)
            {
                hc.Name = strName;
            }

            return hc;
        }

        private void XmlSchema_Validate(object sender, ValidationEventArgs args)
        {

        }

        #endregion

        public void QuickShutdown()
        {
            QuickShutdown(null);
        }

        public void Shutdown()
        {
            Shutdown(null);
        }

        public void QuickShutdown(TaskNotificationCallback tncCallback)
        {
            List<Task> lShutdown = new List<Task>();
            List<Task> lStop = new List<Task>();

            foreach (IHandlerController hcToRemove in lActiveHandlers)
            {
                Task tUnlink = new Task(UnlinkHandler, tncCallback, hcToRemove, "Unlink " + hcToRemove.Name);
                Task tStop = new Task(StopHandler, tncCallback, hcToRemove, "Stop " + hcToRemove.Name);

                lShutdown.Add(tUnlink);
                lStop.Add(tStop);
            }

            foreach (Task t in lShutdown)
            {
                t.Execute();
            }
            foreach (Task t in lStop)
            {
                t.Execute();
            }

            Clear();
        }
    }

    /// <summary>
    /// This class represents the arguments for an InterfaceNotFoundCallback.
    /// </summary>
    public class InterfaceNotFoundEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the system GUID of the missing interface.
        /// </summary>
        public string SystemGUID { get; private set; }
        /// <summary>
        /// Gets the name of the missing interface.
        /// </summary>
        public string InterfaceName { get; private set; }
        /// <summary>
        /// Gets the plugin key of the missing interface.
        /// </summary>
        public string PluginKey { get; private set; }

        /// <summary>
        /// Creates a new instance of this class. 
        /// </summary>
        /// <param name="strInterfaceName">The system GUID of the missing interface</param>
        /// <param name="strSystemGUID">The name of the missing interface</param>
        /// <param name="strPluginKey">The plugin key of the missing interface</param>
        public InterfaceNotFoundEventArgs(string strInterfaceName, string strSystemGUID, string strPluginKey)
        {
            SystemGUID = strSystemGUID;
            InterfaceName = strInterfaceName;
            PluginKey = strPluginKey;
        }
    }

    /// <summary>
    /// This class represents the arguments for a PluginNotFoundCallback.
    /// </summary>
    public class PluginNotFoundArgs : EventArgs
    {
        /// <summary>
        /// Gets the name of the missing plug-in.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Gets the plugin key of the missing plug-in.
        /// </summary>
        public string PluginKey { get; private set; }

        /// <summary>
        /// Creates a new instance of this class. 
        /// </summary>
        /// <param name="strName">The name of the missing plug-in</param>
        /// <param name="strPluginKey">The plugin key of the missing plug-in</param>
        public PluginNotFoundArgs(string strName, string strPluginKey)
        {
            Name = strName;
            PluginKey = strPluginKey;
        }
    }
}
