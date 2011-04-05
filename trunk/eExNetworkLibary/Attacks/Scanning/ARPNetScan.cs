using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using eExNetworkLibrary.ARP;

namespace eExNetworkLibrary.Attacks.Scanning
{
    /// <summary>
    /// This class represents an ARP net scanner, which scans a whole subnet at layer 2 and revals even firewalled hosts.
    /// For analyzing the replies to the sent ARP frames, a NetMap is recommendable.
    /// </summary>
    public class ARPNetScan : DirectInterfaceIOHandler, IScanner
    {
        private List<ARPScanTask> astScanTasks;
        private Thread tWorker;
        private bool bContinue;
        private AutoResetEvent areTasksAvailable;
        private int iInterval;
        private IP.IPAddressAnalysis ipv4Analysis;

        /// <summary>
        /// Represents the method which is used to handle ARP net scanner events
        /// </summary>
        /// <param name="args">The event args</param>
        /// <param name="sender">The object which rised the event</param>
        public delegate void ARPScanEventHandler(ARPScanEventArgs args, object sender);

        /// <summary>
        /// This event is rised when an ARP scan is finished
        /// </summary>
        public event ARPScanEventHandler ARPScanFinished;

        /// <summary>
        /// This event is rised when a single step in an ARP scan was done
        /// </summary>
        public event ARPScanEventHandler ARPScanStepDone;

