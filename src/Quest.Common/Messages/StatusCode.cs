using System;

namespace Quest.Common.Messages
{
    /// <summary>
    ///     contains a definition of a single status code
    /// </summary>
    [Serializable]
    
    public class StatusCode
    {
        /// <summary>
        ///     the status code. In LAS these are items such as AIQ, ENR, ONS, TAR, TRN
        /// </summary>
        
        public string Code { get; set; }

        /// <summary>
        ///     a human description of the status code
        /// </summary>
        
        public string Description { get; set; }
    }

    
}