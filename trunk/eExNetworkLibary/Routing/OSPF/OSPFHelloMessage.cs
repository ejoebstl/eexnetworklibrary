using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace eExNetworkLibrary.Routing.OSPF
{
    /// <summary>
    /// This class represents an OSPF hello message
    /// </summary>
    public class OSPFHelloMessage : Frame
    {
        public static string DefaultFrameType { get { return "OSPFHello"; } }

        private Subnetmask nNetmask; //4 byte
        private int iHelloInterval; //2 byte
        private OSPFOptionsField ospfOptions;
        private byte bPriority; //1 byte
        private int iDeadInterval; //4 byte
        private IPAddress ipaDesignatedRouter; // 4 byte
        private IPAddress ipaBackupDesignatedRouter; //4 byte
        private List<IPAddress> lipaNeighbours; //N*4 bytes

        #region Props

        /// <summary>
        /// Gets or sets the OSPF options field
        /// </summary>
        public OSPFOptionsField Options
        {
            get { return ospfOptions; }
            set { ospfOptions = value; }
        }

        /// <summary>
        /// Gets or sets the subnetmask
        /// </summary>
        public Subnetmask Netmask
        {
            get { return nNetmask; }
            set { nNetmask = value; }
        }

        /// <summary>
        /// Gets or sets the hello interval
        /// </summary>
        public int HelloInterval
        {
            get { return iHelloInterval; }
            set { iHelloInterval = value; }
        }

        /// <summary>
        /// Gets or sets the priority
        /// </summary>
        public byte Priority
        {
            get { return bPriority; }
            set { bPriority = value; }
        }

        /// <summary>
        /// Gets or sets the dead interval
        /// </summary>
        public int DeadInterval
        {
            get { return iDeadInterval; }
            set { iDeadInterval = value; }
        }

        /// <summary>
        /// Gets or sets the designated router
        /// </summary>
        public IPAddress DesignatedRouter
        {
            get { return ipaDesignatedRouter; }
            set { ipaDesignatedRouter = value; }
        }

        /// <summary>
        /// Gets or sets the backup designated router
        /// </summary>
        public IPAddress BackupDesignatedRouter
        {
            get { return ipaBackupDesignatedRouter; }
            set { ipaBackupDesignatedRouter = value; }
        }

        /// <summary>
        /// Removes all neighbours from the neighbour list of this frame
        /// </summary>
        public void ClearNeighbours()
        {
            lipaNeighbours.Clear();
        }

        /// <summary>
        /// Adds a neighbour to the neighbour list of this frame
        /// </summary>
        /// <param name="ipa">The neighbour IP-address to add</param>
        public void AddNeighbour(IPAddress ipa)
        {
            lipaNeighbours.Add(ipa);
        }

        /// <summary>
        /// Returns all neighbours from the neighbour list of this frame
        /// </summary>
        /// <returns>All neighbours IP-addresses from the neighbour list of this frame</returns>
        public IPAddress[] GetNeighbours()
        {
            return lipaNeighbours.ToArray();
        }

        /// <summary>
        /// Checks whether a specified neighbour is contained in this frames neighbour list
        /// </summary>
        /// <param name="ipa">The neighbours IP-address to search for</param>
        /// <returns>A bool indicating whether a specified neighbour is contained in this frames neighbour list</returns>
        public bool ContainsNeighbour(IPAddress ipa)
        {
            return lipaNeighbours.Contains(ipa);
        }

        /// <summary>
        /// Removes a neighbour from the neighbour list of this frame
        /// </summary>
        /// <param name="ipa">The neighbour IP-address to remove</param>
        public void RemoveNeighbour(IPAddress ipa)
        {
            lipaNeighbours.Remove(ipa);
        }

        #endregion

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public OSPFHelloMessage()
        {
            nNetmask = new Subnetmask();
            iHelloInterval = 0;
            bPriority = 0;
            iDeadInterval = 30;
            ipaDesignatedRouter = IPAddress.Any;
            ipaBackupDesignatedRouter = IPAddress.Any;
            lipaNeighbours = new List<IPAddress>();
            ospfOptions = new OSPFOptionsField();
        }

        /// <summary>
        /// Creates a new instance of this class by parsing the given data
        /// </summary>
        /// <param name="bData">The data to pase</param>
        public OSPFHelloMessage(byte[] bData)
        {
            byte[] bTmpBytes = new byte[4];
            for (int iC1 = 0; iC1 < 4; iC1++)
            {
                bTmpBytes[iC1] = bData[iC1];
            }
            nNetmask = new Subnetmask(bTmpBytes);
            iHelloInterval = (bData[5] << 8) + bData[6];
            ospfOptions = new OSPFOptionsField(bData[6]);
            bPriority = bData[7];
            iDeadInterval = ((int)bData[8] << 24) + ((int)bData[9] << 16) + ((int)bData[10] << 8) + bData[11];
           
            for (int iC1 = 12; iC1 < 16; iC1++)
            {
                bTmpBytes[iC1 - 12] = bData[iC1];
            }

            ipaDesignatedRouter = new IPAddress(bTmpBytes);

            for (int iC1 = 16; iC1 < 20; iC1++)
            {
                bTmpBytes[iC1 - 16] = bData[iC1];
            }

            ipaBackupDesignatedRouter = new IPAddress(bTmpBytes);

            lipaNeighbours = new List<IPAddress>();

            for (int iC2 = 20; iC2 < bData.Length; iC2 += 4)
            {
                for (int iC1 = iC2; iC1 < iC2 + 4; iC1++)
                {
                    bTmpBytes[iC1 - iC2] = bData[iC1];
                }
                lipaNeighbours.Add(new IPAddress(bTmpBytes));
            }
        }

        /// <summary>
        /// Creates an identical copy of this frame
        /// </summary>
        /// <returns>An identical copy of this frame</returns>
        public override Frame Clone()
        {
            return new OSPFHelloMessage(this.FrameBytes);
        }

        /// <summary>
        /// Returns the type of this frame.
        /// </summary>
        public override string FrameType
        {
            get { return OSPFHelloMessage.DefaultFrameType; }
        }

        /// <summary>
        /// Returns the raw byte representation of this frame and its encapsulated frame
        /// </summary>
        public override byte[] FrameBytes
        {
            get 
            {
                byte[] bData = new byte[this.Length];
                byte[] bTmpBytes = nNetmask.MaskBytes;

                for (int iC1 = 0; iC1 < 4; iC1++)
                {
                    bData[iC1] = bTmpBytes[iC1];
                }

                bData[4] = (byte)((iHelloInterval >> 8) & 0xFF);
                bData[5] = (byte)((iHelloInterval) & 0xFF);
                bData[6] = ospfOptions.Data;

                bData[7] = bPriority;
                bData[8] = (byte)((iDeadInterval >> 24) & 0xFF);
                bData[9] = (byte)((iDeadInterval >> 16) & 0xFF);
                bData[10] = (byte)((iDeadInterval >> 8) & 0xFF);
                bData[11] = (byte)((iDeadInterval) & 0xFF);

                bTmpBytes = ipaDesignatedRouter.GetAddressBytes();

                for (int iC1 = 12; iC1 < 16; iC1++)
                {
                    bData[iC1] = bTmpBytes[iC1 - 12];
                }

                bTmpBytes = ipaBackupDesignatedRouter.GetAddressBytes();

                for (int iC1 = 16; iC1 < 20; iC1++)
                {
                    bData[iC1] = bTmpBytes[iC1 - 16];
                }

                int iC2 = 20;

                foreach(IPAddress ipa in lipaNeighbours)
                {
                    bTmpBytes = ipa.GetAddressBytes();
                    for (int iC1 = 0; iC1 < 4; iC1++)
                    {
                        bData[iC1 + iC2] = bTmpBytes[iC1];
                    }
                    iC2 += 4;
                }

                if (fEncapsulatedFrame != null)
                {
                    byte[] bEncap = fEncapsulatedFrame.FrameBytes;

                    for (int iC1 = 0; iC1 < bEncap.Length; iC1++)
                    {
                        bData[iC1 + iC2] = bEncap[iC1];
                    }
                }

                return bData;
            }
        }

        /// <summary>
        /// Returns the length of this frame and its encapsulated frame in bytes
        /// </summary>
        public override int Length
        {
            get { return 20 + (lipaNeighbours.Count * 4) + (fEncapsulatedFrame != null ? fEncapsulatedFrame.Length : 0); }
        }
    }
}
