// This source file is part of the eEx Network Library
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

namespace eExNetworkLibrary
{
    /// <summary>
    /// This class represents a running object
    /// </summary>
    public abstract class RunningObject : IDisposable
    {
        /// <summary>
        /// This variable has to be set true or false according to the objects running state..
        /// E.g. if the object's worker threads are supposed to stop, bSouldRun should be set to false.
        /// As soon as the worker threads really exit, bIsRunning is set to false.
        /// </summary>
        protected bool bIsRunning;      
        
        /// <summary>
        /// This variable has to be set true or false according to the objects desireable running state.
        /// E.g. if the object's worker threads are supposed to stop, bSouldRun should be set to false.
        /// As soon as the worker threads really exit, bIsRunning is set to false.
        /// </summary>
        protected bool bSouldRun; 

        /// <summary>
        /// Returns a bool indicating whether this running object is running.
        /// </summary>
        public bool IsRunning
        {
            get { return bIsRunning; }
        }

        /// <summary>
        /// A method called to stop the current running object.
        /// </summary>
        public abstract void Stop();

        /// <summary>
        /// A method called to start the current running object.
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// A method called to pause the current running object.
        /// </summary>
        public abstract void Pause();

        #region IDisposable Members

        /// <summary>
        /// Disposes this running object
        /// </summary>
        public virtual void Dispose()
        {
            Stop();
        }

        #endregion

        /// <summary>
        /// Disposes this running object
        /// </summary>
        ~RunningObject()
        {   
            Stop();
        }

        /// <summary>
        /// Invokes a delegate on any external object with the given params and waits for the invoke's completion.
        /// This method automatically determines whether dynamic invoking is possible or a invoke over the ISynchronizeInvoke interface is required.
        /// </summary>
        /// <param name="d">The delgate to invoke</param>
        /// <param name="param">The params for the invocation</param>
        protected void InvokeExternal(Delegate d, object param)
        {
            if (d != null)
            {
                foreach (Delegate dDelgate in d.GetInvocationList())
                {
                    if (dDelgate.Target != null && dDelgate.Target.GetType().GetInterface(typeof(System.ComponentModel.ISynchronizeInvoke).Name, true) != null
                        && ((System.ComponentModel.ISynchronizeInvoke)(dDelgate.Target)).InvokeRequired)
                    {
                        ((System.ComponentModel.ISynchronizeInvoke)(dDelgate.Target)).Invoke(dDelgate, new object[] { this, param });
                    }
                    else
                    {
                        dDelgate.DynamicInvoke(this, param);
                    }
                }
            }
        }

        /// <summary>
        /// Invokes a delegate asyncronously on any external object with the given params.
        /// This method automatically determines whether dynamic invoking is possible or a invoke over the ISynchronizeInvoke interface is required.
        /// </summary>
        /// <param name="d">The delgate to invoke</param>
        /// <param name="param">The params for the invocation</param>
        protected void InvokeExternalAsync(Delegate d, object param)
        {
            if (d != null)
            {
                foreach (Delegate dDelgate in d.GetInvocationList())
                {
                    if (dDelgate.Target != null && dDelgate.Target.GetType().GetInterface(typeof(System.ComponentModel.ISynchronizeInvoke).Name, true) != null
                        && ((System.ComponentModel.ISynchronizeInvoke)(dDelgate.Target)).InvokeRequired)
                    {
                        ((System.ComponentModel.ISynchronizeInvoke)(dDelgate.Target)).BeginInvoke(dDelgate, new object[] { this, param });
                    }
                    else
                    {
                        dDelgate.DynamicInvoke(this, param);
                    }
                }
            }
        }

        /// <summary>
        /// Invokes a delegate asyncronously on any external object with EventArgs.Empty as params.
        /// This method automatically determines whether dynamic invoking is possible or a invoke over the ISynchronizeInvoke interface is required.
        /// </summary>
        /// <param name="d">The delgate to invoke</param>
        protected void InvokeExternalAsync(Delegate d)
        {
            InvokeExternalAsync(d, EventArgs.Empty);
        }
        /// <summary>
        /// Invokes a delegate on any external object with EventArgs.Empty as params and waits for the invoke's completion.
        /// This method automatically determines whether dynamic invoking is possible or a invoke over the ISynchronizeInvoke interface is required.
        /// </summary>
        /// <param name="d">The delgate to invoke</param>
        protected void InvokeExternal(Delegate d)
        {
            InvokeExternal(d, EventArgs.Empty);
        }
    }
}
