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

namespace eExNetworkLibrary.ICMP.V6
{
    public class NeighborSolicitation : Frame
    {
        private IPAddress ipaTargetAddress;

        public static string DefaultFrameType { get { return "ICMPv6NeighborSolicitation"; } }

        public IPAddress TargetAddress
        {
            get
            {
                return ipaTargetAddress;
            }
            set
            {
                if (value.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
                    throw new ArgumentException("Only IPv6 addresses are supported by the ICMPv6NeighborAdvertisment.");
                ipaTargetAddress = value;
            }
        }

        public NeighborSolicitation()
        {
            ipaTargetAddress = IPAddress.IPv6Any;
        }

        public NeighborSolicitation(byte[] bData)
        {
            byte[] bAddressBytes = new byte[16];

            Array.Copy(bData, 4, bAddressBytes, 0, 16);

            this.ipaTargetAddress = new IPAddress(bAddressBytes);

            byte[] bPayload = new byte[bData.Length - 20];
            Array.Copy(bData, 20, bPayload, 0, bPayload.Length);

            if (bPayload.Length > 0)
            {
                this.fEncapsulatedFrame = new NeighborDiscoveryOption(bPayload);
            }
        }

        public override string FrameType
        {
            get { return NeighborSolicitation.DefaultFrameType; }
        }

        public override byte[] FrameBytes
        {
            get 
            {
                byte[] bData = new byte[Length];

                Array.Copy(ipaTargetAddress.GetAddressBytes(), 0, bData, 4, 16);

                if (fEncapsulatedFrame != null)
                {
                    Array.Copy(fEncapsulatedFrame.FrameBytes, 0, bData, 20, fEncapsulatedFrame.Length);
                }

                return bData;
            }
        }

        public override int Length
        {
            get { return 20 + (fEncapsulatedFrame != null ? fEncapsulatedFrame.Length : 0); }
        }

        public override Frame Clone()
        {
            return new NeighborSolicitation(this.FrameBytes);
        }
    }
}
