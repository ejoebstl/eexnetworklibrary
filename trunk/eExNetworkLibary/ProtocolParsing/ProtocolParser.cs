using System;
using System.Collections.Generic;
using System.Text;

namespace eExNetworkLibrary.ProtocolParsing
{
    public class ProtocolParser
    {
        private Dictionary<string, IProtocolProvider> dictProtocolProviders;
        private Dictionary<string, Dictionary<string, IProtocolProvider>> dictPayloadLookupTree;

        public ProtocolParser(IProtocolProvider[] arUserProviders, bool bIncludeDefaultProviders)
        {
            IProtocolProvider[] arProviders;

            if (bIncludeDefaultProviders)
            {
                IProtocolProvider[] arDefaultProviders
                    = new IProtocolProvider[]{new Providers.ARPProtocolProvider(), 
                    new Providers.EthernetProtocolProvider(), 
                    new Providers.IPv4ProtocolProvider(),
                    new Providers.TCPProtocolProvider(),
                    new Providers.TrafficDescriptionFrameProtocolProvider(),
                    new Providers.UDPProtocolProvider(),
                    new Providers.OSPFProtocolProvider(),
                    
                    new Providers.ICMPv4ProtocolProvider(),
                    new Providers.ICMPv6ProtocolProvider(),

                    new Providers.IPv6ProtocolProvider(),
                    new Providers.IPv6FragmentExtensionProtocolProvider(),
                    new Providers.IPv6RoutingExtensionProtocolProvider()};

                arProviders = new IProtocolProvider[arUserProviders.Length + arDefaultProviders.Length];

                arDefaultProviders.CopyTo(arProviders, 0);
                arUserProviders.CopyTo(arProviders, arDefaultProviders.Length);
            }
            else
            {
                arProviders = arUserProviders;
            }

            dictProtocolProviders = new Dictionary<string, IProtocolProvider>();
            dictPayloadLookupTree = new Dictionary<string, Dictionary<string, IProtocolProvider>>();

            foreach (IProtocolProvider ipProtocol in arProviders)
            {
                if(dictProtocolProviders.ContainsKey(ipProtocol.Protocol))
                {
                    throw new ArgumentException("The protocol " + ipProtocol.Protocol + " was already defined. Another provider for this protocol cannot be added.");
                }
                dictProtocolProviders.Add(ipProtocol.Protocol, ipProtocol);
                foreach (string strKnownPayload in ipProtocol.KnownPayloads)
                {
                    if (!dictPayloadLookupTree.ContainsKey(strKnownPayload))
                    {
                        dictPayloadLookupTree.Add(strKnownPayload, new Dictionary<string, IProtocolProvider>());
                    }
                    Dictionary<string, IProtocolProvider> dictCarriers = dictPayloadLookupTree[strKnownPayload];
                    if (!dictCarriers.ContainsKey(ipProtocol.Protocol))
                    {
                        dictCarriers.Add(ipProtocol.Protocol, ipProtocol);
                    }
                }
            }
        }

        public ProtocolParser()
            : this(new IProtocolProvider[0], true)
        {

        }

        public bool ContainsProviderForProtocol(string strProtocol)
        {
            return dictProtocolProviders.ContainsKey(strProtocol);
        }

        public IProtocolProvider GetProviderForProtocol(string strProtocol)
        {
            return dictProtocolProviders[strProtocol];
        }

        public string[] KnownProtocols
        {
            get
            {
                string[] strProtocols = new string[dictProtocolProviders.Keys.Count];
                dictProtocolProviders.Keys.CopyTo(strProtocols, 0);
                return strProtocols;
            }
        }
        

        /// <summary>
        /// Gets a frame by it's type.
        /// </summary>
        /// <param name="fFrame">The frame which should be searched.</param>
        /// <param name="strFrameType">The type to search for.</param>
        /// <returns>The parsed frame or null, if the frame did not contain a frame with the specified type.</returns>
        public Frame GetFrameByType(Frame fFrame, string strFrameType)
        {
            return GetFrameByType(fFrame, strFrameType, false);
        }

