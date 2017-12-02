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
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net.Configuration;
using System.Collections;
using System.Threading;


namespace eExNetworkLibrary.Utilities
{
    /// <summary>
    /// This class represents a simple utility for network discovery, pinging and tracing.
    /// <remarks>Warning: The implementation is a little bit sloppy (e.g. Thread.Abort etc.), but the class should work.</remarks>
    /// </summary>
    public class NetDiscoveryUtility
    {
        /// <summary>
        /// Delegate for handling ping results
        /// </summary>
        /// <param name="sender">The calling object</param>
        /// <param name="args">Result params</param>
        public delegate void PingResultEventHandler(object sender, PingCompletedEventArgs args);

        /// <summary>
        /// Delegate for handling pathping step results
        /// </summary>
        /// <param name="sender">The calling object</param>
        /// <param name="args">Result params</param>
        public delegate void PathpingStepEventHandler(object sender, PingReply args);

        /// <summary>
        /// Delegate for handling pathping results
        /// </summary>
        /// <param name="sender">The calling object</param>
        /// <param name="args">Result params</param>
        public delegate void PathpingCompletedEventHandler(object sender, PathpingCompletedEventArgs args);

        /// <summary>
        /// Delegate for name resolves results
        /// </summary>
        /// <param name="sender">The calling object</param>
        /// <param name="args">Result params</param>
        public delegate void ResolveCompletedEventHandler(object sender, IPHostEntry args);

        /// <summary>
        /// This event is fired whenever a ping is completed
        /// </summary>
        public event PingResultEventHandler OnPingCompleted;

        /// <summary>
        /// This event is fired whenever a pathping step is completed
        /// </summary>
        public event PathpingStepEventHandler OnPathpingStepCompleted;

        /// <summary>
        /// This event is fired whenever a network scan step is completed
        /// </summary>
        public event PingResultEventHandler OnNetScanStepCompleted;

        /// <summary>
        /// This event is fired whenever a net scan is completed
        /// </summary>
        public event EventHandler OnNetScanFinished;

        /// <summary>
        /// This event is fired whenever a pathping is completed
        /// </summary>
        public event PathpingCompletedEventHandler OnPathpingFinished;

        /// <summary>
        /// This event is fired whenever a name resolve is completed
        /// </summary>
        public event ResolveCompletedEventHandler OnResolveFinished;

        private ArrayList alTreads;
        private ArrayList alNetScanThreads;
        private int iCurrentPathpings;

        private int iMaxPralellPathpings;

        /// <summary>
        /// Gets or sets the maximum count of parralell pathpings.
        /// </summary>
        public int MaxParalellPathpings
        {
            get { return iMaxPralellPathpings; }
            set { iMaxPralellPathpings = value; }
        }

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        public NetDiscoveryUtility()
        {
            alTreads = new ArrayList();
            alNetScanThreads = new ArrayList();
            iCurrentPathpings = 0;
            iMaxPralellPathpings = 15;
        }

        private void myPing_PingCompleted(object sender, PingCompletedEventArgs e)
        {
            if (this.OnPingCompleted != null)
            {
                if (((System.ComponentModel.ISynchronizeInvoke)(OnPingCompleted.Target)).InvokeRequired)
                {
                    ((System.ComponentModel.ISynchronizeInvoke)(OnPingCompleted.Target)).Invoke(OnPingCompleted, new object[] { this, e });
                }
                else
                {
                    OnPingCompleted(this, e);
                }
            }
        }

        private void myPing_NetScanStepCompleted(object sender, PingCompletedEventArgs e)
        {
            if (this.OnNetScanStepCompleted != null)
            {
                if (((System.ComponentModel.ISynchronizeInvoke)(OnNetScanStepCompleted.Target)).InvokeRequired)
                {
                    ((System.ComponentModel.ISynchronizeInvoke)(OnNetScanStepCompleted.Target)).Invoke(OnNetScanStepCompleted, new object[] { this, e });
                }
                else
                {
                    OnNetScanStepCompleted(this, e);
                }
            }
        }
        
        /// <summary>
        /// Performs an asyncronous pathping to a specified target.
        /// </summary>
        /// <param name="ipaTarget">The target</param>
        public void PerformPingAsync(IPAddress ipaTarget)
        {
            Ping myPing = new Ping();
            myPing.PingCompleted += new PingCompletedEventHandler(myPing_PingCompleted);
            myPing.SendAsync(ipaTarget, null);
        }

        /// <summary>
        /// Performs an ping to a specified target
        /// </summary>
        /// <param name="ipaTarget">The target</param>
        /// <returns>The ping reply</returns>
        public PingReply PerformPing(IPAddress ipaTarget)
        {
            Ping myPing = new Ping();
            myPing.PingCompleted += new PingCompletedEventHandler(myPing_PingCompleted);
            return myPing.Send(ipaTarget);
        }

