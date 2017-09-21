using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Quest.Lib.Simulation.DataModelSim
{
    public partial class QuestSimContext : DbContext
    {
        public virtual DbSet<Ampdscodes> Ampdscodes { get; set; }
        public virtual DbSet<CadLinkIncidents> CadLinkIncidents { get; set; }
        public virtual DbSet<Coverage> Coverage { get; set; }
        public virtual DbSet<Destinations> Destinations { get; set; }
        public virtual DbSet<Determinants> Determinants { get; set; }
        public virtual DbSet<Profile> Profile { get; set; }
        public virtual DbSet<ProfileParameter> ProfileParameter { get; set; }
        public virtual DbSet<ProfileParameterType> ProfileParameterType { get; set; }
        public virtual DbSet<Rosters> Rosters { get; set; }
        public virtual DbSet<SimulationAssignments> SimulationAssignments { get; set; }
        public virtual DbSet<SimulationIncidents> SimulationIncidents { get; set; }
        public virtual DbSet<SimulationOrcon> SimulationOrcon { get; set; }
        public virtual DbSet<SimulationResults> SimulationResults { get; set; }
        public virtual DbSet<SimulationRun> SimulationRun { get; set; }
        public virtual DbSet<SimulationStats> SimulationStats { get; set; }
        public virtual DbSet<StationCatchment> StationCatchment { get; set; }
        public virtual DbSet<Vehicles> Vehicles { get; set; }
        public virtual DbSet<VehicleTypes> VehicleTypes { get; set; }

        // Unable to generate entity type for table 'dbo.IncidentType'. Please see the warning messages.
        // Unable to generate entity type for table 'dbo.VehicleRoster'. Please see the warning messages.
        // Unable to generate entity type for table 'dbo.ResponseProfiles'. Please see the warning messages.
        // Unable to generate entity type for table 'dbo.freda_determinants'. Please see the warning messages.
        // Unable to generate entity type for table 'dbo.Personnel'. Please see the warning messages.
        // Unable to generate entity type for table 'dbo.OnsceneStats'. Please see the warning messages.

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer(@"Server=localhost,999;Database=QuestSim;user=sa;pwd=M3Gurdy*");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Ampdscodes>(entity =>
            {
                entity.HasKey(e => e.AmpdsKey);

                entity.ToTable("AMPDScodes");

                entity.HasIndex(e => new { e.Ampdscode, e.Version })
                    .HasName("IX_ampdscodes");

                entity.Property(e => e.AmpdsKey)
                    .HasColumnName("ampdsKey")
                    .ValueGeneratedNever();

                entity.Property(e => e.Ampdscode)
                    .IsRequired()
                    .HasColumnName("ampdscode")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Category)
                    .HasColumnName("category")
                    .HasColumnType("char(2)");

                entity.Property(e => e.Chiefcomplaint)
                    .IsRequired()
                    .HasColumnName("chiefcomplaint")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Chiefcomplaintcode).HasColumnName("chiefcomplaintcode");

                entity.Property(e => e.DefaultDohCategory).HasColumnType("char(1)");

                entity.Property(e => e.DefaultDohSubcategory).HasColumnType("char(2)");

                entity.Property(e => e.DefaultDohSubcategoryFull)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.DefaultLascategory)
                    .HasColumnName("DefaultLASCategory")
                    .HasColumnType("char(2)");

                entity.Property(e => e.DefaultLascategoryFull)
                    .HasColumnName("DefaultLASCategoryFull")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasColumnName("description")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.Enddate)
                    .HasColumnName("enddate")
                    .HasColumnType("datetime");

                entity.Property(e => e.Startdate)
                    .HasColumnName("startdate")
                    .HasColumnType("datetime");

                entity.Property(e => e.Version).HasColumnName("version");
            });

            modelBuilder.Entity<CadLinkIncidents>(entity =>
            {
                entity.HasKey(e => e.CadLinkId);

                entity.HasIndex(e => e.Timestamp)
                    .HasName("tstamp_index");

                entity.HasIndex(e => e.XmlData)
                    .HasName("ix_xml");

                entity.Property(e => e.Timestamp)
                    .HasColumnName("timestamp")
                    .HasColumnType("datetime");

                entity.Property(e => e.XmlData)
                    .IsRequired()
                    .HasColumnType("xml");
            });

            modelBuilder.Entity<Coverage>(entity =>
            {
                entity.Property(e => e.CoverageMap).IsRequired();

                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<Destinations>(entity =>
            {
                entity.HasKey(e => e.DestinationId);

                entity.Property(e => e.Destination)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.E).HasColumnName("e");

                entity.Property(e => e.GroupId).HasDefaultValueSql("((0))");

                entity.Property(e => e.N).HasColumnName("n");
            });

            modelBuilder.Entity<Determinants>(entity =>
            {
                entity.HasKey(e => e.DeterminantId)
                    .ForSqlServerIsClustered(false);

                entity.HasIndex(e => e.Determinant)
                    .HasName("ix-determinant")
                    .IsUnique()
                    .ForSqlServerIsClustered();

                entity.Property(e => e.AllResponders)
                    .HasColumnName("all_responders")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Ambulances)
                    .HasColumnName("ambulances")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Comresp)
                    .HasColumnName("comresp")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Determinant)
                    .HasColumnName("determinant")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Ecps)
                    .HasColumnName("ecps")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Paramedics)
                    .HasColumnName("paramedics")
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Profile>(entity =>
            {
                entity.Property(e => e.ProfileName).HasMaxLength(150);
            });

            modelBuilder.Entity<ProfileParameter>(entity =>
            {
                entity.HasIndex(e => e.ProfileId)
                    .HasName("IX_ProfileParameter");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Value).IsRequired();

                entity.HasOne(d => d.Profile)
                    .WithMany(p => p.ProfileParameter)
                    .HasForeignKey(d => d.ProfileId)
                    .HasConstraintName("FK_ProfileParameter_Profile");

                entity.HasOne(d => d.ProfileParameterType)
                    .WithMany(p => p.ProfileParameter)
                    .HasForeignKey(d => d.ProfileParameterTypeId)
                    .HasConstraintName("FK_ProfileParameter_ProfileParameterType");
            });

            modelBuilder.Entity<ProfileParameterType>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(50);
            });

            modelBuilder.Entity<Rosters>(entity =>
            {
                entity.HasKey(e => e.RosterId);

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.RosterPattern).IsRequired();
            });

            modelBuilder.Entity<SimulationAssignments>(entity =>
            {
                entity.HasKey(e => e.SimulationAssignments1);

                entity.Property(e => e.SimulationAssignments1).HasColumnName("SimulationAssignments");

                entity.Property(e => e.Action).HasMaxLength(150);

                entity.Property(e => e.Callsign).HasMaxLength(150);

                entity.Property(e => e.Incident).HasMaxLength(150);

                entity.Property(e => e.Tstamp)
                    .HasColumnName("tstamp")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.SimulationRun)
                    .WithMany(p => p.SimulationAssignments)
                    .HasForeignKey(d => d.SimulationRunId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_SimulationAssignments_SimulationRun");
            });

            modelBuilder.Entity<SimulationIncidents>(entity =>
            {
                entity.HasKey(e => e.IncidentId);

                entity.HasIndex(e => e.CallStart)
                    .HasName("IX_Incidents");

                entity.Property(e => e.IncidentId).ValueGeneratedNever();

                entity.Property(e => e.Ampdscode)
                    .HasColumnName("AMPDSCode")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Ampdstime)
                    .HasColumnName("AMPDSTime")
                    .HasColumnType("datetime");

                entity.Property(e => e.CallStart).HasColumnType("datetime");

                entity.Property(e => e.OutsideLas).HasColumnName("OutsideLAS");
            });

            modelBuilder.Entity<SimulationOrcon>(entity =>
            {
                entity.Property(e => e.CatAinside).HasColumnName("CatAInside");

                entity.Property(e => e.CatAoutside).HasColumnName("catAOutside");

                entity.Property(e => e.CatBinside).HasColumnName("CatBInside");

                entity.Property(e => e.CatBoutside).HasColumnName("CatBOutside");

                entity.HasOne(d => d.SimulationRun)
                    .WithMany(p => p.SimulationOrcon)
                    .HasForeignKey(d => d.SimulationRunId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_SimulationOrcon_SimulationRun1");
            });

            modelBuilder.Entity<SimulationResults>(entity =>
            {
                entity.HasKey(e => e.SimulationResultId);

                entity.HasIndex(e => e.Incidentid)
                    .HasName("ix_incident");

                entity.Property(e => e.CallStart).HasColumnType("datetime");

                entity.Property(e => e.Closed).HasColumnType("datetime");

                entity.Property(e => e.Frdelay).HasColumnName("FRDelay");

                entity.Property(e => e.FrresourceId).HasColumnName("FRResourceId");

                entity.HasOne(d => d.AmbResource)
                    .WithMany(p => p.SimulationResultsAmbResource)
                    .HasForeignKey(d => d.AmbResourceId)
                    .HasConstraintName("FK_SimulationResults_Vehicles1");

                entity.HasOne(d => d.Frresource)
                    .WithMany(p => p.SimulationResultsFrresource)
                    .HasForeignKey(d => d.FrresourceId)
                    .HasConstraintName("FK_SimulationResults_Vehicles");

                entity.HasOne(d => d.Incident)
                    .WithMany(p => p.SimulationResults)
                    .HasForeignKey(d => d.Incidentid)
                    .HasConstraintName("FK_SimulationResults_SimulationIncidents");

                entity.HasOne(d => d.SimulationRun)
                    .WithMany(p => p.SimulationResults)
                    .HasForeignKey(d => d.SimulationRunId)
                    .HasConstraintName("FK_SimulationResults_SimulationRun1");
            });

            modelBuilder.Entity<SimulationRun>(entity =>
            {
                entity.Property(e => e.Constants).HasMaxLength(250);

                entity.Property(e => e.Notes).HasMaxLength(250);

                entity.Property(e => e.Started).HasColumnType("datetime");

                entity.Property(e => e.Stopped).HasColumnType("datetime");
            });

            modelBuilder.Entity<SimulationStats>(entity =>
            {
                entity.Property(e => e.Status).HasMaxLength(50);

                entity.Property(e => e.Timestamp).HasColumnType("datetime");

                entity.HasOne(d => d.SimulationRun)
                    .WithMany(p => p.SimulationStats)
                    .HasForeignKey(d => d.SimulationRunId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_SimulationStats_SimulationRun");
            });

            modelBuilder.Entity<StationCatchment>(entity =>
            {
                entity.Property(e => e.Area)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Code)
                    .HasColumnName("code")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Complex)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Fid)
                    .HasColumnName("FID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.StationName)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Vehicles>(entity =>
            {
                entity.HasKey(e => e.VehicleId);

                entity.Property(e => e.Callsign).HasMaxLength(50);

                entity.Property(e => e.GroupId).HasDefaultValueSql("((0))");

                entity.Property(e => e.Mpcname)
                    .HasColumnName("MPCName")
                    .HasMaxLength(50);

                entity.HasOne(d => d.DefaultDestination)
                    .WithMany(p => p.Vehicles)
                    .HasForeignKey(d => d.DefaultDestinationId)
                    .HasConstraintName("FK_Vehicles_Destinations");
            });

            modelBuilder.Entity<VehicleTypes>(entity =>
            {
                entity.HasKey(e => e.VehicleTypeId);

                entity.Property(e => e.VehicleTypeId).ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });
        }
    }
}
