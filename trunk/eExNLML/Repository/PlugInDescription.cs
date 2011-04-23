﻿using System;
using System.Collections.Generic;
using System.Text;

namespace eExNLML.Repository
{
    public class PlugInDescription : eExNLML.Extensibility.IPlugin
    {
        public PlugInDescription(string strAuthor, string strDescription, string strName, string strPluginKey, string strPluginType, string strWeblink, string strVersion, string[] arFiles)
        {
            Author = strAuthor;
            Description = strDescription;
            Name = strName;
            PluginKey = strPluginKey;
            PluginType = strPluginType;
            WebLink = strWeblink;
            Version = new System.Version(strVersion);

            Files = arFiles;
        }

        public string Author { get; protected set; }
        public string Description { get; protected set; }
        public string Name { get; protected set; }
        public string PluginKey { get; protected set; }
        public string PluginType { get; protected set; }
        public string WebLink { get; protected set; }
        public Version Version { get; protected set; }

        public string[] Files { get; protected set; }

    }
}
