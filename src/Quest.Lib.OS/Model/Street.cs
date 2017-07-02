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
    using System.Collections.Generic;
    
    public partial class Street
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Street()
        {
            this.LPIs = new HashSet<LPI>();
            this.StreetDescriptors = new HashSet<StreetDescriptor>();
        }
    
        public short RECORD_IDENTIFIER { get; set; }
        public string CHANGE_TYPE { get; set; }
        public int PRO_ORDER { get; set; }
        public int USRN { get; set; }
        public short RECORD_TYPE { get; set; }
        public short SWA_ORG_REF_NAMING { get; set; }
        public Nullable<short> STATE { get; set; }
        public Nullable<System.DateTime> STATE_DATE { get; set; }
        public Nullable<short> STREET_SURFACE { get; set; }
        public Nullable<short> STREET_CLASSIFICATION { get; set; }
        public short VERSION { get; set; }
        public System.DateTime STREET_START_DATE { get; set; }
        public Nullable<System.DateTime> STREET_END_DATE { get; set; }
        public System.DateTime LAST_UPDATE_DATE { get; set; }
        public System.DateTime RECORD_ENTRY_DATE { get; set; }
        public double STREET_START_X { get; set; }
        public double STREET_START_Y { get; set; }
        public double STREET_START_LAT { get; set; }
        public double STREET_START_LONG { get; set; }
        public double STREET_END_X { get; set; }
        public double STREET_END_Y { get; set; }
        public double STREET_END_LAT { get; set; }
        public double STREET_END_LONG { get; set; }
        public double STREET_TOLERANCE { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LPI> LPIs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StreetDescriptor> StreetDescriptors { get; set; }
    }
}
