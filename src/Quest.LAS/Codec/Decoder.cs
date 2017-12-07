﻿using Quest.LAS.Extensions;
using Quest.LAS.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Quest.LAS.Codec
{
    public class Decoder
    {
        public CadMessageCodecTypeEnum DecodeInboundMessage(byte[] data)
        {
            if ((InboundCadMessageTypeEnum)data[0] == InboundCadMessageTypeEnum.InboundMessageIdentifier)
            {
                switch ((InboundCadMessageTypeEnum)data[1])
                {
                    case InboundCadMessageTypeEnum.MIT_CALLSIGN_PARAM_CALLSIGN:
                        return CadMessageCodecTypeEnum.CallsignUpdate;
                }

            }

            switch ((InboundCadMessageTypeEnum)data[0])
            {
                case InboundCadMessageTypeEnum.MIT_CMD_SEND_MESSAGE:
                case InboundCadMessageTypeEnum.MIT_CMD_GENERAL_BROADCAST:
                    return CadMessageCodecTypeEnum.GeneralMessage;
                case InboundCadMessageTypeEnum.MIT_CMD_PARAMETERS:
                    return CadMessageCodecTypeEnum.ParameterUpdate;
                case InboundCadMessageTypeEnum.MIT_CMD_CANCEL_INCIDENT:
                    return CadMessageCodecTypeEnum.CancelIncident;
                case InboundCadMessageTypeEnum.MIT_CMD_INCIDENT:
                    return CadMessageCodecTypeEnum.IncidentUpdate;
                case InboundCadMessageTypeEnum.MIT_CMD_ADMIN:
                    return CadMessageCodecTypeEnum.AdminMessage;
                case InboundCadMessageTypeEnum.EngineeringMessage:
                    return CadMessageCodecTypeEnum.EngineeringMessage;
                case InboundCadMessageTypeEnum.MIT_CMD_SEND_STATUS_R:
                case InboundCadMessageTypeEnum.MIT_CMD_SEND_STATUS_B:
                case InboundCadMessageTypeEnum.MIT_CMD_SEND_STATUS_J:
                case InboundCadMessageTypeEnum.MIT_CMD_SEND_STATUS_U:
                    return CadMessageCodecTypeEnum.StatusUpdate;
            }

            return CadMessageCodecTypeEnum.NotAvailable;
        }

        public CallsignParam DecodeCallsignParameter(byte[] data, int dataLength)
        {
            var pointer = 3;
            var csData = UtilityFunctions.ExtractEncodedData(data, pointer, dataLength);
            CallsignParam cs = UtilityFunctions.SplitCallSignParams(csData);


            if (data.Length > dataLength + 3)
            {
                if ((InboundCadMessageTypeEnum)data[0] == InboundCadMessageTypeEnum.InboundMessageIdentifier)
                {
                    pointer = pointer + dataLength;

                    while (pointer < data.Length)
                    {
                        var identifier = (InboundCadMessageTypeEnum)data[pointer];

                        if (identifier == InboundCadMessageTypeEnum.MIT_CALLSIGN_PARAM_STATUS)
                        {
                            pointer++;
                            var statusIndicatorType = data[pointer];
                            pointer++;
                            var statusValue = data[pointer];

                            if (statusValue >= 1)
                            {
                                cs.StatusUpdate = new StatusUpdate
                                {
                                    ExternalStatusId = statusValue,
                                    StatusOrigin = (CadStatusOrigin)statusIndicatorType
                                };
                            }
                            pointer++;
                        }

                        else if (identifier == InboundCadMessageTypeEnum.MIT_CALLSIGN_PARAM_INCIDENT)
                        {
                            pointer++;
                            var dataByteLength = data[pointer]; //number of bytes used to describe length of data
                            pointer++;

                            var dataLengthArray = new byte[dataByteLength];

                            Array.Copy(data, pointer, dataLengthArray, 0, dataByteLength);

                            var incidentDataLength = UtilityFunctions.DecodeNumeric(dataLengthArray, dataByteLength); //little endian encoded integer value representing the length of the incident data
                            pointer = pointer + dataByteLength;

                            if ((InboundCadMessageTypeEnum)data[pointer] == InboundCadMessageTypeEnum.MIT_CMD_INCIDENT)
                            {
                                var incidentData = new byte[incidentDataLength];
                                Array.Copy(data, pointer, incidentData, 0, incidentDataLength);
                                var incident = DecodeCallsignIncidentParameter(incidentData, incidentDataLength);
                                if (incident != null)
                                {
                                    cs.IncidentUpdate = incident;
                                }
                            }

                            pointer = pointer + incidentDataLength;
                        }
                        //else
                        //{
                        //    cs.StatusUpdate = new StatusUpdate
                        //    {
                        //        ExternalStatusId = (int)InternalStatusEnum.NotSet,
                        //        StatusOrigin = CadStatusOrigin.J
                        //    };
                        //}

                        //pointer++;
                    }



                }

            }

            if (cs.StatusUpdate == null)
            {
                cs.StatusUpdate = new StatusUpdate
                {
                    ExternalStatusId = 0,
                    StatusOrigin = CadStatusOrigin.J
                };
            }

            return cs;
        }

        public IncidentUpdate DecodeCallsignIncidentParameter(byte[] data, int dataLength)
        {
            IncidentUpdate incident = new IncidentUpdate();

            var pointer = 1;

            while (pointer < data.Length)
            {
                var identifier = (IncidentParamTypeEnum)data[pointer];
                pointer++;
                var valueLength = data[pointer];
                pointer++;
                var value = pointer + valueLength <= data.Length
                    ? UtilityFunctions.GetData(pointer, valueLength, data)
                    : UtilityFunctions.GetData(pointer, data.Length - pointer, data);
                switch (identifier)
                {
                    case IncidentParamTypeEnum.IncidentNumber:
                        incident.IncidentNumber = UtilityFunctions.DecodeNumeric(value, valueLength);
                        break;

                    case IncidentParamTypeEnum.Sector:
                        incident.Sector = UtilityFunctions.DecodeString(value);
                        break;

                    case IncidentParamTypeEnum.IncidentDate:
                        incident.IncidentDate = UtilityFunctions.DecodeDate(value);
                        break;

                    case IncidentParamTypeEnum.StartTime:
                        incident.StartTime = UtilityFunctions.DecodeDate(value);
                        break;

                    case IncidentParamTypeEnum.EndTime:
                        incident.EndTime = UtilityFunctions.DecodeDate(value);
                        break;

                    case IncidentParamTypeEnum.Caller:
                        incident.Caller = UtilityFunctions.DecodeString(value);
                        break;

                    case IncidentParamTypeEnum.IncidentEasting:
                        incident.IncidentEasting = UtilityFunctions.DecodeNumeric(value, valueLength);
                        break;

                    case IncidentParamTypeEnum.IncidentNorthing:
                        incident.IncidentNorthing = UtilityFunctions.DecodeNumeric(value, valueLength);
                        break;

                    case IncidentParamTypeEnum.Destination:
                        incident.Destination = UtilityFunctions.DecodeString(value);
                        break;

                    case IncidentParamTypeEnum.DestinationEasting:
                        incident.DestinationEasting = UtilityFunctions.DecodeNumeric(value, valueLength);
                        break;

                    case IncidentParamTypeEnum.DestinationNorthing:
                        incident.DestinationNorthing = UtilityFunctions.DecodeNumeric(value, valueLength);
                        break;

                    case IncidentParamTypeEnum.PatientType:
                        incident.PatientType = UtilityFunctions.DecodeString(value);
                        break;

                    case IncidentParamTypeEnum.Infectious:
                        incident.IsInfectious = UtilityFunctions.DecodeString(value) == "Y";
                        break;

                    case IncidentParamTypeEnum.Psychiatric:
                        incident.IsPsychiatric = UtilityFunctions.DecodeString(value) == "Y";
                        break;

                    case IncidentParamTypeEnum.Location:
                        incident.Location = UtilityFunctions.DecodeString(value);
                        break;

                    case IncidentParamTypeEnum.Patient:
                        incident.PatientName = UtilityFunctions.DecodeString(value);
                        break;

                    case IncidentParamTypeEnum.Age:
                        incident.Age = UtilityFunctions.DecodeString(value);
                        break;

                    case IncidentParamTypeEnum.Sex:
                        incident.Sex = UtilityFunctions.DecodeString(value);
                        break;

                    case IncidentParamTypeEnum.Escort:
                        incident.IsEscort = UtilityFunctions.DecodeString(value) == "Y";
                        break;

                    case IncidentParamTypeEnum.InjuryDescription:
                        incident.InjuryDescription = UtilityFunctions.DecodeString(value);
                        break;

                    case IncidentParamTypeEnum.SpecialInstructions:
                        incident.SpecialInstruction = incident.SpecialInstruction + UtilityFunctions.DecodeString(value) + " ";
                        break;

                    case IncidentParamTypeEnum.OrconTime:
                        incident.OrconTime = UtilityFunctions.DecodeDate(value);
                        break;

                    case IncidentParamTypeEnum.GridRefAccuracy:
                        incident.GridRefAccuracy = UtilityFunctions.DecodeString(value);
                        break;

                    case IncidentParamTypeEnum.IncidentCategory:
                        incident.IncidentCategory = UtilityFunctions.DecodeString(value);
                        break;

                    case IncidentParamTypeEnum.ChiefComplaint:
                        incident.ChiefComplaint = UtilityFunctions.DecodeString(value);
                        break;

                    case IncidentParamTypeEnum.DeterminantText:
                        incident.DeterminantDescription = UtilityFunctions.DecodeString(value);
                        break;

                    case IncidentParamTypeEnum.Conscious:
                        incident.Conscious = UtilityFunctions.DecodeString(value);
                        break;

                    case IncidentParamTypeEnum.Breathing:
                        incident.Breathing = UtilityFunctions.DecodeString(value);
                        break;

                    case IncidentParamTypeEnum.NumberPatients:
                        incident.NumPatients = UtilityFunctions.DecodeNumericString(value, valueLength);
                        break;

                    case IncidentParamTypeEnum.CriticalKqs:
                        incident.CritialKQS = incident.CritialKQS + UtilityFunctions.DecodeString(value) + " ";
                        break;

                    case IncidentParamTypeEnum.CallIncomplete:
                        incident.CallIncomplete = UtilityFunctions.DecodeNumeric(value, valueLength);
                        break;

                    case IncidentParamTypeEnum.JobType:
                        incident.JobType = UtilityFunctions.DecodeNumeric(value, valueLength);
                        break;

                    case IncidentParamTypeEnum.PickupTime:
                        incident.PickupTime = UtilityFunctions.DecodeDate(value);
                        break;

                    case IncidentParamTypeEnum.AppointmentTime:
                        incident.AppointmentTime = UtilityFunctions.DecodeDate(value);
                        break;

                    case IncidentParamTypeEnum.IsResus:
                        incident.IsResus = UtilityFunctions.DecodeString(value) == "Y";
                        break;

                    case IncidentParamTypeEnum.IsOxygen:
                        incident.IsOxygen = UtilityFunctions.DecodeString(value) == "Y";
                        break;

                    case IncidentParamTypeEnum.Mobility:
                        incident.Mobility = UtilityFunctions.DecodeString(value);
                        break;
                }

                pointer += valueLength;
            }

            incident.SpecialInstruction = !string.IsNullOrEmpty(incident.SpecialInstruction) ? incident.SpecialInstruction.TrimEnd(' ') : null;
            incident.CritialKQS = !string.IsNullOrEmpty(incident.CritialKQS) ? incident.CritialKQS.TrimEnd(' ') : null;

            return incident;
        }

        public GeneralMessageParam DecodeGeneralMessage(byte[] data)
        {
            GeneralMessageParam generalMessageParam = new GeneralMessageParam();

            switch ((InboundCadMessageTypeEnum)data[0])
            {
                case InboundCadMessageTypeEnum.MIT_CMD_SEND_MESSAGE:
                    generalMessageParam.MessageType = MdtCadMessageTypeEnum.TextMessage;
                    break;

                case InboundCadMessageTypeEnum.MIT_CMD_GENERAL_BROADCAST:
                    generalMessageParam.MessageType = MdtCadMessageTypeEnum.GeneralBroadcast;
                    break;
            }

            if ((InboundCadMessageTypeEnum)data[1] == InboundCadMessageTypeEnum.MIT_MSG_PRIORITY)
            {
                generalMessageParam.MsgPriority = data[3];
            }

            if ((InboundCadMessageTypeEnum)data[4] == InboundCadMessageTypeEnum.MIT_MSG_TEXT)
            {
                generalMessageParam.MessageText = UtilityFunctions.ExtractEncodedData(data, 6, data[5]);
            }

            return generalMessageParam;
        }

        public List<MdtParameter> DecodeParameters(byte[] data)
        {
            var retParms = new List<MdtParameter>();
            var pointer = 1;

            while (pointer < data.Length)
            {
                var identifier = data[pointer];
                pointer++;
                var dataLength = data[pointer];
                pointer++;
                var value = UtilityFunctions.GetData(pointer, dataLength, data);

                object result = UtilityFunctions.DecodeNumeric(value, dataLength);

                if (dataLength >= 8)
                {
                    var dateString = Encoding.ASCII.GetString(value);
                    var unixDate = dateString.ToInt();
                    result = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixDate);
                }

                retParms.Add(
                    new MdtParameter
                    {
                        Identifier = identifier,
                        DataLength = dataLength,
                        ParameterValue = result,
                        Data = value
                    }
                    );

                pointer += dataLength;
            }

            return retParms;
        }

        public IncidentCancellation DecodeCancelIncident(byte[] data)
        {
            IncidentCancellation cancellation = new IncidentCancellation();
            var pointer = 1;
            while (pointer < data.Length)
            {
                var identifier = data[pointer];
                pointer++;
                var dataLength = data[pointer];
                pointer++;

                var value = UtilityFunctions.GetData(pointer, dataLength, data);

                switch (identifier)
                {
                    case 0x01:
                        cancellation.IncidentNumber = UtilityFunctions.DecodeNumeric(value, dataLength);
                        break;

                    case 0x03:
                        cancellation.IncidentDateTime = UtilityFunctions.DecodeDate(value);
                        break;

                    case 0x23:
                        cancellation.CancelReason = UtilityFunctions.DecodeString(value);
                        break;
                }

                pointer += dataLength;
            }

            return cancellation;
        }

        public AdminMessage DecodeAdminMessage(byte[] data)
        {
            AdminMessage adm = new AdminMessage();

            if (data.Length > 1 && data[1] == 44)
            {
                var dataLength = (int)data[2];
                var dataValue = UtilityFunctions.DecodeNumeric(UtilityFunctions.GetData(6, dataLength, data), dataLength);

                if ((AdminMessageTypeEnum)dataValue == AdminMessageTypeEnum.PingRequest && data.Length > 5)
                {
                    if (data[4] == 45)
                    {
                        var pingDataLength = (int)data[5];
                        var datePart = UtilityFunctions.GetData(6, pingDataLength - 2, data);
                        var milliSecondPart = UtilityFunctions.GetData(10, 2, data);
                        var val = UtilityFunctions.DecodeDate(datePart);
                        val = val.AddMilliseconds(UtilityFunctions.DecodeNumeric(milliSecondPart, 2));

                        adm.AdminMessageType = AdminMessageTypeEnum.PingRequest;
                        adm.PingTime = val;
                    }
                }
                else
                {
                    adm.AdminMessageType = (AdminMessageTypeEnum)dataValue;
                }

            }

            return adm;
        }

        public EngineeringMessageArgs DecodeEngineeringMessage(byte[] data)
        {
            EngineeringMessageArgs engMessage = new EngineeringMessageArgs();

            var messageText = Encoding.ASCII.GetString(data);
            if (messageText.StartsWith("Z"))
            {
                InboundESMessageTypeEnum t;
                Enum.TryParse(messageText.Substring(0, 3), out t);
                engMessage.InboundEsMessageType = t;
            }
            return engMessage;
        }

    }
}
