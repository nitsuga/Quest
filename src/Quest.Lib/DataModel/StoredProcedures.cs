using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Quest.Lib.DataModel
{
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

        public virtual string GetOperationalArea(int buffer)
        {
            using (var dbc = this.Database.GetDbConnection())
            {
                dbc.Open();
                using (var sqlcmd = dbc.CreateCommand())
                {
                    sqlcmd.LoadStoredProc("GetOperationalArea")
                        .WithSqlParam("@Buffer", buffer);

                    using (var reader = sqlcmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                return reader.GetString(0);
                            }
                        }
                    }
                }
            }
            return null;
        }


    }
    public partial class GetClaims_Result
    {
        public string SecuredItemName { get; set; }
        public string SecuredValue { get; set; }
    }

}
