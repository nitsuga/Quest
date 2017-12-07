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
        public byte[] EncodeStatusUpdate(int shortStatusCode, int? statusEasting, int? statusNorthing, int? nonConveyCode, string destinationHospital)
        {
            byte[] statusUpdate = { (byte)OutboundCadMessageTypeEnum.CadUpdateIdentifier };

            //status code value and timestamp
            var statusByte = Convert.ToByte(shortStatusCode);

            byte[] statusUpdateArray = new byte[statusUpdate.Length + 1];
            statusUpdate.CopyTo(statusUpdateArray, 0);
            statusUpdateArray[1] = statusByte;
            statusUpdate = statusUpdateArray;


            statusUpdate = statusUpdate.Concat(DateTime.UtcNow.GetUnixBytes()).ToArray();

            //grid ref
            if (statusEasting.HasValue && statusNorthing.HasValue && statusEasting.Value > 0 && statusNorthing.Value > 0)
            {
                var gridRefBytes = UtilityFunctions.BuildGridReference(statusEasting.Value, statusNorthing.Value);
                statusUpdate = statusUpdate.Concat(gridRefBytes).ToArray();
            }


            //nonconvey code
            if (nonConveyCode.HasValue)
            {
                statusUpdate = statusUpdate.Concat(new byte[] { 0x02, 0x02 }).Concat(BitConverter.GetBytes((ushort)nonConveyCode.Value)).ToArray();
            }


            //destination hospital

            if (!string.IsNullOrEmpty(destinationHospital))
            {
                var hospitalBytes = Encoding.ASCII.GetBytes(destinationHospital);
                var hospitalBytesLength = Convert.ToByte(hospitalBytes.Length);

                statusUpdate = statusUpdate.Concat(new byte[] { 0x03, hospitalBytesLength }).Concat(hospitalBytes).ToArray();
            }

            //statusUpdate = statusUpdate.Concat(DateTime.UtcNow.GetUnixBytes()).ToArray();

            return statusUpdate;
        }

        public byte[] EncodeIncidentUpdateRequest()
        {
            byte[] result = UtilityFunctions.EncodeInitialData(OutboundCadMessageTypeEnum.RequestIncidentUpdate);

            return result;
        }

        public byte[] EncodeConfigRequest()
        {
            byte[] result = UtilityFunctions.EncodeInitialData(OutboundCadMessageTypeEnum.RequestConfig);

            return result;
        }

        public byte[] EncodeCadLogout()
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

        public byte[] EncodeCancelReject(int? statusEasting, int? statusNorthing)
        {
            byte[] result = UtilityFunctions.EncodeInitialData(OutboundCadMessageTypeEnum.CancelReject);

            //grid ref
            if (statusEasting.HasValue && statusNorthing.HasValue)
            {
                result = result.Concat(UtilityFunctions.BuildGridReference(statusEasting.Value, statusNorthing.Value)).ToArray();
            }

            //result = result.Concat(DateTime.UtcNow.GetUnixBytes()).ToArray();

            return result;
        }

        public byte[] EncodeManualNavigation(string destination, int? statusEasting, int? statusNorthing)
        {
            byte[] result = UtilityFunctions.EncodeInitialData(OutboundCadMessageTypeEnum.ManualNavigation);

            //grid ref
            if (statusEasting.HasValue && statusNorthing.HasValue)
            {
                result = result.Concat(UtilityFunctions.BuildGridReference(statusEasting.Value, statusNorthing.Value)).ToArray();
            }

            //result = result.Concat(DateTime.UtcNow.GetUnixBytes()).ToArray();

            return result;
        }

        public byte[] EncodeSkillLevel(int? statusEasting, int? statusNorthing, char skillLevel)
        {
            byte[] result = UtilityFunctions.EncodeInitialData(OutboundCadMessageTypeEnum.SkillLevel);

            result = result.Concat(Encoding.ASCII.GetBytes(new[] { skillLevel, ' ' })).ToArray();

            //grid ref
            if (statusEasting.HasValue && statusNorthing.HasValue)
            {
                result = result.Concat(UtilityFunctions.BuildGridReference(statusEasting.Value, statusNorthing.Value)).ToArray();
            }

            //result = result.Concat(DateTime.UtcNow.GetUnixBytes()).ToArray();

            return result;
        }

        public byte[] EncodeAvls(int easting, int northing, float speed, float direction, float? etaDistance, float? etaMinutes)
        {
            byte[] result = { (byte)OutboundCadMessageTypeEnum.Avls };

            var dir = (int)((direction + 22.5) / 45) % 8;

            if (speed > Math.Pow(2, 5) - 1)
            {
                speed = (float)(Math.Pow(2, 5) - 1) * 5;
            }

            result = result.Concat(UtilityFunctions.BuildGridReference(easting, northing, false)).ToArray();

            var speedDirection = (int)(dir * 32 + Math.Round(speed / 5, 0));

            var speedDirectionByte = BitConverter.GetBytes(speedDirection);

            result = result.Concat(new[] { speedDirectionByte[0] }).ToArray();

            if (etaDistance.HasValue && etaMinutes.HasValue)
            {

            }

            //result = result.Concat(DateTime.UtcNow.GetUnixBytes()).ToArray();

            return result;
        }

        public byte[] EncodeStationResources(string stationCode, int? statusEasting, int? statusNorthing)
        {
            byte[] result = UtilityFunctions.EncodeInitialData(OutboundCadMessageTypeEnum.StationResource);

            result = result.Concat(Encoding.ASCII.GetBytes(stationCode)).ToArray();

            //grid ref
            if (statusEasting.HasValue && statusNorthing.HasValue)
            {
                result = result.Concat(UtilityFunctions.BuildGridReference(statusEasting.Value, statusNorthing.Value)).ToArray();
            }

            //result = result.Concat(DateTime.UtcNow.GetUnixBytes()).ToArray();

            return result;
        }

        public byte[] EncodeLocationResources(string requestEasting, string requestNorthing, int? statusEasting, int? statusNorthing)
        {
            byte[] result = UtilityFunctions.EncodeInitialData(OutboundCadMessageTypeEnum.LocationResource);

            result = result.Concat(Encoding.ASCII.GetBytes(requestEasting)).ToArray();
            result = result.Concat(Encoding.ASCII.GetBytes(requestNorthing)).ToArray();

            //grid ref
            if (statusEasting.HasValue && statusNorthing.HasValue)
            {
                result = result.Concat(UtilityFunctions.BuildGridReference(statusEasting.Value, statusNorthing.Value)).ToArray();
            }

            //result = result.Concat(DateTime.UtcNow.GetUnixBytes()).ToArray();

            return result;
        }

        public byte[] EncodeAtDestination(int atDestinationType, int? nonConveyCode, string destinationHospital, int? statusEasting, int? statusNorthing)
        {
            byte[] result = UtilityFunctions.EncodeInitialData(OutboundCadMessageTypeEnum.AtDestination);

            //at destination subtype
            result = result.Concat(new[] { Convert.ToByte(atDestinationType) }).ToArray();

            //grid ref
            if (statusEasting.HasValue && statusNorthing.HasValue && statusEasting.Value > 0 && statusNorthing.Value > 0)
            {
                result = result.Concat(UtilityFunctions.BuildGridReference(statusEasting.Value, statusNorthing.Value)).ToArray();
            }

            //nonconvey code
            if (nonConveyCode.HasValue && nonConveyCode.Value >= 9000)
            {
                byte[] nonConveyBytes = { 0x02, 0x02 };
                nonConveyBytes = nonConveyBytes.Concat(BitConverter.GetBytes(nonConveyCode.Value)).ToArray();

                result = result.Concat(nonConveyBytes).ToArray();
            }

            //destination hospital
            if (!string.IsNullOrEmpty(destinationHospital))
            {
                var hospitalBytes = Encoding.ASCII.GetBytes(destinationHospital);
                var hospitalBytesLength = BitConverter.GetBytes(hospitalBytes.Length);

                result = result.Concat(new byte[] { 0x03 }).Concat(hospitalBytesLength).Concat(hospitalBytes).ToArray();
            }

            //result = result.Concat(DateTime.UtcNow.GetUnixBytes()).ToArray();

            return result;
        }

        public byte[] EncodeManualNavigate(string destination, int easting, int northing)
        {
            byte[] result = UtilityFunctions.EncodeInitialData(OutboundCadMessageTypeEnum.ManualNavigation);
            result = result.Concat(Encoding.ASCII.GetBytes(destination)).ToArray();
            result = result.Concat(UtilityFunctions.BuildGridReference(easting, northing)).ToArray();
            return result;
        }

        public byte[] EncodeMdtUpdateVersion(DateTime updateDateTime)
        {
            byte[] result =
            {
                (byte) OutboundCadMessageTypeEnum.CadUpdateIdentifier,
                (byte) OutboundCadMessageTypeEnum.MdtUpdateVersion
            };

            result = result.Concat(updateDateTime.GetUnixBytes()).ToArray();

            //result = result.Concat(DateTime.UtcNow.GetUnixBytes()).ToArray();

            return result;
        }

        public byte[] EncodePingResponse(DateTime cadPingTime)
        {
            byte[] result =
            {
                (byte) OutboundCadMessageTypeEnum.CadUpdateIdentifier,
                (byte) OutboundCadMessageTypeEnum.PingResponse
            };

            //result = result.Concat(cadPingTime.GetUnixBytes()).ToArray();

            return result;
        }

        public string EncodeOutboundMid(int signalStrength0, int signalStrength1, string messageTypeId, int outboundSequenceNumber)
        {
            var sb = new StringBuilder();
            sb.Append(signalStrength0.ToString("0"));
            sb.Append(signalStrength1.ToString("0"));
            sb.Append(messageTypeId.Length > 0 ? messageTypeId.Substring(0, 1) : "U");
            sb.Append(outboundSequenceNumber.ToString("000000000"));
            return sb.ToString();
        }

        public byte[] EncodeCadLogin(UInt16 protocolVersion, string configVersion, string imageVersion, string g18Info, string otherInfo)
        {
            //First two bytes indicate this is a cadupdate message and a login
            byte[] messageStart =
            {
                (byte)OutboundCadMessageTypeEnum.CadUpdateIdentifier,
                (byte)OutboundCadMessageTypeEnum.CadLogin
            };

            //Build protocol version
            byte[] protocolIdentifier = { 0x00 };

            byte[] protoversion = protocolIdentifier.Concat(BitConverter.GetBytes(protocolVersion)).ToArray();

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

    }
}