        /// <summary>
        /// Gets a frame by it's type.
        /// </summary>
        /// <param name="fFrame">The frame which should be searched.</param>
        /// <param name="strFrameType">The type to search for.</param>
        /// <param name="bReturnRawDataFrame">A bool indicating whether raw data frames can be returned, if the protocol is known but no protocol provider is available.</param>
        /// <returns>The parsed frame, a raw data frame with the searched frame's data or null, if the frame did not contain a frame with the specified type.</returns>
        public Frame GetFrameByType(Frame fFrame, string strFrameType, bool bReturnRawDataFrame)
        {
            Frame fResult = GetKnownFrameByType(fFrame, strFrameType);

            if (fResult == null)
            {
                foreach (IProtocolProvider ipCarrier in GetCarrierProtocols(strFrameType))
                {
                    Frame fCarrier = GetFrameByType(fFrame, ipCarrier.Protocol, false);
                    if (fCarrier != null)
                    {
                        if (ipCarrier.PayloadType(fCarrier) == strFrameType)
                        {
                            if (dictProtocolProviders.ContainsKey(strFrameType))
                            {
                                fCarrier.EncapsulatedFrame = dictProtocolProviders[strFrameType].Parse(fCarrier.EncapsulatedFrame);
                            }
                            fResult = fCarrier.EncapsulatedFrame;
                        }
                        break;
                    }
                }
            }

            return fResult;
        }

        private Frame GetKnownFrameByType(Frame fFrame, string strFrameType)
        {
            while (fFrame.EncapsulatedFrame != null)
            {
                if (fFrame.FrameType == strFrameType)
                {
                    return fFrame;
                }
                fFrame = fFrame.EncapsulatedFrame;
            }
            return null;
        }

        private IProtocolProvider[] GetCarrierProtocols(string strPayloadProtocol)
        {
            if (dictPayloadLookupTree.ContainsKey(strPayloadProtocol))
            {
                Dictionary<string, IProtocolProvider> dictCarriers = dictPayloadLookupTree[strPayloadProtocol];
                IProtocolProvider[] ipCarriers = new IProtocolProvider[dictCarriers.Values.Count];
                dictCarriers.Values.CopyTo(ipCarriers, 0);

                return ipCarriers;
            }

            return new IProtocolProvider[0];
        }

        /// <summary>
        /// Parses the frame as much as possible.
        /// </summary>
        /// <param name="fFrame"></param>
        public void ParseCompleteFrame(Frame fFrame)
        {
            while (fFrame.EncapsulatedFrame != null)
            {
                if (ParsePayload(fFrame) == "")
                {
                    return;
                }
                fFrame = fFrame.EncapsulatedFrame;
            }
        }

        /// <summary>
        /// Must parse the payload of the given frame and set the parsed frame as the encapsulated frame.
        /// </summary>
        /// <param name="fFrame">The frame which has payload to parse. The frame cannot ba a raw data frame, since the payload protocol cannot be guessed.</param>
        /// <returns>The type of the parsed frame or an empty string if the protocol was not known or not supported.</returns>
        public string ParsePayload(Frame fFrame)
        {
            if (fFrame.FrameType == FrameTypes.Raw)
            {
                throw new InvalidOperationException("The frame which carries the frame to parse cannot be a raw data frame, since the protocol cannot be guessed.");
            }

            if (fFrame.EncapsulatedFrame == null || fFrame.EncapsulatedFrame.Length == 0)
            {
                fFrame.EncapsulatedFrame = null;
                return "";
            }

            if (fFrame.EncapsulatedFrame.FrameType != FrameTypes.Raw)
            {
                return fFrame.EncapsulatedFrame.FrameType; //No need to parse, parsing is already done.
            }

            if (dictProtocolProviders.ContainsKey(fFrame.FrameType))
            {
                string strPayloadProtocol = dictProtocolProviders[fFrame.FrameType].PayloadType(fFrame);
                if (strPayloadProtocol != "" && dictProtocolProviders.ContainsKey(strPayloadProtocol))
                {
                    fFrame.EncapsulatedFrame = dictProtocolProviders[strPayloadProtocol].Parse(fFrame.EncapsulatedFrame);
                    return strPayloadProtocol;
                }
            }

            return "";
        }
    }
}
