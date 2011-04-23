using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary;
using eExNetworkLibrary.Attacks.MITM;
using eExNetworkLibrary.Attacks.Spoofing;
using eExNetworkLibrary.Attacks;

namespace eExNLML.IO.HandlerConfigurationWriters
{
    class ARPSpooferConfigurationWriter : HandlerConfigurationWriter
    {
        private APRAttack thHandler;

        public ARPSpooferConfigurationWriter(TrafficHandler thToSave)
            : base(thToSave)
        {
            thHandler = (APRAttack)thToSave;
        }

        protected override void AddConfiguration(List<NameValueItem> lNameValueItems, IEnvironment eEnviornment)
        {
            lNameValueItems.AddRange(ConvertToNameValueItems("method", thHandler.Method.ToString()));
            lNameValueItems.AddRange(ConvertToNameValueItems("interval", thHandler.SpoofInterval));


            foreach (MITMAttackEntry mitmEntry in thHandler.GetVictims())
            {
                NameValueItem nviVictims = new NameValueItem("victim", "");
                nviVictims.AddChildRange(ConvertToNameValueItems("alice", mitmEntry.VictimAlice));
                nviVictims.AddChildRange(ConvertToNameValueItems("bob", mitmEntry.VictimBob));
                lNameValueItems.Add(nviVictims);
            }
        }
    }
}
