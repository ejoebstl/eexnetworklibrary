// This source file is part of the eEx Network Library
//
// Author: 	    Emanuel Jöbstl <emi@eex-dev.net>
// Weblink: 	http://network.eex-dev.net
//		        http://eex.codeplex.com
//
// Thanks to FuleSnabel for a great advice on multi-threading here
// [http://stackoverflow.com/questions/7083618/alternative-for-using-slow-dynamicinvoke-on-muticast-delegate]
//
// Licensed under the GNU Library General Public License (LGPL) 
//
// (c) eex-dev 2007-2011

using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Reflection.Emit;
using System.ComponentModel;
using System.Reflection;

namespace eExNetworkLibrary.Threading
{
    /// <summary>
    /// A class providing methods for invoking multicast delegates safely concerning System.ComponentModel.ISychronizeInvoke. 
    /// This class creates a cached, statically typed delegate for each delegate it invokes to speed up the process of dynamic invoking. 
    /// </summary>
    public static class InvocationHelper
    {
        delegate void CachedMethodDelegate(object instance, object sender, object param);

        readonly static Dictionary<MethodInfo, CachedMethodDelegate> dictCachedMethods =
            new Dictionary<MethodInfo, CachedMethodDelegate>();

        static CachedMethodDelegate GetOrAdd(MethodInfo mMethod)
        {
            CachedMethodDelegate cachedDelegate;
            bool delegateExists;

            lock (dictCachedMethods)
            {
                delegateExists = dictCachedMethods.TryGetValue(mMethod, out cachedDelegate);
            }

            if (!delegateExists)
            {
                cachedDelegate = CreateCachedMethodDelegate(mMethod);
                lock (dictCachedMethods)
                {
                    dictCachedMethods[mMethod] = cachedDelegate;
                }
            }

            return cachedDelegate;
        }

        static CachedMethodDelegate CreateCachedMethodDelegate(MethodInfo methodInfo)
        {
            if (!methodInfo.DeclaringType.IsClass)
            {
                throw CreateArgumentExceptionForMethodInfo(methodInfo,"Declaring type must be class for method: {0}.{1}");
            }


            if (methodInfo.ReturnType != typeof(void))
            {
                throw CreateArgumentExceptionForMethodInfo(
                    methodInfo,
                    "Method must return void: {0}.{1}"
                    );
            }

            ParameterInfo[] parameters = methodInfo.GetParameters();
            if (parameters.Length != 2)
            {
                throw CreateArgumentExceptionForMethodInfo(methodInfo,"Method must have exactly two parameters: {0}.{1}");
            }


            if (parameters[0].ParameterType != typeof(object))
            {
                throw CreateArgumentExceptionForMethodInfo(methodInfo,"Method first parameter must be of type object: {0}.{1}");
            }

            Type secondParameterType = parameters[1].ParameterType;
            if (!typeof(object).IsAssignableFrom(secondParameterType))
            {
                throw CreateArgumentExceptionForMethodInfo(methodInfo,"Method second parameter must assignable to a variable of type Object: {0}.{1}");
            }

            DynamicMethod dynamicMethod = new DynamicMethod(
                String.Format(CultureInfo.InvariantCulture,
                    "Run_{0}_{1}",
                    methodInfo.DeclaringType.Name,
                    methodInfo.Name),
                null,
                new[] { typeof (object),typeof (object),typeof (object) },
                true);

            ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Castclass, methodInfo.DeclaringType);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldarg_2);
            ilGenerator.Emit(OpCodes.Isinst, secondParameterType);
            if (methodInfo.IsVirtual)
            {
                ilGenerator.EmitCall(OpCodes.Callvirt, methodInfo, null);
            }
            else
            {
                ilGenerator.EmitCall(OpCodes.Call, methodInfo, null);
            }
            ilGenerator.Emit(OpCodes.Ret);

            return (CachedMethodDelegate)dynamicMethod.CreateDelegate(typeof(CachedMethodDelegate));
        }

        static Exception CreateArgumentExceptionForMethodInfo(MethodInfo methodInfo,string message)
        {
            return new ArgumentException(
                String.Format(
                    CultureInfo.InvariantCulture,
                    message,
                    methodInfo.DeclaringType.FullName,
                    methodInfo.Name),
                "methodInfo");
        }


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
                    object objTarget = dDelgate.Target;
                    if (objTarget != null && objTarget is System.ComponentModel.ISynchronizeInvoke
                        && ((ISynchronizeInvoke)(objTarget)).InvokeRequired)
                    {
                        ((ISynchronizeInvoke)(objTarget)).Invoke(dDelgate, new object[] { sender, param });
                    }
                    else
                    {
                        if (objTarget != null)
                        {
                            MethodInfo miMethodInfo = dDelgate.Method;
                            CachedMethodDelegate dChachedDelegate = GetOrAdd(miMethodInfo);
                            dChachedDelegate(objTarget, sender, (EventArgs)param);
                        }
                        else
                        {
                            dDelgate.DynamicInvoke(sender, param);
                        }
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
                    object objTarget = dDelgate.Target;
                    if (objTarget != null && objTarget is System.ComponentModel.ISynchronizeInvoke
                        && ((System.ComponentModel.ISynchronizeInvoke)(objTarget)).InvokeRequired)
                    {
                        ((System.ComponentModel.ISynchronizeInvoke)(objTarget)).BeginInvoke(dDelgate, new object[] { sender, param });
                    }
                    else
                    {
                        if (objTarget != null)
                        {
                            MethodInfo miMethodInfo = dDelgate.Method;
                            CachedMethodDelegate dChachedDelegate = GetOrAdd(miMethodInfo);
                            dChachedDelegate(objTarget, sender, param);
                        }
                        else
                        {
                            dDelgate.DynamicInvoke(sender, param);
                        }
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
