﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class QuestNLPGEntities : DbContext
    {
        public QuestNLPGEntities()
            : base("name=QuestNLPGEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<BLPU> BLPUs { get; set; }
        public virtual DbSet<Classification> Classifications { get; set; }
        public virtual DbSet<Classification_Fail> Classification_Fail { get; set; }
        public virtual DbSet<DPA> DPAs { get; set; }
        public virtual DbSet<LPI> LPIs { get; set; }
        public virtual DbSet<NLPG> NLPGs { get; set; }
        public virtual DbSet<Organisation> Organisations { get; set; }
        public virtual DbSet<Street> Streets { get; set; }
        public virtual DbSet<Successor> Successors { get; set; }
        public virtual DbSet<XREF> XREFs { get; set; }
        public virtual DbSet<StreetDescriptor> StreetDescriptors { get; set; }
    }
}
