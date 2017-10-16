#wait for the SQL Server to come up
sleep 30

#run the setup script to create the DB and the schema in the DB
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P M3Gurdy* -d master -i setup.sql


