using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace eExNetworkLibrary.CommonTrafficAnalysis
{
    /// <summary>
    /// This class is capable of dumping frames in the LibCap dumping format which can be read by wireshark and other protocol analyzers.
    /// <remarks>
    /// This class also provides the capability to create a new wireshark instance and send all captured frames to it in real time. 
    /// This feature requires wireshark to be installed on the executing host.
    /// </remarks>
    /// </summary>
    public class LibCapDumper : TrafficAnalyzer
    {
        Stream sFilestream;
        BinaryWriter bw;
        bool bReadyToLog;
        string strFileName;
        private int iLogByteCount;
        bool bAppend;
        bool bIsLiveLogging;
        Process pWireshark;
        private BinaryWriter bwLiveCapture;


        /// <summary>
        /// This event is fired when the wireshark live logging process exits.
        /// </summary>
        public event EventHandler WiresharkExited;
        /// <summary>
        /// This event is fired when the wireshark live logging process is started.
        /// </summary>
        public event EventHandler WiresharkStarted;

        /// <summary>
        /// This event is rised when logging was started
        /// </summary>
        public event EventHandler LoggingStarted;
        /// <summary>
        /// This event is rised when logging was stopped
        /// </summary>
        public event EventHandler LoggingStopped;

        /// <summary>
        /// Returns the count of all dumped bytes
        /// </summary>
        public int DumpByteCount
        {
            get { return iLogByteCount; }
        }

        /// <summary>
        /// Returns the name of the dump file
        /// </summary>
        public string FileName
        {
            get { return strFileName; }
        }

        /// <summary>
        /// Returns a bool indicating whether this instance is appending its dumps to an existing file
        /// </summary>
        public bool IsAppending
        {
            get { return bAppend; }
        }

        /// <summary>
        /// Returns a bool indicating whether this instance is logging all traffic to a running wireshark format
        /// </summary>
        public bool IsLiveLogging
        {
            get { return bIsLiveLogging; }
        }

        /// <summary>
        /// Returns a bool indicating whether this instance is currently dumping
        /// </summary>
        public bool IsDumping
        {
            get { return bReadyToLog; }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public LibCapDumper()
        {
            strFileName = "";
        }

        /// <summary>
        /// Starts logging into the given file
        /// </summary>
        /// <param name="strFile">The file to log the frames into</param>
        /// <param name="bAppend">A bool indicating whether the data should be appendet or not</param>
        public void StartLogging(string strFile, bool bAppend)
        {
            if (!bReadyToLog)
            {
                this.bAppend = bAppend;
                strFileName = strFile;
                iLogByteCount = 0;
                if (bAppend && File.Exists(strFile))
                {
                    sFilestream = File.Open(strFile, FileMode.Append);
                    bw = new BinaryWriter(sFilestream);
                    bReadyToLog = true;
                }
                else
                {
                    sFilestream = File.Open(strFile, FileMode.Create);
                    bw = new BinaryWriter(sFilestream);
                    WriteLogfieHeader(bw);
                    bReadyToLog = true;
                }
                InvokeExternalAsync(LoggingStarted);
            }
            else
            {
                throw new InvalidOperationException("The current logging process has to be stopped before a new one can be started.");
            }
        }

        /// <summary>
        /// Starts logging into the given file
        /// </summary>
        /// <param name="strFile">The file to log the frames into</param>
        public void StartLogging(string strFile)
        {
            StartLogging(strFile, true);
        }

        /// <summary>
        /// Starts live logging to a wireshark instance. The path to the wireshark executeable file must be given.
        /// </summary>
        /// <param name="strWiresharkExecuteableFilename">The path to the wireshark executeable file.</param>
        public void StartLiveLogging(string strWiresharkExecuteableFilename)
        {
            if (bIsLiveLogging)
            {
                throw new InvalidOperationException("A live logging session is already open for this libpcap dumper.");
            }
            ProcessStartInfo psiInfo = new ProcessStartInfo();
            psiInfo.Arguments = "-k -i -";
            psiInfo.UseShellExecute = false;
            psiInfo.FileName = strWiresharkExecuteableFilename;
            psiInfo.RedirectStandardInput = true;
            pWireshark = new Process();
            pWireshark.StartInfo = psiInfo;
            pWireshark.EnableRaisingEvents = true;
            pWireshark.Exited += new EventHandler(pWireshark_Exited);
            pWireshark.Start();
            InvokeExternalAsync(WiresharkStarted);
            bwLiveCapture = new BinaryWriter(pWireshark.StandardInput.BaseStream);
            WriteLogfieHeader(bwLiveCapture);
            bIsLiveLogging = true;
        }

        /// <summary>
        /// Tries to stop the running wireshark instance.
        /// </summary>
        public void StopLiveLogging()
        {
            if (!bIsLiveLogging)
            {
                throw new InvalidOperationException("No wireshark instance is currently running for this libpcap dumper.");
            }
            pWireshark.CloseMainWindow();
        }

        void pWireshark_Exited(object sender, EventArgs e)
        {
            bIsLiveLogging = false;
            InvokeExternalAsync(WiresharkExited);
            bwLiveCapture.Close();
        }

        /// <summary>
        /// Stops the current logging process
        /// </summary>
        public void StopLogging()
        {
            if (bReadyToLog)
            {
                strFileName = "";
                bReadyToLog = false;
                bw.Close();
                sFilestream.Close();
                InvokeExternalAsync(LoggingStopped);
            }
            else
            {
                throw new InvalidOperationException("There is currently no logging process running.");
            }
        }

        /// <summary>
        /// Writes a libpcap file header to the given binary writer.
        /// </summary>
        /// <param name="bw">The binary writer to write the header to.</param>
        protected void WriteLogfieHeader(BinaryWriter bw)
        {
            iLogByteCount += 24;
            bw.Write(0xa1b2c3d4); //Magic number
            bw.Write((short)2); //Major version
            bw.Write((short)4); //Minior version
            bw.Write(0); //GMT offset
            bw.Write(0); //Accuratiy
            bw.Write(65565); //Snap length
            bw.Write((int)LibCapInterfaceType.Ethernet); //Interface type
        }

        /// <summary>
        /// Writes the packet header for the given frame to the given binary writer.
        /// </summary>
        /// <param name="fFrame">The frame to write the header for</param>
        /// <param name="bw">The binary writer to write the header to.</param>
        protected void WritePacketHeader(Frame fFrame, BinaryWriter bw)
        {
            iLogByteCount += 16;
            TrafficDescriptionFrame tdf = (TrafficDescriptionFrame)GetFrameByType(fFrame, FrameType.TrafficDescriptionFrame);
            uint ts_sec;
            uint ts_usec;
            if (tdf != null)
            {
                DateTime dateToConvert = tdf.CaptureTime;
                DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                TimeSpan diff = dateToConvert - origin; // Seconds since 1970
                ts_sec = (uint)Math.Floor(diff.TotalSeconds); // Microsecond offset
                ts_usec = (uint)(1000000 * (diff.TotalSeconds - ts_sec));
            }
            else
            {
                DateTime dateToConvert = DateTime.Now;
                DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                TimeSpan diff = dateToConvert - origin; // Seconds since 1970
                ts_sec = (uint)Math.Floor(diff.TotalSeconds); // Microsecond offset
                ts_usec = (uint)(1000000 * (diff.TotalSeconds - ts_sec));
            }

            bw.Write(ts_sec); //Seconds
            bw.Write(ts_usec); //Nanos
            bw.Write(fFrame.Length); //Len
            bw.Write(fFrame.Length); //Len
        }

        /// <summary>
        /// Writes the given frame to the dump file
        /// </summary>
        /// <param name="fInputFrame">The frame to dump</param>
        protected override void HandleTraffic(Frame fInputFrame)
        {
            byte[] bData;
            if (bReadyToLog || bIsLiveLogging)
            {
                bData = fInputFrame.FrameBytes;
                if (bReadyToLog)
                {
                    WritePacketHeader(fInputFrame, bw);
                    bw.Write(bData);
                }
                if (bIsLiveLogging)
                {
                    WritePacketHeader(fInputFrame, bwLiveCapture);
                    bwLiveCapture.Write(bData);
                    bwLiveCapture.Flush();
                }
                iLogByteCount += bData.Length;
            }
        }

        /// <summary>
        /// Stops logging and all worker threads
        /// </summary>
        public override void Stop()
        {
            if (this.IsDumping)
            {
                StopLogging();
            }
            base.Stop();
        }

        /// <summary>
        /// Does nothing
        /// </summary>
        public override void Cleanup()
        {
            //Don't need to do anything on init shutdown. 
        }
    }

    /// <summary>
    /// An enumeration for LibCapInterface types, which have to be written into the dumpfile
    /// </summary>
    public enum LibCapInterfaceType
    {
        /// <summary>
        /// Ethernet
        /// </summary>
        Ethernet = 1
    }
}
