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
using eExNetworkLibrary.HTTP;

namespace eExNetworkLibrary.TrafficModifiers.StreamModification.HTTP
{
    public abstract class HTTPStreamModifierCondition : ICloneable
    {
         List<HTTPStreamModifierCondition> lChildRules;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        protected HTTPStreamModifierCondition()
        {
            lChildRules = new List<HTTPStreamModifierCondition>();
        }

        /// <summary>
        /// Adds a child condition to this condition. 
        /// Child rules are validated and the result is and-conjuncted with the result of the parent rule. 
        /// <br />
        /// If there are multiple child rules, and the result of at least one child rule is true, and the result of this rule is true, the end result is true.<br />
        /// If there are multiple child rules, and the result of all child rules is false, the end result is flase.<br />
        /// If the result of this rule is false, the end result is false.<br />
        /// </summary>
        /// <param name="cChild"></param>
        public void AddChildRule(HTTPStreamModifierCondition cChild)
        {
            lock (lChildRules) { lChildRules.Add(cChild); }
        }

        /// <summary>
        /// Removes the given child condition.
        /// </summary>
        /// <param name="cChild">The child to remove</param>
        public void RemoveChildRule(HTTPStreamModifierCondition cChild)
        {
            lock (lChildRules) { lChildRules.Remove(cChild); }
        }

        /// <summary>
        /// Checks whether a given child condition is contained by this condition.
        /// </summary>
        /// <param name="cChild">The child to check for.</param>
        /// <returns>A bool indicating whether a given child condition is contained by this condition.</returns>
        public bool ContainsChildCondition(HTTPStreamModifierCondition cChild)
        {
            lock (lChildRules) { return lChildRules.Contains(cChild); }
        }

        /// <summary>
        /// Clears all child conditions.
        /// </summary>
        public void ClearChildConditions()
        {
            lock (lChildRules) { lChildRules.Clear(); }
        }

        /// <summary>
        /// Gets all child conditions.
        /// </summary>
        public HTTPStreamModifierCondition[] ChildRules
        {
            get { lock (lChildRules) { return lChildRules.ToArray(); } }
        }
        /// <summary>
        /// Checkes whether this rule matches a given message.
        /// </summary>
        /// <param name="httpMessage">The HTTP message to match</param>
        public virtual bool IsMatch(HTTPMessage httpMessage)
        {
            bool bResult = false;

            lock (lChildRules)
            {
                if (lChildRules.Count == 0)
                {
                    return true; //Nothing to validate
                }
                foreach (HTTPStreamModifierCondition htCondition in lChildRules)
                {
                    if (htCondition.IsMatch(httpMessage))
                    {
                        bResult = true;
                    }
                }
            }

            return bResult;
        }

        /// <summary>
        /// Gets the name of this rule
        /// </summary>
        public abstract string Name
        {
            get;
        }

        /// <summary>
        /// Returns the name of this rule
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Returns a long description of this rules condition, without the action. 
        /// </summary>
        /// <returns>A long description of this rules condition</returns>
        public abstract string GetLongDescription();

        /// <summary>
        /// Returns a short description of this rules condition, without the action. 
        /// </summary>
        /// <returns>A short description of this rules condition</returns>
        public abstract string GetShortDescription();

        /// <summary>
        /// Returns an exact copy of this object, including all child conditions.
        /// </summary>
        /// <returns>An exact copy of this object</returns>
        public abstract object Clone();

        /// <summary>
        /// Clones all child conditions of this instance to the given instance.
        /// </summary>
        /// <param name="htCondition">The condition to clone all childs to.</param>
        protected void CloneChildsTo(HTTPStreamModifierCondition htCondition)
        {
            foreach (HTTPStreamModifierCondition htChild in this.ChildRules)
            {
                htCondition.AddChildRule((HTTPStreamModifierCondition)htChild.Clone());
            }
        }
    }
}
