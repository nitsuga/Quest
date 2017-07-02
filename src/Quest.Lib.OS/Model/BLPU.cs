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
    
    public partial class BLPU
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public BLPU()
        {
            this.Classifications = new HashSet<Classification>();
            this.LPIs = new HashSet<LPI>();
            this.Organisations = new HashSet<Organisation>();
            this.Successors = new HashSet<Successor>();
            this.XREFs = new HashSet<XREF>();
        }
    
        public short RECORD_IDENTIFIER { get; set; }
        public string CHANGE_TYPE { get; set; }
        public int PRO_ORDER { get; set; }
        public long UPRN { get; set; }
        public short LOGICAL_STATUS { get; set; }
        public Nullable<short> BLPU_STATE { get; set; }
        public Nullable<System.DateTime> BLPU_STATE_DATE { get; set; }
        public Nullable<long> PARENT_UPRN { get; set; }
        public double X_COORDINATE { get; set; }
        public double Y_COORDINATE { get; set; }
        public int RPC { get; set; }
        public short LOCAL_CUSTODIAN_CODE { get; set; }
        public System.DateTime START_DATE { get; set; }
        public Nullable<System.DateTime> END_DATE { get; set; }
        public System.DateTime LAST_UPDATE_DATE { get; set; }
        public System.DateTime ENTRY_DATE { get; set; }
        public string POSTAL_ADDRESS { get; set; }
        public string POSTCODE_LOCATOR { get; set; }
        public int MULTI_OCC_COUNT { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Classification> Classifications { get; set; }
        public virtual DPA DPA { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LPI> LPIs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Organisation> Organisations { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Successor> Successors { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<XREF> XREFs { get; set; }
    }
}
