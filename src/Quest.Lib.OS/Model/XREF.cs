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
    
    public partial class XREF
    {
        public short RECORD_IDENTIFIER { get; set; }
        public string CHANGE_TYPE { get; set; }
        public int PRO_ORDER { get; set; }
        public long UPRN { get; set; }
        public string XREF_KEY { get; set; }
        public string CROSS_REFERENCE { get; set; }
        public Nullable<int> VERSION { get; set; }
        public string SOURCE { get; set; }
        public System.DateTime START_DATE { get; set; }
        public Nullable<System.DateTime> END_DATE { get; set; }
        public System.DateTime LAST_UPDATE_DATE { get; set; }
        public System.DateTime ENTRY_DATE { get; set; }
    
        public virtual BLPU BLPU { get; set; }
    }
}