        #region NetScanAsync

        /// <summary>
        /// Starts an asyncronous netscan.
        /// </summary>
        /// <param name="ipaStart">The start IPAddress of the scan range</param>
        /// <param name="ipaEnd">The end IPAddress of the scan range</param>
        /// <param name="iSleepDuration">The timeout between each ping</param>
        /// <param name="iTimeout">The timeout to wait for each ping</param>
        public void NetScanAsync(IPAddress ipaStart, IPAddress ipaEnd, int iSleepDuration, int iTimeout)
        {
            Thread tNetScanner = new Thread(new ParameterizedThreadStart(this.NetScanAsyncInternalCall));
            tNetScanner.Name = "NetworkScanningThread" + alTreads.Count.ToString();
            tNetScanner.Start((object)new object[] { ipaStart, ipaEnd, iSleepDuration, iTimeout });
            alTreads.Add(tNetScanner);
            alNetScanThreads.Add(tNetScanner);
        }

        private void NetScanAsyncInternalCall(object oArgs)
        {
            object[] aoArgs = (object[])oArgs;
            IPAddress ipaStart = (IPAddress)aoArgs[0];
            IPAddress ipaEnd = (IPAddress)aoArgs[1];
            int iSleepDuration = (int)aoArgs[2];
            int iTimeout = (int)aoArgs[3];
            try
            {
                byte[] byteStartIP = ipaStart.GetAddressBytes();
                byte[] byteEndIP = ipaEnd.GetAddressBytes();
                IPAddress ipToPing;
                Ping myPing;

                while ((byteStartIP[0] << 24) + (byteStartIP[1] << 16) + (byteStartIP[2] << 8) + (byteStartIP[3]) <= (byteEndIP[0] << 24) + (byteEndIP[1] << 16) + (byteEndIP[2] << 8) + (byteEndIP[3]))
                {
                    ipToPing = new IPAddress(byteStartIP);
                    myPing = new Ping();
                    myPing.PingCompleted += new PingCompletedEventHandler(myPing_NetScanStepCompleted);
                    myPing.SendAsync(ipToPing, iTimeout, null);


                    byteStartIP[3] = Convert.ToByte(Convert.ToInt32(byteStartIP[3]) + 1);
                    if (byteStartIP[3] == Convert.ToByte(255))
                    {
                        byteStartIP[3] = Convert.ToByte(0);
                        byteStartIP[2] = Convert.ToByte(Convert.ToInt32(byteStartIP[2]) + 1);
                        if (byteStartIP[2] == Convert.ToByte(255))
                        {
                            byteStartIP[2] = Convert.ToByte(0);
                            byteStartIP[1] = Convert.ToByte(Convert.ToInt32(byteStartIP[1]) + 1);
                            if (byteStartIP[1] == Convert.ToByte(255))
                            {
                                byteStartIP[1] = Convert.ToByte(0);
                                byteStartIP[0] = Convert.ToByte(Convert.ToInt32(byteStartIP[0]) + 1);
                            }
                        }
                    }

                    Thread.Sleep(iSleepDuration);
                }
            }
            finally
            {
                Thread.Sleep(iTimeout);
                alTreads.Remove(Thread.CurrentThread);
                alNetScanThreads.Remove(Thread.CurrentThread);
                if (this.OnNetScanFinished != null)
                {
                    if (((System.ComponentModel.ISynchronizeInvoke)(OnNetScanFinished.Target)).InvokeRequired)
                    {
                        ((System.ComponentModel.ISynchronizeInvoke)(OnNetScanFinished.Target)).Invoke(OnNetScanFinished, new object[] { this, new EventArgs() });
                    }
                    else
                    {
                        this.OnNetScanFinished(this, new EventArgs());
                    }
                }
            }
        }

        #endregion

        #region NetScanSync

