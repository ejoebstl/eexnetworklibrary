﻿using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.IP.V6;

namespace eExNetworkLibrary.IP
{
    public static class IPFragmenter
    {
        public static IPFrame[] Fragment(IPFrame ipFrame, int iMaximumTransmissionUnit)
        {
            if (ipFrame.FrameType == FrameTypes.IPv4)
            {
                return FragmentV4((IPv4Frame)ipFrame, iMaximumTransmissionUnit);
            }
            else if (ipFrame.FrameType == FrameTypes.IPv6)
            {
                return FragmentV6((IPv6Frame)ipFrame, iMaximumTransmissionUnit);
            }

            throw new ArgumentException("Only IPv4 and IPv6 frames are supported.");
        }

        public static IPv4Frame[] FragmentV4(IPv4Frame ipv4Frame, int iMaximumTransmissionUnit)
        {
            Frame fFrame = ipv4Frame.EncapsulatedFrame;
            List<IPv4Frame> lIPv4Frames = new List<IPv4Frame>();

            if (fFrame.Length + (ipv4Frame.InternetHeaderLength * 4) > iMaximumTransmissionUnit)
            {
                byte[][] bChunks = CreateChunks(fFrame.FrameBytes, iMaximumTransmissionUnit - (ipv4Frame.InternetHeaderLength * 4));

                int iDataCounter = 0;

                for (int iC1 = 0; iC1 < bChunks.Length; iC1++)
                {
                    IPv4Frame ipv4Clone = (IPv4Frame)ipv4Frame.Clone();

                    ipv4Clone.EncapsulatedFrame = new RawDataFrame(bChunks[iC1]);
                    ipv4Clone.FragmentOffset = (ushort)((iDataCounter) / 2);
                    ipv4Clone.PacketFlags.MoreFragments = iC1 != bChunks.Length - 1;

                    iDataCounter += bChunks[iC1].Length;

                    lIPv4Frames.Add(ipv4Clone);
                }
            }
            else
            {
                lIPv4Frames.Add(ipv4Frame);
            }

            return lIPv4Frames.ToArray();
        }

        public static IPv6Frame[] FragmentV6(IPv6Frame ipv6Frame, int iMaximumTransmissionUnit)
        {
            return FragmentV6(ipv6Frame, iMaximumTransmissionUnit, (uint)(new Random().Next(Int32.MaxValue)));
        }

        public static IPv6Frame[] FragmentV6(IPv6Frame ipv6Frame, int iMaximumTransmissionUnit, uint iIdentification)
        {
            Frame fFrame = ipv6Frame.EncapsulatedFrame;
            List<IPv6Frame> lIPv6Frames = new List<IPv6Frame>();

            if (fFrame.Length + ipv6Frame.Length > iMaximumTransmissionUnit)
            {
                ipv6Frame.EncapsulatedFrame = null;
                byte[][] bChunks = CreateChunks(fFrame.FrameBytes, iMaximumTransmissionUnit - ipv6Frame.Length);

                int iDataCounter = 0;

                for (int iC1 = 0; iC1 < bChunks.Length; iC1++)
                {
                    IPv6Frame ipv6Clone = (IPv6Frame)ipv6Frame.Clone();
                    FragmentExtensionHeader fragHeader = new FragmentExtensionHeader();

                    fragHeader.EncapsulatedFrame = new RawDataFrame(bChunks[iC1]);
                    ipv6Frame.EncapsulatedFrame = fragHeader;

                    fragHeader.FragmentOffset = ((iDataCounter) / 2);
                    fragHeader.MoreFragments = iC1 != bChunks.Length - 1;
                    fragHeader.Identification = iIdentification;

                    fragHeader.Protocol = ipv6Frame.Protocol;
                    ipv6Frame.Protocol = IPProtocol.IPv6_Frag;

                    iDataCounter += bChunks[iC1].Length;

                    lIPv6Frames.Add(ipv6Clone);
                }
            }
            else
            {
                lIPv6Frames.Add(ipv6Frame);
            }

            return lIPv6Frames.ToArray();
        }

        private static byte[][] CreateChunks(byte[] bBuffer, int iChunkSize)
        {
            List<byte[]> lChunks = new List<byte[]>();
            iChunkSize -= iChunkSize % 8;
            for (int iC1 = 0; iC1 < bBuffer.Length; iC1 += iChunkSize)
            {
                byte[] bChunk = new byte[Math.Min(iChunkSize, (bBuffer.Length - iC1))];

                for (int iC2 = iC1; iC2 < iC1 + iChunkSize && iC2 < bBuffer.Length; iC1++)
                {
                    bChunk[iC2 - iC1] = bBuffer[iC2];
                }

                lChunks.Add(bChunk);
            }
            return lChunks.ToArray();
        }
    }
}
