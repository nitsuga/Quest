//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Quest.Lib.DataModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class ResourceStatu
    {
        public ResourceStatu()
        {
            this.ResourceStatusHistories = new HashSet<ResourceStatusHistory>();
            this.Resources = new HashSet<Resource>();
            this.Devices = new HashSet<Device>();
        }
    
        public int ResourceStatusID { get; set; }
        public string Status { get; set; }
        public Nullable<bool> Available { get; set; }
        public Nullable<bool> Busy { get; set; }
        public Nullable<bool> Rest { get; set; }
        public Nullable<bool> Offroad { get; set; }
        public Nullable<bool> NoSignal { get; set; }
        public Nullable<bool> BusyEnroute { get; set; }
    
        public virtual ICollection<ResourceStatusHistory> ResourceStatusHistories { get; set; }
        public virtual ICollection<Resource> Resources { get; set; }
        public virtual ICollection<Device> Devices { get; set; }
    }
}
