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
using System.Runtime.InteropServices;
using System.Net;
using System.Threading;
using eExNetworkLibrary.Ethernet;

namespace eExNetworkLibrary.Utilities
{
    /// <summary>
    /// This class is a managed wrapper for WinPcap, the famous packet capture library for windows.
    /// For information about WinPcap see http://www.winpcap.org.
    /// </summary>
    public class WinPcapDotNet
    {
        #if LIBPCAP
                private const string Lib = "libpcap.so";
        #else
                private const string Lib = "winpcap.dll";
        #endif

        [DllImport(Lib)]
        private static extern int pcap_findalldevs(ref IntPtr piDevices, string strErrorBuffer);
        [DllImport(Lib)]
        private static extern int pcap_setbuff(IntPtr iptrBuffer, int iSize);
        [DllImport(Lib)]
        private static extern int pcap_next_ex(IntPtr iptrBuffer, ref IntPtr iptrHeader, ref IntPtr iptrPacket);
        [DllImport(Lib)]
        private static extern IntPtr pcap_open(string source, int iSnaplen, PcapOpenflags flags, int iReadTimeOut, IntPtr pcapAuth, string strErrorBuffer);
        [DllImport(Lib)]
        private static extern void pcap_close(IntPtr iptrBuffer);
        [DllImport(Lib)]
        private static extern void pcap_freealldevs(IntPtr piDevices);
        [DllImport(Lib)]
        private static extern int pcap_loop(IntPtr ptrDevice, int iReadTimeout, PacketHandler phCallback, string strErrorBuffer);
        [DllImport(Lib)]
        private static extern int pcap_sendpacket(IntPtr ptrDevice, byte[] bPacket, int iPacketSize);
        [DllImport(Lib)]
        private static extern int pcap_compile(IntPtr ptrDevice, ref WinPcapFilter.WinPcapFilterStruct wpfsFilter, string strString, int iOptimize, uint uiNetmask);
        [DllImport(Lib)]
        private static extern string pcap_geterr(IntPtr ptrDevice);
        [DllImport(Lib)]
        private static extern int pcap_setfilter(IntPtr ptrDevice, ref WinPcapFilter.WinPcapFilterStruct wpfFilter);


        private IntPtr iptrOpenDevice;
        private Thread tWorker;
        private object oSendLock;
        private bool bRun;
        private WinPcapFilter wpfCurrentFilter;
        private WinPcapInterface wpcCurrentOpenInterface;
        /// <summary>
        /// This event is fired when a error occours in the internal worker threads.
        /// </summary>
        public event eExNetworkLibrary.TrafficHandler.ExceptionEventHandler ExceptionThrown;

        /// <summary>
        /// Gets or sets the kernel level filter for this device.
        /// </summary>
        public WinPcapFilter Filter
        {
            get
            {
                return wpfCurrentFilter;
            }
            set
            {
                if (value == null)
                {
                    Filter = CompileFilter("", false, new Subnetmask());
                }
                else
                {
                    if (iptrOpenDevice == IntPtr.Zero)
                    {
                        throw new Exception("No device is currently open");
                    }
                    WinPcapFilter.WinPcapFilterStruct wpfStruct = value.FilterStruct;
                    if (pcap_setfilter(iptrOpenDevice, ref wpfStruct) != 0)
                    {
                        throw new Exception("An error occured while setting the filter: " + pcap_geterr(iptrOpenDevice));
                    }

                    wpfCurrentFilter = value;
                }
            }
        }

        private delegate void PacketHandler(IntPtr iptrParams, PcapPacketHeader pHeader, IntPtr pPacketData);

        /// <summary>
        /// This delegate is used for handling captured bytes.
        /// </summary>
        /// <param name="wpcHeader">A WinPcapHeader describing packet properties</param>
        /// <param name="bPacketData">The captured packet data</param>
        /// <param name="sender">The calling object</param>
        public delegate void ByteCapturedHandler(WinPcapCaptureHeader wpcHeader, byte[] bPacketData, object sender);

        /// <summary>
        /// This event is fired whan bytes are captured
        /// </summary>
        public event ByteCapturedHandler BytesCaptured;

