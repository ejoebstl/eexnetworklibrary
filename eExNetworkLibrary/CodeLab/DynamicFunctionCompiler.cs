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
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Reflection;
using System.Windows.Forms;
using System.IO;

namespace eExNetworkLibrary.CodeLab
{
    /// <summary>
    /// This class can be used to compile class sourcecode which implements IDynamicHandler to just in time plugins or plugin DLLs which can
    /// in turn be used by the dynamic function handler.
    ///  <example><code>
    /// // Load the sourcecode
    /// string strSourcecode = "your class sourcecode which implements IDynamicHandler goes here";
    ///
    /// // Create a new dynamic function handler
    /// DynamicFunctionHandler dfHandler = new DynamicFunctionHandler();
    /// 
    /// // Start the dynamic function handler
    /// dfHandler.Start();
    /// 
    /// // Create a new dynamic function compiler
    /// DynamicFunctionCompiler dfCompiler = new DynamicFunctionCompiler();
    /// 
    /// // Compile the sourcecode to a just in time plugin
    /// IDynamicHandler dynamicHandler = dfCompiler.BuildPreview(strSourcecode);
    /// 
    /// // Assign the just compiled dynamic handler to the dynamic function handler
    /// dfHandler.DynamicHandler = dynamicHandler;
    /// </code></example>
    /// </summary>
    public class DynamicFunctionCompiler
    {
        CSharpCodeProvider cscp;
        //test: new eExNetworkLibrary.CodeLab.DynamicFunctionCompiler().BuildPreview(System.IO.File.ReadAllText("C:\\function_test.txt"), "test");

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public DynamicFunctionCompiler()
        {
            cscp = new CSharpCodeProvider();
        }

        /// <summary>
        /// Builds a just in time plugin from the given sourcecode
        /// </summary>
        /// <param name="strSource">The sourcecode to compile. This code must be a class sourcecode including the using directives. The class in this code must implement IDynamicHandler</param>
        /// <returns>A just in time plugin which can be used with the DynamicFunctionHandler</returns>
        public IDynamicHandler BuildPreview(string strSource)
        {
            CompilerParameters cp = new CompilerParameters();
            cp.IncludeDebugInformation = true;
            cp.GenerateExecutable = false;
            cp.GenerateInMemory = true;
            cp.ReferencedAssemblies.Add("System.dll");
            cp.ReferencedAssemblies.Add("System.Drawing.dll");
            cp.ReferencedAssemblies.Add("System.Windows.Forms.dll");
            cp.ReferencedAssemblies.Add(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "eExNetworkLibrary.dll"));
            cp.CompilerOptions = "/optimize";
            CompilerResults cr = cscp.CompileAssemblyFromSource(cp, strSource);
            
            if (cr.Errors.HasErrors)
            {
                throw new CompilerErrorException(cr.Errors);
            }

            Assembly aAssembly = cr.CompiledAssembly;
            Type[] tTypes = aAssembly.GetTypes();

            foreach (Type tType in tTypes)
            {
                if (tType.IsPublic && !tType.IsAbstract)
                {
                    Type tPlugin = tType.GetInterface(typeof(IDynamicHandler).FullName, true);

                    if (tPlugin != null)
                    {
                        IDynamicHandler idhPlugin = (IDynamicHandler)Activator.CreateInstance(tType);
                        return idhPlugin;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Builds a blugin DLL for the eExNetLab from the given source code to the given destination
        /// </summary>
        /// <param name="strSource">The sourcecode to compile. This code must be a class sourcecode including the using directives. The class in this code must implement IDynamicHandler</param>
        /// <param name="strName">The name of this plugin</param>
        /// <param name="strDescription">The description of this plugin</param>
        /// <param name="strAuthor">The author of this plugin</param>
        /// <param name="strDestinationFolder">The destination folder where the generated code and plugin should be saved</param>
        public void BuildPlugin(string strSource, string strName, string strDescription, string strAuthor, string strDestinationFolder)
        {
            string strSafeName = MakeToSafeName(strName);
            GeneratePluginSource(strSource, strName, strDescription, strAuthor, strDestinationFolder);

            string strPluginPath = strDestinationFolder + "\\" + strSafeName + "_NetLabPlugin.cs";
            string strHandlerPath = strDestinationFolder + "\\" + strSafeName + "_DynamicHandler.cs";

            string strOutPath = strDestinationFolder + "\\" + strSafeName + "_NetLabPlugin.dll";

            CompilerParameters cp = new CompilerParameters();
            cp.IncludeDebugInformation = true;
            cp.GenerateExecutable = true;
            cp.GenerateInMemory = false;
            cp.ReferencedAssemblies.Add("System.dll");
            cp.ReferencedAssemblies.Add("System.Drawing.dll");
            cp.ReferencedAssemblies.Add("System.Windows.Forms.dll");
            cp.ReferencedAssemblies.Add(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "eExNetworkLibrary.dll"));
            cp.ReferencedAssemblies.Add(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "eExNetLab.exe"));
            cp.ReferencedAssemblies.Add(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "eExNetLabExtentionLibrary.dll"));
            cp.CompilerOptions = "/optimize /target:library /out:\"" + strOutPath + "\"";
            CompilerResults cr = cscp.CompileAssemblyFromFile(cp, strPluginPath, strHandlerPath);

            if (cr.Errors.HasErrors)
            {
                throw new CompilerErrorException(cr.Errors);
            }

        }

