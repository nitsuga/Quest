//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Quest.Lib.OS.Model
{
    using System;

    public partial class RoadRouteInfo
    {
        public int RoadRouteInfoId { get; set; }
        public string fid { get; set; }
        public Nullable<int> StartRoadLinkId { get; set; }
        public Nullable<int> EndRoadLinkId { get; set; }
        public string Instruction { get; set; }
        public string FromFid { get; set; }
        public string ToFid { get; set; }
    
        public virtual RoadLink RoadLink { get; set; }
        public virtual RoadLink RoadLink1 { get; set; }
    }
}
