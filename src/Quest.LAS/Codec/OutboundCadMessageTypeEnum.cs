using System;

namespace Quest.LAS.Codec
{
    public enum OutboundCadMessageTypeEnum
    {
        CadLogin = 0x83,
        CadUpdateIdentifier = 0x53,
        AtDestination = 0x80,
        RequestConfig = 0x82,
        CadLogout = 0x84,
        RequestIncidentUpdate = 0x85,
        CancelReject = 0x86,
        ManualNavigation = 0x88,
        SkillLevel = 0x8E,
        StationResource = 0x8F,
        LocationResource = 0x90,
        MdtUpdateVersion = 0x8C,
        PingResponse = 0x8B,
        Avls = 0x41
    }
}
