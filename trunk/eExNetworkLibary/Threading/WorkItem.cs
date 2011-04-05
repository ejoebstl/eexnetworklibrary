using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace eExNetworkLibrary.Threading
{
    /// <summary>
    /// A work item used internally by the ISynchronizeInvoke model
    /// </summary>
    class WorkItem : IAsyncResult
    {
        object[] aroArgs;
        object oAsyncState;
        bool bCompleted;
        Delegate dTarget;
        ManualResetEvent mreDone;
        object oMethodReturnValue;

        public WorkItem(object oAsyncState, Delegate dTarget, object[] aroArgs)
        {
            this.oAsyncState = oAsyncState;
            this.dTarget = dTarget;
            this.aroArgs = aroArgs;
            this.mreDone = new ManualResetEvent(false);
            this.bCompleted = false;
        }

        public void CallBack()
        {
            this.oMethodReturnValue = dTarget.DynamicInvoke(aroArgs);
            mreDone.Set();
            bCompleted = true;
        }

        public object MethodReturnValue
        {
            get { return oMethodReturnValue; }
        }

        public object AsyncState
        {
            get { return oAsyncState; }
        }

        public System.Threading.WaitHandle AsyncWaitHandle
        {
            get { return mreDone; }
        }

        public bool CompletedSynchronously
        {
            get { return false; }
        }

        public bool IsCompleted
        {
            get { return bCompleted; }
        }
    }
}
