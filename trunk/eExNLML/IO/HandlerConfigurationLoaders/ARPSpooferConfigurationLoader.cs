using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.Attacks.MITM;
using eExNetworkLibrary;
using System.Net;
using eExNetworkLibrary.Attacks.Spoofing;
using eExNetworkLibrary.Attacks;
using eExNLML.Extensibility;

namespace eExNLML.IO.HandlerConfigurationLoaders
{
    class ARPSpooferConfigurationLoader : HandlerConfigurationLoader
    {
        private APRAttack thHandler;

        public ARPSpooferConfigurationLoader(TrafficHandler thHandler)
            : base(thHandler)
        {
            this.thHandler = (APRAttack)thHandler;
        }
        protected override void ParseConfiguration(Dictionary<string, NameValueItem[]> strNameValues, IEnvironment eEnviornment)
        {
            thHandler.Method = APRAttackMethod.UseReplyPackets.ToString() == ConvertToString(strNameValues["method"])[0] ? APRAttackMethod.UseReplyPackets : APRAttackMethod.UseRequestPackets;
            thHandler.SpoofInterval = ConvertToInt(strNameValues["interval"])[0];

            if (strNameValues.ContainsKey("victim"))
            {
                foreach (NameValueItem nvi in strNameValues["victim"])
                {
                    IPAddress[] ipaAlice = ConvertToIPAddress(nvi.GetChildsByName("alice"));
                    IPAddress[] ipaBob = ConvertToIPAddress(nvi.GetChildsByName("bob"));

                    if (ipaAlice.Length != ipaBob.Length)
                    {
                        throw new ArgumentException("Invalid data");
                    }

                    for (int iC1 = 0; iC1 < ipaAlice.Length; iC1++)
                    {
                        thHandler.AddToVictimList(new MITMAttackEntry(ipaAlice[iC1], ipaBob[iC1]));
                    }
                }
            }
        }
    }
}

