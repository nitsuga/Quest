﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Quest.Lib.DataModel
{
    public partial class QuestContext : DbContext
    {
        public virtual DbSet<AspNetRoles> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUserClaims> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogins> AspNetUserLogins { get; set; }
        public virtual DbSet<AspNetUserRoles> AspNetUserRoles { get; set; }
        public virtual DbSet<AspNetUsers> AspNetUsers { get; set; }
        public virtual DbSet<Call> Call { get; set; }
        public virtual DbSet<Callsign> Callsign { get; set; }
        public virtual DbSet<CoverageMapDefinition> CoverageMapDefinition { get; set; }
        public virtual DbSet<CoverageMapStore> CoverageMapStore { get; set; }
        public virtual DbSet<Destinations> Destinations { get; set; }
        public virtual DbSet<DeviceRole> DeviceRole { get; set; }
        public virtual DbSet<Devices> Devices { get; set; }
        public virtual DbSet<Incident> Incident { get; set; }
        public virtual DbSet<JobTemplate> JobTemplate { get; set; }
        public virtual DbSet<MapOverlay> MapOverlay { get; set; }
        public virtual DbSet<MapOverlayItem> MapOverlayItem { get; set; }
        public virtual DbSet<MigrationHistory> MigrationHistory { get; set; }
        public virtual DbSet<Profile> Profile { get; set; }
        public virtual DbSet<ProfileParameter> ProfileParameter { get; set; }
        public virtual DbSet<ProfileParameterType> ProfileParameterType { get; set; }
        public virtual DbSet<Resource> Resource { get; set; }
        public virtual DbSet<ResourceArea> ResourceArea { get; set; }
        public virtual DbSet<ResourceStatus> ResourceStatus { get; set; }
        public virtual DbSet<ResourceStatusHistory> ResourceStatusHistory { get; set; }
        public virtual DbSet<ResourceType> ResourceType { get; set; }
        public virtual DbSet<RoadLinkEdge> RoadLinkEdge { get; set; }
        public virtual DbSet<RoadLinkEdgeLink> RoadLinkEdgeLink { get; set; }
        public virtual DbSet<RoadSpeed> RoadSpeed { get; set; }
        public virtual DbSet<RoadSpeedMatrixDoW> RoadSpeedMatrixDoW { get; set; }
        public virtual DbSet<RoadSpeedMatrixHoD> RoadSpeedMatrixHoD { get; set; }
        public virtual DbSet<RoadSpeedMatrixHoW> RoadSpeedMatrixHoW { get; set; }
        public virtual DbSet<RoadTypes> RoadTypes { get; set; }
        public virtual DbSet<SecuredItemLinks> SecuredItemLinks { get; set; }
        public virtual DbSet<SecuredItems> SecuredItems { get; set; }
        public virtual DbSet<StationCatchment> StationCatchment { get; set; }
        public virtual DbSet<Variable> Variable { get; set; }
        public virtual DbSet<Vehicle> Vehicle { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer(@"Server=localhost,999;Database=Quest;user=sa;pwd=M3Gurdy*");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AspNetRoles>(entity =>
            {
                entity.HasIndex(e => e.Name)
                    .HasName("RoleNameIndex")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasMaxLength(128)
                    .ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256);
            });

            modelBuilder.Entity<AspNetUserClaims>(entity =>
            {
                entity.HasIndex(e => e.UserId)
                    .HasName("IX_UserId");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserClaims)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId");
            });

            modelBuilder.Entity<AspNetUserLogins>(entity =>
            {
                entity.HasKey(e => new { e.LoginProvider, e.ProviderKey, e.UserId });

                entity.HasIndex(e => e.UserId)
                    .HasName("IX_UserId");

                entity.Property(e => e.LoginProvider).HasMaxLength(128);

                entity.Property(e => e.ProviderKey).HasMaxLength(128);

                entity.Property(e => e.UserId).HasMaxLength(128);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserLogins)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId");
            });

            modelBuilder.Entity<AspNetUserRoles>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId });

                entity.HasIndex(e => e.RoleId)
                    .HasName("IX_RoleId");

                entity.HasIndex(e => e.UserId)
                    .HasName("IX_UserId");

                entity.Property(e => e.UserId).HasMaxLength(128);

                entity.Property(e => e.RoleId).HasMaxLength(128);

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.AspNetUserRoles)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserRoles)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId");
            });

            modelBuilder.Entity<AspNetUsers>(entity =>
            {
                entity.HasIndex(e => e.UserName)
                    .HasName("UserNameIndex")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasMaxLength(128)
                    .ValueGeneratedNever();

                entity.Property(e => e.Email).HasMaxLength(256);

                entity.Property(e => e.LockoutEndDateUtc).HasColumnType("datetime");

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(256);
            });

            modelBuilder.Entity<Call>(entity =>
            {
                entity.Property(e => e.Address1)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.Address2)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.Address3)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.Address4)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.Address5)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.Address6)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.Event)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Extension)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TimeAnswered).HasColumnType("datetime");

                entity.Property(e => e.TimeClosed).HasColumnType("datetime");

                entity.Property(e => e.TimeConnected).HasColumnType("datetime");

                entity.Property(e => e.Updated).HasColumnType("datetime");
            });

            modelBuilder.Entity<Callsign>(entity =>
            {
                entity.HasIndex(e => new { e.CallsignId, e.Callsign1 })
                    .HasName("_dta_index_Callsign_11_661577395__K2_1");

                entity.Property(e => e.CallsignId).HasColumnName("CallsignID");

                entity.Property(e => e.Callsign1)
                    .HasColumnName("Callsign")
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CoverageMapDefinition>(entity =>
            {
                entity.Property(e => e.CoverageMapDefinitionId).ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.RoutingResource).HasMaxLength(50);

                entity.Property(e => e.StyleCode).HasMaxLength(50);

                entity.Property(e => e.VehicleCodes).HasMaxLength(250);
            });

            modelBuilder.Entity<CoverageMapStore>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Tstamp)
                    .HasColumnName("tstamp")
                    .HasColumnType("datetime");
            });

            modelBuilder.Entity<Destinations>(entity =>
            {
                entity.HasKey(e => e.DestinationId);

                entity.Property(e => e.DestinationId).HasColumnName("DestinationID");

                entity.Property(e => e.Destination)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Shortcode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Timestamp).HasColumnType("datetime");

                entity.Property(e => e.Wkt).IsUnicode(false);
            });

            modelBuilder.Entity<DeviceRole>(entity =>
            {
                entity.Property(e => e.DeviceRoleId)
                    .HasColumnName("DeviceRoleID")
                    .ValueGeneratedNever();

                entity.Property(e => e.DeviceRoleName).HasMaxLength(50);
            });

            modelBuilder.Entity<Devices>(entity =>
            {
                entity.HasKey(e => e.DeviceId);

                entity.Property(e => e.DeviceId).HasColumnName("DeviceID");

                entity.Property(e => e.AuthToken).IsUnicode(false);

                entity.Property(e => e.Destination)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.DeviceCallsign)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DeviceIdentity).IsUnicode(false);

                entity.Property(e => e.DeviceMake)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DeviceModel)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DeviceRoleId).HasColumnName("DeviceRoleID");

                entity.Property(e => e.Event)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.IsEnabled).HasColumnName("isEnabled");

                entity.Property(e => e.LastStatusUpdate).HasColumnType("datetime");

                entity.Property(e => e.LastUpdate).HasColumnType("datetime");

                entity.Property(e => e.LoggedOffTime).HasColumnType("datetime");

                entity.Property(e => e.LoggedOnTime).HasColumnType("datetime");

                entity.Property(e => e.NotificationId)
                    .HasColumnName("NotificationID")
                    .HasMaxLength(1024)
                    .IsUnicode(false);

                entity.Property(e => e.NotificationTypeId).HasColumnName("NotificationTypeID");

                entity.Property(e => e.Osversion)
                    .HasColumnName("OSVersion")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OwnerId)
                    .HasColumnName("OwnerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PrevStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ResourceId).HasColumnName("ResourceID");

                entity.Property(e => e.Road)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.Skill)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.HasOne(d => d.DeviceRole)
                    .WithMany(p => p.Devices)
                    .HasForeignKey(d => d.DeviceRoleId)
                    .HasConstraintName("FK_Devices_DeviceRole");

                entity.HasOne(d => d.Resource)
                    .WithMany(p => p.Devices)
                    .HasForeignKey(d => d.ResourceId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_Devices_Resource");

                entity.HasOne(d => d.ResourceStatus)
                    .WithMany(p => p.Devices)
                    .HasForeignKey(d => d.ResourceStatusId)
                    .HasConstraintName("FK_Devices_ResourceStatus");
            });

            modelBuilder.Entity<Incident>(entity =>
            {
                entity.HasIndex(e => e.Serial)
                    .HasName("NonClusteredIndex-20160719-103259");

                entity.Property(e => e.IncidentId).HasColumnName("IncidentID");

                entity.Property(e => e.Az)
                    .HasColumnName("AZ")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CallerTelephone)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Complaint)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Created).HasColumnType("datetime");

                entity.Property(e => e.Determinant)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DeterminantDescription).IsUnicode(false);

                entity.Property(e => e.DisconnectTime).HasColumnType("datetime");

                entity.Property(e => e.IncidentType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LastUpdated).HasColumnType("datetime");

                entity.Property(e => e.Location)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.LocationComment).IsUnicode(false);

                entity.Property(e => e.PatientAge)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PatientSex)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Priority)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ProblemDescription).IsUnicode(false);

                entity.Property(e => e.Sector)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Serial)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<JobTemplate>(entity =>
            {
                entity.Property(e => e.Group).HasMaxLength(50);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.Task).HasMaxLength(50);
            });

            modelBuilder.Entity<MapOverlay>(entity =>
            {
                entity.Property(e => e.MapOverlayId)
                    .HasColumnName("MapOverlayID")
                    .ValueGeneratedNever();

                entity.Property(e => e.OverlayName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Stroke)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<MapOverlayItem>(entity =>
            {
                entity.Property(e => e.MapOverlayItemId).ValueGeneratedNever();

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.FillColour)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Wkt).IsUnicode(false);

                entity.HasOne(d => d.MapOverlay)
                    .WithMany(p => p.MapOverlayItem)
                    .HasForeignKey(d => d.MapOverlayId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MapOverlayItem_MapOverlay");
            });

            modelBuilder.Entity<MigrationHistory>(entity =>
            {
                entity.HasKey(e => new { e.MigrationId, e.ContextKey });

                entity.ToTable("__MigrationHistory");

                entity.Property(e => e.MigrationId).HasMaxLength(150);

                entity.Property(e => e.ContextKey).HasMaxLength(300);

                entity.Property(e => e.Model).IsRequired();

                entity.Property(e => e.ProductVersion)
                    .IsRequired()
                    .HasMaxLength(32);
            });

            modelBuilder.Entity<Profile>(entity =>
            {
                entity.Property(e => e.ProfileId).ValueGeneratedNever();

                entity.Property(e => e.ProfileName).HasMaxLength(150);
            });

            modelBuilder.Entity<ProfileParameter>(entity =>
            {
                entity.Property(e => e.ProfileParameterId).ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Value).IsRequired();

                entity.HasOne(d => d.Profile)
                    .WithMany(p => p.ProfileParameter)
                    .HasForeignKey(d => d.ProfileId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProfileParameter_Profile");

                entity.HasOne(d => d.ProfileParameterType)
                    .WithMany(p => p.ProfileParameter)
                    .HasForeignKey(d => d.ProfileParameterTypeId)
                    .HasConstraintName("FK_ProfileParameter_ProfileParameterType");
            });

            modelBuilder.Entity<ProfileParameterType>(entity =>
            {
                entity.Property(e => e.ProfileParameterTypeId).ValueGeneratedNever();

                entity.Property(e => e.Name).HasMaxLength(50);
            });

            modelBuilder.Entity<Resource>(entity =>
            {
                entity.HasIndex(e => e.CallsignId)
                    .HasName("NonClusteredIndex-20160719-125115");

                entity.Property(e => e.ResourceId).HasColumnName("ResourceID");

                entity.Property(e => e.Agency)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CallsignId).HasColumnName("CallsignID");

                entity.Property(e => e.Class)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Comment).IsUnicode(false);

                entity.Property(e => e.Destination).IsUnicode(false);

                entity.Property(e => e.Emergency)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Eta)
                    .HasColumnName("ETA")
                    .HasColumnType("datetime");

                entity.Property(e => e.EventType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LastUpdated).HasColumnType("datetime");

                entity.Property(e => e.ResourceStatusId).HasColumnName("ResourceStatusID");

                entity.Property(e => e.ResourceStatusPrevId).HasColumnName("ResourceStatusPrevID");

                entity.Property(e => e.Road)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.Sector)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Serial)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Skill)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Callsign)
                    .WithMany(p => p.Resource)
                    .HasForeignKey(d => d.CallsignId)
                    .HasConstraintName("FK_Resource_Callsign");

                entity.HasOne(d => d.ResourceStatus)
                    .WithMany(p => p.ResourceResourceStatus)
                    .HasForeignKey(d => d.ResourceStatusId)
                    .HasConstraintName("FK_Resource_ResourceStatus");

                entity.HasOne(d => d.ResourceStatusPrev)
                    .WithMany(p => p.ResourceResourceStatusPrev)
                    .HasForeignKey(d => d.ResourceStatusPrevId)
                    .HasConstraintName("FK_Resource_ResourceStatus1");

                entity.HasOne(d => d.ResourceType)
                    .WithMany(p => p.Resource)
                    .HasForeignKey(d => d.ResourceTypeId)
                    .HasConstraintName("FK_Resource_ResourceType");
            });

            modelBuilder.Entity<ResourceArea>(entity =>
            {
                entity.HasKey(e => e.AreaId);

                entity.Property(e => e.AreaId).ValueGeneratedNever();

                entity.Property(e => e.Area).HasMaxLength(50);

                entity.Property(e => e.Wkt).IsUnicode(false);
            });

            modelBuilder.Entity<ResourceStatus>(entity =>
            {
                entity.HasIndex(e => e.Status)
                    .HasName("NonClusteredIndex-20141229-203340");

                entity.HasIndex(e => new { e.Available, e.Busy })
                    .HasName("NonClusteredIndex-20141215-164957");

                entity.Property(e => e.ResourceStatusId).HasColumnName("ResourceStatusID");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<ResourceStatusHistory>(entity =>
            {
                entity.Property(e => e.ResourceStatusHistoryId).HasColumnName("ResourceStatusHistoryID");

                entity.Property(e => e.ResourceId).HasColumnName("ResourceID");

                entity.Property(e => e.ResourceStatusId).HasColumnName("ResourceStatusID");

                entity.HasOne(d => d.Resource)
                    .WithMany(p => p.ResourceStatusHistory)
                    .HasForeignKey(d => d.ResourceId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_ResourceStatusHistory_Resource");

                entity.HasOne(d => d.ResourceStatus)
                    .WithMany(p => p.ResourceStatusHistory)
                    .HasForeignKey(d => d.ResourceStatusId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_ResourceStatusHistory_ResourceStatus");
            });

            modelBuilder.Entity<ResourceType>(entity =>
            {
                entity.Property(e => e.ResourceType1)
                    .HasColumnName("ResourceType")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ResourceTypeGroup)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<RoadLinkEdge>(entity =>
            {
                entity.Property(e => e.RoadLinkEdgeId).ValueGeneratedNever();

                entity.Property(e => e.RoadName)
                    .IsRequired()
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.Wkt)
                    .HasColumnName("WKT")
                    .IsUnicode(false);
            });

            modelBuilder.Entity<RoadSpeed>(entity =>
            {
                entity.HasIndex(e => e.RoadLinkEdgeId)
                    .HasName("NonClusteredIndex-20161222-131845");
            });

            modelBuilder.Entity<RoadSpeedMatrixDoW>(entity =>
            {
                entity.Property(e => e.RoadSpeedMatrixDoWid).HasColumnName("RoadSpeedMatrixDoWId");
            });

            modelBuilder.Entity<RoadSpeedMatrixHoD>(entity =>
            {
                entity.HasKey(e => e.RoadSpeedMatrixId);
            });

            modelBuilder.Entity<RoadSpeedMatrixHoW>(entity =>
            {
                entity.HasKey(e => e.RoadSpeedMatrixId);
            });

            modelBuilder.Entity<RoadTypes>(entity =>
            {
                entity.HasKey(e => e.RoadTypeId);

                entity.Property(e => e.RoadTypeId).ValueGeneratedNever();

                entity.Property(e => e.RoadType).HasMaxLength(100);
            });

            modelBuilder.Entity<SecuredItemLinks>(entity =>
            {
                entity.HasKey(e => e.SecuredItemLinkId);

                entity.Property(e => e.SecuredItemIdchild).HasColumnName("SecuredItemIDChild");

                entity.Property(e => e.SecuredItemIdparent).HasColumnName("SecuredItemIDParent");

                entity.HasOne(d => d.SecuredItemIdchildNavigation)
                    .WithMany(p => p.SecuredItemLinksSecuredItemIdchildNavigation)
                    .HasForeignKey(d => d.SecuredItemIdchild)
                    .HasConstraintName("FK_SecuredItemLinks_SecuredItems1");

                entity.HasOne(d => d.SecuredItemIdparentNavigation)
                    .WithMany(p => p.SecuredItemLinksSecuredItemIdparentNavigation)
                    .HasForeignKey(d => d.SecuredItemIdparent)
                    .HasConstraintName("FK_SecuredItemLinks_SecuredItems");
            });

            modelBuilder.Entity<SecuredItems>(entity =>
            {
                entity.HasKey(e => e.SecuredItemId);

                entity.Property(e => e.SecuredItemId).HasColumnName("SecuredItemID");

                entity.Property(e => e.SecuredItemName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<StationCatchment>(entity =>
            {
                entity.Property(e => e.StationCatchmentId).ValueGeneratedNever();

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

                entity.Property(e => e.Wkt).IsUnicode(false);
            });

            modelBuilder.Entity<Variable>(entity =>
            {
                entity.Property(e => e.VariableId).HasColumnName("VariableID");

                entity.Property(e => e.Description).HasMaxLength(250);

                entity.Property(e => e.Type).HasMaxLength(50);

                entity.Property(e => e.Variable1)
                    .HasColumnName("Variable")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.Property(e => e.Vehicle1)
                    .HasColumnName("Vehicle")
                    .HasMaxLength(50);
            });

            modelBuilder.HasSequence("RevisionSequence");
        }
    }
}