using System;
using System.Collections.Generic;
using System.Text;

namespace eExNLML.Repository
{
    public class PlugInDependency
    {
        public PlugInDependency(string strName, string strResource, string strLink, DependencyType tType)
        {
            this.Name = strName;
            this.Resource = strResource;
            this.Link = strLink;
            this.Type = tType;
        }

        public string Name{get; protected set;}
        public string Resource { get; protected set; }
        public string Link { get; protected set; }
        public DependencyType Type { get; protected set; }
    }

    public enum DependencyType
    {
        /// <summary>
        /// Another extension from the eex repository
        /// </summary>
        Extension = 1,
        /// <summary>
        /// A 3rd party software library
        /// </summary>
        Library = 2
    }
}