        /// <summary>
        /// Starts an syncronous netscan.
        /// </summary>
        /// <param name="ipaStart">The start IPAddress of the scan range</param>
        /// <param name="ipaEnd">The end IPAddress of the scan range</param>
        /// <param name="iTimeout">The timeout to wait for each ping</param>
        /// <returns>The ping replies for this netscan</returns>
        public PingReply[] NetScan(IPAddress ipaStart, IPAddress ipaEnd, int iTimeout)
        {
            byte[] byteStartIP = ipaStart.GetAddressBytes();
            byte[] byteEndIP = ipaEnd.GetAddressBytes();
            IPAddress ipToPing;
            System.Collections.ArrayList alReplies = new System.Collections.ArrayList();
            Ping myPing = new Ping();

            while ((byteStartIP[0] << 24) + (byteStartIP[1] << 16) + (byteStartIP[2] << 8) + (byteStartIP[3]) <= (byteEndIP[0] << 24) + (byteEndIP[1] << 16) + (byteEndIP[2] << 8) + (byteEndIP[3]))
            {
                ipToPing = new IPAddress(byteStartIP);

                alReplies.Add(myPing.Send(ipToPing, iTimeout));

                byteStartIP[3] = Convert.ToByte(Convert.ToInt32(byteStartIP[3]) + 1);
                if (byteStartIP[3] == Convert.ToByte(255))
                {
                    byteStartIP[3] = Convert.ToByte(0);
                    byteStartIP[2] = Convert.ToByte(Convert.ToInt32(byteStartIP[2]) + 1);
                    if (byteStartIP[2] == Convert.ToByte(255))
                    {
                        byteStartIP[2] = Convert.ToByte(0);
                        byteStartIP[1] = Convert.ToByte(Convert.ToInt32(byteStartIP[1]) + 1);
                        if (byteStartIP[1] == Convert.ToByte(255))
                        {
                            byteStartIP[1] = Convert.ToByte(0);
                            byteStartIP[0] = Convert.ToByte(Convert.ToInt32(byteStartIP[0]) + 1);
                        }
                    }
                }
            }

            PingReply[] prReturnValue = new PingReply[alReplies.Count];
            for (int iC1 = 0; iC1 < alReplies.Count; iC1++)
            {
                prReturnValue[iC1] = (PingReply)alReplies[iC1];
            }
            return prReturnValue;
        }
        #endregion

        /// <summary>
        /// Performs a pathping
        /// </summary>
        /// <param name="ipaTarget">The target</param>
        /// <param name="iHopcount">The maximum hopcount</param>
        /// <param name="iTimeout">The timeout for each ping</param>
        /// <returns>An array of PingReplys for the whole path</returns>
        public PingReply[] PerformPathping(IPAddress ipaTarget, int iHopcount, int iTimeout)
        {
            System.Collections.ArrayList arlPingReply = new System.Collections.ArrayList();
            Ping myPing = new Ping();
            PingReply prResult;
            for (int iC1 = 1; iC1 < iHopcount; iC1++)
            {
                prResult = myPing.Send(ipaTarget, iTimeout, new byte[10], new PingOptions(iC1, false));
                if (prResult.Status == IPStatus.Success)
                {
                    iC1 = iHopcount;
                }
                arlPingReply.Add(prResult);
            }
            PingReply[] prReturnValue = new PingReply[arlPingReply.Count];
            for (int iC1 = 0; iC1 < arlPingReply.Count; iC1++)
            {
                prReturnValue[iC1] = (PingReply)arlPingReply[iC1];
            }
            return prReturnValue;
        }

        #region PerformPathpingAsync

        /// <summary>
        /// Performs an asnycronous pathping
        /// </summary>
        /// <param name="ipTarget">The target</param>
        /// <param name="iHopcount">The maximum hopcount</param>
        /// <param name="iTimeout">The timeout for each ping</param>
        public void PerformPathpingAsync(IPAddress ipTarget, int iHopcount, int iTimeout)
        {
            Thread tPathping = new Thread(new ParameterizedThreadStart(PerformPathpingAsyncInternalCall));
            tPathping.Name = "PathpingThread" + alTreads.Count.ToString();
            tPathping.Start((object)new object[] { ipTarget, iHopcount, iTimeout });
            alTreads.Add(tPathping);
        }

