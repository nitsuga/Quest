//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Quest.Lib.Simulation.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class OnsceneStat
    {
        public int CdfId { get; set; }
        public Nullable<int> cdfType { get; set; }
        public Nullable<int> hour { get; set; }
        public string vehicleType { get; set; }
        public string cdf { get; set; }
        public string ampds { get; set; }
        public Nullable<double> mean { get; set; }
        public Nullable<double> stddev { get; set; }
        public Nullable<int> count { get; set; }
        public Nullable<int> VehicleTypeId { get; set; }
    }
}