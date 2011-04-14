using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using eExNetworkLibrary.IP;

namespace eExNetworkLibrary.Attacks.Scanning
{
    public abstract class ScanTask
    {
        protected IPAddress StartAddress { get; private set; }
        protected IPAddress EndAddress { get; private set; }
        protected IPAddress SourceAddress { get; private set; }
        protected TrafficHandler OutputHandler { get; private set; }

        private bool bIsFinished;
        private byte[] byteStartIP;
        private byte[] byteEndIP;

        /// <summary>
        /// Creates a new instance of this class with the given params
        /// </summary>
        /// <param name="ipaStart">The start IP address of the range to scan</param>
        /// <param name="ipaEnd">The end IP address of the range to scan</param>
        /// <param name="ipLocal">The IP address which should be spoofed during scanning</param>
        /// <param name="thOut">The traffic handler to which the generated ARP frames should be forwarded. It is wise to assign an ARP net scanner here</param>
        protected ScanTask(IPAddress ipaStart, IPAddress ipaEnd, IPAddress ipLocal, TrafficHandler thOut)
        {
            if (IPAddressAnalysis.Compare(ipaStart, ipaEnd) == -1)
            {
                this.StartAddress = ipaStart;
                this.EndAddress = ipaEnd;
            }
            else
            {
                this.StartAddress = ipaEnd;
                this.EndAddress = ipaStart;
            }
            
            this.SourceAddress = ipLocal;
            this.OutputHandler = thOut;
            this.bIsFinished = false;

            this.byteStartIP = this.StartAddress.GetAddressBytes();
            this.byteEndIP = this.EndAddress.GetAddressBytes();
            this.ScannedCount = 0;

            ScanCount = IPAddressAnalysis.GetIpCount(ipaStart, ipaEnd);
        }        
        
        /// <summary>
        /// Gets a bool indicating whether this scan task is finished
        /// </summary>
        public bool IsFinished { get; private set; }

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
        public ulong ScanCount { get; private set; }

        /// <summary>
        /// Returns the count of all scanned hosts in this scan range
        /// </summary>
        public ulong ScannedCount { get; private set; }

        private void ScanInternal()
        {
            if(IPAddressAnalysis.Compare(byteStartIP, byteEndIP) != 1)
            {

                Scan(new IPAddress(byteStartIP));

                IPAddressAnalysis.Increase(byteStartIP);

                ScannedCount++;
            }
            else
            {
                IsFinished = true;
            }
        }

        /// <summary>
        /// Scans the given address.
        /// </summary>
        /// <param name="ipaAddress">The address to scan.</param>
        protected abstract void Scan(IPAddress ipaAddress);
    }
}
