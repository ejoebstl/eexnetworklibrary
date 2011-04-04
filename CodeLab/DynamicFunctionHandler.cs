using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary.CodeLab
{
    /// <summary>
    /// This traffic modifier provides the capability of running any class which implements IDynamicHandler.
    /// The IDynamicHandler implementation has to be simply assigned to the DynamicHandler property.
    /// This class can be used to run just in time compiled code via the IDynamicHandler interface.
    /// <example><code>
    /// // Load the sourcecode
    /// string strSourcecode = "your class sourcecode which implements IDynamicHandler goes here";
    ///
    /// // Create a new dynamic function handler
    /// DynamicFunctionHandler dfHandler = new DynamicFunctionHandler();
    /// 
    /// // Start the dynamic function handler
    /// dfHandler.Start();
    /// 
    /// // Create a new dynamic function compiler
    /// DynamicFunctionCompiler dfCompiler = new DynamicFunctionCompiler();
    /// 
    /// // Compile the sourcecode to a just in time plugin
    /// IDynamicHandler dynamicHandler = dfCompiler.BuildPreview(strSourcecode);
    /// 
    /// // Assign the just compiled dynamic handler to the dynamic function handler
    /// dfHandler.DynamicHandler = dynamicHandler;
    /// 
    /// </code></example>
    /// </summary>
    public class DynamicFunctionHandler : TrafficModifiers.TrafficModifier
    {
        private IDynamicHandler icCodeLabHandler;

        /// <summary>
        /// Gets or sets the dynamic handler
        /// </summary>
        public IDynamicHandler DynamicHandler
        {
            get
            {
                return icCodeLabHandler;
            }
            set
            {
                if (icCodeLabHandler != null)
                {
                    icCodeLabHandler.Cleanup();
                    icCodeLabHandler.Stop();
                }
                icCodeLabHandler = value;
                if (icCodeLabHandler != null)
                {
                    icCodeLabHandler.Start();
                }

                InvokePropertyChanged();
            }
        }

        /// <summary>
        /// Calls the ModifyTraffic method of the assigned IDynamicHandler with the given frame
        /// </summary>
        /// <param name="fInputFrame">The frame to handler</param>
        /// <returns>The modified frame or null</returns>
        protected override Frame ModifyTraffic(Frame fInputFrame)
        {
            if (icCodeLabHandler != null)
            {
                return icCodeLabHandler.ModifyTraffic(fInputFrame);
            }

            return fInputFrame;
        }

        /// <summary>
        /// Calls the Cleanup method of the assigned IDynamicHandler
        /// </summary>
        public override void Cleanup()
        {
            if (icCodeLabHandler != null)
            {
                icCodeLabHandler.Cleanup() ;
            }
        }

        /// <summary>
        /// Calls the Start method of the assigned IDynamicHandler
        /// </summary>
        public override void Start()
        {
            base.Start();
            if (icCodeLabHandler != null)
            {
                icCodeLabHandler.Start();
            }
        }

        /// <summary>
        /// Calls the Stop method of the assigned IDynamicHandler
        /// </summary>
        public override void Stop()
        {
            if (icCodeLabHandler != null)
            {
                icCodeLabHandler.Stop();
            }
            base.Stop();
        }
    }
}
