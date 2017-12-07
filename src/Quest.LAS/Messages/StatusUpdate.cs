using Quest.LAS.Messages;
using System;

namespace Quest.LAS.Messages
{
    public class StatusUpdate : IDeviceMessage
    {
        public int ShortStatusCode;
        public int? StatusEasting;
        public int? StatusNorthing;
        public int? NonConveyCode;
        public string DestinationHospital;
    }

    public class RequestIncidentUpdate : IDeviceMessage
    {
    }

    public class EngineeringUpdate : IDeviceMessage
    {
        public string MessagePayload;
    }

    public class ConfigRequest : IDeviceMessage
    {
    }

    public class CadLogout : IDeviceMessage
    {
    }

    public class CancelReject : IDeviceMessage
    {
        public int? StatusEasting;
        public int? StatusNorthing;
    }

    public class ManualNavigation : IDeviceMessage
    {
        public string Destination;
        public int Easting;
        public int Northing;
    }

    public class SetSkillLevel : IDeviceMessage
    {
        public int? StatusEasting;
        public int? StatusNorthing;
        public char SkillLevel;
    }

    public class AvlsUpdate : IDeviceMessage
    {
        public int Easting;
        public int Northing;
        public float Speed;
        public float Direction;
        public float? EtaDistance;
        public float? EtaMinutes;
    }

    public class StationResources : IDeviceMessage
    {
        public string StationCode;
        public int? StatusEasting;
        public int? StatusNorthing;
    }

    public class LocationResources : IDeviceMessage
    {
        public string RequestEasting;
        public string RequestNorthing;
        public int? StatusEasting;
        public int? StatusNorthing;
    }
    public class AtDestination : IDeviceMessage
    {
        public int AtDestinationType;
        public int? NonConveyCode;
        public string DestinationHospital;
        public int? StatusEasting;
        public int? StatusNorthing;
    }

    public class MdtUpdateVersion : IDeviceMessage
    {
        public DateTime UpdateDateTime;
    }
    public class PingResponse : IDeviceMessage
    {
        public DateTime CadPingTime;
    }
    public class CadLogin : IDeviceMessage
    {
        public UInt16 ProtocolVersion;
        public string ConfigVersion;
        public string ImageVersion;
        public string G18Info;
        public string OtherInfo;
    }

}
