﻿using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.Attacks.Modification;
using eExNLML.IO.HandlerConfigurationLoaders;
using eExNLML.IO;
using eExNLML.IO.HandlerConfigurationWriters;
using eExNLML.Extensibility;
using eExNetworkLibrary;

namespace eExNLML.DefaultControllers
{
    public class DNSOTFSpooferController : HandlerController
    {
        public DNSOTFSpooferController(IHandlerDefinition hbDefinition, IEnvironment env)
            : base(hbDefinition, env, null)
        { }

        protected override eExNetworkLibrary.TrafficHandler Create(object param)
        {
            return new DNSOnTheFlySpoofer();
        }

        protected override HandlerConfigurationLoader CreateConfigurationLoader(TrafficHandler h, object param)
        {
            return new DNSOnTheFlySpooferConfigurationLoader(h);
        }

        protected override HandlerConfigurationWriter CreateConfigurationWriter(TrafficHandler h, object param)
        {
            return new DNSOnTheFlySpooferConfigurationWriter(h);
        }

        protected override TrafficHandlerPort[] CreateTrafficHandlerPorts(TrafficHandler h, object param)
        {
            return CreateDefaultPorts(h, true, true, false, false, false);
        }
    }
}
