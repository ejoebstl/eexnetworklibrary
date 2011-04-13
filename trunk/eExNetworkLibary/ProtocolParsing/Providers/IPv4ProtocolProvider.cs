using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.IP;

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

            switch (((IPFrame)fFrame).Protocol)
            {
                case IPProtocol.OSPF: return FrameTypes.OSPF; break;
                case IPProtocol.UDP: return FrameTypes.UDP; break;
                case IPProtocol.TCP: return FrameTypes.TCP; break;
                case IPProtocol.ICMP: return FrameTypes.ICMPv4; break;
                case IPProtocol.IPv6_ICMP: return FrameTypes.ICMPv6; break;
                case IPProtocol.HOPOPT: return FrameTypes.IPv6HOPOPT; break;
                case IPProtocol.IGMP: return FrameTypes.IGMPv4; break;
                case IPProtocol.GGP: return FrameTypes.GGP; break;
                case IPProtocol.CBT: return FrameTypes.CBT; break;
                case IPProtocol.EGP: return FrameTypes.EGP; break;
                case IPProtocol.IGP: return FrameTypes.IGP; break;
                case IPProtocol.NVP_II: return FrameTypes.NVP_II; break;
                case IPProtocol.PUP: return FrameTypes.PUP; break;
                case IPProtocol.ARGUS: return FrameTypes.ARGUS; break;
                case IPProtocol.EMCON: return FrameTypes.EMCON; break;
                case IPProtocol.XNET: return FrameTypes.XNET; break;
                case IPProtocol.CHAOS: return FrameTypes.CHAOS; break;
                case IPProtocol.MUX: return FrameTypes.MUX; break;
                case IPProtocol.HMP: return FrameTypes.HMP; break;
                case IPProtocol.PRM: return FrameTypes.PRM; break;
                case IPProtocol.XNS_IDP: return FrameTypes.XNS_IDP; break;
                case IPProtocol.RDP: return FrameTypes.RDP; break;
                case IPProtocol.IRTP: return FrameTypes.IRTP; break;
                case IPProtocol.NETBLT: return FrameTypes.NETBLT; break;
                case IPProtocol.MFE_NSP: return FrameTypes.MFE_NSP; break;
                case IPProtocol.MERIT_INP: return FrameTypes.MERIT_INP; break;
                case IPProtocol.DCCP: return FrameTypes.DCCP; break;
                case IPProtocol.IDPR: return FrameTypes.IDPR; break;
                case IPProtocol.XTP: return FrameTypes.XTP; break;
                case IPProtocol.DDP: return FrameTypes.DDP; break;
                case IPProtocol.IDPR_CMTP: return FrameTypes.IDPR_CMTP; break;
                case IPProtocol.TP_PLUSPLUS: return FrameTypes.TP_PLUSPLUS; break;
                case IPProtocol.IL: return FrameTypes.IL; break;
                case IPProtocol.SDRP: return FrameTypes.SDRP; break;
                case IPProtocol.IPv6_Route: return FrameTypes.IPv6Route; break;
                case IPProtocol.IPv6_Frag: return FrameTypes.IPv6Frag; break;
                case IPProtocol.IDRP: return FrameTypes.IDRP; break;
                case IPProtocol.RSVP: return FrameTypes.RSVP; break;
                case IPProtocol.GRE: return FrameTypes.GRE; break;
                case IPProtocol.MHRP: return FrameTypes.MHRP; break;
                case IPProtocol.BNA: return FrameTypes.BNA; break;
                case IPProtocol.ESP: return FrameTypes.ESP; break;
                case IPProtocol.AH: return FrameTypes.AH; break;
                case IPProtocol.SWIPE: return FrameTypes.SWIPE; break;
                case IPProtocol.NARP: return FrameTypes.NARP; break;
                case IPProtocol.MOBILE: return FrameTypes.MOBILE; break;
                case IPProtocol.TLSP: return FrameTypes.TLSP; break;
                case IPProtocol.SKIP: return FrameTypes.SKIP; break;
                case IPProtocol.IPv6_NoNxt: return FrameTypes.IPv6NoNxt; break;
                case IPProtocol.IPv6_Opts: return FrameTypes.IPv6Opts; break;
                case IPProtocol.CFTP: return FrameTypes.CFTP; break;
                case IPProtocol.SAT_EXPAK: return FrameTypes.SAT_EXPAK; break;
                case IPProtocol.KRYPTOPLAN: return FrameTypes.KRYPTOPLAN; break;
                case IPProtocol.RVD: return FrameTypes.RVD; break;
                case IPProtocol.IPPC: return FrameTypes.IPPC; break;
                case IPProtocol.SAT_MON: return FrameTypes.SAT_MON; break;
                case IPProtocol.VISA: return FrameTypes.VISA; break;
                case IPProtocol.IPCV: return FrameTypes.IPCV; break;
                case IPProtocol.CPNX: return FrameTypes.CPNX; break;
                case IPProtocol.CPHB: return FrameTypes.CPHB; break;
                case IPProtocol.WSN: return FrameTypes.WSN; break;
                case IPProtocol.PVP: return FrameTypes.PVP; break;
                case IPProtocol.BR_SAT_MON: return FrameTypes.BR_SAT_MON; break;
                case IPProtocol.SUN_ND: return FrameTypes.SUN_ND; break;
                case IPProtocol.WB_MON: return FrameTypes.WB_MON; break;
                case IPProtocol.WB_EXPAK: return FrameTypes.WB_EXPAK; break;
                case IPProtocol.ISO_IP: return FrameTypes.ISO_IP; break;
                case IPProtocol.VMTP: return FrameTypes.VMTP; break;
                case IPProtocol.SECURE_VMTP: return FrameTypes.SECURE_VMTP; break;
                case IPProtocol.VINES: return FrameTypes.VINES; break;
                case IPProtocol.TTP: return FrameTypes.TTP; break;
                case IPProtocol.NSFNET_IGP: return FrameTypes.NSFNET_IGP; break;
                case IPProtocol.DGP: return FrameTypes.DGP; break;
                case IPProtocol.TCF: return FrameTypes.TCF; break;
                case IPProtocol.EIGRP: return FrameTypes.EIGRP; break;
                case IPProtocol.Sprite_RPC: return FrameTypes.Sprite_RPC; break;
                case IPProtocol.LARP: return FrameTypes.LARP; break;
                case IPProtocol.MTP: return FrameTypes.MTP; break;
                case IPProtocol.AX_25: return FrameTypes.AX_25; break;
                case IPProtocol.IPIP: return FrameTypes.IPIP; break;
                case IPProtocol.MICP: return FrameTypes.MICP; break;
                case IPProtocol.SSC_SP: return FrameTypes.SSC_SP; break;
                case IPProtocol.ETHERIP: return FrameTypes.ETHERIP; break;
                case IPProtocol.ENCAP: return FrameTypes.ENCAP; break;
                case IPProtocol.GMTP: return FrameTypes.GMTP; break;
                case IPProtocol.IFMP: return FrameTypes.IFMP; break;
                case IPProtocol.PNNI: return FrameTypes.PNNI; break;
                case IPProtocol.PIM: return FrameTypes.PIM; break;
                case IPProtocol.ARIS: return FrameTypes.ARIS; break;
                case IPProtocol.SCPS: return FrameTypes.SCPS; break;
                case IPProtocol.QNX: return FrameTypes.QNX; break;
                case IPProtocol.ActiveNetworks: return FrameTypes.ActiveNetworks; break;
                case IPProtocol.IPComp: return FrameTypes.IPComp; break;
                case IPProtocol.SNP: return FrameTypes.SNP; break;
                case IPProtocol.Compaq_Peer: return FrameTypes.Compaq_Peer; break;
                case IPProtocol.IPX_in_IP: return FrameTypes.IPXinIP; break;
                case IPProtocol.VRRP: return FrameTypes.VRRP; break;
                case IPProtocol.PGM: return FrameTypes.PGM; break;
                case IPProtocol.L2TP: return FrameTypes.L2TP; break;
                case IPProtocol.DDX: return FrameTypes.DDX; break;
                case IPProtocol.IATP: return FrameTypes.IATP; break;
                case IPProtocol.STP: return FrameTypes.STP; break;
                case IPProtocol.SRP: return FrameTypes.SRP; break;
                case IPProtocol.UTI: return FrameTypes.UTI; break;
                case IPProtocol.SMP: return FrameTypes.SMP; break;
                case IPProtocol.SM: return FrameTypes.SM; break;
                case IPProtocol.PTP: return FrameTypes.PTP; break;
                case IPProtocol.ISIS_over_IPv4: return FrameTypes.ISISoverIPv4; break;
                case IPProtocol.FIRE: return FrameTypes.FIRE; break;
                case IPProtocol.CRTP: return FrameTypes.CRTP; break;
                case IPProtocol.CRUDP: return FrameTypes.CRUDP; break;
                case IPProtocol.SSCOPMCE: return FrameTypes.SSCOPMCE; break;
                case IPProtocol.IPLT: return FrameTypes.IPLT; break;
                case IPProtocol.SPS: return FrameTypes.SPS; break;
                case IPProtocol.PIPE: return FrameTypes.PIPE; break;
                case IPProtocol.SCTP: return FrameTypes.SCTP; break;
                case IPProtocol.FC: return FrameTypes.FC; break;
                case IPProtocol.RSVP_E2E_IGNORE: return FrameTypes.RSVP_E2E_IGNORE; break;
                case IPProtocol.MobilityHeader: return FrameTypes.MobilityHeader; break;
                case IPProtocol.UDPLite: return FrameTypes.UDPLite; break;
                case IPProtocol.MPLS_in_IP: return FrameTypes.MPLSinIP; break;
                case IPProtocol.HIP: return FrameTypes.HIP; break;
            }

            return "";
        }
    }
}
