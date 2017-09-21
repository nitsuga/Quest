using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Quest.Lib.Exceptions;
using Quest.Lib.Trace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Quest.Api
{

    public class ApiResult
    {
        public HttpStatusCode Code;
        public string Message;
        public string Stack;
        public string Error;
    }

    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context /* other scoped dependencies */)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var code = HttpStatusCode.InternalServerError; // 500 if unexpected
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(1);
            Logger.Write($"{sf.GetMethod().Name} failed: {exception.Message}", TraceEventType.Verbose, "InternalService");

            string result = "";
            if (exception is QuestException)
            {
                var ex = exception as QuestException;
                code = ex.Error.Code;
                result = JsonConvert.SerializeObject(new ApiResult { Code = code, Message = ex.Error.Message, Stack = st.ToString() });
            }
            else
                result = JsonConvert.SerializeObject(new ApiResult { Error = exception.Message });

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;
            return context.Response.WriteAsync(result);
        }
    }
}

