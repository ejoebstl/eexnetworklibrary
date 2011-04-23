using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;

namespace eExNLML.Extensibility
{
    public class PluginLoader<T>
    {
        public T[] LoadPluginsFromDirectory(string strDirectory)
        {
            List<T> lPlugins = new List<T>();
            if (Directory.Exists(strDirectory))
            {
                string[] strFiles = Directory.GetFiles(strDirectory);

                foreach (string strFile in strFiles)
                {
                    if (strFile.EndsWith(".dll"))
                    {
                        try
                        {
                            lPlugins.AddRange(LoadPlugins(strFile));
                        }
                        catch { }
                    }
                }
            }
            return lPlugins.ToArray();
        }

        public T[] LoadPlugins(string strFilename)
        {
            List<T> lPlugins = new List<T>();
            if (strFilename.EndsWith(".dll"))
            {
                Assembly pAssemblyToLoad = Assembly.LoadFrom(strFilename);

                foreach (Type tType in pAssemblyToLoad.GetTypes())
                {
                    if (tType.IsPublic && !tType.IsAbstract)
                    {
                        Type tPlugin = tType.GetInterface(typeof(T).FullName, true);

                        if (tPlugin != null)
                        {
                            T dtpPlugin = (T)Activator.CreateInstance(tType);
                            lPlugins.Add(dtpPlugin);
                        }
                    }
                }
            }
            return lPlugins.ToArray();
        }
    }
}
