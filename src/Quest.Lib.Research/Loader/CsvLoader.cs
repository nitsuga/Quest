using System;
using System.Diagnostics;
using System.Text;
using Microsoft.VisualBasic.FileIO;
using Quest.Lib.Research.Model;

namespace Quest.Lib.Research.Loader
{
    public static class CsvLoader
    {
        public static void Load(string filename, int headers, Func<string[], string> processRow)
        {
            // throw away header line
            using (var parser = new TextFieldParser(filename))
            {

                parser.SetDelimiters("\t");

                using (var db = new QuestResearchEntities())
                {
                    db.Database.CommandTimeout = int.MaxValue;

                    // skip header
                    while (headers > 0)
                    {
                        headers--;
                        parser.ReadFields();
                    }

                    var batch = new StringBuilder();
                    var readRowcount = 0;
                    var writeRowcount = 0;
                    var skipped = 0;
                    while (!parser.EndOfData)
                    {
                        var data = parser.ReadFields();

                        if (data == null)
                        {
                            skipped++;
                            continue;
                        }

                        string sqlToExec;
                        try
                        {
                            sqlToExec = processRow(data);

                        }
                        catch (Exception)
                        {
                            skipped++;
                            continue;
                        }

                        if (sqlToExec == null)
                        {
                            skipped++;
                            continue;
                        }

                        batch.Append(sqlToExec);

                        readRowcount++;

                        if (readRowcount % 10000 != 0) continue;

                        try
                        {
                            var rows = db.Database.ExecuteSqlCommand(batch.ToString());
                            writeRowcount += rows;
                            batch.Clear();
                            Debug.WriteLine($"{filename} {readRowcount} {skipped} {writeRowcount} {rows}");
                        }
                        catch (Exception)
                        {
                            Debug.WriteLine($"{batch} ");
                            throw;
                        }
                    }

                    if (batch.ToString().Length > 0)
                        db.Database.ExecuteSqlCommand(batch.ToString());

                }
            }
        }

        public static string GetValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "NULL";
            else
                return value;
        }

        public static string Getvaluestring(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "NULL";
            else
                return "'" + value.Replace("'","''") + "'";
        }

        public static int GetVehicleId(string value)
        {
            var vehId = -1;
            switch (value)
            {
                case "AEU":
                    vehId = 1;
                    break;
                case "FRU":
                    vehId = 2;
                    break;
            }
            return vehId;
        }

        public static string GetDate(string value)
        {
            if (value == "NULL")
                return value;

            var date1 = DateTime.Parse(value);
            var dt1 = date1.ToString("O");
            dt1 = dt1.Substring(0, dt1.IndexOf('.'));
            return "'" + dt1 + "'";
        }
    }
}
