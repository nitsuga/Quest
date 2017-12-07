using Quest.LAS.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quest.LAS.Codec
{

    public delegate void DuplicateSequenceDelegate(object sender);

    public delegate void EqNetworkSwitchDelegate(object sender, string eqNetworkName);

    public delegate void EqMessageReceivedDelegate(object sender, int queueSize);

    public delegate void EqMessageCountersDelegate(object sender, EqMessageCountersArgs args);

    public delegate void RcvUnknownEvent(object sender);

    public delegate void RcvMdtEsDatabaseUpdateEvent(object sender);

    public delegate void RcvMdtEsMessageEvent(object sender, EngineeringMessageArgs engMessageArgs);

    public delegate void RcvStatusUpdateEvent(object sender, StatusUpdateEventArgs statusUpdateEventArgs);

    public delegate void RcvSetDestinationEvent(object sender);

    public delegate void RcvRequestEtaEvent(object sender);

    public delegate void RcvParametersEvent(object sender, List<MdtParameter> parameters, long sequenceNumber, DateTime messageTimeStamp);

    public delegate void RcvGeneralMessageEvent(object sender, int priority, string messageText, long cadSequenceNumber, DateTime messageDateTime, MdtCadMessageTypeEnum messageType);

    public delegate void RcvIncidentUpdateEvent(object sender, IncidentUpdateEventArgs incidentEventArgs);

    public delegate void RcvCancelIncidentEvent(object sender, IncidentCancellation cancellation, long cadSequenceNumber, DateTime messageDateTime);

    public delegate void RcvCallsignUpdateEvent(object sender, CallsignParam csParam, long sequenceNumber, DateTime messageTimeStamp);

    public class MessageCommunication : IMessageCommunication
    {
        public ICadMessageCodec MessageCodec { get; set; }
        public ICadInterface CadInterface { get; set; }

        public int AgedMessages { get; set; }
        public int DuplicateMessages { get; set; }
        public long LastRcvdSequenceNumber { get; set; }
        public event RcvCallsignUpdateEvent RcvCallsignUpdate;
        public event RcvCancelIncidentEvent RcvCancelIncident;
        public event RcvIncidentUpdateEvent RcvIncidentUpdate;
        public event RcvGeneralMessageEvent RcvGeneralMessage;
        public event RcvParametersEvent RcvParameters;
        public event RcvRequestEtaEvent RcvRequestEta;
        public event RcvSetDestinationEvent RcvSetDestinationEvent;
        public event RcvStatusUpdateEvent RcvStatusUpdate;
        public event RcvMdtEsMessageEvent RcvMdtEsMessage;
        public event RcvMdtEsDatabaseUpdateEvent RcvMdtEsDatabaseUpdate;
        public event RcvUnknownEvent RcvUnknown;


        public void Initialise()
        {
            CadInterface.EqMessageReceived += _cadInterface_EqMessageReceived;

        }

        void _cadInterface_EqMessageReceived(object sender, int queueSize)
        {
            var newMessages = CadInterface.ReadInboundMessages();

            if (newMessages != null && newMessages.Count > 0)
            {
                foreach (var newMesage in newMessages)
                {
                    if (LastRcvdSequenceNumber == newMesage.SequenceNumber)
                        DuplicateMessages++;

                    if (LastRcvdSequenceNumber < newMesage.SequenceNumber)
                        AgedMessages++;

                    LastRcvdSequenceNumber = newMesage.SequenceNumber;

                    IdentifyInboundMessage(newMesage.MessageText, newMesage.SequenceNumber, newMesage.MdtTimestamp, newMesage.CadTimestamp, newMesage.RxQueueSize);
                }

            }
        }


        public void IdentifyInboundMessage(byte[] messageText, long sequenceNumber, DateTime mdtTimeStamp, DateTime cadTimeStamp, int rxQueueSize)
        {
            var result = MessageCodec.DecodeInboundMessage(messageText);

            switch (result)
            {
                case CadMessageCodecTypeEnum.CallsignUpdate:
                    HandleCallsignUpdate(messageText, sequenceNumber, cadTimeStamp);
                    break;

                case CadMessageCodecTypeEnum.GeneralMessage:
                    HandleGeneralMessage(messageText, sequenceNumber, cadTimeStamp);
                    break;

                case CadMessageCodecTypeEnum.ParameterUpdate:
                    HandleParameters(messageText, sequenceNumber, cadTimeStamp);
                    break;

                case CadMessageCodecTypeEnum.IncidentUpdate:
                    HandleIncidentUpdate(messageText, sequenceNumber, cadTimeStamp);
                    break;

                case CadMessageCodecTypeEnum.CancelIncident:
                    HandleCancelIncident(messageText, sequenceNumber, cadTimeStamp);
                    break;

                case CadMessageCodecTypeEnum.AdminMessage:
                    HandleAdminMessage(messageText, sequenceNumber, cadTimeStamp);
                    break;

                case CadMessageCodecTypeEnum.SetDestination:
                    HandleSetDestination(messageText, sequenceNumber, cadTimeStamp);
                    break;

                case CadMessageCodecTypeEnum.StatusUpdate:
                    HandleStatusUpdate(messageText, sequenceNumber, cadTimeStamp);
                    break;

                case CadMessageCodecTypeEnum.EngineeringMessage:
                    HandleEngineeringMessage(messageText, sequenceNumber, cadTimeStamp);
                    break;
            }


        }

        private void HandleEngineeringMessage(byte[] data, long sequenceNumber, DateTime cadTimeStamp)
        {
            var engMessage = MessageCodec.DecodeEngineeringMessage(data);
            if (engMessage != null && RcvMdtEsMessage != null)
            {
                RcvMdtEsMessage(this, engMessage);
            }
        }

        private void HandleStatusUpdate(byte[] data, long sequenceNumber, DateTime cadTimeStamp)
        {
            var statusIndicatorType = (CadStatusOrigin)data[0];
            var statusValue = data[2];

            if (RcvStatusUpdate != null)
            {
                RcvStatusUpdate(this, new StatusUpdateEventArgs
                {
                    StatusOrigin = statusIndicatorType,
                    StatusValue = statusValue,
                    SequenceNumber = sequenceNumber,
                    MessageDateTime = cadTimeStamp,
                    IsCallsignUpdate = false
                });
            }
        }

        private void HandleSetDestination(byte[] data, long sequenceNumber, DateTime cadTimeStamp)
        {
            throw new NotImplementedException();
        }


        private void HandleAdminMessage(byte[] data, long sequenceNumber, DateTime cadTimeStamp)
        {
            var adminMessage = MessageCodec.DecodeAdminMessage(data);

            //Todo - pass in as ES message or admin message?

            //if (adminMessage != null && RcvMdtEsMessage != null)
            //{
            //    RcvMdtEsMessage(this, new EngineeringMessageArgs());
            //}
        }


        private void HandleCancelIncident(byte[] data, long sequenceNumber, DateTime messageTimeStamp)
        {
            var cancellation = MessageCodec.DecodeCancelIncident(data);

            if (cancellation != null && RcvCancelIncident != null)
            {
                RcvCancelIncident(this, cancellation, sequenceNumber, messageTimeStamp);
            }
        }

        private void HandleIncidentUpdate(byte[] data, long sequenceNumber, DateTime messageTimeStamp)
        {
            var incident = MessageCodec.DecodeCallsignIncidentParameter(data, data.Length);

            if (incident != null && RcvIncidentUpdate != null)
            {
                RcvIncidentUpdate(this, new IncidentUpdateEventArgs
                {
                    IncidentUpdate = incident,
                    SequenceNumber = sequenceNumber,
                    MessageDateTime = messageTimeStamp,
                    Completed = false
                });

            }
        }

        private void HandleParameters(byte[] data, long sequenceNumber, DateTime messageTimeStamp)
        {
            var parameters = MessageCodec.DecodeParameters(data);

            if (parameters != null && RcvParameters != null)
            {
                RcvParameters(this, parameters, sequenceNumber, messageTimeStamp);
            }
        }

        private void HandleGeneralMessage(byte[] data, long sequenceNumber, DateTime messageTimeStamp)
        {
            var generalMessageParam = MessageCodec.DecodeGeneralMessage(data);

            if (RcvGeneralMessage != null && generalMessageParam != null)
            {
                RcvGeneralMessage(this, generalMessageParam.MsgPriority, generalMessageParam.MessageText, sequenceNumber, messageTimeStamp, generalMessageParam.MessageType);
            }
        }

        private void HandleCallsignUpdate(byte[] data, long sequenceNumber, DateTime messageTimeStamp)
        {
            var csLength = Convert.ToInt16(data[2]);

            var callSignParam = MessageCodec.DecodeCallsignParameter(data, csLength);

            if (callSignParam != null && RcvCallsignUpdate != null)
            {
                RcvCallsignUpdate(this, callSignParam, sequenceNumber, messageTimeStamp);

                if (callSignParam.IncidentUpdate != null && RcvIncidentUpdate != null)
                {
                    var incidentEventArgs = new IncidentUpdateEventArgs
                    {
                        IncidentUpdate = callSignParam.IncidentUpdate,
                        SequenceNumber = sequenceNumber,
                        MessageDateTime = messageTimeStamp,
                        Completed = false
                    };

                    RcvIncidentUpdate(this, incidentEventArgs);

                    if (incidentEventArgs.Completed && callSignParam.StatusUpdate != null && RcvStatusUpdate != null)
                    {
                        var statusUpdateArgs = new StatusUpdateEventArgs
                        {
                            StatusOrigin = callSignParam.StatusUpdate.StatusOrigin,
                            StatusValue = callSignParam.StatusUpdate.ExternalStatusId,
                            SequenceNumber = sequenceNumber,
                            MessageDateTime = messageTimeStamp,
                            IsCallsignUpdate = true
                        };

                        RcvStatusUpdate(this, statusUpdateArgs);
                    }

                }
                else if (callSignParam.StatusUpdate != null && RcvStatusUpdate != null)
                {
                    RcvStatusUpdate(this, new StatusUpdateEventArgs
                    {
                        StatusOrigin = callSignParam.StatusUpdate.StatusOrigin,
                        StatusValue = callSignParam.StatusUpdate.ExternalStatusId,
                        SequenceNumber = sequenceNumber,
                        MessageDateTime = messageTimeStamp,
                        IsCallsignUpdate = true
                    });
                }
            }

        }

        public void SendCadLogin()
        {
            //Todo - params for login need to be properly passed in
            var encodedLogin = MessageCodec.EncodeCadLogin(18, string.Empty, string.Empty, string.Empty, string.Empty);

            CadInterface.SendOutboundMessage(encodedLogin, "S");
        }

        public void SendMdtEs(string messagePayload)
        {
            CadInterface.SendOutboundMessage(Encoding.ASCII.GetBytes(messagePayload).Concat(new byte[] { 0xAC }).ToArray(), "E");
        }

        public void SendCadAtDestination(AtDestinationMessageTypeEnum atDestinationMessageType, int easting, int northing, int? dispositionCode, string destinationHospital)
        {
            var encodedAtDestination = MessageCodec.EncodeAtDestination((int)atDestinationMessageType, dispositionCode, destinationHospital, easting, northing);
            CadInterface.SendOutboundMessage(encodedAtDestination, "S");
        }

        public void SendCadAtDestination(int destinationType, int easting, int northing)
        {
            var encodedAtDestination = MessageCodec.EncodeAtDestination(destinationType, null, null, easting, northing);
            CadInterface.SendOutboundMessage(encodedAtDestination, "S");
        }

        public void SendCadAvls(int easting, int northing, float speed, float direction, float? etaDistance, float? etaMinutes)
        {
            var avlsMessage = MessageCodec.EncodeAvls(easting, northing, speed, direction, etaDistance, etaMinutes);
            CadInterface.SendOutboundMessage(avlsMessage, "A");
        }

        public void SendCadEta()
        {

        }

        public void SendCadReqEventUpdate()
        {

        }

        public void SendCadReqConfig()
        {

        }

        public void SendCadLogout()
        {

        }

        public void SendCadManualNavigation(string destination, int easting, int northing)
        {
            var manualNav = CadInterface.CadMessageCodec.EncodeManualNavigate(destination, easting, northing);
            if (manualNav != null && manualNav.Length > 0)
            {
                CadInterface.SendOutboundMessage(manualNav, "S");
            }
        }

        public void SendCadRejectCancellation(int? easting, int? northing)
        {
            var cadRejectCancel = CadInterface.CadMessageCodec.EncodeCancelReject(easting, northing);
            if (cadRejectCancel != null && cadRejectCancel.Length > 0)
            {
                CadInterface.SendOutboundMessage(cadRejectCancel, "S");
            }
        }

        public void SendCadSkillLevel(char skillLevelCode, int? easting, int? northing)
        {
            var cadSkillLevel = CadInterface.CadMessageCodec.EncodeSkillLevel(easting, northing, skillLevelCode);
            if (cadSkillLevel != null)
            {
                CadInterface.SendOutboundMessage(cadSkillLevel, "S");
            }
        }

        public void SendCadUpdateStatus(int externalStatusCode, int easting, int northing, int? dispositionCode, string destination)
        {
            var statusUpdate = CadInterface.CadMessageCodec.EncodeStatusUpdate(externalStatusCode, easting, northing, dispositionCode, destination);
            if (statusUpdate != null && statusUpdate.Length > 0)
            {
                CadInterface.SendOutboundMessage(statusUpdate, "S");
            }
        }
    }
}
