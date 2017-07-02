USE NLPG
GO
/****** Object:  User [questuser]    Script Date: 25/07/2016 11:55:18 ******/
CREATE USER [questuser] FOR LOGIN [questuser] WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_owner] ADD MEMBER [questuser]
GO
ALTER ROLE [db_datareader] ADD MEMBER [questuser]
GO
ALTER ROLE [db_datawriter] ADD MEMBER [questuser]
GO
/****** Object:  Schema [NLPG]    Script Date: 25/07/2016 11:55:18 ******/
CREATE SCHEMA [NLPG]
GO
/****** Object:  Table [NLPG].[BLPU]    Script Date: 25/07/2016 11:55:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [NLPG].[BLPU](
	[RECORD_IDENTIFIER] [smallint] NOT NULL,
	[CHANGE_TYPE] [varchar](1) NOT NULL,
	[PRO_ORDER] [int] NOT NULL,
	[UPRN] [bigint] NOT NULL,
	[LOGICAL_STATUS] [smallint] NOT NULL,
	[BLPU_STATE] [smallint] NULL,
	[BLPU_STATE_DATE] [datetime] NULL,
	[PARENT_UPRN] [bigint] NULL,
	[X_COORDINATE] [float] NOT NULL,
	[Y_COORDINATE] [float] NOT NULL,
	[RPC] [int] NOT NULL,
	[LOCAL_CUSTODIAN_CODE] [smallint] NOT NULL,
	[START_DATE] [datetime] NOT NULL,
	[END_DATE] [datetime] NULL,
	[LAST_UPDATE_DATE] [datetime] NOT NULL,
	[ENTRY_DATE] [datetime] NOT NULL,
	[POSTAL_ADDRESS] [varchar](1) NOT NULL,
	[POSTCODE_LOCATOR] [varchar](8) NOT NULL,
	[MULTI_OCC_COUNT] [int] NOT NULL,
 CONSTRAINT [PK_BLPU] PRIMARY KEY CLUSTERED 
(
	[UPRN] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [NLPG].[Classification]    Script Date: 25/07/2016 11:55:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [NLPG].[Classification](
	[RECORD_IDENTIFIER] [smallint] NOT NULL,
	[CHANGE_TYPE] [varchar](1) NOT NULL,
	[PRO_ORDER] [int] NOT NULL,
	[UPRN] [bigint] NOT NULL,
	[CLASS_KEY] [varchar](14) NOT NULL,
	[CLASSIFICATION_CODE] [varchar](6) NOT NULL,
	[CLASS_SCHEME] [varchar](60) NOT NULL,
	[SCHEME_VERSION] [real] NOT NULL,
	[START_DATE] [datetime] NOT NULL,
	[END_DATE] [datetime] NULL,
	[LAST_UPDATE_DATE] [datetime] NOT NULL,
	[ENTRY_DATE] [datetime] NOT NULL,
 CONSTRAINT [PK_Classification] PRIMARY KEY CLUSTERED 
(
	[CLASS_KEY] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [NLPG].[DPA]    Script Date: 25/07/2016 11:55:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [NLPG].[DPA](
	[RECORD_IDENTIFIER] [smallint] NOT NULL,
	[CHANGE_TYPE] [varchar](1) NOT NULL,
	[PRO_ORDER] [int] NOT NULL,
	[UPRN] [bigint] NOT NULL,
	[ORGANISATION_NAME] [varchar](60) NULL,
	[DEPARTMENT_NAME] [varchar](60) NULL,
	[SUB_BUILDING_NAME] [varchar](30) NULL,
	[BUILDING_NAME] [varchar](50) NULL,
	[BUILDING_NUMBER] [smallint] NULL,
	[DEPENDENT_THOROUGHFARE] [varchar](80) NULL,
	[THOROUGHFARE] [varchar](80) NULL,
	[DOUBLE_DEPENDENT_LOCALITY] [varchar](35) NULL,
	[DEPENDENT_LOCALITY] [varchar](35) NULL,
	[POST_TOWN] [varchar](30) NOT NULL,
	[POSTCODE] [varchar](8) NOT NULL,
	[POSTCODE_TYPE] [varchar](1) NOT NULL,
	[WELSH_DEPENDENT_THOROUGHFARE] [varchar](80) NULL,
	[WELSH_THOROUGHFARE] [varchar](80) NULL,
	[WELSH_DOUBLE_DEPENDENT_LOCALITY] [varchar](35) NULL,
	[WELSH_DEPENDENT_LOCALITY] [varchar](35) NULL,
	[WELSH_POST_TOWN] [varchar](30) NULL,
	[PO_BOX_NUMBER] [varchar](6) NULL,
	[PROCESS_DATE] [datetime] NOT NULL,
	[START_DATE] [datetime] NOT NULL,
	[END_DATE] [datetime] NULL,
	[LAST_UPDATE_DATE] [datetime] NOT NULL,
	[ENTRY_DATE] [datetime] NOT NULL,
	[PARENT_ADDRESSABLE_UPRN] [bigint] NULL,
	[RM_UPRN] [int] NOT NULL,
 CONSTRAINT [PK_DPA] PRIMARY KEY CLUSTERED 
(
	[UPRN] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [NLPG].[Header]    Script Date: 25/07/2016 11:55:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [NLPG].[Header](
	[RECORD_IDENTIFIER] [smallint] NULL,
	[CUSTODIAN_NAME] [varchar](100) NULL,
	[LOCAL_CUSTODIAN_NAME] [varchar](100) NULL,
	[PROCESS_DATE] [datetime] NULL,
	[VOLUME_NUMBER] [smallint] NULL,
	[ENTRY_DATE] [datetime] NULL,
	[TIME_STAMP] [datetime] NULL,
	[VERSION] [real] NULL,
	[FILE_TYPE] [varchar](50) NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [NLPG].[LPI]    Script Date: 25/07/2016 11:55:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [NLPG].[LPI](
	[RECORD_IDENTIFIER] [smallint] NOT NULL,
	[CHANGE_TYPE] [varchar](1) NOT NULL,
	[PRO_ORDER] [int] NOT NULL,
	[UPRN] [bigint] NOT NULL,
	[LPI_KEY] [varchar](14) NOT NULL,
	[LANGUAGE] [varchar](3) NOT NULL,
	[LOGICAL_STATUS] [smallint] NOT NULL,
	[START_DATE] [datetime] NOT NULL,
	[END_DATE] [datetime] NULL,
	[LAST_UPDATE_DATE] [datetime] NOT NULL,
	[ENTRY_DATE] [datetime] NOT NULL,
	[SAO_START_NUMBER] [smallint] NULL,
	[SAO_START_SUFFIX] [varchar](2) NULL,
	[SAO_END_NUMBER] [smallint] NULL,
	[SAO_END_SUFFIX] [varchar](2) NULL,
	[SAO_TEXT] [varchar](90) NULL,
	[PAO_START_NUMBER] [smallint] NULL,
	[PAO_START_SUFFIX] [varchar](2) NULL,
	[PAO_END_NUMBER] [smallint] NULL,
	[PAO_END_SUFFIX] [varchar](2) NULL,
	[PAO_TEXT] [varchar](250) NULL,
	[USRN] [int] NOT NULL,
	[USRN_MATCH_INDICATOR] [varchar](1) NOT NULL,
	[AREA_NAME] [varchar](35) NULL,
	[LEVEL] [varchar](30) NULL,
	[OFFICIAL_FLAG] [varchar](1) NULL,
 CONSTRAINT [PK_LPI] PRIMARY KEY CLUSTERED 
(
	[LPI_KEY] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [NLPG].[Metadata]    Script Date: 25/07/2016 11:55:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [NLPG].[Metadata](
	[RECORD_IDENTIFIER] [smallint] NULL,
	[GAZ_NAME] [varchar](60) NULL,
	[GAZ_SCOPE] [varchar](60) NULL,
	[TER_OF_USE] [varchar](60) NULL,
	[LINKED_DATA] [varchar](100) NULL,
	[GAZ_OWNER] [varchar](15) NULL,
	[NGAZ_FREQ] [varchar](1) NULL,
	[CUSTODIAN_NAME] [varchar](40) NULL,
	[CUSTODIAN_UPRN] [bigint] NULL,
	[LOCAL_CUSTODIAN_CODE] [smallint] NULL,
	[CO_ORD_SYSTEM] [varchar](40) NULL,
	[CO_ORD_UNIT] [varchar](10) NULL,
	[META_DATE] [datetime] NULL,
	[CLASS_SCHEME] [varchar](60) NULL,
	[GAZ_DATE] [datetime] NULL,
	[LANGUAGE] [varchar](3) NULL,
	[CHARACTER_SET] [varchar](30) NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [NLPG].[Organisation]    Script Date: 25/07/2016 11:55:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [NLPG].[Organisation](
	[RECORD_IDENTIFIER] [smallint] NOT NULL,
	[CHANGE_TYPE] [varchar](1) NOT NULL,
	[PRO_ORDER] [int] NOT NULL,
	[UPRN] [bigint] NOT NULL,
	[ORG_KEY] [varchar](14) NOT NULL,
	[ORGANISATION] [varchar](250) NOT NULL,
	[LEGAL_NAME] [varchar](250) NULL,
	[START_DATE] [datetime] NOT NULL,
	[END_DATE] [datetime] NULL,
	[LAST_UPDATE_DATE] [datetime] NOT NULL,
	[ENTRY_DATE] [datetime] NOT NULL,
 CONSTRAINT [PK_Organisation] PRIMARY KEY CLUSTERED 
(
	[ORG_KEY] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [NLPG].[Street]    Script Date: 25/07/2016 11:55:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [NLPG].[Street](
	[RECORD_IDENTIFIER] [smallint] NOT NULL,
	[CHANGE_TYPE] [varchar](1) NOT NULL,
	[PRO_ORDER] [int] NOT NULL,
	[USRN] [int] NOT NULL,
	[RECORD_TYPE] [smallint] NOT NULL,
	[SWA_ORG_REF_NAMING] [smallint] NOT NULL,
	[STATE] [smallint] NULL,
	[STATE_DATE] [datetime] NULL,
	[STREET_SURFACE] [smallint] NULL,
	[STREET_CLASSIFICATION] [smallint] NULL,
	[VERSION] [smallint] NOT NULL,
	[STREET_START_DATE] [datetime] NOT NULL,
	[STREET_END_DATE] [datetime] NULL,
	[LAST_UPDATE_DATE] [datetime] NOT NULL,
	[RECORD_ENTRY_DATE] [datetime] NOT NULL,
	[STREET_START_X] [float] NOT NULL,
	[STREET_START_Y] [float] NOT NULL,
	[STREET_START_LAT] [float] NOT NULL,
	[STREET_START_LONG] [float] NOT NULL,
	[STREET_END_X] [float] NOT NULL,
	[STREET_END_Y] [float] NOT NULL,
	[STREET_END_LAT] [float] NOT NULL,
	[STREET_END_LONG] [float] NOT NULL,
	[STREET_TOLERANCE] [float] NOT NULL,
 CONSTRAINT [PK_Street] PRIMARY KEY CLUSTERED 
(
	[USRN] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [NLPG].[StreetDescriptor]    Script Date: 25/07/2016 11:55:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [NLPG].[StreetDescriptor](
	[RECORD_IDENTIFIER] [smallint] NOT NULL,
	[CHANGE_TYPE] [varchar](1) NOT NULL,
	[PRO_ORDER] [int] NOT NULL,
	[USRN] [int] NOT NULL,
	[STREET_DESCRIPTION] [varchar](250) NOT NULL,
	[LOCALITY_NAME] [varchar](35) NULL,
	[TOWN_NAME] [varchar](30) NULL,
	[ADMINSTRATIVE_AREA] [varchar](30) NOT NULL,
	[LANGUAGE] [varchar](3) NOT NULL,
 CONSTRAINT [PK_StreetDescriptor] PRIMARY KEY CLUSTERED 
(
	[USRN] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [NLPG].[Successor]    Script Date: 25/07/2016 11:55:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [NLPG].[Successor](
	[RECORD_IDENTIFIER] [smallint] NULL,
	[CHANGE_TYPE] [varchar](1) NULL,
	[PRO_ORDER] [int] NULL,
	[UPRN] [bigint] NULL,
	[SUCC_KEY] [varchar](14) NOT NULL,
	[START_DATE] [datetime] NULL,
	[END_DATE] [datetime] NULL,
	[LAST_UPDATE_DATE] [datetime] NULL,
	[ENTRY_DATE] [datetime] NULL,
	[SUCCESSOR] [bigint] NULL,
 CONSTRAINT [PK_Successor] PRIMARY KEY CLUSTERED 
(
	[SUCC_KEY] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [NLPG].[Trailer]    Script Date: 25/07/2016 11:55:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [NLPG].[Trailer](
	[RECORD_IDENTIFIER] [smallint] NULL,
	[NEXT_VOLUME_NUMBER] [int] NULL,
	[RECORD_COUNT] [int] NULL,
	[ENTRY_DATE] [datetime] NULL,
	[TIME_STAMP] [datetime] NULL
) ON [PRIMARY]

GO
/****** Object:  Table [NLPG].[XREF]    Script Date: 25/07/2016 11:55:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [NLPG].[XREF](
	[RECORD_IDENTIFIER] [smallint] NOT NULL,
	[CHANGE_TYPE] [varchar](1) NOT NULL,
	[PRO_ORDER] [int] NOT NULL,
	[UPRN] [bigint] NOT NULL,
	[XREF_KEY] [varchar](14) NOT NULL,
	[CROSS_REFERENCE] [varchar](50) NOT NULL,
	[VERSION] [int] NULL,
	[SOURCE] [varchar](6) NOT NULL,
	[START_DATE] [datetime] NOT NULL,
	[END_DATE] [datetime] NULL,
	[LAST_UPDATE_DATE] [datetime] NOT NULL,
	[ENTRY_DATE] [datetime] NOT NULL,
 CONSTRAINT [PK_XREF] PRIMARY KEY CLUSTERED 
(
	[XREF_KEY] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  View [dbo].[vw_NLPG]    Script Date: 25/07/2016 11:55:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO






CREATE view [dbo].[vw_NLPG] as
SELECT 

 ISNULL(Row_Number() OVER (ORDER BY (SELECT        1)), - 1) AS Id,

 --ROW_NUMBER() over (order by lpi_key desc) as id, 
 l.uprn,
 c.CLASSIFICATION_CODE,
 l.sao_text,
 l.sao_start_number,
 l.sao_start_suffix,
 l.sao_end_number,
 l.sao_end_suffix,
 l.pao_text,
 l.pao_start_number,
 l.pao_start_suffix,
 l.pao_end_number,
 l.pao_end_suffix,
 l.usrn,
 l.logical_status,
 s.street_description,
 s.town_name,
 s.locality_name,
 b.postcode_locator, 
 b.x_coordinate as nlpg_x_coordinate,
 b.y_coordinate as nlpg_y_coordinate,
/* Concatenate a single GEOGRAPHIC address line label
This code takes into account all possible combinations os pao/sao numbers and suffixes */ 
case 
	when o.organisation is not null and o.organisation<>l.sao_text then o.organisation + ', ' else '' end 
