// This source file is part of the eEx Network Library Management Layer (NLML)
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
using eExNetworkLibrary;
using eExNetworkLibrary.Monitoring;

namespace eExNLML.IO.HandlerConfigurationWriters
{
    class DumperConfigurationWriter : HandlerConfigurationWriter
    {
        private LibPcapDumper thHandler;

        public DumperConfigurationWriter(TrafficHandler thToSave)
            : base(thToSave)
        {
            thHandler = (LibPcapDumper)thToSave;
        }

        protected override void AddConfiguration(List<NameValueItem> lNameValueItems, IEnvironment eEnviornment)
        {
            lNameValueItems.AddRange(ConvertToNameValueItems("dumping", thHandler.IsDumping));
            lNameValueItems.AddRange(ConvertToNameValueItems("fileName", thHandler.FileName));
        }
    }
}