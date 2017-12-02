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

namespace eExNLML
{
    /// <summary>
    /// This class represents the arguments of a task notification.
    /// </summary>
    public class TaskNotificationArgs : EventArgs
    {
        /// <summary>
        /// Gets the status of the task
        /// </summary>
        public TaskStatus Status { get; private set; }
        /// <summary>
        /// Gets the error associated with this task or null if no error happened
        /// </summary>
        public Exception Error { get; private set; }
        /// <summary>
        /// Gets a tag associated with the task
        /// </summary>
        public object Tag { get; private set; }
        /// <summary>
        /// Gets the description of the task
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="sStatus">The status of the task</param>
        /// <param name="eError">The error associated with this task or null if no error happened</param>
        /// <param name="oTag">A tag associated with the task</param>
        /// <param name="strDescription">The description of the task</param>
        public TaskNotificationArgs(TaskStatus sStatus, Exception eError, object oTag, string strDescription)
        {
            Status = sStatus;
            Error = eError;
            Tag = oTag;
            Description = strDescription;
        }
    }

    /// <summary>
    /// Defines various statistics of a task
    /// </summary>
    public enum TaskStatus
    {
        /// <summary>
        /// The task termintaed with an error
        /// </summary>
        Error = 0,
        /// <summary>
        /// The task finished successfully
        /// </summary>
        Finished = 1,
        /// <summary>
        /// The task was started
        /// </summary>
        Started = 2,
        /// <summary>
        /// The task was created and is ready for execution
        /// </summary>
        Created = 3
    }

    /// <summary>
    /// Defines a callback to handle errors and notifications during several tasks.
    /// </summary>
    /// <param name="sender">The class which called the callback.</param>
    /// <param name="args">The arguments of the event.</param>
    public delegate void TaskNotificationCallback(object sender, TaskNotificationArgs args);

    /// <summary>
    /// Defines a task callback, which is used to call a method from a task
    /// </summary>
    /// <param name="sender">The task which called the callback</param>
    public delegate void TaskCallback(Task sender);

    /// <summary>
    /// This class defines a task.
    /// </summary>
    public class Task
    {
        private TaskNotificationCallback cCallback;
        private TaskCallback cTaskToExecute;

        /// <summary>
        /// Gets the status of the task
        /// </summary>
        public TaskStatus Status { get; private set; }
        /// <summary>
        /// Gets the error associated with this task or null if no error happened
        /// </summary>
        public Exception Error { get; private set; }
        /// <summary>
        /// Gets a tag associated with the task
        /// </summary>
        public object Tag { get; private set; }
        /// <summary>
        /// Gets the description of the task
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="cTask">The method to execute</param>
        /// <param name="cTaskNotificationCallback">The callback method for task notifications</param>
        /// <param name="oTag">A tag associated with the task</param>
        /// <param name="strDescription">The description of the task</param>
        public Task(TaskCallback cTask, TaskNotificationCallback cTaskNotificationCallback, object oTag, string strDescription)
        {
            if (cTask == null)
            {
                throw new ArgumentException("Cannot create a task without a method to execute.");
            }
            cCallback = cTaskNotificationCallback;
            cTaskToExecute = cTask;
            Tag = oTag;
            Description = strDescription;
            Status = TaskStatus.Created;

            if (cCallback != null)
            {
                cCallback.Invoke(this, new TaskNotificationArgs(Status, Error, Tag, Description));
            }
        }

        /// <summary>
        /// Executes this task on the calling thread.
        /// </summary>
        public void Execute()
        {
            if (Status != TaskStatus.Created)
            {
                throw new InvalidOperationException("The task has already been started");
            }

            Status = TaskStatus.Started;

            if (cCallback != null)
            {
                cCallback.Invoke(this, new TaskNotificationArgs(Status, Error, Tag, Description));
            }

            try
            {
                cTaskToExecute(this);

                Status = TaskStatus.Finished;

                if (cCallback != null)
                {
                    cCallback.Invoke(this, new TaskNotificationArgs(Status, Error, Tag, Description));
                }
            }
            catch (Exception ex)
            {
                Status = TaskStatus.Error;
                Error = ex;

                if (cCallback != null)
                {
                    cCallback.Invoke(this, new TaskNotificationArgs(Status, Error, Tag, Description));
                }
                else
                {
                    throw ex;
                }
            }

        }
    }
}
