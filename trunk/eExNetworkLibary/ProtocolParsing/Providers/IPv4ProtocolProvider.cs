using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.IP;
using eExNetworkLibrary.IP.V6;

namespace eExNetworkLibrary.ProtocolParsing.Providers
{
    public class IPv4ProtocolProvider : IProtocolProvider
    {
        public virtual string Protocol
        {
            get { return FrameTypes.IPv4; }
        }

        public string[] KnownPayloads
        {
            get
            {
                return new string[] { FrameTypes.OSPF, FrameTypes.UDP, FrameTypes.ICMPv4, FrameTypes.ICMPv6, FrameTypes.TCP, 
                    FrameTypes.IPv6HOPOPT, FrameTypes.IGMPv4, FrameTypes.GGP, FrameTypes.CBT, FrameTypes.EGP, FrameTypes.IGP, 
                    FrameTypes.NVP_II, FrameTypes.PUP, FrameTypes.ARGUS, FrameTypes.EMCON, FrameTypes.XNET, FrameTypes.CHAOS, 
                    FrameTypes.MUX, FrameTypes.HMP, FrameTypes.PRM, FrameTypes.XNS_IDP, FrameTypes.RDP, 
                    FrameTypes.IRTP, FrameTypes.NETBLT, FrameTypes.MFE_NSP, FrameTypes.MERIT_INP, FrameTypes.DCCP, 
                    FrameTypes.IDPR, FrameTypes.XTP, FrameTypes.DDP, FrameTypes.IDPR_CMTP, FrameTypes.TP_PLUSPLUS, 
                    FrameTypes.IL, FrameTypes.SDRP, FrameTypes.IPv6Route, FrameTypes.IPv6Frag, FrameTypes.IDRP, 
                    FrameTypes.RSVP, FrameTypes.GRE, FrameTypes.MHRP, FrameTypes.BNA, FrameTypes.ESP, FrameTypes.AH, 
                    FrameTypes.SWIPE, FrameTypes.NARP, FrameTypes.MOBILE, FrameTypes.TLSP, FrameTypes.SKIP, 
                    FrameTypes.IPv6NoNxt, FrameTypes.IPv6Opts, FrameTypes.CFTP, FrameTypes.SAT_EXPAK, FrameTypes.KRYPTOPLAN, 
                    FrameTypes.RVD, FrameTypes.IPPC, FrameTypes.SAT_MON, FrameTypes.VISA, 
                    FrameTypes.IPCV, FrameTypes.CPNX, FrameTypes.CPHB, FrameTypes.WSN, FrameTypes.PVP, FrameTypes.BR_SAT_MON, 
                    FrameTypes.SUN_ND, FrameTypes.WB_MON, FrameTypes.WB_EXPAK, FrameTypes.ISO_IP, FrameTypes.VMTP, FrameTypes.SECURE_VMTP, 
                    FrameTypes.VINES, FrameTypes.TTP, FrameTypes.NSFNET_IGP, FrameTypes.DGP, FrameTypes.TCF, FrameTypes.EIGRP,
                    FrameTypes.Sprite_RPC, FrameTypes.LARP, FrameTypes.MTP, FrameTypes.AX_25, FrameTypes.IPIP, FrameTypes.MICP, FrameTypes.SSC_SP, 
                    FrameTypes.ETHERIP, FrameTypes.ENCAP, FrameTypes.GMTP, FrameTypes.IFMP, FrameTypes.PNNI, 
                    FrameTypes.PIM, FrameTypes.ARIS, FrameTypes.SCPS, FrameTypes.QNX, FrameTypes.ActiveNetworks, FrameTypes.IPComp, FrameTypes.SNP, 
                    FrameTypes.Compaq_Peer, FrameTypes.IPXinIP, FrameTypes.VRRP, FrameTypes.PGM, FrameTypes.L2TP, 
                    FrameTypes.DDX,FrameTypes.IATP, FrameTypes.STP, FrameTypes.SRP, FrameTypes.UTI, FrameTypes.SMP, FrameTypes.SM, FrameTypes.PTP, 
                    FrameTypes.ISISoverIPv4, FrameTypes.FIRE, FrameTypes.CRTP, FrameTypes.CRUDP, FrameTypes.SSCOPMCE, FrameTypes.IPLT, FrameTypes.SPS, 
                    FrameTypes.PIPE, FrameTypes.SCTP, FrameTypes.FC, FrameTypes.RSVP_E2E_IGNORE, FrameTypes.MobilityHeader, FrameTypes.UDPLite, 
                    FrameTypes.MPLSinIP, FrameTypes.HIP};
            } 
        }

        public virtual Frame Parse(Frame fFrame)
        {
            if (fFrame.FrameType == this.Protocol)
            {
                return fFrame;
            }

            return new IPv4Frame(fFrame.FrameBytes);
        }