--Secondary Addressable Information------------------------------------------------------------------------------------- 
	+ case when l.sao_text is not null then l.sao_text + ', ' else '' end 
--case statement for different combinations of the sao start numbers (e.g. if no sao start suffix) 
+ case 
	when l.sao_start_number is not null and l.sao_start_suffix is null and l.sao_end_number is null then cast(l.sao_start_number as varchar(4)) + ', '
	when l.sao_start_number is null then '' 
	else cast(l.sao_start_number as varchar(4)) + '' 
 end 	
--case statement for different combinations of the sao start suffixes (e.g. if no sao end number)  
+ case 
	when l.sao_start_suffix is not null and l.sao_end_number is null then l.sao_start_suffix + ', '
	when l.sao_start_suffix is not null and l.sao_end_number is not null then l.sao_start_suffix else '' 
 end 
--Add a '-' between the start and end of the secondary address (e.g. only when sao start and sao end)  
+ case 
	when l.sao_end_suffix is not null and l.sao_end_number is not null then '-'
	when l.sao_start_number is not null and l.sao_end_number is not null then '-' else '' end 
--case statement for different combinations of the sao end numbers and sao end suffixes  
+ case 
	when l.sao_end_number is not null and l.sao_end_suffix is null then cast(l.sao_end_number as varchar(4)) + ', ' 
	when l.sao_end_number is null then '' else cast(l.sao_end_number as varchar(4))
