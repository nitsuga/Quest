using Quest.Lib.Data;

namespace Quest.Lib.Research.Loader
{
    public static class IncidentLoader
    {
        public static void Load(IDatabaseFactory _dbFactory, string filename, int headers)
        {
            CsvLoader.Load(_dbFactory, filename, headers, ProcessRow);
        }

        static string ProcessRow(string[] data)
        {
            var incidentDate = CsvLoader.GetDate(data[0]);
            var cadref = CsvLoader.GetValue(data[1]);
            var dohcategory = CsvLoader.Getvaluestring(data[2]);
            var dohsubcat = CsvLoader.Getvaluestring(data[3]);
            var lascat = CsvLoader.Getvaluestring(data[4]);
            var callstart = CsvLoader.GetDate(data[5]);
            var t0 = CsvLoader.GetValue(data[6]);
            var t1 = CsvLoader.GetValue(data[7]);
            var t2 = CsvLoader.GetValue(data[8]);
            var t3 = CsvLoader.GetValue(data[9]);
            var area = CsvLoader.Getvaluestring(data[10]);
            var postcode = CsvLoader.Getvaluestring(data[11]);
            var complaint = CsvLoader.Getvaluestring(data[12]);
            var ampds = CsvLoader.Getvaluestring(data[13]);
            var firstdispatch = CsvLoader.GetDate(data[14]);
            var firstarrival = CsvLoader.GetDate(data[15]);
            var duration = CsvLoader.GetValue(data[16]);
            var athospital = CsvLoader.GetDate(data[17]);
            var hospital = CsvLoader.Getvaluestring(data[18]);
            var x = CsvLoader.GetValue(data[19]);
            var y = CsvLoader.GetValue(data[20]);

            const string sql = "INSERT INTO[dbo].[Incidents] ([IncidentDate],[cadref],[dohcategory],[dohsubcat],[lascat],[callstart],[T0],[T1],[T2],[T3],[area],[postcode],[complaint],[AMPDS],[firstdispatch],[firstarrival],[duration],[athospital],[hospital],[X],[Y]) VALUES ";
            var sql2 =
                $"( {incidentDate},{cadref},{dohcategory},{dohsubcat},{lascat},{callstart},{t0},{t1},{t2},{t3},{area},{postcode},{complaint},{ampds},{firstdispatch},{firstarrival},{duration},{athospital},{hospital},{x},{y})\n;";

            return sql + sql2;
        }
    }
}