        /// <summary>
        /// Gets a bool indicating wheather this WinPcap devive is ready to capture data.
        /// </summary>
        public bool IsOpen
        {
            get { return iptrOpenDevice != IntPtr.Zero; }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public WinPcapDotNet()
        {
            iptrOpenDevice = IntPtr.Zero;
            oSendLock = new object();
        }

        /// <summary>
        /// Returns all knwon WinPcapInterfaces of this computer
        /// </summary>
        /// <returns>All knwon WinPcapInterfaces of this computer</returns>
        public static List<WinPcapInterface> GetAllDevices()
        {
            List<WinPcapInterface> lpInterfaces = new List<WinPcapInterface>();

            IntPtr pDevice = IntPtr.Zero;
            string strErrorBuffer = "";
            pcap_findalldevs(ref pDevice, strErrorBuffer);
            if (pDevice != IntPtr.Zero)
            {
                PcapInterface piDevice = (PcapInterface)Marshal.PtrToStructure(pDevice, typeof(PcapInterface));
                lpInterfaces.Add(new WinPcapInterface(piDevice.Name, piDevice.Description, GetIPAdresses(piDevice), piDevice.Flags));
                while (piDevice.Next != IntPtr.Zero)
                {
                    piDevice = (PcapInterface)Marshal.PtrToStructure(piDevice.Next, typeof(PcapInterface));
                    lpInterfaces.Add(new WinPcapInterface(piDevice.Name, piDevice.Description, GetIPAdresses(piDevice), piDevice.Flags));
                }
            }
            pcap_freealldevs(pDevice);
            return lpInterfaces;
        }

        /// <summary>
        /// Pushes a byte array to the output queue of this WinPacp interface. 
        /// </summary>
        /// <param name="bPacket">The bytes to send</param>
        public void SendPacket(byte[] bPacket)
        {
            lock (oSendLock)
            {
                if (iptrOpenDevice == IntPtr.Zero)
                {
                    throw new Exception("No device is currently open");
                }
                int iReturnValue = pcap_sendpacket(iptrOpenDevice, bPacket, bPacket.Length);
                if (iReturnValue != 0)
                {
                    throw new Exception("An error occured while sending. (Error Number: " + iReturnValue + ")");
                }
            }
        }

        /// <summary>
        /// Compiles a filter string to a kernel level WinPcap filter
        /// </summary>
        /// <param name="strFilterString">The filter expression to compile</param>
        /// <param name="bOptimize">A bool indicating if the expression should be atomatically optimized</param>
        /// <param name="smMask">The subnetmask to use for this expression. This subnetmask is only important for IP multicast or broadcast probes.</param>
        /// <returns>The compiled WinPcap filter</returns>
        public WinPcapFilter CompileFilter(string strFilterString, bool bOptimize, Subnetmask smMask)
        {
            if (iptrOpenDevice == IntPtr.Zero)
            {
                throw new Exception("No device is currently open");
            }

            WinPcapFilter.WinPcapFilterStruct wpcStruct = new WinPcapFilter.WinPcapFilterStruct();

            if (pcap_compile(iptrOpenDevice, ref wpcStruct, strFilterString, bOptimize ? 1 : 0, smMask.IntNotation) != 0)
            {
                throw new Exception("An error occured while trying to compile the filter: " + pcap_geterr(iptrOpenDevice));
            }

            return new WinPcapFilter(strFilterString, wpcStruct, smMask);
        }

        /// <summary>
        /// Opens the specified device for sniffing
        /// </summary>
        /// <param name="wpiInterface">The device to open</param>
        /// <param name="pofFlags">Configuration flags for opening</param>
        public void OpenDevice(WinPcapInterface wpiInterface, PcapOpenflags pofFlags)
        {
            if (bRun)
            {
                throw new InvalidOperationException("A capture is currently running");
            }
            CloseDevice();
            string strErrorbuff = "";
            IntPtr ptrDevice = pcap_open(wpiInterface.Name, 65535, pofFlags, 5, IntPtr.Zero, strErrorbuff);
            if (ptrDevice == IntPtr.Zero)
            {
                throw new Exception("Error when trying to open WinPcap device: " + strErrorbuff);
            }
            iptrOpenDevice = ptrDevice;
            wpcCurrentOpenInterface = wpiInterface;
            wpfCurrentFilter = CompileFilter("", true, new Subnetmask());
        }

        /// <summary>
        /// Starts the capture on the before opened device
        /// </summary>
        public void StartCapture()
        {
            if (iptrOpenDevice == IntPtr.Zero)
            {
                throw new InvalidOperationException("No device is currently open");
            }
            bRun = true;
            tWorker = new Thread(WorkingLoop);
            tWorker.Name = "WinPcap Worker Thread (" + wpcCurrentOpenInterface.Description + ")";
            tWorker.Start();
        }

        /// <summary>
        /// Stops the currently running capture
        /// </summary>
        public void StopCapture()
        {
            if (tWorker != null)
            {
                bRun = false;
                tWorker.Join();
            }
        }

        /// <summary>
        /// Closes the currently open device
        /// </summary>
        public void CloseDevice()
        {
            if (bRun)
            {
                throw new InvalidOperationException("A capture is currently running");
            }
            if (iptrOpenDevice != IntPtr.Zero)
            {
                pcap_close(iptrOpenDevice);
                iptrOpenDevice = IntPtr.Zero;
                wpcCurrentOpenInterface = null;
                wpfCurrentFilter = null;
            }
        }

        private void WorkingLoop()
        {
            try
            {
                IntPtr ptrPacket = IntPtr.Zero;
                IntPtr ptrHeader = IntPtr.Zero;

                while (bRun)
                {
                    int iReturn = pcap_next_ex(iptrOpenDevice, ref ptrHeader, ref ptrPacket);
                    if (iReturn == 1)
                    {
                        OnPacketCaptured(IntPtr.Zero, (PcapPacketHeader)Marshal.PtrToStructure(ptrHeader, typeof(PcapPacketHeader)), ptrPacket);
                    }
                    else if (iReturn == -1)
                    {
                        throw new Exception("An error occurred while reading from a WinPcap interface. (Error number: " + iReturn + ")");
                    }
                }

                ptrPacket = IntPtr.Zero;
                ptrHeader = IntPtr.Zero;
            }
            catch (Exception ex)
            {
                if (ExceptionThrown != null)
                {
                    foreach (Delegate dDelgate in ExceptionThrown.GetInvocationList())
                    {
                        if (dDelgate.Target != null && dDelgate.Target.GetType().GetInterface(typeof(System.ComponentModel.ISynchronizeInvoke).Name, true) != null
                            && ((System.ComponentModel.ISynchronizeInvoke)(dDelgate.Target)).InvokeRequired)
                        {
                            ((System.ComponentModel.ISynchronizeInvoke)(dDelgate.Target)).BeginInvoke(dDelgate, new object[] { this, new ExceptionEventArgs(ex, DateTime.Now) });
                        }
                        else
                        {
                            dDelgate.DynamicInvoke(this, new ExceptionEventArgs(ex, DateTime.Now));
                        }
                    }
                }
            }
        }

        private void OnPacketCaptured(IntPtr iptrParams, PcapPacketHeader pHeader, IntPtr pPacketData)
        {
            if (BytesCaptured != null)
            {
                DateTime dtCaptureDate = DateTime.Now;
                byte[] bBuffer = new byte[pHeader.PacketLength];
                Marshal.Copy(pPacketData, bBuffer, 0, (int)pHeader.PacketLength);
                WinPcapCaptureHeader wpcHeader = new WinPcapCaptureHeader(dtCaptureDate, (int)pHeader.CaptureLength, (int)pHeader.PacketLength);

                if (BytesCaptured != null)
                {
                    foreach (Delegate dDelgate in BytesCaptured.GetInvocationList())
                    {
                        if (dDelgate.Target != null && dDelgate.Target.GetType().GetInterface(typeof(System.ComponentModel.ISynchronizeInvoke).Name, true) != null
                            && ((System.ComponentModel.ISynchronizeInvoke)(dDelgate.Target)).InvokeRequired)
                        {
                            ((System.ComponentModel.ISynchronizeInvoke)(dDelgate.Target)).BeginInvoke(dDelgate, new object[] { wpcHeader, bBuffer, this });
                        }
                        else
                        {
                            dDelgate.DynamicInvoke(wpcHeader, bBuffer, this);
                        }
                    }
                }
            }
        }

        private static IPAddress[] GetIPAdresses(PcapInterface piDevice)
        {
            List<IPAddress> lipAdresses = new List<IPAddress>();
            IntPtr ptrAddress = piDevice.Address;
            SocketAddress saAddress;
            PcapAddress pcAddress;
            while (ptrAddress != IntPtr.Zero)
            {
                pcAddress = (PcapAddress)Marshal.PtrToStructure(ptrAddress, typeof(PcapAddress));
                saAddress = (SocketAddress)Marshal.PtrToStructure(pcAddress.Address, typeof(SocketAddress));
                lipAdresses.Add(new IPAddress(saAddress.Address));
                ptrAddress = pcAddress.Next;
            }
            return lipAdresses.ToArray();
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PcapInterface
        {
            public IntPtr Next;
            public string Name;
            public string Description;
            public IntPtr Address;
            public uint Flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PcapAddress
        {
            public IntPtr Next;
            public IntPtr Address;
            public IntPtr Netmask;
            public IntPtr BroadcastAddress;
            public IntPtr Destination;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PcapPacketHeader
        {
            public Timestamp Timestamp;
            public UInt32 CaptureLength;
            public UInt32 PacketLength;
        }


        [StructLayout(LayoutKind.Sequential)]
        private struct Timestamp
        {
            public System.UInt32 Seconds; // sec
            public System.UInt32 Milliseconds; // millisec
        }


        [StructLayout(LayoutKind.Sequential)]
        private struct SocketAddress
        {
            public short Family;
            public ushort Port;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] Address;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] Zero;
        }
    }

    /// <summary>
    /// Represents a WinPcapCaptureHeader
    /// </summary>
    public class WinPcapCaptureHeader
    {
        private DateTime dtTimestamp;
        private int iCapturelength;
        private int iPacketlength;

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="dtTimestamp">The timestamp</param>
        /// <param name="iCapturelength">The length of the captured data</param>
        /// <param name="iPacketlength">The length of the packet</param>
        public WinPcapCaptureHeader(DateTime dtTimestamp, int iCapturelength, int iPacketlength)
        {
            this.dtTimestamp = dtTimestamp;
            this.iCapturelength = iCapturelength;
            this.iPacketlength = iPacketlength;
        }

        /// <summary>
        /// The length of the captured bytes
        /// </summary>
        public int Capturelength
        {
            get { return iCapturelength; }
        }


        /// <summary>
        /// The length of the frame
        /// </summary>
        public int Packetlength
        {
            get { return iPacketlength; }
        }

        /// <summary>
        /// The timestamp when the data was captrued
        /// </summary>
        public DateTime Timestamp
        {
            get { return dtTimestamp; }
        }
    }

    /// <summary>
    /// This class represents a WinPcap capable network interface
    /// </summary>
    public class WinPcapInterface
    {
        private string strName;
        private string strDescription;
        private IPAddress[] ipAddresses;
        private uint uiFlags;

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="strName">The name of the interface</param>
        /// <param name="strDescription">The description of the interface</param>
        /// <param name="ipAddresses">The IP addresses of the interface</param>
        /// <param name="uiFlags">The flags of the interface</param>
        public WinPcapInterface(string strName, string strDescription, IPAddress[] ipAddresses, uint uiFlags)
        {
            this.strName = strName;
            this.strDescription = strDescription;
            this.ipAddresses = ipAddresses;
            this.uiFlags = uiFlags;
        }

        /// <summary>
        /// Gets the name of the interface
        /// </summary>
        public string Name
        {
            get { return strName; }
        }

        /// <summary>
        /// Gets the description of the interface
        /// </summary>
        public string Description
        {
            get { return strDescription; }
        }

        /// <summary>
        /// Gets the IP addresses of the interface
        /// </summary>
        public IPAddress[] Addresses
        {
            get { return ipAddresses; }
        }
        /// <summary>
        /// Gets the flags of the interface
        /// </summary>
        public uint Flags
        {
            get { return uiFlags; }
        }
    }

    /// <summary>
    /// This class represents a WinPcap kernel level filter
    /// </summary>
    public class WinPcapFilter
    {
        string strFilterString;
        WinPcapFilterStruct wpcFilterStruct;
        Subnetmask smSubnetmask;

        /// <summary>
        /// The filter expression of this WinPcap filter
        /// </summary>
        public string FilterExpression
        {
            get { return strFilterString; }
        }        
        
        /// <summary>
        /// The subnet mask associated with this WinPcap filter
        /// </summary>
        public Subnetmask Subnetmask
        {
            get { return smSubnetmask; }
        }

        /// <summary>
        /// The compiled WinPcap filter structure
        /// </summary>
        internal WinPcapFilterStruct FilterStruct
        {
            get { return wpcFilterStruct; }
        }

        internal WinPcapFilter(string strFilterString, WinPcapFilterStruct wpcFilterStruct, Subnetmask smSubnetmask)
        {
            this.strFilterString = strFilterString;
            this.wpcFilterStruct = wpcFilterStruct;
            this.smSubnetmask = smSubnetmask;
        }

        /// <summary>
        /// Internal representation of WinPcap filters
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct WinPcapFilterStruct
        {
            /// <summary>
            /// Length of the filter program
            /// </summary>
            public System.UInt32 FilterLength;
            /// <summary>
            /// A pointer to the first instruction of the filter program
            /// </summary>
            public System.IntPtr FirstInstruction;
        }
    }



    /// <summary>
    /// An enumeration for WinPcap open flags
    /// For more information see http://www.winpcap.org/docs/docs_41b5/html/group__remote__open__flags.html
    /// </summary>
    public enum PcapOpenflags
    {
        /// <summary>
        /// Defines if the adapter has to go in promiscuous mode.
        /// </summary>
        Promiscuous = 1,
        /// <summary>
        /// Defines if the data trasfer (in case of a remote capture) has to be done with UDP protocol. 
        /// </summary>
        Data_UDP = 2,
        /// <summary>
        /// Defines if the remote probe will capture its own generated traffic. 
        /// </summary>
        Nocapture_RPCAP = 4,
        /// <summary>
        /// Defines if the local adapter will capture its own generated traffic. 
        /// </summary>
        Nocapture_Local = 8,
        /// <summary>
        /// This flag configures the adapter for maximum responsiveness. 
        /// </summary>
        Max_Responsiveness = 16
    }
}
