using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary.HTTP
{
    /// <summary>
    /// Represents a collection of HTTP headers
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Count: {Count}")] 
    public class HTTPHeaderCollection
    {
        List<HTTPHeader> lHeaders;
        
        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public HTTPHeaderCollection()
        {
            lHeaders = new List<HTTPHeader>();
        }

        /// <summary>
        /// Checks whether the header collection contains the given header at least once.
        /// </summary>
        /// <param name="strName">The header to search for.</param>
        /// <returns>A bool indicating whether the header collection contains the given header.</returns>
        public bool Contains(string strName)
        {
            foreach (HTTPHeader hHeader in lHeaders)
            {
                if (hHeader.Name.Equals(strName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes the headers with the given name.
        /// </summary>
        /// <param name="strName">The name of the headers to remove.</param>
        public void Remove(string strName)
        {
            for (int iC1 = lHeaders.Count - 1; iC1 >= 0; iC1--)
            {
                if (lHeaders[iC1].Name.Equals(strName, StringComparison.OrdinalIgnoreCase))
                {
                    lHeaders.RemoveAt(iC1);
                }
            }
        }


        /// <summary>
        /// Removes the given header.
        /// </summary>
        /// <param name="hHeader">The header to remove.</param>
        public void Remove(HTTPHeader hHeader)
        {
            for (int iC1 = lHeaders.Count - 1; iC1 >= 0; iC1--)
            {
                if (lHeaders[iC1].Equals(hHeader))
                {
                    lHeaders.RemoveAt(iC1);
                }
            }
        }

        /// <summary>
        /// Adds the given header to this collection.
        /// </summary>
        /// <param name="hHeader">The header to add.</param>
        public void Add(HTTPHeader hHeader)
        {
            lHeaders.Add(hHeader);
        }

        /// <summary>
        /// Clears this header collection.
        /// </summary>
        public void Clear()
        {
            lHeaders.Clear();
        }

        /// <summary>
        /// Gets the headers with the specified name
        /// </summary>
        /// <param name="strName">The name to get the headers for</param>
        /// <returns>The headers for the given name or an empty array if no header was not found.</returns>
        [System.Runtime.CompilerServices.IndexerName("Headers")]
        public HTTPHeader[] this[string strName]
        {
            get
            {
                List<HTTPHeader> lFound = new List<HTTPHeader>();

                foreach (HTTPHeader hHeader in lHeaders)
                {
                    if (hHeader.Name.Equals(strName, StringComparison.OrdinalIgnoreCase))
                    {
                        lFound.Add(hHeader);
                    }
                }

                return lFound.ToArray();
            }
        }

        /// <summary>
        /// Gets all header names present in this collection
        /// </summary>
        public HTTPHeader[] AllHeaders
        {
            get
            {
                return lHeaders.ToArray();
            }
        }

        /// <summary>
        /// Returns the count of headers in this collection
        /// </summary>
        public int Count
        {
            get
            {
                return lHeaders.Count;
            }
        }
    }

    public class HTTPHeader
    {
        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        public HTTPHeader() : this("", "") { }
        
        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        public HTTPHeader(string strName, string strValue)
        {
            Name = strName;
            Value = strValue;
        }


        /// <summary>
        /// Gets or sets the name of the header
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the value of the header
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Returns the string representation of this header.
        /// </summary>
        /// <returns>The string representation of this header.</returns>
        public override string ToString()
        {
            return Name + ": " + Value;
        }

        /// <summary>
        /// Compares this object to another object.
        /// </summary>
        /// <param name="obj">The object to compare to this object.</param>
        /// <returns>A bool indicating whether the two objects are the same.</returns>
        public override bool Equals(object obj)
        {
            if (obj is HTTPHeader)
            {
                HTTPHeader h = (HTTPHeader)obj;

                return Name.Equals(h.Name, StringComparison.OrdinalIgnoreCase) && Value == h.Value;
            }

            return false;
        }

        /// <summary>
        /// Gets the hash code of this object.
        /// </summary>
        /// <returns>The hash code of this object.</returns>
        public override int GetHashCode()
        {
            return Name.Length + Value.Length;
        }
    }
}
