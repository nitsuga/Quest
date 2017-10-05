using Quest.Lib.Data;

namespace Quest.Lib.Research.Loader
{
    public static class ActivationsLoader 
    {
        public static void Load(IDatabaseFactory _dbFactory, string filename, int headers)
        {
            CsvLoader.Load(_dbFactory, filename, headers, ProcessRow);
        }

        static string ProcessRow(string[] data)
        {

            var inc = CsvLoader.GetValue(data[0]);
            var dt1 = CsvLoader.GetDate(data[1]);
            var dt2 = CsvLoader.GetDate(data[2]);
            var callsign = CsvLoader.Getvaluestring(data[3]);
            var vehId = CsvLoader.GetVehicleId(data[4]);
            var x = CsvLoader.GetValue(data[5]);
            var y = CsvLoader.GetValue(data[6]);

            if (x != "NULL")
                x += "00";

            if (y != "NULL")
                y += "00";

            if (vehId <= 0)
                return null;

            const string sql = "INSERT INTO [dbo].[Activations] ([IncidentId],[Dispatched],[Arrived],[Callsign],[VehicleId],[X],[Y]) VALUES ";
            var sql2 = $"( {inc},{dt1},{dt2},{callsign},{vehId},{x},{y});";

            return sql + sql2;
        }
    }
}