        public string PayloadType(Frame fFrame)
        {
            if (fFrame.FrameType != this.Protocol)
            {
                fFrame = Parse(fFrame);
            }

            switch (((IIPHeader)fFrame).Protocol)
            {
                case IPProtocol.OSPF: return FrameTypes.OSPF; 
                case IPProtocol.UDP: return FrameTypes.UDP; 
                case IPProtocol.TCP: return FrameTypes.TCP; 
                case IPProtocol.ICMP: return FrameTypes.ICMPv4; 
                case IPProtocol.IPv6_ICMP: return FrameTypes.ICMPv6; 
                case IPProtocol.HOPOPT: return FrameTypes.IPv6HOPOPT; 
                case IPProtocol.IGMP: return FrameTypes.IGMPv4; 
                case IPProtocol.GGP: return FrameTypes.GGP; 
                case IPProtocol.CBT: return FrameTypes.CBT; 
                case IPProtocol.EGP: return FrameTypes.EGP; 
                case IPProtocol.IGP: return FrameTypes.IGP; 
                case IPProtocol.NVP_II: return FrameTypes.NVP_II; 
                case IPProtocol.PUP: return FrameTypes.PUP; 
                case IPProtocol.ARGUS: return FrameTypes.ARGUS; 
                case IPProtocol.EMCON: return FrameTypes.EMCON; 
                case IPProtocol.XNET: return FrameTypes.XNET; 
                case IPProtocol.CHAOS: return FrameTypes.CHAOS; 
                case IPProtocol.MUX: return FrameTypes.MUX; 
                case IPProtocol.HMP: return FrameTypes.HMP; 
                case IPProtocol.PRM: return FrameTypes.PRM; 
                case IPProtocol.XNS_IDP: return FrameTypes.XNS_IDP; 
                case IPProtocol.RDP: return FrameTypes.RDP; 
                case IPProtocol.IRTP: return FrameTypes.IRTP; 
                case IPProtocol.NETBLT: return FrameTypes.NETBLT; 
                case IPProtocol.MFE_NSP: return FrameTypes.MFE_NSP; 
                case IPProtocol.MERIT_INP: return FrameTypes.MERIT_INP; 
                case IPProtocol.DCCP: return FrameTypes.DCCP; 
                case IPProtocol.IDPR: return FrameTypes.IDPR; 
                case IPProtocol.XTP: return FrameTypes.XTP; 
                case IPProtocol.DDP: return FrameTypes.DDP; 
                case IPProtocol.IDPR_CMTP: return FrameTypes.IDPR_CMTP; 
                case IPProtocol.TP_PLUSPLUS: return FrameTypes.TP_PLUSPLUS; 
                case IPProtocol.IL: return FrameTypes.IL; 
                case IPProtocol.SDRP: return FrameTypes.SDRP; 
                case IPProtocol.IPv6_Route: return FrameTypes.IPv6Route; 
                case IPProtocol.IPv6_Frag: return FrameTypes.IPv6Frag; 
                case IPProtocol.IDRP: return FrameTypes.IDRP; 
                case IPProtocol.RSVP: return FrameTypes.RSVP; 
                case IPProtocol.GRE: return FrameTypes.GRE; 
                case IPProtocol.MHRP: return FrameTypes.MHRP; 
                case IPProtocol.BNA: return FrameTypes.BNA; 
                case IPProtocol.ESP: return FrameTypes.ESP; 
                case IPProtocol.AH: return FrameTypes.AH; 
                case IPProtocol.SWIPE: return FrameTypes.SWIPE; 
                case IPProtocol.NARP: return FrameTypes.NARP; 
                case IPProtocol.MOBILE: return FrameTypes.MOBILE; 
                case IPProtocol.TLSP: return FrameTypes.TLSP; 
                case IPProtocol.SKIP: return FrameTypes.SKIP; 
                case IPProtocol.IPv6_NoNxt: return FrameTypes.IPv6NoNxt; 
                case IPProtocol.IPv6_Opts: return FrameTypes.IPv6Opts; 
                case IPProtocol.CFTP: return FrameTypes.CFTP; 
                case IPProtocol.SAT_EXPAK: return FrameTypes.SAT_EXPAK; 
                case IPProtocol.KRYPTOPLAN: return FrameTypes.KRYPTOPLAN; 
                case IPProtocol.RVD: return FrameTypes.RVD; 
                case IPProtocol.IPPC: return FrameTypes.IPPC; 
                case IPProtocol.SAT_MON: return FrameTypes.SAT_MON; 
                case IPProtocol.VISA: return FrameTypes.VISA; 
                case IPProtocol.IPCV: return FrameTypes.IPCV; 
                case IPProtocol.CPNX: return FrameTypes.CPNX; 
                case IPProtocol.CPHB: return FrameTypes.CPHB; 
                case IPProtocol.WSN: return FrameTypes.WSN; 
                case IPProtocol.PVP: return FrameTypes.PVP; 
                case IPProtocol.BR_SAT_MON: return FrameTypes.BR_SAT_MON; 
                case IPProtocol.SUN_ND: return FrameTypes.SUN_ND; 
                case IPProtocol.WB_MON: return FrameTypes.WB_MON; 
                case IPProtocol.WB_EXPAK: return FrameTypes.WB_EXPAK; 
                case IPProtocol.ISO_IP: return FrameTypes.ISO_IP; 
                case IPProtocol.VMTP: return FrameTypes.VMTP; 
                case IPProtocol.SECURE_VMTP: return FrameTypes.SECURE_VMTP; 
                case IPProtocol.VINES: return FrameTypes.VINES; 
                case IPProtocol.TTP: return FrameTypes.TTP; 
                case IPProtocol.NSFNET_IGP: return FrameTypes.NSFNET_IGP; 
                case IPProtocol.DGP: return FrameTypes.DGP; 
                case IPProtocol.TCF: return FrameTypes.TCF; 
                case IPProtocol.EIGRP: return FrameTypes.EIGRP; 
                case IPProtocol.Sprite_RPC: return FrameTypes.Sprite_RPC; 
                case IPProtocol.LARP: return FrameTypes.LARP; 
                case IPProtocol.MTP: return FrameTypes.MTP; 
                case IPProtocol.AX_25: return FrameTypes.AX_25; 
                case IPProtocol.IPIP: return FrameTypes.IPIP; 
                case IPProtocol.MICP: return FrameTypes.MICP; 
                case IPProtocol.SSC_SP: return FrameTypes.SSC_SP; 
                case IPProtocol.ETHERIP: return FrameTypes.ETHERIP; 
                case IPProtocol.ENCAP: return FrameTypes.ENCAP; 
                case IPProtocol.GMTP: return FrameTypes.GMTP; 
                case IPProtocol.IFMP: return FrameTypes.IFMP; 
                case IPProtocol.PNNI: return FrameTypes.PNNI; 
                case IPProtocol.PIM: return FrameTypes.PIM; 
                case IPProtocol.ARIS: return FrameTypes.ARIS; 
                case IPProtocol.SCPS: return FrameTypes.SCPS; 
                case IPProtocol.QNX: return FrameTypes.QNX; 
                case IPProtocol.ActiveNetworks: return FrameTypes.ActiveNetworks; 
                case IPProtocol.IPComp: return FrameTypes.IPComp; 
                case IPProtocol.SNP: return FrameTypes.SNP; 
                case IPProtocol.Compaq_Peer: return FrameTypes.Compaq_Peer; 
                case IPProtocol.IPX_in_IP: return FrameTypes.IPXinIP; 
                case IPProtocol.VRRP: return FrameTypes.VRRP; 
                case IPProtocol.PGM: return FrameTypes.PGM; 
                case IPProtocol.L2TP: return FrameTypes.L2TP; 
                case IPProtocol.DDX: return FrameTypes.DDX; 
                case IPProtocol.IATP: return FrameTypes.IATP; 
                case IPProtocol.STP: return FrameTypes.STP; 
                case IPProtocol.SRP: return FrameTypes.SRP; 
                case IPProtocol.UTI: return FrameTypes.UTI; 
                case IPProtocol.SMP: return FrameTypes.SMP; 
                case IPProtocol.SM: return FrameTypes.SM; 
                case IPProtocol.PTP: return FrameTypes.PTP; 
                case IPProtocol.ISIS_over_IPv4: return FrameTypes.ISISoverIPv4; 
                case IPProtocol.FIRE: return FrameTypes.FIRE; 
                case IPProtocol.CRTP: return FrameTypes.CRTP; 
                case IPProtocol.CRUDP: return FrameTypes.CRUDP; 
                case IPProtocol.SSCOPMCE: return FrameTypes.SSCOPMCE; 
                case IPProtocol.IPLT: return FrameTypes.IPLT; 
                case IPProtocol.SPS: return FrameTypes.SPS; 
                case IPProtocol.PIPE: return FrameTypes.PIPE; 
                case IPProtocol.SCTP: return FrameTypes.SCTP; 
                case IPProtocol.FC: return FrameTypes.FC; 
                case IPProtocol.RSVP_E2E_IGNORE: return FrameTypes.RSVP_E2E_IGNORE; 
                case IPProtocol.MobilityHeader: return FrameTypes.MobilityHeader; 
                case IPProtocol.UDPLite: return FrameTypes.UDPLite; 
                case IPProtocol.MPLS_in_IP: return FrameTypes.MPLSinIP; 
                case IPProtocol.HIP: return FrameTypes.HIP; 
            }

            return "";
        }
    }
}
