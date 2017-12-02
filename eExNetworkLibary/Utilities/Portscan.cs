// This source file is part of the eEx Network Library
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
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace eExNetworkLibrary.Utilities
{
    /// <summary>
    /// This class is capable of doing simple portscans
    /// </summary>
    public class Portscan
    {
        /// <summary>
        /// A delegate for handling finished portscans
        /// </summary>
        /// <param name="sender">The calling object</param>
        /// <param name="args">The arguments</param>
        public delegate void PortscanCompletetEventHandler(object sender, PortscanCompletedEventArgs args);

        /// <summary>
        /// This event is fired whan a portscan is finished
        /// </summary>
        public event PortscanCompletetEventHandler onPortscanCompleted;

        private IPAddress ipaTarget;
        private int iPort;
        private Thread tWorker;

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="ipaTarget">The IPAddress to scan</param>
        /// <param name="iPort">The port to scan</param>
        public Portscan(IPAddress ipaTarget, int iPort)
        {
            this.ipaTarget = ipaTarget;
            this.iPort = iPort;
        }

        /// <summary>
        /// Scans the target port on the target host synchronously.
        /// </summary>
        /// <returns>A bool indicating whether the port is open.</returns>
        public bool Scan()
        {
            Socket sSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sSocket.SendTimeout = 1000;
            sSocket.ReceiveTimeout = 1000;
            
            try
            {
                sSocket.Connect(new IPEndPoint(ipaTarget, iPort));
                sSocket.Close();
                //System.Diagnostics.Debug.WriteLine("Sucess!");
                return true;
            }
            catch(Exception)
            {
                //System.Diagnostics.Debug.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Scans the target port on the target host asynchronously.
        /// The result will be delivered by rising the PortscanCompleted event.
        /// </summary>
        public void ScanAsync()
        {
            tWorker = new Thread(new ThreadStart(ScanAsyncInternal));
            tWorker.Start();
        }

        private void ScanAsyncInternal()
        {
            try
            {
                PortscanCompletedEventArgs pceArgs = new PortscanCompletedEventArgs(ipaTarget, iPort, Scan());

                if (this.onPortscanCompleted != null)
                {
                    if (((System.ComponentModel.ISynchronizeInvoke)(onPortscanCompleted.Target)).InvokeRequired)
                    {
                        ((System.ComponentModel.ISynchronizeInvoke)(onPortscanCompleted.Target)).Invoke(onPortscanCompleted, new object[] { this, pceArgs });
                    }
                    else
                    {
                        onPortscanCompleted(this, pceArgs);
                    }
                }
            }
            catch (ThreadAbortException) { }
        }

        /// <summary>
        /// Immideately stops the current asyncronous scan.
        /// </summary>
        public void StopAsyncScan()
        {
            if (tWorker != null && tWorker.IsAlive)
            {
                tWorker.Abort();
            }
        }
    }

    /// <summary>
    /// A class which represents simple EventArgs to deliver a completed portscans status.
    /// </summary>
    public class PortscanCompletedEventArgs : EventArgs
    {
        private IPAddress ipaIP;
        private int iPort;
        private bool bSuccess;


        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="ipaTarget">The target host</param>
        /// <param name="iPort">The target port</param>
        /// <param name="bSuccess">A bool indicating whether the port was open</param>
        public PortscanCompletedEventArgs(IPAddress ipaTarget, int iPort, bool bSuccess)
            : base()
        {
            this.ipaIP = ipaTarget;
            this.iPort = iPort;
            this.bSuccess = bSuccess;
        }

        /// <summary>
        /// The target host
        /// </summary>
        public IPAddress Target
        {
            get { return ipaIP; }
        }

        /// <summary>
        /// The target port
        /// </summary>
        public int Port
        {
            get { return iPort; }
        }

        /// <summary>
        /// A bool indicating whether the port was open
        /// </summary>
        public bool Success
        {
            get { return bSuccess; }
        }
    }
}
