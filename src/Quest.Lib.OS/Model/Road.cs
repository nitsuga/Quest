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
    using System.Collections.Generic;

    public partial class Road
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Road()
        {
            this.RoadNetworkMembers = new HashSet<RoadNetworkMember>();
        }
    
        public int RoadId { get; set; }
        public string RoadName { get; set; }
        public string fid { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RoadNetworkMember> RoadNetworkMembers { get; set; }
    }
}