        /// <summary>
        /// Generates the sourcecode of a plugin DLL for the eExNetLab but does not compile it
        /// </summary>
        /// <param name="strSource">The sourcecode to compile. This code must be a class sourcecode including the using directives. The class in this code must implement IDynamicHandler</param>
        /// <param name="strName">The name of this plugin</param>
        /// <param name="strDescription">The description of this plugin</param>
        /// <param name="strAuthor">The author of this plugin</param>
        /// <param name="strDestinationFolder">The destination folder where the generated code and plugin should be saved</param>
        public void GeneratePluginSource(string strSource, string strName, string strDescription, string strAuthor, string strDestinationFolder)
        {
            string strSafeName = MakeToSafeName(strName);
            string strTemplate = global::eExNetworkLibrary.Properties.Resources.DynamicPluginSource;
            strTemplate = strTemplate.Replace("_classname_", RemoveQuotations(strSafeName));
            strTemplate = strTemplate.Replace("_name_", RemoveQuotations(strName));
            strTemplate = strTemplate.Replace("_description_", RemoveQuotations(strDescription));
            strTemplate = strTemplate.Replace("_author_", RemoveQuotations(strAuthor));

            File.WriteAllText(strDestinationFolder + "\\" + strSafeName + "_NetLabPlugin.cs", strTemplate);
            File.WriteAllText(strDestinationFolder + "\\" + strSafeName + "_DynamicHandler.cs", strSource);
        }

        private string RemoveQuotations(string strValue)
        {
            strValue = strValue.Replace("\\", "\\\\");
            strValue = strValue.Replace("\"", "\\\"");
            strValue = strValue.Replace("\'", "\\\'");

            return strValue;
        }

        private string MakeToSafeName(string strName)
        {
            strName = strName.Replace("<", "_");
            strName = strName.Replace(">", "_");
            strName = strName.Replace("?", "_");
            strName = strName.Replace("+", "_");
            strName = strName.Replace("-", "_");
            strName = strName.Replace("%", "_");
            strName = strName.Replace("&", "_");
            strName = strName.Replace("$", "_");
            strName = strName.Replace("\"", "_");
            strName = strName.Replace("\'", "_");
            strName = strName.Replace(" ", "_");
            strName = strName.Replace("\t", "_");
            strName = strName.Replace("|", "_");
            strName = strName.Replace("=", "_");
            strName = strName.Replace("*", "_");
            strName = strName.Replace("~", "_");
            strName = strName.Replace("#", "_");
            strName = strName.Replace("(", "_");
            strName = strName.Replace(")", "_");
            strName = strName.Replace("[", "_");
            strName = strName.Replace("]", "_");
            strName = strName.Replace("\\", "_");
            strName = strName.Replace("/", "_");
            strName = strName.Replace("{", "_");
            strName = strName.Replace("}", "_");

            return strName;
        }
    }

    /// <summary>
    /// This class represents a wrapper for compuler errors
    /// </summary>
    public class CompilerErrorException : Exception
    {
        CompilerErrorCollection crErrors;

        /// <summary>
        /// Gets the errors which happend during compile time
        /// </summary>
        public CompilerErrorCollection Errors
        {
            get { return crErrors; }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="crErrors">The compiler error collection</param>
        public CompilerErrorException(CompilerErrorCollection crErrors)
            : this(crErrors, "Errors have been thrown during compilation")
        { }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="crErrors">The compiler error collection</param>
        /// <param name="strMessage">The message of this exception</param>
        public CompilerErrorException(CompilerErrorCollection crErrors, string strMessage) : base(strMessage)
        {
            this.crErrors = crErrors;
        }
    }
}
