using System;
namespace eExNetworkLibrary.Routing.OSPF
{
    [Obsolete("This class is marked depreceated in cause of a sloppy implementation and only limited compatibility..", false)]
    interface IOSPFRouter
    {
        int DeadTimer { get; set; }
        uint ID { get; set; }
        int Priority { get; set; }
        System.Net.IPAddress IPAddress { get; set; }
        DirectAttachedNetwork AttachedNetwork { get; set; }
        OSPFState State { get; set; }
        MACAddress MACAddress { get; set; }
        OSPFOptionsField Options { get; set; }
        bool IsMaster { get; set; }
    }
}
