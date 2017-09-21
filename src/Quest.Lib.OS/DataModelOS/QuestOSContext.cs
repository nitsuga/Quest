using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Quest.Lib.OS.DataModelOS
{
    public partial class QuestOSContext : DbContext
    {
        public virtual DbSet<Junctions> Junctions { get; set; }
        public virtual DbSet<Paf> Paf { get; set; }
        public virtual DbSet<Road> Road { get; set; }
        public virtual DbSet<RoadLink> RoadLink { get; set; }
        public virtual DbSet<StaticRoadNames> StaticRoadNames { get; set; }
        public virtual DbSet<RoadNetworkMember> RoadNetworkMember { get; set; }
        public virtual DbSet<RoadNode> RoadNode { get; set; }
        public virtual DbSet<RoadRouteInfo> RoadRouteInfo { get; set; }
        public virtual DbSet<RoadTypes> RoadTypes { get; set; }
        public virtual DbSet<StaticRoadLinks> StaticRoadLinks { get; set; }
        public virtual DbSet<StaticRoadNode> StaticRoadNode { get; set; }

        // Unable to generate entity type for table 'dbo.RoadNames'. Please see the warning messages.
        // Unable to generate entity type for table 'dbo.StaticRoadLinksGeom'. Please see the warning messages.

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer(@"Server=localhost,999;Database=QuestOS;user=sa;pwd=M3Gurdy*");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Junctions>(entity =>
            {
                entity.HasKey(e => e.JunctionId);

                entity.Property(e => e.JunctionId).HasColumnName("JunctionID");

                entity.Property(e => e.R1)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.R2)
                    .IsRequired()
                    .HasMaxLength(200);
            });

            modelBuilder.Entity<Paf>(entity =>
            {
                entity.ToTable("PAF");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.BuildingName)
                    .HasColumnName("BUILDING_NAME")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.BuildingNumber).HasColumnName("BUILDING_NUMBER");

                entity.Property(e => e.ChangeType)
                    .IsRequired()
                    .HasColumnName("CHANGE_TYPE")
                    .HasMaxLength(1)
                    .IsUnicode(false);

                entity.Property(e => e.DepartmentName)
                    .HasColumnName("DEPARTMENT_NAME")
                    .HasMaxLength(60)
                    .IsUnicode(false);

                entity.Property(e => e.DependentLocality)
                    .HasColumnName("DEPENDENT_LOCALITY")
                    .HasMaxLength(35)
                    .IsUnicode(false);

                entity.Property(e => e.DependentThoroughfare)
                    .HasColumnName("DEPENDENT_THOROUGHFARE")
                    .HasMaxLength(80)
                    .IsUnicode(false);

                entity.Property(e => e.DoubleDependentLocality)
                    .HasColumnName("DOUBLE_DEPENDENT_LOCALITY")
                    .HasMaxLength(35)
                    .IsUnicode(false);

                entity.Property(e => e.EndDate)
                    .HasColumnName("END_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.EntryDate)
                    .HasColumnName("ENTRY_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.Fulladdress)
                    .HasColumnName("FULLADDRESS")
                    .HasMaxLength(512)
                    .IsUnicode(false);

                entity.Property(e => e.LastUpdateDate)
                    .HasColumnName("LAST_UPDATE_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.OrganisationName)
                    .HasColumnName("ORGANISATION_NAME")
                    .HasMaxLength(60)
                    .IsUnicode(false);

                entity.Property(e => e.ParentAddressableUprn).HasColumnName("PARENT_ADDRESSABLE_UPRN");

                entity.Property(e => e.PoBoxNumber)
                    .HasColumnName("PO_BOX_NUMBER")
                    .HasMaxLength(6)
                    .IsUnicode(false);

                entity.Property(e => e.PostTown)
                    .IsRequired()
                    .HasColumnName("POST_TOWN")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Postcode)
                    .IsRequired()
                    .HasColumnName("POSTCODE")
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.Property(e => e.PostcodeType)
                    .IsRequired()
                    .HasColumnName("POSTCODE_TYPE")
                    .HasMaxLength(1)
                    .IsUnicode(false);

                entity.Property(e => e.ProOrder).HasColumnName("PRO_ORDER");

                entity.Property(e => e.ProcessDate)
                    .HasColumnName("PROCESS_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.RecordIdentifier).HasColumnName("RECORD_IDENTIFIER");

                entity.Property(e => e.RmUprn).HasColumnName("RM_UPRN");

                entity.Property(e => e.StartDate)
                    .HasColumnName("START_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.SubBuildingName)
                    .HasColumnName("SUB_BUILDING_NAME")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Thoroughfare)
                    .HasColumnName("THOROUGHFARE")
                    .HasMaxLength(80)
                    .IsUnicode(false);

                entity.Property(e => e.Uprn).HasColumnName("UPRN");

                entity.Property(e => e.Usrn).HasColumnName("USRN");

                entity.Property(e => e.WelshDependentLocality)
                    .HasColumnName("WELSH_DEPENDENT_LOCALITY")
                    .HasMaxLength(35)
                    .IsUnicode(false);

                entity.Property(e => e.WelshDependentThoroughfare)
                    .HasColumnName("WELSH_DEPENDENT_THOROUGHFARE")
                    .HasMaxLength(80)
                    .IsUnicode(false);

                entity.Property(e => e.WelshDoubleDependentLocality)
                    .HasColumnName("WELSH_DOUBLE_DEPENDENT_LOCALITY")
                    .HasMaxLength(35)
                    .IsUnicode(false);

                entity.Property(e => e.WelshPostTown)
                    .HasColumnName("WELSH_POST_TOWN")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.WelshThoroughfare)
                    .HasColumnName("WELSH_THOROUGHFARE")
                    .HasMaxLength(80)
                    .IsUnicode(false);

                entity.Property(e => e.XCoordinate).HasColumnName("X_COORDINATE");

                entity.Property(e => e.YCoordinate).HasColumnName("Y_COORDINATE");
            });

            modelBuilder.Entity<Road>(entity =>
            {
                entity.HasIndex(e => e.Fid)
                    .HasName("idx_Road_1")
                    .IsUnique();

                entity.Property(e => e.Fid)
                    .HasColumnName("fid")
                    .HasColumnType("char(50)");

                entity.Property(e => e.RoadName)
                    .HasMaxLength(150)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<RoadLink>(entity =>
            {
                entity.HasIndex(e => e.Include)
                    .HasName("idx_RoadLink_1");

                entity.HasIndex(e => new { e.RoadLinkId, e.Fid })
                    .HasName("idx_RoadLink_5")
                    .IsUnique();

                entity.HasIndex(e => new { e.RoadLinkId, e.FromRoadNodeId })
                    .HasName("RoadlinkFromId");

                entity.HasIndex(e => new { e.RoadLinkId, e.ToFid })
                    .HasName("idx_RoadLink_3");

                entity.HasIndex(e => new { e.RoadLinkId, e.FromRoadNodeId, e.FromFid })
                    .HasName("idx_RoadLink_4");

                entity.Property(e => e.RoadLinkId).ValueGeneratedOnAdd();

                entity.Property(e => e.Fid)
                    .HasColumnName("fid")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.FromFid)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.NatureOfRoad)
                    .HasColumnName("natureOfRoad")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.RoadType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ToFid)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Wkt)
                    .HasColumnName("WKT")
                    .IsUnicode(false);

                entity.HasOne(d => d.FromRoadNode)
                    .WithMany(p => p.RoadLink)
                    .HasForeignKey(d => d.FromRoadNodeId)
                    .HasConstraintName("FK_RoadLink_RoadNode");

                entity.HasOne(d => d.RoadLinkNavigation)
                    .WithOne(p => p.InverseRoadLinkNavigation)
                    .HasForeignKey<RoadLink>(d => d.RoadLinkId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RoadLink_RoadLink");
            });

            modelBuilder.Entity<StaticRoadNames>(entity =>
            {
                entity.HasKey(e => e.RoadNetworkMemberId);

                entity.Property(e => e.RoadNetworkMemberId).ValueGeneratedNever();

                entity.Property(e => e.RoadName)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.Wkt).IsUnicode(false);
            });

            modelBuilder.Entity<RoadNetworkMember>(entity =>
            {
                entity.HasIndex(e => new { e.RoadNetworkMemberId, e.NetworkFid })
                    .HasName("idx_RoadNetworkMember_2");

                entity.HasIndex(e => new { e.RoadNetworkMemberId, e.RoadFid })
                    .HasName("idx_RoadNetworkMember_1");

                entity.Property(e => e.NetworkFid)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.RoadFid)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Road)
                    .WithMany(p => p.RoadNetworkMember)
                    .HasForeignKey(d => d.RoadId)
                    .HasConstraintName("FK_RoadNetworkMember_Road");
            });

            modelBuilder.Entity<RoadNode>(entity =>
            {
                entity.HasIndex(e => e.Fid)
                    .HasName("idx_RoadNode_3")
                    .IsUnique();

                entity.HasIndex(e => e.Include)
                    .HasName("idx_RoadNode_1");

                entity.Property(e => e.Fid)
                    .HasColumnName("fid")
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<RoadRouteInfo>(entity =>
            {
                entity.HasIndex(e => e.Fid)
                    .HasName("idx_RoadRouteInfo_1")
                    .IsUnique();

                entity.Property(e => e.Fid)
                    .HasColumnName("fid")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.FromFid)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Instruction)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ToFid)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.EndRoadLink)
                    .WithMany(p => p.RoadRouteInfoEndRoadLink)
                    .HasForeignKey(d => d.EndRoadLinkId)
                    .HasConstraintName("FK_RoadRouteInfo_RoadLink1");

                entity.HasOne(d => d.StartRoadLink)
                    .WithMany(p => p.RoadRouteInfoStartRoadLink)
                    .HasForeignKey(d => d.StartRoadLinkId)
                    .HasConstraintName("FK_RoadRouteInfo_RoadLink");
            });

            modelBuilder.Entity<RoadTypes>(entity =>
            {
                entity.HasKey(e => e.RoadTypeId);

                entity.Property(e => e.RoadTypeId).ValueGeneratedNever();

                entity.Property(e => e.RoadType).HasMaxLength(100);
            });

            modelBuilder.Entity<StaticRoadLinks>(entity =>
            {
                entity.HasKey(e => e.RoadLinkId);

                entity.HasIndex(e => e.RoadLinkId)
                    .HasName("idx_StaticRoadLinks_1");

                entity.Property(e => e.RoadLinkId).ValueGeneratedNever();

                entity.Property(e => e.RoadName)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.Wkt)
                    .HasColumnName("WKT")
                    .IsUnicode(false);

                entity.HasOne(d => d.FromRoadNode)
                    .WithMany(p => p.StaticRoadLinksFromRoadNode)
                    .HasForeignKey(d => d.FromRoadNodeId)
                    .HasConstraintName("FK_StaticRoadLinks_RoadNode");

                entity.HasOne(d => d.RoadType)
                    .WithMany(p => p.StaticRoadLinks)
                    .HasForeignKey(d => d.RoadTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_StaticRoadLinks_RoadTypes");

                entity.HasOne(d => d.ToRoadNode)
                    .WithMany(p => p.StaticRoadLinksToRoadNode)
                    .HasForeignKey(d => d.ToRoadNodeId)
                    .HasConstraintName("FK_StaticRoadLinks_RoadNode1");
            });

            modelBuilder.Entity<StaticRoadNode>(entity =>
            {
                entity.HasKey(e => e.RoadNodeId);

                entity.Property(e => e.RoadNodeId).ValueGeneratedNever();
            });

            modelBuilder.HasSequence("RevisionSequence");
        }
    }
}
