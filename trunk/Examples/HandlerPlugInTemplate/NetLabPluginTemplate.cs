using System;
using System.Collections.Generic;
using System.Text;
using eExNetLab.Extensibility;
using eExNetLab;

// This is a template for a plug-in on NetLab-Layer.
// Notice that both classes inherit from classes in the eExNetLab.Extensibility namespace.
// It can be confusing that this plug-in looks very similar to the plugin on eExNLML layer, 
// but it is important to do not mix up the plug-in types.
namespace PluginTemplate
{
    // This is the plug-in definition it is only instantiated once and 
    // responsible for providing information about your plug-in
    public class NetLabPluginDefinition : eExNetLab.Extensibility.HandlerDefinition
    {
        public NetLabPluginDefinition() { }

        /// <summary>
        /// Must return a icon for this plug-in or null to use the default icon
        /// </summary>
        protected override System.Drawing.Image GetIcon()
        {
            return null;
        }

        /// <summary>
        /// Must create the NLML base definition for this NetLabHandlerDefinition.
        /// </summary>
        /// <returns>The base definition for this NetLabHandlerDefinition</returns>
        protected override eExNLML.Extensibility.IHandlerDefinition CreateBaseDefinition()
        {
            // Return the NLML-layer version of your plug-in. 
            // The plug-in engine will automatically use all available information from it, so 
            // you only have to worry about the UI when writing a NetLab plug-in.
            return new PluginDefinition();
        }

        /// <summary>
        /// Must create the HandlerController on NetLab layer for this NetLabHandlerDefiniton. 
        /// </summary>
        /// <param name="env">The environment for the handler controller.</param>
        /// <returns>The eExNetLab.Extensibility.IHandlerController for this NetLabHandlerDefiniton.</returns>
        public override eExNetLab.Extensibility.IHandlerController Create(eExNetLab.INetLabEnvironment env)
        {
            return new NetLabPluginController(this, env);
        }
    }

    public class NetLabPluginController : HandlerController
    {
        public NetLabPluginController(IHandlerDefinition hdBaseDefinition, INetLabEnvironment iEnvironment) : base(hdBaseDefinition, iEnvironment, null) { }

        /// <summary>
        /// When overriden by a derived class, must return the base controller 
        /// on NLML-layer of this controller.
        /// </summary>
        /// <param name="env">The environment to create the base controller for.</param>
        /// <param name="param">A parameter associated with the controller to create</param>
        /// <returns>The base controller of this controller</returns>
        protected override eExNLML.Extensibility.IHandlerController GetBaseController(eExNetLab.INetLabEnvironment env, object param)
        {
            //Simply return a new instance of your plugin controller on NLML-layer.
            //You can use the BaseDefinition property to access the base definition on NLML-layer.
            return new PluginController(BaseDefinition, env); 
        }

        /// <summary>
        /// Must create the settings UI for the given environment or null to use no settings UI. This UI will be shown left of the workbench if the traffic handler gets selected.
        /// </summary>
        /// <param name="env">The environment to create the settings UI for</param>
        /// <param name="param">A parameter associated with the component to create</param>
        /// <returns>The settings UI for the traffic handlerdefined by this controller </returns>
        protected override eExNetLab.eExNetLabUIComponentBase CreateSettingsUI(eExNetLab.INetLabEnvironment env, object param)
        {
            return null;
        }

        /// <summary>
        /// When overriden by a derived class, must create the statistics UI for the given environment or null to use no statistics UI. This UI will be shown below the workbench if the traffic handler gets selected.
        /// </summary>
        /// <param name="env">The environment to create the statistics UI for</param>
        /// <param name="param">A parameter associated with the component to create</param>
        /// <returns>The statistics UI for the traffic handler defined by this controller</returns>
        protected override eExNetLab.eExNetLabUIComponentBase CreateStatisticsUI(eExNetLab.INetLabEnvironment env, object param)
        {
            return null;
        }

        /// <summary>
        /// When overriden by a derived class, must create the traffic handler UI for the given environment. This UI will be shown on the workbench.
        /// </summary>
        /// <param name="env">The environment to create the traffic handler UI for</param>
        /// <param name="param">A parameter associated with the component to create</param>
        /// <returns>The traffic handler UI for the traffic handler defined by this controller</returns>
        protected override eExNetLab.TrafficHandlerUIs.TrafficHandlerUI CreateTrafficHandlerUI(INetLabEnvironment env, object param)
        {
            // Implementing this is optional - the plug-in engine is capable of creating a default UI 
            // based on the port information provided by the NLML plug-in
            return base.CreateTrafficHandlerUI(env, param);
        }

        /// <summary>
        /// When overriden by a derived class, must create the port information which is used by the NetLab
        /// </summary>
        /// <param name="env">The environment to create the ports for</param>
        /// <param name="param">A parameter associated with the component to create</param>
        /// <returns>An array of traffic handler ports associated with this controller.</returns>
        protected override NetLabTrafficHandlerPort[] CreatePortInformations(INetLabEnvironment env, object param)
        {        
            // Implementing this is optional - the plug-in engine is capable of creating 
            // default ports based on the port information provided by the NLML plug-in
            return base.CreatePortInformations(env, param);
        }
    }
}
