using Microsoft.EntityFrameworkCore;

namespace Quest.Lib.Research.DataModelResearch
{
    public partial class QuestDataContext : DbContext
    {
        public virtual DbSet<Activations> Activations { get; set; }
        public virtual DbSet<Avls> Avls { get; set; }
        public virtual DbSet<AvlsRoad> AvlsRoad { get; set; }
        public virtual DbSet<IncidentRouteEstimate> IncidentRouteEstimate { get; set; }
        public virtual DbSet<IncidentRouteRun> IncidentRouteRun { get; set; }
        public virtual DbSet<IncidentRoutes> IncidentRoutes { get; set; }
        public virtual DbSet<Incidents> Incidents { get; set; }
        public virtual DbSet<RoadSpeed> RoadSpeed { get; set; }
        public virtual DbSet<RoadSpeedItem> RoadSpeedItem { get; set; }
        public virtual DbSet<RoadSpeedMatrixHoD> RoadSpeedMatrixHoD { get; set; }
        public virtual DbSet<RoadSpeedMatrixHoW> RoadSpeedMatrixHoW { get; set; }

        // Unable to generate entity type for table 'dbo.IncidentRouteRoadSpeedsData'. Please see the warning messages.

        public QuestDataContext(DbContextOptions<QuestDataContext> options) : base(options)
        {
        }

        public QuestDataContext()
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Activations>(entity =>
            {
                entity.HasKey(e => e.ActivationId);

                entity.Property(e => e.Arrived).HasColumnType("datetime");

                entity.Property(e => e.Callsign).HasColumnType("char(8)");

                entity.Property(e => e.Dispatched).HasColumnType("datetime");
            });

            modelBuilder.Entity<Avls>(entity =>
            {
                entity.HasKey(e => e.RawAvlsId);

                entity.HasIndex(e => e.AvlsDateTime)
                    .HasName("NonClusteredIndex-20170627-174826");

                entity.HasIndex(e => new { e.IncidentId, e.Process })
                    .HasName("IX_Avls");

                entity.Property(e => e.AvlsDateTime).HasColumnType("datetime");

                entity.Property(e => e.Callsign).HasColumnType("char(8)");

                entity.Property(e => e.Category).HasColumnType("char(1)");

                entity.Property(e => e.LocationX).HasColumnType("decimal(25, 20)");

                entity.Property(e => e.LocationY).HasColumnType("decimal(25, 20)");

                entity.Property(e => e.Process).HasDefaultValueSql("((0))");

                entity.Property(e => e.Status).HasColumnType("char(8)");
            });

            modelBuilder.Entity<IncidentRouteEstimate>(entity =>
            {
                entity.HasIndex(e => new { e.RoutingMethod, e.IncidentRouteId })
                    .HasName("IX_IncidentRouteEstimate");

                entity.HasOne(d => d.IncidentRoute)
                    .WithMany(p => p.IncidentRouteEstimate)
                    .HasForeignKey(d => d.IncidentRouteId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_IncidentRouteEstimate_IncidentRoutes");
            });

            modelBuilder.Entity<IncidentRouteRun>(entity =>
            {
                entity.Property(e => e.Parameters)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.Timestamp).HasColumnType("datetime");
            });

            modelBuilder.Entity<IncidentRoutes>(entity =>
            {
                entity.HasKey(e => e.IncidentRouteId);

                entity.HasIndex(e => new { e.IncidentRouteId, e.Scanned })
                    .HasName("NonClusteredIndex-20160116-143041");

                entity.HasIndex(e => new { e.IncidentId, e.Callsign, e.StartTime, e.EndTime })
                    .HasName("NonClusteredIndex-20160219-153637");

                entity.Property(e => e.IncidentRouteId).HasColumnName("IncidentRouteID");

                entity.Property(e => e.Callsign).HasColumnType("char(8)");

                entity.Property(e => e.EndTime).HasColumnType("datetime");

                entity.Property(e => e.IsBadGps).HasColumnName("IsBadGPS");

                entity.Property(e => e.Scanned).HasDefaultValueSql("((0))");

                entity.Property(e => e.StartTime).HasColumnType("datetime");
            });

            modelBuilder.Entity<Incidents>(entity =>
            {
                entity.HasKey(e => e.Cadref)
                    .ForSqlServerIsClustered(false);

                entity.HasIndex(e => e.Cadref)
                    .HasName("ClusteredIndex-20170112-163941")
                    .ForSqlServerIsClustered();

                entity.Property(e => e.Cadref)
                    .HasColumnName("cadref")
                    .ValueGeneratedNever();

                entity.Property(e => e.Ampds)
                    .HasColumnName("AMPDS")
                    .HasMaxLength(16)
                    .IsUnicode(false);

                entity.Property(e => e.Area)
                    .HasColumnName("area")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Athospital)
                    .HasColumnName("athospital")
                    .HasColumnType("datetime");

                entity.Property(e => e.Callstart)
                    .HasColumnName("callstart")
                    .HasColumnType("datetime");

                entity.Property(e => e.Complaint)
                    .HasColumnName("complaint")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Dohcategory)
                    .HasColumnName("dohcategory")
                    .HasMaxLength(1)
                    .IsUnicode(false);

                entity.Property(e => e.Dohsubcat)
                    .HasColumnName("dohsubcat")
                    .HasMaxLength(2)
                    .IsUnicode(false);

                entity.Property(e => e.Duration).HasColumnName("duration");

                entity.Property(e => e.Firstarrival)
                    .HasColumnName("firstarrival")
                    .HasColumnType("datetime");

                entity.Property(e => e.Firstdispatch)
                    .HasColumnName("firstdispatch")
                    .HasColumnType("datetime");

                entity.Property(e => e.Hospital)
                    .HasColumnName("hospital")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.IncidentDate).HasColumnType("datetime");

                entity.Property(e => e.Lascat)
                    .HasColumnName("lascat")
                    .HasMaxLength(2)
                    .IsUnicode(false);

                entity.Property(e => e.Postcode)
                    .HasColumnName("postcode")
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<RoadSpeed>(entity =>
            {
                entity.HasIndex(e => e.RoadLinkEdgeId)
                    .HasName("NonClusteredIndex-20170126-202534");
            });

            modelBuilder.Entity<RoadSpeedItem>(entity =>
            {
                entity.HasIndex(e => e.RoadLinkEdgeId)
                    .HasName("RoadspeedItem_idx2");

                entity.HasIndex(e => new { e.DateTime, e.RoadLinkEdgeId, e.IncidentRouteId, e.Speed })
                    .HasName("RoadspeedItem_idx1");

                entity.HasIndex(e => new { e.IncidentRouteId, e.DateTime, e.RoadLinkEdgeId, e.Speed })
                    .HasName("RoadspeedItem_idx3");

                entity.Property(e => e.DateTime).HasColumnType("datetime");

                entity.HasOne(d => d.IncidentRoute)
                    .WithMany(p => p.RoadSpeedItem)
                    .HasForeignKey(d => d.IncidentRouteId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RoadSpeedItem_IncidentRoutes1");
            });

            modelBuilder.Entity<RoadSpeedMatrixHoD>(entity =>
            {
                entity.HasKey(e => e.RoadSpeedMatrixId);
            });

            modelBuilder.Entity<RoadSpeedMatrixHoW>(entity =>
            {
                entity.HasKey(e => e.RoadSpeedMatrixId);
            });

            modelBuilder.HasSequence("RevisionSequence");
        }
    }
}
