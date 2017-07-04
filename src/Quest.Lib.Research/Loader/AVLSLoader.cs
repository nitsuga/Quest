using Quest.Lib.Utils;

namespace Quest.Lib.Research.Loader
{
    public static class AvlsLoader
    {
        public static void Load(string filename, int headers)
        {
            CsvLoader.Load(filename, headers, ProcessRow);
        }

        static string ProcessRow(string[] data)
        {
            var vehId = CsvLoader.GetVehicleId(data[5]);

            if (vehId <= 0)
                return null;

            var dt = CsvLoader.GetDate(data[1]);
            var inc = CsvLoader.GetValue(data[2]);
            var callsign = CsvLoader.Getvaluestring(data[3]);
            var status = CsvLoader.Getvaluestring(data[6]);
            var speed = CsvLoader.GetValue(data[7]);
            var dir = CsvLoader.GetValue(data[8]);
            var y = CsvLoader.GetValue(data[9]);
            var x = CsvLoader.GetValue(data[10]);

            double lat, lon;

            double.TryParse(y, out lat);
            double.TryParse(x, out lon);

            var os = LatLongConverter.WGS84ToOSRef(lat, lon);

            if (os == null)
                return null;

            const string sql = "INSERT INTO[dbo].[Avls] ([AvlsDateTime],[Status],[Speed],[Direction],[LocationX],[LocationY],[VehicleTypeId],[Callsign],[IncidentId],[scanned],X,Y) VALUES ";
            var sql2 = $"({dt},{status},{speed},{dir},{x},{y},{vehId},{callsign},{inc},0,{os.Easting},{os.Northing});";

            return sql + sql2;
        }

    }
}
