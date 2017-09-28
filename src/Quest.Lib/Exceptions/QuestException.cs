using System;
using System.Net;

namespace Quest.Lib.Exceptions
{
    public class QuestExceptionDetail
    {
        public QuestExceptionDetail()
        {
            Code = HttpStatusCode.InternalServerError;
        }

        public QuestExceptionDetail(Exception ex)
        {
            Message = ex.Message;
            Stack = ex.ToString();
            Code = HttpStatusCode.InternalServerError;
        }

        public string Message { get; set; }

        public string Stack { get; set; }

        public System.Net.HttpStatusCode Code { get; set; }

    }


    /// <summary>
    /// This class is designed to wrap errors in the service and expose them gracefully as FaultExceptions to the client
    /// 
    /// Server-side Usage: throw FaultException<DungBeetleServiceFaultException>(new DungBeetleServiceFault("My Error"));
    /// 
    /// Client-side usage: catch(FaultException<DungBeetleServiceFaultException> fault)
    /// </summary>
    /// 
    public class QuestException : Exception
    {
        public QuestExceptionDetail Error { get; set; }

        public QuestException(string message, HttpStatusCode code = HttpStatusCode.InternalServerError)
        {
            Error = new QuestExceptionDetail();
            Error.Message = message;
            Error.Stack = "";
            Error.Code = code;
        }

        public QuestException(Exception ex)
        {
            Error = new QuestExceptionDetail();
            Error.Message = ex.Message;
            Error.Stack = ex.ToString();
        }
    }
}
