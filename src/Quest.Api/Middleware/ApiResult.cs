using System.Net;

namespace Quest.Api.Middleware
{
    public class ApiResult
    {
        public HttpStatusCode Code;
        public string Message;
        public string Stack;
        public string Error;
    }
}

