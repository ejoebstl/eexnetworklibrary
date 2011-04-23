using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace eExNLML.IO
{
    /// <summary>
    /// Represents a name value item for storing configurations
    /// </summary>
    [DebuggerDisplay("Name: {Name}, Value: {Value}")]
    public class NameValueItem
    {
        string strName;
        string strValue;
        List<NameValueItem> arChilds;

        /// <summary>
        /// Gets the name of this item
        /// </summary>
        public string Name
        {
            get { return strName; }
        }
        
        /// <summary>
        /// Gets the value of this item
        /// </summary>
        public string Value
        {
            get { return strValue; }
        }

        /// <summary>
        /// Creates a new instance of this class with the given name and value
        /// </summary>
        /// <param name="strName">The name</param>
        /// <param name="strValue">The value</param>
        public NameValueItem(string strName, string strValue)
        {
            this.strName = strName;
            this.strValue = strValue;
            arChilds = new List<NameValueItem>();
        }

        /// <summary>
        /// Adds a child configuration item
        /// </summary>
        /// <param name="nvi">The child configuration item to add</param>
        public void AddChildItem(NameValueItem nvi)
        {
            arChilds.Add(nvi);
        }

        /// <summary>
        /// Adds a child configuration item range
        /// </summary>
        /// <param name="nvi">The child configuration item range to add</param>
        public void AddChildRange(NameValueItem[] nvi)
        {
            arChilds.AddRange(nvi);
        }

        public NameValueItem[] this[string strName]
        {
            get
            {
                return GetChildsByName(strName);
            }
        }

        /// <summary>
        /// Gets all child configuration items
        /// </summary>
        public NameValueItem[] ChildItems
        {
            get { return arChilds.ToArray(); }
        }

        /// <summary>
        /// Returns a bool indicating whether this name value item contains an item with the given name.
        /// </summary>
        /// <param name="strName">The name to search for.</param>
        /// <returns>A bool indicating whether this name value item contains an item with the given name</returns>
        public bool ContainsChildItem(string strName)
        {
            foreach (NameValueItem nvi in arChilds)
            {
                if (nvi.Name == strName)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns all child configuration items for the given name
        /// </summary>
        /// <param name="strName">The name of the items to search</param>
        /// <returns>All items with the given name stored in an array, or an empty array if no items with the given name were found.</returns>
        public NameValueItem[] GetChildsByName(string strName)
        {
            List<NameValueItem> lNvi = new List<NameValueItem>();

            foreach (NameValueItem nvi in arChilds)
            {
                if (nvi.Name == strName)
                {
                    lNvi.Add(nvi);
                }
            }

            return lNvi.ToArray();
        }
    }
}
