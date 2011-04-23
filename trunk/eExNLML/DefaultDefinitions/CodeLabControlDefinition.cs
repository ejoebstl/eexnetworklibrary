using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.CodeLab;
using eExNLML.Extensibility;
using eExNLML.IO;
using eExNetworkLibrary;
using eExNLML.DefaultControllers;

namespace eExNLML.DefaultDefinitions
{
    public class CodeLabControlDefinition : HandlerDefinition
    {
        public CodeLabControlDefinition()
        {
            Name = "Code Lab";
            Description = "With this traffic handler it is possible to script small traffic modifiers and compile and use them at runtime. Compiling to plug-ins is also supported";
            Author = "Emanuel Jöbstl";
            WebLink = "http://www.eex-dev.net";
            PluginType = PluginTypes.TrafficHandler;
            PluginKey = "eex_code_lab";
        }

        public override IHandlerController Create(IEnvironment env)
        {
            return new CodeLabController(this, env);
        }
    }
}
