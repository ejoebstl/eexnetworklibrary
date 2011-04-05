using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using eExNetworkLibrary.Utilities;
using eExNetworkLibrary.Ethernet;
using eExNetworkLibrary.ARP;
using eExNetworkLibrary.IP;

namespace eExNetworkLibrary.Attacks.Scanning
{
    /// <summary>
    /// This class represents a scan task for the ARP net scanner
    /// </summary>
    public class ARPScanTask
    {
        private IPAddress ipaStart;
        private IPAddress ipaEnd;
        private MACAddress macLocal;
        private IPAddress ipLocal;
        private TrafficHandler thOut;
        private IPAddressAnalysis ipAnalysis;
        private bool bIsFinished;
        private byte[] byteStartIP;
        private byte[] byteEndIP;
        private ulong iScanned;
        private ulong iScanCount;

        /// <summary>
        /// Creates a new instance of this class with the given params
        /// </summary>
        /// <param name="ipaStart">The start IP address of the range to scan</param>
        /// <param name="ipaEnd">The end IP address of the range to scan</param>
        /// <param name="macLocal">The MAC address which should be spoofed during scanning</param>
        /// <param name="ipLocal">The IP address which should be spoofed during scanning</param>
        /// <param name="thOut">The traffic handler to which the generated ARP frames should be forwarded. It is wise to assign an ARP net scanner here</param>
        public ARPScanTask(IPAddress ipaStart, IPAddress ipaEnd, MACAddress macLocal, IPAddress ipLocal, TrafficHandler thOut)
        {
            ipAnalysis = new IPAddressAnalysis();
            this.ipaEnd = ipaEnd;
            this.ipaStart = ipaStart;
            this.macLocal = macLocal;
            this.ipLocal = ipLocal;
            this.thOut = thOut;
            this.bIsFinished = false;
            byteStartIP = ipaStart.GetAddressBytes();
            byteEndIP = ipaEnd.GetAddressBytes();
            iScanned = 0;

            iScanCount = ipAnalysis.GetIpCount(ipaStart, ipaEnd);
        }

        /// <summary>
        /// Gets a bool indicating whether this scan task is finished
        /// </summary>
        public bool IsFinished
        {
            get { return bIsFinished; }
        }

        /// <summary>
        /// Scans the next host in the range of this scan task
        /// </summary>
        public void ScanNext()
        {
            ScanInternal();
        }

        /// <summary>
        /// Returns the IP address which was scanned last
        /// </summary>
        public IPAddress LastScannedAddress
        {
            get
            {
                return new IPAddress(byteStartIP);
            }
        }

        /// <summary>
        /// Returns the count of all hosts in this scan range
        /// </summary>
        public ulong ScanCount
        {
            get
            {
                return iScanCount;
            }
        }

        /// <summary>
        /// Returns the count of all scanned hosts in this scan range
        /// </summary>
        public ulong ScannedCount
        {
            get
            {
                return iScanned;
            }
        }

        private void ScanInternal()
        {
            if ((byteStartIP[0] << 24) + (byteStartIP[1] << 16) + (byteStartIP[2] << 8) + (byteStartIP[3]) <= (byteEndIP[0] << 24) + (byteEndIP[1] << 16) + (byteEndIP[2] << 8) + (byteEndIP[3]))
            {
                EthernetFrame ethFrame = new EthernetFrame();
                ethFrame.CanocialFormatIndicator = false;
                ethFrame.Destination = MACAddress.Parse("ff:ff:ff:ff:ff:ff");
                ethFrame.Source = macLocal;
                ethFrame.VlanTagExists = false;
                ethFrame.EtherType = EtherType.ARP;

                ARPFrame arpFrame = new ARPFrame();
                arpFrame.DestinationIP = new IPAddress(byteStartIP);
                arpFrame.SourceIP = ipLocal;
                arpFrame.DestinationMAC = MACAddress.Parse("ff:ff:ff:ff:ff:ff");
                arpFrame.SourceMAC = macLocal;
                arpFrame.ProtocolAddressType = EtherType.IPv4;
                arpFrame.HardwareAddressType = HardwareAddressType.Ethernet;
                arpFrame.Operation = ARPOperation.Request;

                ethFrame.EncapsulatedFrame = arpFrame;

                thOut.PushTraffic(ethFrame);

                ipAnalysis.Increase(byteStartIP);

                iScanned++;
            }
            else
            {
                bIsFinished = true;
            }
        }
    }
}
