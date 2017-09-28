using Microsoft.EntityFrameworkCore;

namespace Quest.Lib.OS.DataModelNLPG
{
    public partial class QuestNLPGContext : DbContext
    {
        public virtual DbSet<Blpu> Blpu { get; set; }
        public virtual DbSet<Classification> Classification { get; set; }
        public virtual DbSet<ClassificationFail> ClassificationFail { get; set; }
        public virtual DbSet<Dpa> Dpa { get; set; }
        public virtual DbSet<Lpi> Lpi { get; set; }
        public virtual DbSet<Nlpg> Nlpg { get; set; }
        public virtual DbSet<Organisation> Organisation { get; set; }
        public virtual DbSet<Street> Street { get; set; }
        public virtual DbSet<Successor> Successor { get; set; }
        public virtual DbSet<Xref> Xref { get; set; }

        // Unable to generate entity type for table 'NLPG.Metadata'. Please see the warning messages.
        // Unable to generate entity type for table 'NLPG.StreetDescriptor'. Please see the warning messages.
        // Unable to generate entity type for table 'NLPG.Trailer'. Please see the warning messages.
        // Unable to generate entity type for table 'NLPG.Header'. Please see the warning messages.

        public QuestNLPGContext(DbContextOptions<QuestNLPGContext> options) : base(options)
        {
        }

