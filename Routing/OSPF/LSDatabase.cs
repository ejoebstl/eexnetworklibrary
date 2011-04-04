using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace eExNetworkLibrary.Routing.OSPF
{
    [Obsolete("This class is marked depreceated in cause of a sloppy implementation and only limited compatibility..", false)]
    class LSDatabase
    {
        private List<LSAHeader> lLSA;
        private Timer tAge;

        public LSDatabase()
        {
            lLSA = new List<LSAHeader>();
            tAge = new Timer();
            tAge.Interval = 1000;
            tAge.AutoReset = true;
            tAge.Elapsed += new ElapsedEventHandler(tAge_Elapsed);
        }

        void tAge_Elapsed(object sender, ElapsedEventArgs e)
        {
            bool bRemoved = false;
            lock (lLSA)
            {
                LSAHeader[] arlsaHeader = lLSA.ToArray();
                foreach (LSAHeader lsaHeader in arlsaHeader)
                {
                    lsaHeader.LSAge++;
                    if (lsaHeader.LSAge >= LSAHeader.LS_MaxAge)
                    {
                        lLSA.Remove(lsaHeader);
                        bRemoved = true;
                    }
                }
            }

            if (bRemoved)
            {
                Invoke(LSAOutdated, EventArgs.Empty);
            }
        }

        public event EventHandler LSAOutdated;
        public event EventHandler LSAAdded;
        public event EventHandler LSARemoved;

        public void AddLSARange(LSAHeader[] arlsa)
        {
            bool bAdded = false;
            bool bFound;
            lock (lLSA)
            {
                LSAHeader[] arlsaHeader = lLSA.ToArray();
                foreach (LSAHeader lsa in arlsa)
                {
                    bFound = false;
                    foreach (LSAHeader lsaHeader in arlsaHeader)
                    {
                        if (LSAEquals(lsaHeader, lsa))
                        {
                            bFound = true;
                            if (lsaHeader.SequenceNumber < lsa.SequenceNumber)
                            {
                                lLSA.Remove(lsaHeader);
                                lLSA.Add(lsa);
                                bAdded = true;
                                break;
                            }
                        }
                    }
                    if (!bFound)
                    {
                        lLSA.Add(lsa);
                        bAdded = true;
                    }
                }
            }
            if (bAdded)
            {
                Invoke(LSAAdded, EventArgs.Empty);
            }
        }

        public void RemoveLSARange(LSAHeader[] arlsa)
        {
            bool bRemoved = false;
            lock (lLSA)
            {
                LSAHeader[] arlsaHeader = lLSA.ToArray();
                foreach (LSAHeader lsa in arlsa)
                {
                    foreach (LSAHeader lsaHeader in arlsaHeader)
                    {
                        if (LSAEquals(lsaHeader, lsa))
                        {
                            lLSA.Remove(lsaHeader);
                            bRemoved = true;
                            break;
                        }
                    }
                }
            }
            if (bRemoved)
            {
                Invoke(LSARemoved, EventArgs.Empty);
            }
        }

        private bool LSAEquals(LSAHeader lsaOne, LSAHeader lsaTwo)
        {
            if (lsaOne.LSType != lsaTwo.LSType)
            {
                return false;
            }
            if (lsaOne.AdvertisingRouter != lsaTwo.AdvertisingRouter)
            {
                return false;
            }
            if (lsaOne.LinkStateID != lsaTwo.LinkStateID)
            {
                return false;
            }

            return true;
        }

        public LSAHeader[] GetLSAs()
        {
            LSAHeader[] arlsaHeader;
            lock (lLSA)
            {
                arlsaHeader = lLSA.ToArray();
            }
            return arlsaHeader;
        }

        public void Clear()
        {
            lock (lLSA)
            {
                lLSA.Clear();
            }
        }


        protected void Invoke(Delegate d, object param)
        {
            if (d != null)
            {
                foreach (Delegate dDelgate in d.GetInvocationList())
                {
                    if (dDelgate.Target != null && dDelgate.Target.GetType().GetInterface(typeof(System.ComponentModel.ISynchronizeInvoke).Name, true) != null
                        && ((System.ComponentModel.ISynchronizeInvoke)(dDelgate.Target)).InvokeRequired)
                    {
                        ((System.ComponentModel.ISynchronizeInvoke)(dDelgate.Target)).BeginInvoke(dDelgate, new object[] { this, param });
                    }
                    else
                    {
                        dDelgate.DynamicInvoke(this, param);
                    }
                }
            }
        }

    }
}
