using Microsoft.EntityFrameworkCore;

namespace Quest.Lib.Data
{
    /// <summary>
    /// https://stackoverflow.com/questions/35631903/raw-sql-query-without-dbset-entity-framework-core
    /// </summary>
    public static class RDFacadeExtensions
    {

#if false
        private static RelationalDataReader ExecuteSqlQuery(this DatabaseFacade databaseFacade, string sql, params object[] parameters)
        {
            var concurrencyDetector = databaseFacade.GetService<IConcurrencyDetector>();

            using (concurrencyDetector.EnterCriticalSection())
            {
                var rawSqlCommand = databaseFacade
                    .GetService<IRawSqlCommandBuilder>()
                    .Build(sql, parameters);

                return rawSqlCommand
                    .RelationalCommand
                    .ExecuteReader(
                        databaseFacade.GetService<IRelationalConnection>(),
                        parameterValues: rawSqlCommand.ParameterValues);
            }
        }

        private static async Task<RelationalDataReader> ExecuteSqlQueryAsync(this DatabaseFacade databaseFacade, string sql, CancellationToken cancellationToken = default(CancellationToken), params object[] parameters)
        {
            var concurrencyDetector = databaseFacade.GetService<IConcurrencyDetector>();

            using (concurrencyDetector.EnterCriticalSection())
            {
                var rawSqlCommand = databaseFacade
                    .GetService<IRawSqlCommandBuilder>()
                    .Build(sql, parameters);

                using (var conn = databaseFacade.GetService<IRelationalConnection>())
                    return await rawSqlCommand
                        .RelationalCommand
                        .ExecuteReaderAsync(
                            conn,
                            parameterValues: rawSqlCommand.ParameterValues,
                            cancellationToken: cancellationToken);
            }
        }

        public static async Task<int> ExecuteAsync(this DbContext context, string sql, Action<DbDataReader> func = null, params object[] parameters)
        {
            if (func != null)
            {
                // Execute a query.
                using (var dr = await context.Database.ExecuteSqlQueryAsync(sql, parameters))
                using (var reader = dr.DbDataReader)// Output rows.
                    while (reader.Read())
                        func(reader);
                return 0;
            }
            else
            {
                var dr = await context.Database.ExecuteSqlAsync(sql, parameters);
                return dr;
            }
        }
#endif

        // public methods

        public static int Execute(this DbContext context, string sql, params object[] parameters)
        {
            var dr = context.Database.ExecuteSqlCommand(sql, parameters);
            return dr;
        }
    }
}
