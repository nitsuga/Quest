using Quest.Common.Messages;
using Quest.Common.Messages.Telephony;

namespace Quest.Lib.EISEC
{
    /// <summary>
    ///     generic class for handling a result
    /// </summary>
    public class EisecResponse : MessageBase
    {
        public ReturnCode Code;
        public CallLookupResponse Details;
        public string Message;
        public int SubCode;

        public EisecResponse()
        {
        }

        public EisecResponse(ReturnCode code)
        {
            Code = code;
            Message = code.ToString();
            SubCode = 0;
        }

        public EisecResponse(ReturnCode code, string message)
        {
            Code = code;
            Message = message;
            SubCode = 0;
        }

        public EisecResponse(ReturnCode code, string message, int subcode, CallLookupResponse details)
        {
            Code = code;
            Message = message;
            SubCode = subcode;
            Details = details;
        }

        public override string ToString()
        {
            return string.Format("{1}|{2}|{0}", Message, Code, SubCode);
        }
    }
}