end 
--pao end suffix 
 + case when l.sao_end_suffix is not null then l.sao_end_suffix + ', ' else '' end 
 
--Primary Addressable Information---------------------------------------------------------------------------------------------------------

+ case when l.pao_text is not null then l.pao_text + ', ' else '' end
--case statement for different combinations of the pao start numbers (e.g. if no pao start suffix) 
 + case 
	when l.pao_start_number is not null and l.pao_start_suffix is null and l.pao_end_number is null then cast(l.pao_start_number as varchar(4)) + ', '                                                                                                 
	when l.pao_start_number is null then ''
	else cast(l.pao_start_number as varchar(4)) + ''
	end

--case statement for different combinations of the pao start suffixes (e.g. if no pao end number) 
+ case 
	when l.pao_start_suffix is not null and l.pao_end_number is null then l.pao_start_suffix + ', '
	when l.pao_start_suffix is not null and l.pao_end_number is not null then l.pao_start_suffix else '' end
---Add a '-' between the start and end of the primary address (e.g. only when pao start and pao end) 
+ case 
	when l.pao_end_suffix is not null and l.pao_end_number is not null then '-' 
	when l.pao_start_number is not null and l.pao_end_number is not null then '-'
	else '' 
end
--case statement for different combinations of the pao end numbers and pao end suffixes 
+ case 
		when l.pao_end_number is not null and l.pao_end_suffix is null then cast(l.pao_end_number as varchar(4)) + ', ' 
		when l.pao_end_number is null then '' 
		else cast(l.pao_end_number as varchar(4)) + ' '
  end
