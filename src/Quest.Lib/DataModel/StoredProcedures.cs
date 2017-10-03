using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace Quest.Lib.DataModel
{
    public static class SqlExtensions
    {
        public static DbCommand LoadStoredProc(this DbCommand cmd, string storedProcName)
        {
            cmd.CommandText = storedProcName;
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            return cmd;
        }

        public static DbCommand WithSqlParam(this DbCommand cmd, string paramName, object paramValue)
        {
            if (string.IsNullOrEmpty(cmd.CommandText))
                throw new InvalidOperationException("Call LoadStoredProc before using this method");

            var param = cmd.CreateParameter();
            param.ParameterName = paramName;
            param.Value = paramValue;
            cmd.Parameters.Add(param);
            return cmd;
        }

        public static IList<T> MapToList<T>(this DbDataReader dr)
        {
            var objList = new List<T>();
            var props = typeof(T).GetRuntimeProperties();

            var colMapping = dr.GetColumnSchema()
                .Where(x => props.Any(y => y.Name.ToLower() == x.ColumnName.ToLower()))
                .ToDictionary(key => key.ColumnName.ToLower());

            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    T obj = Activator.CreateInstance<T>();
                    foreach (var prop in props)
                    {
                        var val = dr.GetValue(colMapping[prop.Name.ToLower()].ColumnOrdinal.Value);
                        prop.SetValue(obj, val == DBNull.Value ? null : val);
                    }
                    objList.Add(obj);
                }
            }
            return objList;
        }

    }

    public partial class QuestContext : DbContext
    {

        /// <summary>
        ///  dotnet ef dbcontext scaffold "Server=localhost,999;Database=QuestNLPG;user=sa;pwd=M3Gurdy*" Microsoft.EntityFrameworkCore.SqlServer -f -o DataModelNLPG
        ///  dotnet ef dbcontext scaffold "Server=localhost,999;Database=QuestData;user=sa;pwd=M3Gurdy*" Microsoft.EntityFrameworkCore.SqlServer -f -o DataModelResearch
        /// </summary>
        /// <param name="claimType"></param>
        /// <param name="claimValue"></param>
        /// <returns></returns>
        public virtual IList<GetClaims_Result> GetClaims(string claimType, string claimValue)
        {
            using (var dbc = this.Database.GetDbConnection())
            {
                dbc.Open();
                using (var sqlcmd = dbc.CreateCommand())
                {
                    sqlcmd.LoadStoredProc("GetClaims")
                        .WithSqlParam("@ClaimType", claimType)
                        .WithSqlParam("@ClaimValue", claimValue);

                    using (var reader = sqlcmd.ExecuteReader())
                    {
                        var result = reader.MapToList<GetClaims_Result>();
                        return result;
                    }
                }
            }
        }

        public virtual string GetOperationalArea(Nullable<int> buffer)
        {
            using (var dbc = this.Database.GetDbConnection())
            {
                using (var sqlcmd = dbc.CreateCommand())
                {
                    sqlcmd.CommandText = $"EXEC [GetOperationalArea] @Buffer = {buffer}";
                    sqlcmd.CommandType = System.Data.CommandType.StoredProcedure;
                    using (var reader = sqlcmd.ExecuteReader())
                    {
                        var result = reader.GetString(0);
                    }
                }
            }
            return null;
        }

        public virtual int CleanCoverage()
        {
            //return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("CleanCoverage");
            return 0;
        }

        public virtual List<GetVehicleCoverage_Result> GetVehicleCoverage(Nullable<int> vehtype)
        {
            //var vehtypeParameter = vehtype.HasValue ?
            //    new ObjectParameter("vehtype", vehtype) :
            //    new ObjectParameter("vehtype", typeof(int));

            //return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetVehicleCoverage_Result>("GetVehicleCoverage", vehtypeParameter);
            return null;
        }


        public virtual List<GetIncidentDensity_Result> GetIncidentDensity()
        {
            //return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetIncidentDensity_Result>("GetIncidentDensity");
            return null;
        }
    }
    public partial class GetClaims_Result
    {
        public string SecuredItemName { get; set; }
        public string SecuredValue { get; set; }
    }

    public partial class GetIncidentDensity_Result
    {
        public Nullable<int> Quantity { get; set; }
        public Nullable<int> CellX { get; set; }
        public Nullable<int> CellY { get; set; }
    }

    public partial class GetVehicleCoverage_Result
    {
        public int TypeId { get; set; }
        public string Name { get; set; }
        public byte[] Data { get; set; }
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }
        public int Blocksize { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
        public Nullable<double> Percent { get; set; }
        public Nullable<System.DateTime> tstamp { get; set; }
    }

}
