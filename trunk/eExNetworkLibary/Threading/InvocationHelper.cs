using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary.Threading
{
    public static class InvocationHelper
    {

        /// <summary>
        /// Invokes a delegate on any external object with the given params and waits for the invoke's completion.
        /// This method automatically determines whether dynamic invoking is possible or a invoke over the ISynchronizeInvoke interface is required.
        /// The delegate has to be in the form Delegate(object sender, object param) for the invocation to work correctly. 
        /// </summary>
        /// <param name="d">The delgate to invoke</param>
        /// <param name="param">The params for the invocation</param>
        /// <param name="sender">The sender param of the invocation</param>
        public static void InvokeExternal(Delegate d, object param, object sender)
        {
            if (d != null)
            {
                foreach (Delegate dDelgate in d.GetInvocationList())
                {
                    if (dDelgate.Target != null && dDelgate.Target.GetType().GetInterface(typeof(System.ComponentModel.ISynchronizeInvoke).Name, true) != null
                        && ((System.ComponentModel.ISynchronizeInvoke)(dDelgate.Target)).InvokeRequired)
                    {
                        ((System.ComponentModel.ISynchronizeInvoke)(dDelgate.Target)).Invoke(dDelgate, new object[] { sender, param });
                    }
                    else
                    {
                        dDelgate.DynamicInvoke(sender, param);
                    }
                }
            }
        }

        /// <summary>
        /// Invokes a delegate asyncronously on any external object with the given params.
        /// This method automatically determines whether dynamic invoking is possible or a invoke over the ISynchronizeInvoke interface is required.
        /// The delegate has to be in the form Delegate(object sender, object param) for the invocation to work correctly. 
        /// </summary>
        /// <param name="d">The delgate to invoke</param>
        /// <param name="param">The params for the invocation</param>
        /// <param name="sender">The sender param of the invocation</param>
        public static void InvokeExternalAsync(Delegate d, object param, object sender)
        {
            if (d != null)
            {
                foreach (Delegate dDelgate in d.GetInvocationList())
                {
                    if (dDelgate.Target != null && dDelgate.Target.GetType().GetInterface(typeof(System.ComponentModel.ISynchronizeInvoke).Name, true) != null
                        && ((System.ComponentModel.ISynchronizeInvoke)(dDelgate.Target)).InvokeRequired)
                    {
                        ((System.ComponentModel.ISynchronizeInvoke)(dDelgate.Target)).BeginInvoke(dDelgate, new object[] { sender, param });
                    }
                    else
                    {
                        dDelgate.DynamicInvoke(sender, param);
                    }
                }
            }
        }

        /// <summary>
        /// Invokes a delegate asyncronously on any external object with EventArgs.Empty as params.
        /// This method automatically determines whether dynamic invoking is possible or a invoke over the ISynchronizeInvoke interface is required.
        /// The delegate has to be in the form Delegate(object sender, object param) for the invocation to work correctly. 
        /// </summary>
        /// <param name="d">The delgate to invoke</param>
        /// <param name="sender">The sender param of the invocation</param>
        public static void InvokeExternalAsync(Delegate d, object sender)
        {
            InvokeExternalAsync(d, EventArgs.Empty, sender);
        }
        /// <summary>
        /// Invokes a delegate on any external object with EventArgs.Empty as params and waits for the invoke's completion.
        /// This method automatically determines whether dynamic invoking is possible or a invoke over the ISynchronizeInvoke interface is required.
        /// The delegate has to be in the form Delegate(object sender, object param) for the invocation to work correctly. 
        /// </summary>
        /// <param name="d">The delgate to invoke</param>
        /// <param name="sender">The sender param of the invocation</param>
        public static void InvokeExternal(Delegate d, object sender)
        {
            InvokeExternal(d, EventArgs.Empty, sender);
        }
    }
}