        public QuestNLPGContext()
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blpu>(entity =>
            {
                entity.HasKey(e => e.Uprn);

                entity.ToTable("BLPU", "NLPG");

                entity.Property(e => e.Uprn)
                    .HasColumnName("UPRN")
                    .ValueGeneratedNever();

                entity.Property(e => e.BlpuState).HasColumnName("BLPU_STATE");

                entity.Property(e => e.BlpuStateDate)
                    .HasColumnName("BLPU_STATE_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.ChangeType)
                    .IsRequired()
                    .HasColumnName("CHANGE_TYPE")
                    .HasMaxLength(1)
                    .IsUnicode(false);

                entity.Property(e => e.EndDate)
                    .HasColumnName("END_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.EntryDate)
                    .HasColumnName("ENTRY_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.LastUpdateDate)
                    .HasColumnName("LAST_UPDATE_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.LocalCustodianCode).HasColumnName("LOCAL_CUSTODIAN_CODE");

                entity.Property(e => e.LogicalStatus).HasColumnName("LOGICAL_STATUS");

                entity.Property(e => e.MultiOccCount).HasColumnName("MULTI_OCC_COUNT");

                entity.Property(e => e.ParentUprn).HasColumnName("PARENT_UPRN");

                entity.Property(e => e.PostalAddress)
                    .IsRequired()
                    .HasColumnName("POSTAL_ADDRESS")
                    .HasMaxLength(1)
                    .IsUnicode(false);

                entity.Property(e => e.PostcodeLocator)
                    .IsRequired()
                    .HasColumnName("POSTCODE_LOCATOR")
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.Property(e => e.ProOrder).HasColumnName("PRO_ORDER");

                entity.Property(e => e.RecordIdentifier).HasColumnName("RECORD_IDENTIFIER");

                entity.Property(e => e.Rpc).HasColumnName("RPC");

                entity.Property(e => e.StartDate)
                    .HasColumnName("START_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.XCoordinate).HasColumnName("X_COORDINATE");

                entity.Property(e => e.YCoordinate).HasColumnName("Y_COORDINATE");
            });

            modelBuilder.Entity<Classification>(entity =>
            {
                entity.HasKey(e => e.ClassKey);

                entity.ToTable("Classification", "NLPG");

                entity.Property(e => e.ClassKey)
                    .HasColumnName("CLASS_KEY")
                    .HasMaxLength(14)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.ChangeType)
                    .IsRequired()
                    .HasColumnName("CHANGE_TYPE")
                    .HasMaxLength(1)
                    .IsUnicode(false);

                entity.Property(e => e.ClassScheme)
                    .IsRequired()
                    .HasColumnName("CLASS_SCHEME")
                    .HasMaxLength(60)
                    .IsUnicode(false);

                entity.Property(e => e.ClassificationCode)
                    .IsRequired()
                    .HasColumnName("CLASSIFICATION_CODE")
                    .HasMaxLength(6)
                    .IsUnicode(false);

                entity.Property(e => e.EndDate)
                    .HasColumnName("END_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.EntryDate)
                    .HasColumnName("ENTRY_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.LastUpdateDate)
                    .HasColumnName("LAST_UPDATE_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.ProOrder).HasColumnName("PRO_ORDER");

                entity.Property(e => e.RecordIdentifier).HasColumnName("RECORD_IDENTIFIER");

                entity.Property(e => e.SchemeVersion).HasColumnName("SCHEME_VERSION");

                entity.Property(e => e.StartDate)
                    .HasColumnName("START_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.Uprn).HasColumnName("UPRN");

                entity.HasOne(d => d.UprnNavigation)
                    .WithMany(p => p.Classification)
                    .HasForeignKey(d => d.Uprn)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Classification_BLPU");
            });

            modelBuilder.Entity<ClassificationFail>(entity =>
            {
                entity.HasKey(e => e.ClassKey);

                entity.ToTable("Classification_Fail", "NLPG");

                entity.Property(e => e.ClassKey)
                    .HasColumnName("CLASS_KEY")
                    .HasMaxLength(14)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.ChangeType)
                    .IsRequired()
                    .HasColumnName("CHANGE_TYPE")
                    .HasMaxLength(1)
                    .IsUnicode(false);

                entity.Property(e => e.ClassScheme)
                    .IsRequired()
                    .HasColumnName("CLASS_SCHEME")
                    .HasMaxLength(60)
                    .IsUnicode(false);

                entity.Property(e => e.ClassificationCode)
                    .IsRequired()
                    .HasColumnName("CLASSIFICATION_CODE")
                    .HasMaxLength(6)
                    .IsUnicode(false);

                entity.Property(e => e.EndDate)
                    .HasColumnName("END_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.EntryDate)
                    .HasColumnName("ENTRY_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.LastUpdateDate)
                    .HasColumnName("LAST_UPDATE_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.ProOrder).HasColumnName("PRO_ORDER");

                entity.Property(e => e.RecordIdentifier).HasColumnName("RECORD_IDENTIFIER");

                entity.Property(e => e.SchemeVersion).HasColumnName("SCHEME_VERSION");

                entity.Property(e => e.StartDate)
                    .HasColumnName("START_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.Uprn).HasColumnName("UPRN");
            });

            modelBuilder.Entity<Dpa>(entity =>
            {
                entity.HasKey(e => e.Uprn);

                entity.ToTable("DPA", "NLPG");

                entity.Property(e => e.Uprn)
                    .HasColumnName("UPRN")
                    .ValueGeneratedNever();

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

                entity.HasOne(d => d.UprnNavigation)
                    .WithOne(p => p.Dpa)
                    .HasForeignKey<Dpa>(d => d.Uprn)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DPA_BLPU");
            });

            modelBuilder.Entity<Lpi>(entity =>
            {
                entity.HasKey(e => e.LpiKey);

                entity.ToTable("LPI", "NLPG");

                entity.HasIndex(e => e.Uprn);

                entity.HasIndex(e => e.Usrn);

                entity.Property(e => e.LpiKey)
                    .HasColumnName("LPI_KEY")
                    .HasMaxLength(14)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.AreaName)
                    .HasColumnName("AREA_NAME")
                    .HasMaxLength(35)
                    .IsUnicode(false);

                entity.Property(e => e.ChangeType)
                    .IsRequired()
                    .HasColumnName("CHANGE_TYPE")
                    .HasMaxLength(1)
                    .IsUnicode(false);

                entity.Property(e => e.EndDate)
                    .HasColumnName("END_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.EntryDate)
                    .HasColumnName("ENTRY_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.Language)
                    .IsRequired()
                    .HasColumnName("LANGUAGE")
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.LastUpdateDate)
                    .HasColumnName("LAST_UPDATE_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.Level)
                    .HasColumnName("LEVEL")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.LogicalStatus).HasColumnName("LOGICAL_STATUS");

                entity.Property(e => e.OfficialFlag)
                    .HasColumnName("OFFICIAL_FLAG")
                    .HasMaxLength(1)
                    .IsUnicode(false);

                entity.Property(e => e.PaoEndNumber).HasColumnName("PAO_END_NUMBER");

                entity.Property(e => e.PaoEndSuffix)
                    .HasColumnName("PAO_END_SUFFIX")
                    .HasMaxLength(2)
                    .IsUnicode(false);

                entity.Property(e => e.PaoStartNumber).HasColumnName("PAO_START_NUMBER");

                entity.Property(e => e.PaoStartSuffix)
                    .HasColumnName("PAO_START_SUFFIX")
                    .HasMaxLength(2)
                    .IsUnicode(false);

                entity.Property(e => e.PaoText)
                    .HasColumnName("PAO_TEXT")
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.ProOrder).HasColumnName("PRO_ORDER");

                entity.Property(e => e.RecordIdentifier).HasColumnName("RECORD_IDENTIFIER");

                entity.Property(e => e.SaoEndNumber).HasColumnName("SAO_END_NUMBER");

                entity.Property(e => e.SaoEndSuffix)
                    .HasColumnName("SAO_END_SUFFIX")
                    .HasMaxLength(2)
                    .IsUnicode(false);

                entity.Property(e => e.SaoStartNumber).HasColumnName("SAO_START_NUMBER");

                entity.Property(e => e.SaoStartSuffix)
                    .HasColumnName("SAO_START_SUFFIX")
                    .HasMaxLength(2)
                    .IsUnicode(false);

                entity.Property(e => e.SaoText)
                    .HasColumnName("SAO_TEXT")
                    .HasMaxLength(90)
                    .IsUnicode(false);

                entity.Property(e => e.StartDate)
                    .HasColumnName("START_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.Uprn).HasColumnName("UPRN");

                entity.Property(e => e.Usrn).HasColumnName("USRN");

                entity.Property(e => e.UsrnMatchIndicator)
                    .IsRequired()
                    .HasColumnName("USRN_MATCH_INDICATOR")
                    .HasMaxLength(1)
                    .IsUnicode(false);

                entity.HasOne(d => d.UprnNavigation)
                    .WithMany(p => p.Lpi)
                    .HasForeignKey(d => d.Uprn)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_LPI_BLPU");

                entity.HasOne(d => d.UsrnNavigation)
                    .WithMany(p => p.Lpi)
                    .HasForeignKey(d => d.Usrn)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_LPI_Street");
            });

            modelBuilder.Entity<Nlpg>(entity =>
            {
                entity.ToTable("NLPG", "NLPG");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ClassificationCode)
                    .HasColumnName("CLASSIFICATION_CODE")
                    .HasMaxLength(6)
                    .IsUnicode(false);

                entity.Property(e => e.GeoSingleAddressLabel)
                    .HasColumnName("geo_single_address_label")
                    .HasMaxLength(967)
                    .IsUnicode(false);

                entity.Property(e => e.LocalityName)
                    .HasColumnName("locality_name")
                    .HasMaxLength(35)
                    .IsUnicode(false);

                entity.Property(e => e.LogicalStatus).HasColumnName("logical_status");

                entity.Property(e => e.NlpgXCoordinate).HasColumnName("nlpg_x_coordinate");

                entity.Property(e => e.NlpgYCoordinate).HasColumnName("nlpg_y_coordinate");

                entity.Property(e => e.PaoEndNumber).HasColumnName("pao_end_number");

                entity.Property(e => e.PaoEndSuffix)
                    .HasColumnName("pao_end_suffix")
                    .HasMaxLength(2)
                    .IsUnicode(false);

                entity.Property(e => e.PaoStartNumber).HasColumnName("pao_start_number");

                entity.Property(e => e.PaoStartSuffix)
                    .HasColumnName("pao_start_suffix")
                    .HasMaxLength(2)
                    .IsUnicode(false);

                entity.Property(e => e.PaoText)
                    .HasColumnName("pao_text")
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.PostcodeLocator)
                    .HasColumnName("postcode_locator")
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.Property(e => e.SaoEndNumber).HasColumnName("sao_end_number");

                entity.Property(e => e.SaoEndSuffix)
                    .HasColumnName("sao_end_suffix")
                    .HasMaxLength(2)
                    .IsUnicode(false);

                entity.Property(e => e.SaoStartNumber).HasColumnName("sao_start_number");

                entity.Property(e => e.SaoStartSuffix)
                    .HasColumnName("sao_start_suffix")
                    .HasMaxLength(2)
                    .IsUnicode(false);

                entity.Property(e => e.SaoText)
                    .HasColumnName("sao_text")
                    .HasMaxLength(90)
                    .IsUnicode(false);

                entity.Property(e => e.StreetDescription)
                    .HasColumnName("street_description")
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.TownName)
                    .HasColumnName("town_name")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Uprn).HasColumnName("uprn");

                entity.Property(e => e.Usrn).HasColumnName("usrn");
            });

            modelBuilder.Entity<Organisation>(entity =>
            {
                entity.HasKey(e => e.OrgKey);

                entity.ToTable("Organisation", "NLPG");

                entity.Property(e => e.OrgKey)
                    .HasColumnName("ORG_KEY")
                    .HasMaxLength(14)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.ChangeType)
                    .IsRequired()
                    .HasColumnName("CHANGE_TYPE")
                    .HasMaxLength(1)
                    .IsUnicode(false);

                entity.Property(e => e.EndDate)
                    .HasColumnName("END_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.EntryDate)
                    .HasColumnName("ENTRY_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.LastUpdateDate)
                    .HasColumnName("LAST_UPDATE_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.LegalName)
                    .HasColumnName("LEGAL_NAME")
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.Organisation1)
                    .IsRequired()
                    .HasColumnName("ORGANISATION")
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.ProOrder).HasColumnName("PRO_ORDER");

                entity.Property(e => e.RecordIdentifier).HasColumnName("RECORD_IDENTIFIER");

                entity.Property(e => e.StartDate)
                    .HasColumnName("START_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.Uprn).HasColumnName("UPRN");

                entity.HasOne(d => d.UprnNavigation)
                    .WithMany(p => p.Organisation)
                    .HasForeignKey(d => d.Uprn)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Organisation_BLPU");
            });

            modelBuilder.Entity<Street>(entity =>
            {
                entity.HasKey(e => e.Usrn);

                entity.ToTable("Street", "NLPG");

                entity.Property(e => e.Usrn)
                    .HasColumnName("USRN")
                    .ValueGeneratedNever();

                entity.Property(e => e.ChangeType)
                    .IsRequired()
                    .HasColumnName("CHANGE_TYPE")
                    .HasMaxLength(1)
                    .IsUnicode(false);

                entity.Property(e => e.LastUpdateDate)
                    .HasColumnName("LAST_UPDATE_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.ProOrder).HasColumnName("PRO_ORDER");

                entity.Property(e => e.RecordEntryDate)
                    .HasColumnName("RECORD_ENTRY_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.RecordIdentifier).HasColumnName("RECORD_IDENTIFIER");

                entity.Property(e => e.RecordType).HasColumnName("RECORD_TYPE");

                entity.Property(e => e.State).HasColumnName("STATE");

                entity.Property(e => e.StateDate)
                    .HasColumnName("STATE_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.StreetClassification).HasColumnName("STREET_CLASSIFICATION");

                entity.Property(e => e.StreetEndDate)
                    .HasColumnName("STREET_END_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.StreetEndLat).HasColumnName("STREET_END_LAT");

                entity.Property(e => e.StreetEndLong).HasColumnName("STREET_END_LONG");

                entity.Property(e => e.StreetEndX).HasColumnName("STREET_END_X");

                entity.Property(e => e.StreetEndY).HasColumnName("STREET_END_Y");

                entity.Property(e => e.StreetStartDate)
                    .HasColumnName("STREET_START_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.StreetStartLat).HasColumnName("STREET_START_LAT");

                entity.Property(e => e.StreetStartLong).HasColumnName("STREET_START_LONG");

                entity.Property(e => e.StreetStartX).HasColumnName("STREET_START_X");

                entity.Property(e => e.StreetStartY).HasColumnName("STREET_START_Y");

                entity.Property(e => e.StreetSurface).HasColumnName("STREET_SURFACE");

                entity.Property(e => e.StreetTolerance).HasColumnName("STREET_TOLERANCE");

                entity.Property(e => e.SwaOrgRefNaming).HasColumnName("SWA_ORG_REF_NAMING");

                entity.Property(e => e.Version).HasColumnName("VERSION");
            });

            modelBuilder.Entity<Successor>(entity =>
            {
                entity.HasKey(e => e.SuccKey);

                entity.ToTable("Successor", "NLPG");

                entity.Property(e => e.SuccKey)
                    .HasColumnName("SUCC_KEY")
                    .HasMaxLength(14)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.ChangeType)
                    .HasColumnName("CHANGE_TYPE")
                    .HasMaxLength(1)
                    .IsUnicode(false);

                entity.Property(e => e.EndDate)
                    .HasColumnName("END_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.EntryDate)
                    .HasColumnName("ENTRY_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.LastUpdateDate)
                    .HasColumnName("LAST_UPDATE_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.ProOrder).HasColumnName("PRO_ORDER");

                entity.Property(e => e.RecordIdentifier).HasColumnName("RECORD_IDENTIFIER");

                entity.Property(e => e.StartDate)
                    .HasColumnName("START_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.Successor1).HasColumnName("SUCCESSOR");

                entity.Property(e => e.Uprn).HasColumnName("UPRN");

                entity.HasOne(d => d.UprnNavigation)
                    .WithMany(p => p.Successor)
                    .HasForeignKey(d => d.Uprn)
                    .HasConstraintName("FK_Successor_BLPU");
            });

            modelBuilder.Entity<Xref>(entity =>
            {
                entity.HasKey(e => e.XrefKey);

                entity.ToTable("XREF", "NLPG");

                entity.Property(e => e.XrefKey)
                    .HasColumnName("XREF_KEY")
                    .HasMaxLength(14)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.ChangeType)
                    .IsRequired()
                    .HasColumnName("CHANGE_TYPE")
                    .HasMaxLength(1)
                    .IsUnicode(false);

                entity.Property(e => e.CrossReference)
                    .IsRequired()
                    .HasColumnName("CROSS_REFERENCE")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.EndDate)
                    .HasColumnName("END_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.EntryDate)
                    .HasColumnName("ENTRY_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.LastUpdateDate)
                    .HasColumnName("LAST_UPDATE_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.ProOrder).HasColumnName("PRO_ORDER");

                entity.Property(e => e.RecordIdentifier).HasColumnName("RECORD_IDENTIFIER");

                entity.Property(e => e.Source)
                    .IsRequired()
                    .HasColumnName("SOURCE")
                    .HasMaxLength(6)
                    .IsUnicode(false);

                entity.Property(e => e.StartDate)
                    .HasColumnName("START_DATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.Uprn).HasColumnName("UPRN");

                entity.Property(e => e.Version).HasColumnName("VERSION");

                entity.HasOne(d => d.UprnNavigation)
                    .WithMany(p => p.Xref)
                    .HasForeignKey(d => d.Uprn)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_XREF_BLPU");
            });
        }
    }
}
