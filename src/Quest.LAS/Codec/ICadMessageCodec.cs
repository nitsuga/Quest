using Quest.LAS.Messages;
using System.Collections.Generic;

namespace Quest.LAS.Codec
{
    public interface ICadMessageCodec 
    {

        string EncodeOutboundMid(int signalStrength0, int signalStrength1, string messageTypeId, int outboundSequenceNumber);

        CadMessageCodecTypeEnum DecodeInboundMessage(byte[] data);
        CallsignParam DecodeCallsignParameter(byte[] data, int dataLength);
        IncidentUpdate DecodeCallsignIncidentParameter(byte[] data, int dataLength);
        byte[] EncodeStatusUpdate(int shortStatusCode, int? statusEasting, int? statusNorthing, int? nonConveyCode, string destinationHospital);
        byte[] EncodeCancelReject(int? statusEasting, int? statusNorthing);
        byte[] EncodeCadLogin(ushort protocolVersion, string configVersion, string imageVersion, string g18Info, string otherInfo);
        GeneralMessageParam DecodeGeneralMessage(byte[] data);
        List<MdtParameter> DecodeParameters(byte[] data);
        IncidentCancellation DecodeCancelIncident(byte[] data);
        AdminMessage DecodeAdminMessage(byte[] data);
        byte[] EncodeSkillLevel(int? easting, int? northing, char skillLevelCode);
        byte[] EncodeAvls(int easting, int northing, float speed, float direction, float? etaDistance, float? etaMinutes);
        byte[] EncodeAtDestination(int atDestinationType, int? nonConveyCode, string destinationHospital, int? statusEasting, int? statusNorthing);
        EngineeringMessageArgs DecodeEngineeringMessage(byte[] data);
        byte[] EncodeManualNavigate(string destination, int easting, int northing);
    }
}
