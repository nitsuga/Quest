USE [master]
GO
if db_id('Quest') is null
	CREATE DATABASE [Quest] ON 
	( FILENAME = N'/tmp/data1/Quest.mdf' ),
	( FILENAME = N'/tmp/data1/Quest_log.ldf' )
	 FOR ATTACH
GO
