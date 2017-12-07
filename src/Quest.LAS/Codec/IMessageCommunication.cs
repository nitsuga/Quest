using Quest.LAS.Messages;
using System;

namespace Quest.LAS.Codec
{
    public interface IMessageCommunication 
    {

        ICadInterface CadInterface { get; set; }
        ICadMessageCodec MessageCodec { get; set; }

        Int32 AgedMessages { get; set; }
        Int32 DuplicateMessages { get; set; }
        Int64 LastRcvdSequenceNumber { get; set; }

        event RcvCallsignUpdateEvent RcvCallsignUpdate;
        event RcvCancelIncidentEvent RcvCancelIncident;
        event RcvIncidentUpdateEvent RcvIncidentUpdate;
        event RcvGeneralMessageEvent RcvGeneralMessage;
        event RcvParametersEvent RcvParameters;
        event RcvRequestEtaEvent RcvRequestEta;
        event RcvSetDestinationEvent RcvSetDestinationEvent;
        event RcvStatusUpdateEvent RcvStatusUpdate;
        event RcvMdtEsMessageEvent RcvMdtEsMessage;
        event RcvMdtEsDatabaseUpdateEvent RcvMdtEsDatabaseUpdate;
        event RcvUnknownEvent RcvUnknown;

        void IdentifyInboundMessage(byte[] messageText, long sequenceNumber, DateTime mdtTimeStamp, DateTime cadTimeStamp, int rxQueueSize);
        void SendCadLogin();
        void SendMdtEs(string messagePayload);
        void SendCadAtDestination(AtDestinationMessageTypeEnum atDestinationMessageType, int easting, int northing, int? dispositionCode, string destinationHospital);
        void SendCadAtDestination(int destinationType, int easting, int northing);
        void SendCadAvls(int easting, int northing, float speed, float direction, float? etaDistance, float? etaMinutes);
        void SendCadEta();
        void SendCadReqEventUpdate();
        void SendCadReqConfig();
        void SendCadLogout();
        void SendCadRejectCancellation(int? easting, int? northing);
        void SendCadSkillLevel(char skillLevelCode, int? easting, int? northing);
        void SendCadUpdateStatus(int externalStatusCode, int easting, int northing, int? dispositionCode, string destination);
        void SendCadManualNavigation(string destination, int easting, int northing);
    }
}