--pao end suffix 
+ case when l.pao_end_suffix is not null then l.pao_end_suffix + ', ' else '' end
--Street Information----------------------------------------------------------------------------------------------------------
 + case when s.street_description is not null then s.street_description + ', ' else '' end                                  
--Locality-----------------------------------------------------------------------------------------------------------------------
 + case when s.locality_name is not null then s.locality_name + ', ' else '' end
--Town-------------------------------------------------------------------------------------------------------------------------
 + case when s.town_name is not null then s.town_name + ', ' else '' end 
--Postcode--------------------------------------------------------------------------------------------------------------------
 + case when b.postcode_locator is not null then b.postcode_locator else '' end AS geo_single_address_label 
FROM 
nlpg.BLPU AS b 
join nlpg.LPI AS l on b.uprn = l.uprn
join NLPG.Classification as c on c.UPRN = b.UPRN
join nlpg.streetdescriptor AS s on l.usrn = s.usrn AND l.language = s.language
full outer join nlpg.organisation AS o on (l.uprn = o.uprn) 








GO
/****** Object:  View [dbo].[vw_NLPG_ROAD]    Script Date: 25/07/2016 11:55:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





CREATE view [dbo].[vw_NLPG_ROAD] as
SELECT 

	ISNULL(Row_Number() OVER (ORDER BY (SELECT        1)), - 1) AS Id

	 ,s.street_description as STREET
	 ,s.LOCALITY_NAME
	 ,s.USRN
     ,[TOWN_NAME]
     ,[ADMINSTRATIVE_AREA]
     ,[STREET_START_X]
     ,[STREET_START_Y]
	 ,case when s.street_description is not null then s.street_description + ', ' else '' end                                  
	  + case when s.locality_name is not null then s.locality_name + ', ' else '' end
	  + case when s.town_name is not null then s.town_name  else '' end as STREET_DESCRIPTION
 FROM 
[NLPG].[Street] st join nlpg.streetdescriptor AS s on st.usrn = s.usrn 








GO
ALTER TABLE [NLPG].[Classification]  WITH CHECK ADD  CONSTRAINT [FK_Classification_BLPU] FOREIGN KEY([UPRN])
REFERENCES [NLPG].[BLPU] ([UPRN])
GO
ALTER TABLE [NLPG].[Classification] CHECK CONSTRAINT [FK_Classification_BLPU]
GO
ALTER TABLE [NLPG].[DPA]  WITH CHECK ADD  CONSTRAINT [FK_DPA_BLPU] FOREIGN KEY([UPRN])
REFERENCES [NLPG].[BLPU] ([UPRN])
GO
ALTER TABLE [NLPG].[DPA] CHECK CONSTRAINT [FK_DPA_BLPU]
GO
ALTER TABLE [NLPG].[LPI]  WITH CHECK ADD  CONSTRAINT [FK_LPI_BLPU] FOREIGN KEY([UPRN])
REFERENCES [NLPG].[BLPU] ([UPRN])
GO
ALTER TABLE [NLPG].[LPI] CHECK CONSTRAINT [FK_LPI_BLPU]
GO
ALTER TABLE [NLPG].[LPI]  WITH CHECK ADD  CONSTRAINT [FK_LPI_Street] FOREIGN KEY([USRN])
REFERENCES [NLPG].[Street] ([USRN])
GO
ALTER TABLE [NLPG].[LPI] CHECK CONSTRAINT [FK_LPI_Street]
GO
ALTER TABLE [NLPG].[Organisation]  WITH CHECK ADD  CONSTRAINT [FK_Organisation_BLPU] FOREIGN KEY([UPRN])
REFERENCES [NLPG].[BLPU] ([UPRN])
GO
ALTER TABLE [NLPG].[Organisation] CHECK CONSTRAINT [FK_Organisation_BLPU]
GO
ALTER TABLE [NLPG].[StreetDescriptor]  WITH CHECK ADD  CONSTRAINT [FK_StreetDescriptor_Street] FOREIGN KEY([USRN])
REFERENCES [NLPG].[Street] ([USRN])
GO
ALTER TABLE [NLPG].[StreetDescriptor] CHECK CONSTRAINT [FK_StreetDescriptor_Street]
GO
ALTER TABLE [NLPG].[Successor]  WITH CHECK ADD  CONSTRAINT [FK_Successor_BLPU] FOREIGN KEY([UPRN])
REFERENCES [NLPG].[BLPU] ([UPRN])
GO
ALTER TABLE [NLPG].[Successor] CHECK CONSTRAINT [FK_Successor_BLPU]
GO
ALTER TABLE [NLPG].[XREF]  WITH CHECK ADD  CONSTRAINT [FK_XREF_BLPU] FOREIGN KEY([UPRN])
REFERENCES [NLPG].[BLPU] ([UPRN])
GO
ALTER TABLE [NLPG].[XREF] CHECK CONSTRAINT [FK_XREF_BLPU]
GO












truncate table nlpg.classification
truncate table nlpg.dpa
truncate table nlpg.header
truncate table nlpg.lpi
truncate table nlpg.metadata
truncate table nlpg.organisation
truncate table nlpg.street
truncate table nlpg.streetdescriptor
truncate table nlpg.trailer
truncate table nlpg.xref
truncate table nlpg.successor
truncate table nlpg.blpu

ALTER TABLE [NLPG].[BLPU] ADD  CONSTRAINT [PK_BLPU] PRIMARY KEY CLUSTERED ([UPRN] ASC);
ALTER TABLE [NLPG].[Street] ADD  CONSTRAINT [PK_Street] PRIMARY KEY CLUSTERED ([USRN] ASC);
ALTER TABLE [NLPG].[StreetDescriptor]  WITH CHECK ADD  CONSTRAINT [FK_StreetDescriptor_Street] FOREIGN KEY([USRN]) REFERENCES [NLPG].[Street] ([USRN]);
ALTER TABLE [NLPG].[StreetDescriptor] CHECK CONSTRAINT [FK_StreetDescriptor_Street];
ALTER TABLE [NLPG].[Classification]  WITH CHECK ADD  CONSTRAINT [FK_Classification_BLPU] FOREIGN KEY([UPRN]) REFERENCES [NLPG].[BLPU] ([UPRN]);
ALTER TABLE [NLPG].[Classification] CHECK CONSTRAINT [FK_Classification_BLPU]
ALTER TABLE [NLPG].XREF  WITH CHECK ADD  CONSTRAINT [FK_XREF_BLPU] FOREIGN KEY([UPRN]) REFERENCES [NLPG].[BLPU] ([UPRN])
ALTER TABLE [NLPG].XREF CHECK CONSTRAINT [FK_XREF_BLPU]
ALTER TABLE [NLPG].DPA  WITH CHECK ADD  CONSTRAINT [FK_DPA_BLPU] FOREIGN KEY([UPRN]) REFERENCES [NLPG].[BLPU] ([UPRN])
ALTER TABLE [NLPG].DPA CHECK CONSTRAINT [FK_DPA_BLPU]
ALTER TABLE [NLPG].LPI  WITH CHECK ADD  CONSTRAINT [FK_LPI_BLPU] FOREIGN KEY([UPRN]) REFERENCES [NLPG].[BLPU] ([UPRN])
ALTER TABLE [NLPG].LPI CHECK CONSTRAINT [FK_LPI_BLPU]
ALTER TABLE [NLPG].Successor  WITH CHECK ADD  CONSTRAINT [FK_Successor_BLPU] FOREIGN KEY([UPRN]) REFERENCES [NLPG].[BLPU] ([UPRN])
ALTER TABLE [NLPG].Successor CHECK CONSTRAINT [FK_Successor_BLPU]
ALTER TABLE [NLPG].Organisation  WITH CHECK ADD  CONSTRAINT [FK_Organisation_BLPU] FOREIGN KEY([UPRN]) REFERENCES [NLPG].[BLPU] ([UPRN])
ALTER TABLE [NLPG].Organisation CHECK CONSTRAINT [FK_Organisation_BLPU]
ALTER TABLE [NLPG].LPI  WITH CHECK ADD  CONSTRAINT [FK_LPI_Street] FOREIGN KEY([USRN]) REFERENCES [NLPG].[Street] ([USRN])
ALTER TABLE [NLPG].LPI CHECK CONSTRAINT [FK_LPI_Street]




