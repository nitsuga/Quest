using System;

namespace Quest.Common.Messages
{
    /// <summary>
    ///     a response to a logout request. check the success or failure flags
    /// </summary>

    [Serializable]
    public class GetCoverageResponse : Response
    {
        public Heatmap Map;
    }
}