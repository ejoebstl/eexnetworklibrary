using System;
using System.Collections.Generic;
using System.Text;
using eExNLML.Extensibility;
using eExNLML;

// This is a template for a plug-in on NLML-Layer.
// Notice that both classes inherit from classes in the eExNLML.Extensibility namespace.
namespace PluginTemplate
{
    // This is the plug-in controller. 
    // This class is instantiated for each plug-in instance which is created. 
    public class PluginController : eExNLML.Extensibility.HandlerController
    {
        public PluginController(IHandlerDefinition hbDefinition, IEnvironment env)
            : base(hbDefinition, env, null) //Simply call the base constructor.
        { }

        /// <summary>
        /// Must create the traffic handler which is defined in this plug-in.
        /// This method is called once on plugin creation.
        /// </summary>
        /// <param name="param">A param associated with the controller to create</param>
        /// <returns>The traffic handler which is defined in this plug-in</returns>
        protected override eExNetworkLibrary.TrafficHandler Create(object param)
        {
            return new HandlerTemplate(); //Crate and return your handler/analyzer here.
        }

        /// <summary>
        /// Must create a configuration loader for the given traffic handler, which loads the configuration or null to do not support loading. 
        /// This method is called once on plugin creation.
        /// </summary>
        /// <param name="h">The traffic handler to create the configuration loader for</param>
        /// <param name="param">A param associated with the controller to create</param>
        /// <returns>The configuration loader for the given traffic handler or null, if no configuration loader should be used</returns>
        protected override eExNLML.IO.HandlerConfigurationLoader CreateConfigurationLoader(eExNetworkLibrary.TrafficHandler h, object param)
        {
            return null; //Create your configuration loader here, return null for no configuration loader
        }

        /// <summary>
        /// Must create a configuration writer for the given traffic handler, which writes the configuration or null to do not support saving. 
        /// This method is called once on plugin creation.
        /// </summary>
        /// <param name="h">The traffic handler to create the configuration writer for</param>
        /// <param name="param">A param associated with the controller to create</param>
        /// <returns>The configuration writer for the given traffic handler or null, if no configuration writer should be used</returns>
        protected override eExNLML.IO.HandlerConfigurationWriter CreateConfigurationWriter(eExNetworkLibrary.TrafficHandler h, object param)
        {
            return null; //Create your configuration writer here, return null for no configuration writer
        }

        /// <summary>
        /// Must create all traffic handler ports owned by this handler and also connect the appropriate event handlers.
        /// This method is called once on plugin creation.
        /// </summary>
        /// <param name="param">A param associated with the controller to create</param>
        /// <returns>All traffic handler ports owned by this handler</returns>
        protected override eExNLML.TrafficHandlerPort[] CreateTrafficHandlerPorts(eExNetworkLibrary.TrafficHandler h, object param)
        {
            return CreateDefaultPorts(h, true, true, false, false, false); //Create default ports, you can also add custom ports and return them all
        }
    }

    // This is the plug-in definition it is only instantiated once and responsible for providing information about your plug-in
    public class PluginDefinition : eExNLML.Extensibility.HandlerDefinition
    {
        public PluginDefinition()
        {
            //Set plug-in parameters
            Name = "NetLab and NLML Test Template";
            Description = "This is for tests only.";
            Author = "Emanuel Jöbstl";
            WebLink = "http://www.eex-dev.net";

            //Plug-In key - has to be unique
            PluginKey = "eex_nlml_netlab_plugin_template";

            //Plug-In type - defines what your plug-in is. (For example, it could also be an interface). 
            PluginType = PluginTypes.TrafficHandler;
        }

        /// <summary>
        /// This method must create a new HandlerController instance associated with this class.<br />
        /// This is called whenever your plug-in is instantiated.</br>
        /// </summary>
        /// <param name="env">The environment of the controller</param>
        /// <returns>The created HandlerController instance</returns>
        public override eExNLML.Extensibility.IHandlerController Create(IEnvironment env)
        {
            return new PluginController(this, env); //Instantiate a handler controller with the given environment.
        }
    }
}
