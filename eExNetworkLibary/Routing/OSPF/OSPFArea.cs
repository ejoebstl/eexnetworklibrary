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

namespace eExNetworkLibrary.Routing.OSPF
{
    [Obsolete("This class is marked depreceated in cause of a sloppy implementation and only limited compatibility..", false)]
    class OSPFArea
    {
        private uint iAreaID;
        //private int iDistance;
        //private List<OSPFAreaVertex> lAreaBorderRouters;
        //private List<OSPFAreaVertex> lSystemBorderRouters;
        private LSDatabase lsDatabase;
        //private List<OSPFArea> lConnectedArea;

        #region props

        public uint AreaID
        {
            get { return iAreaID; }
        }

        public LSDatabase Database
        {
            get { return lsDatabase;  }
        }

        /*
        public OSPFAreaVertex[] GetAreaBorderRouters()
        {
            return lAreaBorderRouters.ToArray();
        }

        public OSPFAreaVertex[] GetSystemBorderRouters()
        {
            return lSystemBorderRouters.ToArray();
        }
        public OSPFArea[] GetConnectedAreas()
        {
            return lConnectedArea.ToArray();
        }

        public bool ContainsConnectedArea(OSPFArea check)
        {
            return lConnectedArea.Contains(check);
        }

        public void AddConnectedArea(OSPFArea aToAdd)
        {
            lConnectedArea.Add(aToAdd);
        }

        public void ClearConnectedAreas()
        {
            lConnectedArea.Clear();
        }
        */
        #endregion

        public OSPFArea(uint iAreaID)
        {
            this.iAreaID = iAreaID;
            lsDatabase = new LSDatabase();
        }
    }

    [Obsolete("This class is marked depreceated in cause of a sloppy implementation and only limited compatibility..", false)]
    class OSPFAreaVertex
    {
        private uint iID;
        private LSAHeader lsAssociatedLSA;
        private List<Link> lNextHops;
        private int iDistanceFromRoot;
        private int iAge;
        private List<IPAddress> lIPAddresses;
        private List<OSPFArea> lAreas;
        private LSType lsType;
        private bool bIsDesignated;
        private bool bIsBackupDesignated;

        #region propos

        public bool IsBackupDesignated
        {
            get { return bIsBackupDesignated; }
            set { bIsBackupDesignated = value; }
        }

        public bool IsDesignated
        {
            get { return bIsDesignated; }
            set { bIsDesignated = value; }
        }

        public LSType Type
        {
            get { return lsType; }
            set { lsType = value; }
        }

        public int Age
        {
            get { return iAge; }
            set { iAge = value; }
        }

        public uint ID
        {
            get { return iID; }
            set { iID = value;  }
        }

        public LSAHeader AssociatedLSA
        {
            get { return lsAssociatedLSA; }
            set { lsAssociatedLSA = value; }
        }

        public int DistanceFromRoot
        {
            get { return iDistanceFromRoot; }
            set { iDistanceFromRoot = value; }
        }

        public IPAddress[] GetAddresses()
        {
            return lIPAddresses.ToArray();
        }

        public bool ContainsAddress(IPAddress check)
        {
            return lIPAddresses.Contains(check);
        }

        public void AddAddress(IPAddress ipaToAdd)
        {
            lIPAddresses.Add(ipaToAdd);
        }

        public void ClearAddresses()
        {
            lIPAddresses.Clear();
        }

        public Link[] GetNextHops()
        {
            return lNextHops.ToArray();
        }

        public bool ContainsNextHop(Link check)
        {
            return lNextHops.Contains(check);
        }

        public void AddNextHop(Link lToAdd)
        {
            lToAdd.Source = this;
            lNextHops.Add(lToAdd);
        }

        public void ClearNextHops()
        {
            lNextHops.Clear();
        }

        public OSPFArea[] GetAreas()
        {
            return lAreas.ToArray();
        }

        public bool ContainsArea(OSPFArea check)
        {
            return lAreas.Contains(check);
        }

        public void AddArea(OSPFArea oaToAdd)
        {
            lAreas.Add(oaToAdd);
        }

        public void ClearAreas()
        {
            lAreas.Clear();
        }

        #endregion

        public OSPFAreaVertex(uint iID, int iDistanceFromRoot, LSAHeader lAssociatedLSA, int iAge, LSType lType)
        {
            this.iID = iID;
            this.iDistanceFromRoot = iDistanceFromRoot;
            this.lsAssociatedLSA = lAssociatedLSA;
            this.iAge = iAge;
            lNextHops = new List<Link>();
            lIPAddresses = new List<IPAddress>();
            lAreas = new List<OSPFArea>();
            this.lsType = lType;
        }

        public override bool Equals(object obj)
        {
            if (obj is OSPFAreaVertex)
            {
                OSPFAreaVertex oav = obj as OSPFAreaVertex;
                if (oav.iID == this.iID)
                {
                    return true;
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (int)(this.iID & 0x0fffff);
        }

        public OSPFAreaVertex(uint iID) : this(iID, 0, null, 0, LSType.Router) { }
    }

    [Obsolete("This class is marked depreceated in cause of a sloppy implementation and only limited compatibility..", false)]
    class Link
    {
        int iCost;
        OSPFAreaVertex vertexSource;
        OSPFAreaVertex vertexDestination;

        public OSPFAreaVertex Destination
        {
            get { return vertexDestination; }
            set { vertexDestination = value; }
        }

        public OSPFAreaVertex Source
        {
            get { return vertexSource; }
            set { vertexSource = value; }
        }

        public int Cost
        {
            get { return iCost; }
            set { iCost = value; }
        }

        public Link() {}

        public Link(int iCost, OSPFAreaVertex vertexSource, OSPFAreaVertex vertexDestination)
        {
            this.iCost = iCost;
            this.vertexSource = vertexSource;
            this.vertexDestination = vertexDestination;
        }

    }
}