        /// <summary>
        /// Gets or sets the interval between the sending of packets
        /// </summary>
        public int Interval
        {
            get { return iInterval; }
            set
            {
                iInterval = value;
                InvokePropertyChanged();
            }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public ARPNetScan()
        {
            bContinue = false;
            areTasksAvailable = new AutoResetEvent(false);
            astScanTasks = new List<ARPScanTask>();
            ipv4Analysis = new eExNetworkLibrary.IP.IPAddressAnalysis();
            iInterval = 20;
        }

        /// <summary>
        /// Receives an ARP frame from the ARP scan task and pushes it to the out queue of the according interface
        /// </summary>
        /// <param name="fInputFrame">The frame to receive</param>
        protected override void HandleTraffic(Frame fInputFrame)
        {
            ARPFrame fARPFrame = this.GetARPFrame(fInputFrame);

            if (fARPFrame != null)
            {
                foreach (IPInterface ipi in lInterfaces)
                {
                    for (int iC1 = 0; iC1 < ipi.IpAddresses.Length && iC1 < ipi.Subnetmasks.Length; iC1++)
                    {
                        if (ipv4Analysis.GetClasslessNetworkAddress(ipi.IpAddresses[iC1], ipi.Subnetmasks[iC1]).Equals(ipv4Analysis.GetClasslessNetworkAddress(fARPFrame.DestinationIP, ipi.Subnetmasks[iC1])))
                        {
                            ipi.Send(fInputFrame);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds an ARP scan task to this scanners task queue, where the scanner will start the scan as soon as possible
        /// </summary>
        /// <param name="ast">The scan task to do</param>
        public void AddARPScanTask(ARPScanTask ast)
        {
            lock (astScanTasks)
            {
                astScanTasks.Add(ast);
                areTasksAvailable.Set();
            }
        }

        /// <summary>
        /// Removes a scan task. This only works if the scan task has not been done yet.
        /// </summary>
        /// <param name="ast">The ARP scan task to remove</param>
        public void RemoveARPScanTask(ARPScanTask ast)
        {
            lock (astScanTasks)
            {
                astScanTasks.Remove(ast);
                InvokeScanFinished(ast);
            }
        }

        private void run()
        {
            while (bContinue)
            {
                areTasksAvailable.WaitOne();
                while (astScanTasks.Count > 0 && bContinue)
                {
                    Thread.Sleep(iInterval);
                    lock (astScanTasks)
                    {
                        for (int iC1 = 0; iC1 < astScanTasks.Count && bContinue; iC1++)
                        {
                            astScanTasks[iC1].ScanNext();
                            InvokeScanStepDone(astScanTasks[iC1]);
                            if (astScanTasks[iC1].IsFinished)
                            {
                                InvokeScanFinished(astScanTasks[iC1]);
                                astScanTasks.RemoveAt(iC1);
                                iC1--;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Starts this traffic handler's working threads
        /// </summary>
        public override void Start()
        {
            if (tWorker == null)
            {
                bContinue = true;
                tWorker = new Thread(run);
                tWorker.Start();
            }
            base.Start();
        }

        /// <summary>
        /// Stops this traffic handler's working threads
        /// </summary>
        public override void Stop()
        {
            Cleanup();
            base.Stop();
        }

        /// <summary>
        /// Stops this traffic handler's scanner thread
        /// </summary>
        public override void Cleanup()
        {
            if (tWorker != null)
            {
                bContinue = false;
                areTasksAvailable.Set();
                tWorker.Join();
                tWorker = null;
            }
        }

        private void InvokeScanStepDone(ARPScanTask args)
        {
            if (ARPScanStepDone != null)
            {
                if (ARPScanFinished.Target != null
                    && ARPScanStepDone.Target.GetType().GetInterface(typeof(System.ComponentModel.ISynchronizeInvoke).Name, true) != null
                    && ((System.ComponentModel.ISynchronizeInvoke)(ARPScanStepDone.Target)).InvokeRequired)
                {
                    ((System.ComponentModel.ISynchronizeInvoke)(ARPScanStepDone.Target)).BeginInvoke(ARPScanStepDone, new object[] { new ARPScanEventArgs(args), this });
                }
                else
                {
                    ARPScanStepDone(new ARPScanEventArgs(args), this);
                }
            }
        }

        private void InvokeScanFinished(ARPScanTask args)
        {
            if (ARPScanFinished != null)
            {
                if (ARPScanFinished.Target != null 
                    && ARPScanFinished.Target.GetType().GetInterface(typeof(System.ComponentModel.ISynchronizeInvoke).Name, true) != null
                    && ((System.ComponentModel.ISynchronizeInvoke)(ARPScanFinished.Target)).InvokeRequired)
                {
                    ((System.ComponentModel.ISynchronizeInvoke)(ARPScanFinished.Target)).BeginInvoke(ARPScanFinished, new object[] { new ARPScanEventArgs(args), this });
                }
                else
                {
                    ARPScanFinished(new ARPScanEventArgs(args), this);
                }
            }
        }

        /// <summary>
        /// Adds ARP scan tasks for the range between the given start and the given end address and associates them with the according interfaces
        /// </summary>
        /// <param name="ipaStart">The IP address where scanning starts</param>
        /// <param name="ipaEnd">The IP address where scanning ends</param>
        public void AddARPScanTask(System.Net.IPAddress ipaStart, System.Net.IPAddress ipaEnd)
        {
            if (lInterfaces.Count < 1)
            {
                throw new Exception("There are currently no interfaces present.");
            }
            foreach (EthernetInterface ipi in lInterfaces)
            {
                for (int iC1 = 0; iC1 < ipi.IpAddresses.Length && iC1 < ipi.Subnetmasks.Length; iC1++)
                {
                    if (ipv4Analysis.GetClasslessNetworkAddress(ipi.IpAddresses[iC1], ipi.Subnetmasks[iC1]).Equals(ipv4Analysis.GetClasslessNetworkAddress(ipaStart, ipi.Subnetmasks[iC1])) ||
                        ipv4Analysis.GetClasslessNetworkAddress(ipi.IpAddresses[iC1], ipi.Subnetmasks[iC1]).Equals(ipv4Analysis.GetClasslessNetworkAddress(ipaEnd, ipi.Subnetmasks[iC1])))
                    {
                        this.AddARPScanTask(new ARPScanTask(ipaStart, ipaEnd, ipi.PrimaryMACAddress, ipi.IpAddresses[iC1], this));
                    }
                }
            }
        }

        #region IScanner Members

        /// <summary>
        /// Adds ARP scan tasks for the range between the given start and the given end address and associates them with the according interfaces
        /// </summary>
        /// <param name="ipaScanStart">The IP address where scanning starts</param>
        /// <param name="ipaScanEnd">The IP address where scanning ends</param>
        public void Scan(System.Net.IPAddress ipaScanStart, System.Net.IPAddress ipaScanEnd)
        {
            AddARPScanTask(ipaScanStart, ipaScanEnd); 
        }

        #endregion

        /// <summary>
        /// Setting output handlers is not supported by ARP net scanners
        /// </summary>
        public override TrafficHandler OutputHandler
        {
            get { return null; }
            set { throw new InvalidOperationException("Traffic analyzers must not have any output"); }
        }

        /// <summary>
        /// Adds an interface to this ARP net scanner
        /// </summary>
        /// <param name="ipInterface">The interface to add only ethernet Interfaces are supported</param>
        public override void AddInterface(IPInterface ipInterface)
        {
            if (ipInterface.GetType() != typeof(EthernetInterface))
            {
                throw new ArgumentException("Only ethernet interfaces are supported for ARP scanning");
            }
            base.AddInterface(ipInterface);
        }
    }

    /// <summary>
    /// This class contains event arguments for ARP net scanner events
    /// </summary>
    public class ARPScanEventArgs : EventArgs
    {
        private ARPScanTask astTask;

        /// <summary>
        /// The ARP scan task associated with this event
        /// </summary>
        public ARPScanTask Task
        {
            get { return astTask; }
            set { astTask = value; }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="astTask">The ARP scan task associated with this event</param>
        public ARPScanEventArgs(ARPScanTask astTask)
        {
            this.astTask = astTask;
        }
    }
}
