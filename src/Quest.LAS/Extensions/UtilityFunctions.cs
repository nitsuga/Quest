using Quest.LAS.Codec;
using Quest.LAS.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quest.LAS.Extensions
{
    public static class UtilityFunctions
    {
        private const int MinEasting = 498732;
        private const int MinNorthing = 145232;

        public static CallsignParam SplitCallSignParams(string csData)
        {
            //sample data: EC46.#|9909

            var csParams = new CallsignParam();

            var parts = csData.Split('|');

            var fleet = parts[1].Trim(' ');

            csParams.FleetNo = Convert.ToInt16(fleet);

            var leftPart = parts[0].Trim(' ');

            if (!leftPart.Contains("#"))
            {
                csParams.DsoFlag = false;
            }
            else
            {
                csParams.DsoFlag = true;
                leftPart = leftPart.Replace("#", String.Empty);
            }

            if (!leftPart.Contains("."))
            {
                csParams.OnShift = true;
            }
            else
            {
                csParams.OnShift = false;
                leftPart = leftPart.Replace(".", String.Empty);
            }

            if (leftPart.Length > 4)
            {
                csParams.SkillLevel = leftPart.Substring(leftPart.Length - 1);
                leftPart = leftPart.TrimEnd(Convert.ToChar(leftPart.Substring(leftPart.Length - 1)));
            }

            if (leftPart.Length > 3)
            {
                csParams.Callsign = leftPart;
            }


            return csParams;
        }


        public static string ExtractEncodedData(byte[] data, int startIndex, int dataLength)
        {
            string outputValue = String.Empty;

            for (var i = startIndex; i < dataLength + startIndex; i++)
            {
                int value = Convert.ToInt16(data[i]);
                outputValue += Char.ConvertFromUtf32(value);
            }

            return outputValue;
        }

        public static byte[] GetData(int pointer, int dataLength, byte[] data)
        {
            var retData = new byte[dataLength];
            Array.Copy(data, pointer, retData, 0, dataLength);
            return retData;
        }

        public static string DecodeString(byte[] value)
        {
            return Encoding.ASCII.GetString(value);
        }

        public static DateTime DecodeDate(byte[] value)
        {
            var unixDate = BitConverter.ToInt32(value, 0);
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixDate);
        }


        public static byte[] BuildGridReference(int easting, int northing, bool useRunLength = true)
        {
            var finalEasting = easting - MinEasting;
            var finalNorthing = northing - MinNorthing;

            var eastingBytes = BitConverter.GetBytes((short)finalEasting);
            var northingBytes = BitConverter.GetBytes((short)finalNorthing);

            byte[] gridRef = { 0x01, 0x04 };

            if (useRunLength)
            {
                gridRef = gridRef.Concat(eastingBytes).Concat(northingBytes).ToArray();
            }
            else
            {
                gridRef = eastingBytes.Concat(northingBytes).ToArray();
            }

            return gridRef;
        }


        public static int DecodeNumeric(byte[] value, int dataLength)
        {
            int intValue;
            switch (dataLength)
            {
                case 1:
                    intValue = value[0];
                    break;
                case 2:
                    intValue = BitConverter.ToInt16(value, 0);
                    break;
                case 3:
                    var newData = new byte[4];
                    Array.Copy(value, 0, newData, 0, 3);
                    intValue = (int)BitConverter.ToUInt32(newData, 0);
                    break;
                case 4:
                    intValue = BitConverter.ToInt32(value, 0);
                    break;
                default:
                    intValue = 0;
                    break;
            }

            return intValue;
        }


        public static int DecodeNumericString(byte[] value, int dataLength)
        {
            var stringArray = new byte[dataLength];
            Array.Copy(value, 0, stringArray, 0, dataLength);
            var numString = DecodeString(stringArray);
            return numString.ToInt();
        }

        public static byte[] EncodeInitialData(OutboundCadMessageTypeEnum identifier)
        {
            byte[] result =
            {
                (byte) OutboundCadMessageTypeEnum.CadUpdateIdentifier,
                (byte) identifier
            };

            result = result.Concat(DateTime.UtcNow.GetUnixBytes()).ToArray();

            return result;
        }
    }
}
