#wait for the SQL Server to come up

echo waiting for 30 seconds
sleep 30
echo running attach script

#run the setup script to create the DB and the schema in the DB
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P M3Gurdy* -d master -i setup.sql

echo attach script complete


