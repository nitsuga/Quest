using Quest.LAS.Extensions;
using Quest.LAS.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Quest.LAS.Codec
{
    public class Encoder
    {
        private class Handler
        {
            public Handler(Func<IDeviceMessage, byte[]> method, string messageType)
            {
                Method = method;
                MessageType = messageType;
            }
            public Func<IDeviceMessage, byte[]> Method;
            public string MessageType;
        }

        private Dictionary<Type, Handler> encoders;

        public Encoder()
        {
            encoders = new Dictionary<Type, Handler>();

            encoders.Add(typeof(StatusUpdate), new Handler(Encoder.EncodeStatusUpdate, "S"));
            encoders.Add(typeof(RequestIncidentUpdate), new Handler(Encoder.EncodeIncidentUpdateRequest, "S"));
            encoders.Add(typeof(ConfigRequest), new Handler(Encoder.EncodeConfigRequest, "S"));
            encoders.Add(typeof(CadLogout), new Handler(Encoder.EncodeCadLogout, "S"));
            encoders.Add(typeof(CancelReject), new Handler(Encoder.EncodeCancelReject, "S"));
            encoders.Add(typeof(SetSkillLevel), new Handler(Encoder.EncodeSkillLevel, "S"));
            encoders.Add(typeof(AvlsUpdate), new Handler(Encoder.EncodeAvls, "A"));
            encoders.Add(typeof(StationResources), new Handler(Encoder.EncodeStationResources, "S"));
            encoders.Add(typeof(LocationResources), new Handler(Encoder.EncodeLocationResources, "S"));
            encoders.Add(typeof(AtDestination), new Handler(Encoder.EncodeAtDestination, "S"));
            encoders.Add(typeof(ManualNavigation), new Handler(Encoder.EncodeManualNavigate, "S"));
            encoders.Add(typeof(MdtUpdateVersion), new Handler(Encoder.EncodeMdtUpdateVersion, "S"));
            encoders.Add(typeof(PingResponse), new Handler(Encoder.EncodePingResponse, "S"));
            encoders.Add(typeof(CadLogin), new Handler(Encoder.EncodeCadLogin, "S"));
            encoders.Add(typeof(EngineeringUpdate), new Handler(Encoder.EncodeAvls, "E"));
        }

        /// <summary>
        /// Encode an IDeviceMessage as an ExpressQ message
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public string Encode(IDeviceMessage msg, EqMessageSettings settings)
        {
            Handler handler = null;
            encoders.TryGetValue(msg.GetType(), out handler);

            if (handler != null)
            {
                var bytes= handler.Method(msg);

                if (bytes != null)
                    return MakeMessage(bytes, handler.MessageType, settings);
            }
            return null;
        }

        private string MakeMessage(byte[] messageText, string messageType, EqMessageSettings settings)
        {
            messageText = messageText.Concat(DateTime.UtcNow.AddSeconds(settings.OutboundTimestampDelta).GetUnixBytes()).ToArray();

            // Todo - Read signal strength from provider
            var mid = EncodeOutboundMid(settings.S1, settings.S2, messageType, settings.Sequence);

            var textMessage = $"PRT:{settings.Priority}\nLIFETIME:{settings.Lifetime}\nOPT:{settings.Opt}\nMID:{mid}\nFROM:{settings.Source}\nTO:{settings.Destination}\n\n{messageText}";

            return textMessage;
        }

        public static string EncodeOutboundMid(int signalStrength0, int signalStrength1, string messageTypeId, int outboundSequenceNumber)
        {
            var sb = new StringBuilder();
            sb.Append(signalStrength0.ToString("0"));
            sb.Append(signalStrength1.ToString("0"));
            sb.Append(messageTypeId.Length > 0 ? messageTypeId.Substring(0, 1) : "U");
            sb.Append(outboundSequenceNumber.ToString("000000000"));
            return sb.ToString();
        }

        private static byte[] EncodeStatusUpdate(IDeviceMessage message)
        {
            var msg = message as StatusUpdate;
            byte[] statusUpdate = { (byte)OutboundCadMessageTypeEnum.CadUpdateIdentifier };

            //status code value and timestamp
            var statusByte = Convert.ToByte(msg.ShortStatusCode);

            byte[] statusUpdateArray = new byte[statusUpdate.Length + 1];
            statusUpdate.CopyTo(statusUpdateArray, 0);
            statusUpdateArray[1] = statusByte;
            statusUpdate = statusUpdateArray;


            statusUpdate = statusUpdate.Concat(DateTime.UtcNow.GetUnixBytes()).ToArray();

            //grid ref
            if (msg.StatusEasting.HasValue && msg.StatusNorthing.HasValue && msg.StatusEasting.Value > 0 && msg.StatusNorthing.Value > 0)
            {
                var gridRefBytes = UtilityFunctions.BuildGridReference(msg.StatusEasting.Value, msg.StatusNorthing.Value);
                statusUpdate = statusUpdate.Concat(gridRefBytes).ToArray();
            }


            //nonconvey code
            if (msg.NonConveyCode.HasValue)
            {
                statusUpdate = statusUpdate.Concat(new byte[] { 0x02, 0x02 }).Concat(BitConverter.GetBytes((ushort)msg.NonConveyCode.Value)).ToArray();
            }


            //destination hospital

            if (!string.IsNullOrEmpty(msg.DestinationHospital))
            {
                var hospitalBytes = Encoding.ASCII.GetBytes(msg.DestinationHospital);
                var hospitalBytesLength = Convert.ToByte(hospitalBytes.Length);

                statusUpdate = statusUpdate.Concat(new byte[] { 0x03, hospitalBytesLength }).Concat(hospitalBytes).ToArray();
            }

            //statusUpdate = statusUpdate.Concat(DateTime.UtcNow.GetUnixBytes()).ToArray();

            return statusUpdate;
        }

        private static byte[] EncodeIncidentUpdateRequest(IDeviceMessage msg)
        {
            var message = msg as RequestIncidentUpdate;
            byte[] result = UtilityFunctions.EncodeInitialData(OutboundCadMessageTypeEnum.RequestIncidentUpdate);

            return result;
        }

        private static byte[] EncodeConfigRequest(IDeviceMessage msg)
        {
            byte[] result = UtilityFunctions.EncodeInitialData(OutboundCadMessageTypeEnum.RequestConfig);

            return result;
        }

        public static byte[] EncodeCadLogout(IDeviceMessage msg)
        {
            //info message and logout identifier
            byte[] logout = { (byte)OutboundCadMessageTypeEnum.CadUpdateIdentifier, (byte)OutboundCadMessageTypeEnum.CadLogout, 0x00 };

            //logout time
            logout = logout.Concat(DateTime.UtcNow.GetUnixBytes()).ToArray();

            //ToDo - logout errors 

            //timestamp
            //logout = logout.Concat(DateTime.UtcNow.GetUnixBytes()).ToArray();

            return logout;
        }

        public static byte[] EncodeCancelReject(IDeviceMessage msg)
        {
            var message = msg as CancelReject;
            byte[] result = UtilityFunctions.EncodeInitialData(OutboundCadMessageTypeEnum.CancelReject);

            //grid ref
            if (message.StatusEasting.HasValue && message.StatusNorthing.HasValue)
            {
                result = result.Concat(UtilityFunctions.BuildGridReference(message.StatusEasting.Value, message.StatusNorthing.Value)).ToArray();
            }

            //result = result.Concat(DateTime.UtcNow.GetUnixBytes()).ToArray();

            return result;
        }

        public static byte[] EncodeSkillLevel(IDeviceMessage msg)
        {
            var message = msg as SetSkillLevel;

            byte[] result = UtilityFunctions.EncodeInitialData(OutboundCadMessageTypeEnum.SkillLevel);

            result = result.Concat(Encoding.ASCII.GetBytes(new[] { message.SkillLevel, ' ' })).ToArray();

            //grid ref
            if (message.StatusEasting.HasValue && message.StatusNorthing.HasValue)
            {
                result = result.Concat(UtilityFunctions.BuildGridReference(message.StatusEasting.Value, message.StatusNorthing.Value)).ToArray();
            }

            //result = result.Concat(DateTime.UtcNow.GetUnixBytes()).ToArray();

            return result;
        }

        public static byte[] EncodeAvls(IDeviceMessage msg)
        {
            var message = msg as AvlsUpdate;

            byte[] result = { (byte)OutboundCadMessageTypeEnum.Avls };

            var dir = (int)((message.Direction + 22.5) / 45) % 8;

            if (message.Speed > Math.Pow(2, 5) - 1)
            {
                message.Speed = (float)(Math.Pow(2, 5) - 1) * 5;
            }

            result = result.Concat(UtilityFunctions.BuildGridReference(message.Easting, message.Northing, false)).ToArray();

            var speedDirection = (int)(dir * 32 + Math.Round(message.Speed / 5, 0));

            var speedDirectionByte = BitConverter.GetBytes(speedDirection);

            result = result.Concat(new[] { speedDirectionByte[0] }).ToArray();

            if (message.EtaDistance.HasValue && message.EtaMinutes.HasValue)
            {

            }

            //result = result.Concat(DateTime.UtcNow.GetUnixBytes()).ToArray();

            return result;
        }

        public static byte[] EncodeStationResources(IDeviceMessage msg)
        {
            var message = msg as StationResources;

            byte[] result = UtilityFunctions.EncodeInitialData(OutboundCadMessageTypeEnum.StationResource);

            result = result.Concat(Encoding.ASCII.GetBytes(message.StationCode)).ToArray();

            //grid ref
            if (message.StatusEasting.HasValue && message.StatusNorthing.HasValue)
            {
                result = result.Concat(UtilityFunctions.BuildGridReference(message.StatusEasting.Value, message.StatusNorthing.Value)).ToArray();
            }

            //result = result.Concat(DateTime.UtcNow.GetUnixBytes()).ToArray();

            return result;
        }

        public static byte[] EncodeLocationResources(IDeviceMessage msg)
        {
            var message = msg as LocationResources;

            byte[] result = UtilityFunctions.EncodeInitialData(OutboundCadMessageTypeEnum.LocationResource);

            result = result.Concat(Encoding.ASCII.GetBytes(message.RequestEasting)).ToArray();
            result = result.Concat(Encoding.ASCII.GetBytes(message.RequestNorthing)).ToArray();

            //grid ref
            if (message.StatusEasting.HasValue && message.StatusNorthing.HasValue)
            {
                result = result.Concat(UtilityFunctions.BuildGridReference(message.StatusEasting.Value, message.StatusNorthing.Value)).ToArray();
            }

            //result = result.Concat(DateTime.UtcNow.GetUnixBytes()).ToArray();

            return result;
        }

        public static byte[] EncodeAtDestination(IDeviceMessage msg)
        {
            var message = msg as AtDestination;

            byte[] result = UtilityFunctions.EncodeInitialData(OutboundCadMessageTypeEnum.AtDestination);

            //at destination subtype
            result = result.Concat(new[] { Convert.ToByte(message.AtDestinationType) }).ToArray();

            //grid ref
            if (message.StatusEasting.HasValue && message.StatusNorthing.HasValue && message.StatusEasting.Value > 0 && message.StatusNorthing.Value > 0)
            {
                result = result.Concat(UtilityFunctions.BuildGridReference(message.StatusEasting.Value, message.StatusNorthing.Value)).ToArray();
            }

            //nonconvey code
            if (message.NonConveyCode.HasValue && message.NonConveyCode.Value >= 9000)
            {
                byte[] nonConveyBytes = { 0x02, 0x02 };
                nonConveyBytes = nonConveyBytes.Concat(BitConverter.GetBytes(message.NonConveyCode.Value)).ToArray();

                result = result.Concat(nonConveyBytes).ToArray();
            }

            //destination hospital
            if (!string.IsNullOrEmpty(message.DestinationHospital))
            {
                var hospitalBytes = Encoding.ASCII.GetBytes(message.DestinationHospital);
                var hospitalBytesLength = BitConverter.GetBytes(hospitalBytes.Length);

                result = result.Concat(new byte[] { 0x03 }).Concat(hospitalBytesLength).Concat(hospitalBytes).ToArray();
            }

            //result = result.Concat(DateTime.UtcNow.GetUnixBytes()).ToArray();

            return result;
        }

        public static byte[] EncodeManualNavigate(IDeviceMessage msg)
        {
            var message = msg as ManualNavigation;
            byte[] result = UtilityFunctions.EncodeInitialData(OutboundCadMessageTypeEnum.ManualNavigation);
            result = result.Concat(Encoding.ASCII.GetBytes(message.Destination)).ToArray();
            result = result.Concat(UtilityFunctions.BuildGridReference(message.Easting, message.Northing)).ToArray();
            return result;
        }

        public static byte[] EncodeMdtUpdateVersion(IDeviceMessage msg)
        {
            var message = msg as MdtUpdateVersion;
            byte[] result =
            {
                (byte) OutboundCadMessageTypeEnum.CadUpdateIdentifier,
                (byte) OutboundCadMessageTypeEnum.MdtUpdateVersion
            };

            result = result.Concat(message.UpdateDateTime.GetUnixBytes()).ToArray();

            //result = result.Concat(DateTime.UtcNow.GetUnixBytes()).ToArray();

            return result;
        }

        public static byte[] EncodePingResponse(IDeviceMessage msg)
        {
            var message = msg as PingResponse;
            byte[] result =
            {
                (byte) OutboundCadMessageTypeEnum.CadUpdateIdentifier,
                (byte) OutboundCadMessageTypeEnum.PingResponse
            };

            //result = result.Concat(cadPingTime.GetUnixBytes()).ToArray();

            return result;
        }

        public static byte[] EncodeCadLogin(IDeviceMessage msg)
        {
            var message = msg as CadLogin;
            //First two bytes indicate this is a cadupdate message and a login
            byte[] messageStart =
            {
                (byte)OutboundCadMessageTypeEnum.CadUpdateIdentifier,
                (byte)OutboundCadMessageTypeEnum.CadLogin
            };

            //Build protocol version
            byte[] protocolIdentifier = { 0x00 };

            byte[] protoversion = protocolIdentifier.Concat(BitConverter.GetBytes(message.ProtocolVersion)).ToArray();

            //Config version - identifier only
            byte[] configVersionIdentifier = { 0x01 };
            var configResult = configVersionIdentifier.Concat(DateTime.UtcNow.GetUnixBytes());

            //Build start time
            byte[] startTimeIdentifier = { 0x02 };
            var stResult = startTimeIdentifier.Concat(DateTime.UtcNow.GetUnixBytes());

            //Build image version
            byte[] imageVersionIdentifier = { 0x03 };
            var imageResult = imageVersionIdentifier.Concat(DateTime.UtcNow.GetUnixBytes());

            //final complete login message
            var result = messageStart.Concat(protoversion).Concat(configResult).Concat(stResult).Concat(imageResult).ToArray();

            return result;
        }

        public static byte[] EncodeEngineeringUpdate(IDeviceMessage msg)
        {
            var message = msg as EngineeringUpdate;

            var bytes= Encoding.ASCII.GetBytes(message.MessagePayload).Concat(new byte[] { 0xAC }).ToArray();

            return bytes;

        }

    }
}
