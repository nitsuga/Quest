using System;

namespace Quest.Common.Messages.Entities
{
    /// <summary>
    /// get entities newer than the specified Revision
    /// </summary>
    [Serializable]
    public class GetEntitiesRequest : Request
    {
        /// <summary>
        /// current revision
        /// </summary>
        public int Revision { get; set; }
    }
}