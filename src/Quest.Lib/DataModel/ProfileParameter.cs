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
    
    public partial class ProfileParameter
    {
        public int ProfileParameterId { get; set; }
        public int ProfileId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public Nullable<int> ProfileParameterTypeId { get; set; }
    
        public virtual Profile Profile { get; set; }
        public virtual ProfileParameterType ProfileParameterType { get; set; }
    }
}
