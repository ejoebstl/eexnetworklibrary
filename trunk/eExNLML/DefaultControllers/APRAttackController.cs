﻿// This source file is part of the eEx Network Library Management Layer (NLML)
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
using eExNetworkLibrary.Attacks.MITM;
using eExNetworkLibrary.Attacks.Spoofing;
using eExNLML.Extensibility;
using eExNLML.IO;
using eExNLML.IO.HandlerConfigurationLoaders;
using eExNLML.IO.HandlerConfigurationWriters;
using eExNetworkLibrary;

namespace eExNLML.DefaultControllers
{
    public class APRAttackController : HandlerController
    {
        public APRAttackController(IHandlerDefinition hbDefinition, IEnvironment env) : base(hbDefinition, env, null)
        { }

        protected override eExNetworkLibrary.TrafficHandler Create(object param)
        {
            return new APRAttack();
        }

        protected override HandlerConfigurationLoader CreateConfigurationLoader(TrafficHandler h, object param)
        {
            return new ARPSpooferConfigurationLoader(h);
        }

        protected override HandlerConfigurationWriter CreateConfigurationWriter(TrafficHandler h, object param)
        {
            return new ARPSpooferConfigurationWriter(h);
        }

        protected override TrafficHandlerPort[] CreateTrafficHandlerPorts(TrafficHandler h, object param)
        {
            return CreateDefaultPorts(h, false, false, true, false, false);
        }
    }
}
