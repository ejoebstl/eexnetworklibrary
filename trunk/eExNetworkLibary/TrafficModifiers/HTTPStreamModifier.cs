using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.TrafficModifiers.StreamModification;
using System.Net;
using eExNetworkLibrary.TrafficModifiers.StreamModification.HTTP;

namespace eExNetworkLibrary.TrafficModifiers
{
    public class HTTPStreamModifier : TCPStreamModifier
    {
        List<HTTPStreamModifierAction> lActions;

        public event EventHandler<HTTPStreamModifierActionEventArgs> ActionAdded;
        public event EventHandler<HTTPStreamModifierActionEventArgs> ActionRemoved;

        /// <summary>
        /// Gets or sets the HTTP port to use.
        /// </summary>
        public int HTTPPort
        {
            get;
            set;
        }

        /// <summary>
        /// Adds an action to this modifier.
        /// </summary>
        /// <param name="aAction"></param>
        public void AddAction(HTTPStreamModifierAction aAction)
        {
            lActions.Add(aAction);
            InvokeExternalAsync(ActionAdded, new HTTPStreamModifierActionEventArgs(aAction));
        }

        /// <summary>
        /// Removes the given action.
        /// </summary>
        /// <param name="aAction">The action to remove</param>
        public void RemoveAction(HTTPStreamModifierAction aAction)
        {
            lActions.Remove(aAction);
            InvokeExternalAsync(ActionRemoved, new HTTPStreamModifierActionEventArgs(aAction));
        }

        /// <summary>
        /// Checks whether a given action is contained by this modifier.
        /// </summary>
        /// <param name="aAction">The action to check for.</param>
        /// <returns>A bool indicating whether a given action is contained by this modifier.</returns>
        public bool ContainsAction(HTTPStreamModifierAction aAction)
        {
            return lActions.Contains(aAction);
        }

        /// <summary>
        /// Clears all actions.
        /// </summary>
        public void ClearActions()
        {
            lActions.Clear();
        }

        /// <summary>
        /// Gets all actions.
        /// </summary>
        public HTTPStreamModifierAction[] Actions
        {
            get { return lActions.ToArray(); }
        }

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        public HTTPStreamModifier()
        {
            HTTPPort = 80;
            lActions = new List<HTTPStreamModifierAction>();
        }

        protected HTTPStreamModifierAction[] GetClonedActions()
        {
            List<HTTPStreamModifierAction> lClones = new List<HTTPStreamModifierAction>();
            
            foreach (HTTPStreamModifierAction htAction in lActions)
            {
                lClones.Add((HTTPStreamModifierAction)htAction.Clone());
            }

            return lClones.ToArray();
        }

        protected override NetworkStreamModifier[] CreateAndLinkStreamOperators(eExNetworkLibrary.Sockets.NetworkStream nsAlice, eExNetworkLibrary.Sockets.NetworkStream nsBob)
        {
            HTTPStreamModifierOperator sroOperator = new HTTPStreamModifierOperator(nsAlice, nsBob, GetClonedActions());
            return new NetworkStreamModifier[] { sroOperator };
        }

        protected override bool ShouldIntercept(IPAddress ipaSource, IPAddress ipaDestination, int iSourcePort, int iDestinationPort)
        {
            return iSourcePort == HTTPPort || iDestinationPort == HTTPPort;
        }
    }

    public class HTTPStreamModifierActionEventArgs : EventArgs
    {
        public HTTPStreamModifierAction Action { get; private set; }

        public HTTPStreamModifierActionEventArgs(HTTPStreamModifierAction htAction)
        {
            Action = htAction;
        }
    }
}