        private void PerformPathpingAsyncInternalCall(object oArgs)
        {
            try
            {
                while (iCurrentPathpings > iMaxPralellPathpings)
                {
                    Thread.Sleep(500);
                }
                iCurrentPathpings++;
                IPAddress ipaTarget = (IPAddress)((object[])oArgs)[0];
                int iHopcount = (int)((object[])oArgs)[1];
                int iTimeout = (int)((object[])oArgs)[2];

                System.Collections.ArrayList arlPingReply = new System.Collections.ArrayList();
                Ping myPing = new Ping();
                PingReply prResult;
                for (int iC1 = 1; iC1 < iHopcount; iC1++)
                {
                    prResult = myPing.Send(ipaTarget, iTimeout, new byte[10], new PingOptions(iC1, false));
                    if (prResult.Status == IPStatus.Success)
                    {
                        iC1 = iHopcount;
                    }
                    if (prResult.Address != null)
                    {
                        prResult = myPing.Send(prResult.Address, iTimeout, new byte[10]);
                    }
                    arlPingReply.Add(prResult);
                    if (this.OnPathpingStepCompleted != null)
                    {
                        if (((System.ComponentModel.ISynchronizeInvoke)(OnPathpingStepCompleted.Target)).InvokeRequired)
                        {
                            ((System.ComponentModel.ISynchronizeInvoke)(OnPathpingStepCompleted.Target)).Invoke(OnPathpingStepCompleted, new object[] { this, prResult });
                        }
                        else
                        {
                            OnPathpingStepCompleted.Invoke(this, prResult);
                        }
                    }
                }

                if (this.OnPathpingFinished != null)
                {
                    PingReply[] prReturnValue = new PingReply[arlPingReply.Count];
                    for (int iC1 = 0; iC1 < arlPingReply.Count; iC1++)
                    {
                        prReturnValue[iC1] = (PingReply)arlPingReply[iC1];
                    }

                    if (((System.ComponentModel.ISynchronizeInvoke)(OnPathpingFinished.Target)).InvokeRequired)
                    {
                        ((System.ComponentModel.ISynchronizeInvoke)(OnPathpingFinished.Target)).Invoke(OnPathpingFinished, new object[] { this, new PathpingCompletedEventArgs(prReturnValue, ipaTarget) });
                    }
                    else
                    {
                        OnPathpingFinished.Invoke(this, new PathpingCompletedEventArgs(prReturnValue, ipaTarget));
                    }
                }
                alTreads.Remove(Thread.CurrentThread);

            }
            catch (PingException)
            {
                //nothing.
            }
            catch (ThreadAbortException)
            {

            }
            catch (ThreadInterruptedException)
            {

            }
            finally
            {
                iCurrentPathpings--;
            }
        }

	    #endregion  

        /// <summary>
        /// Cancels all currently running operations
        /// </summary>
        public void CancelAll()
        {
            object[] aThreads = alTreads.ToArray();
            for (int iC1 = 0; iC1 < aThreads.Length; iC1++)
            {
                if (aThreads[iC1] != null)
                {
                    ((Thread)aThreads[iC1]).Abort();
                }
            }
            alTreads.Clear();
        }

        /// <summary>
        /// Cancels all currently running netscans
        /// </summary>
        public void CancelNetScans()
        {
            object[] aThreads = alNetScanThreads.ToArray();
            for (int iC1 = 0; iC1 < aThreads.Length; iC1++)
            {
                ((Thread)aThreads[iC1]).Abort();
            }
        }

        #region ResolveHostnameAsync

        /// <summary>
        /// Resolves a hostname asyncronously
        /// </summary>
        /// <param name="ipaToResolve">The IPAddress to resolve</param>
        public void ResolveHostnameAsnc(IPAddress ipaToResolve)
        {
            Thread tResolver = new Thread(new ParameterizedThreadStart(ResolveInternalCall));
            tResolver.Start((object)ipaToResolve);
            tResolver.Name = "NameResolvingThread" + alTreads.Count.ToString();
            alTreads.Add(tResolver);
        }

        private void ResolveInternalCall(object args)
        {
            IPHostEntry iheReuslt;
            try
            {
                IPAddress ipaToResolve = (IPAddress)args;
                try
                {
                    iheReuslt = Dns.GetHostEntry(ipaToResolve);
                }
                catch
                {
                    iheReuslt = new IPHostEntry();
                    iheReuslt.AddressList = new IPAddress[] { ipaToResolve };
                    iheReuslt.HostName = "<unknown>";
                    iheReuslt.Aliases = new string[0];
                }
                if (this.OnResolveFinished != null)
                {
                    if (((System.ComponentModel.ISynchronizeInvoke)(OnResolveFinished.Target)).InvokeRequired)
                    {
                        ((System.ComponentModel.ISynchronizeInvoke)(OnResolveFinished.Target)).Invoke(OnResolveFinished, new object[] { this, iheReuslt });
                    }
                    else
                    {
                        OnResolveFinished.Invoke(this, iheReuslt);
                    }
                }
            }
            catch(ThreadAbortException)
            {
                
            }
            alTreads.Remove(Thread.CurrentThread);
        }

        #endregion

        /// <summary>
        /// Disposes this class and stops all threads
        /// </summary>
        ~NetDiscoveryUtility()
        {
            CancelAll();
        }
    }

    /// <summary>
    /// Represents simple class to notify about pathping results
    /// </summary>
    public class PathpingCompletedEventArgs : EventArgs
    {
        private PingReply[] prReplies;
        private IPAddress ipaTarget;

        /// <summary>
        /// The replies
        /// </summary>
        public PingReply[] Replies
        {
            get { return prReplies; }
        }

        /// <summary>
        /// The target
        /// </summary>
        public IPAddress Target
        {
            get { return ipaTarget; }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="prReplies">A array of replies</param>
        /// <param name="ipaTarget">The target</param>
        public PathpingCompletedEventArgs(PingReply[] prReplies, IPAddress ipaTarget) : base()
        {
            this.prReplies = prReplies;
            this.ipaTarget = ipaTarget;
        }
    }
}
