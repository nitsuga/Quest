using System;
using System.Collections.Generic;

namespace Quest.Common.Messages.Routing
{
    [Serializable]
    public class RouteMatcherResponse
    {
        public string Name;

        public List<RoadLinkEdgeSpeed> Results;

        public Visual.Visual Fixes;

        public Visual.Visual Route;

        public Visual.Visual Particles;

        /// <summary>
        /// Text of the network in GraphVis format. Generated if GenerateGraphVis=true
        /// </summary>
        public string GraphVis;

        public bool IsSuccess;
        public string Message;
    }
}