using System;
using System.Collections.Generic;

namespace Quest.Common.Messages
{
    /// <summary>
    ///     The LoginResponse packet contains a response to the LoginRquest for a specific device
    /// </summary>

    [Serializable]
    public class LoginResponse : Response
    {
        /// <summary>
        ///     This token to be passed in all subsequent calls
        ///     Include in the header as Authentication = Bearer /AccessToken/
        /// </summary>
        public string SessionId;

        public DateTime ValidTo;

        /// <summary>
        ///     Server wants the client to send back its callsign
        /// </summary>
        
        public bool RequiresCallsign { get; set; }

        /// <summary>
        ///     server wants to set the callsign of the client.
        /// </summary>
        
        public string Callsign { get; set; }

        /// <summary>
        ///     The version of Quest that this server is implementing
        /// </summary>        
        public int QuestApi { get; set; }

        public StatusCode Status { get; set; }

        /// <summary>
        /// A list of claims that belong to this principle.
        /// </summary>
        public List<AuthorisationClaim> Claims;
    }

    
}