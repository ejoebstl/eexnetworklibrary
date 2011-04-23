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