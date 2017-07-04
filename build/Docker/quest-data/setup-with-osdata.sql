USE [master]
GO
if db_id('Quest') is null
	CREATE DATABASE [Quest] ON 
	( FILENAME = N'/tmp/data1/Quest.mdf' ),
	( FILENAME = N'/tmp/data1/Quest_log.ldf' )
	 FOR ATTACH
GO
if db_id('QuestNLPG') is null
	CREATE DATABASE [QuestNLPG] ON 
	( FILENAME = N'/tmp/data2/QuestNLPG.mdf' ),
	( FILENAME = N'/tmp/data2/QuestNLPG_log.ldf' )
	 FOR ATTACH
GO
if db_id('QuestOS') is null
	CREATE DATABASE [QuestOS] ON 
	( FILENAME = N'/tmp/data2/QuestOS.mdf' ),
	( FILENAME = N'/tmp/data2/QuestOS_log.ldf' )
	 FOR ATTACH
GO
if db_id('QuestOsm') is null
	CREATE DATABASE [QuestOsm] ON 
	( FILENAME = N'/tmp/data2/QuestOsm.mdf' ),
	( FILENAME = N'/tmp/data2/QuestOsm_log.ldf' )
	 FOR ATTACH
GO
