namespace Quest.Common.Messages
{
    /// <summary>
    ///     a standard response class containing success or failure indicator
    /// </summary>
    public class Response : MessageBase
    {
        public Response()
        {
            Message = "success";
            Success = true;
        }


        /// <summary>
        ///     The request id in which this is respponding to
        /// </summary>
        
        public string RequestId { get; set; }

        /// <summary>
        ///     message indicating any failure or specific success
        /// </summary>
        
        public string Message { get; set; }

        /// <summary>
        ///     flag indicating whether the request was successful
        /// </summary>
        
        public bool Success { get; set; }
    }
}