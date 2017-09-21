#if false

//TODO: !!

using Autofac;
using Quest.Lib.DependencyInjection;
using System;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;

namespace Quest.Lib.DataModel
{

    /// Loaded through the cofiguration file so it doesn't neet attributes
    /// </summary>
    public class QuestDbConnection
    {
        public string RemoteConnectionString { get; set; }
        public string LocalConnectionString { get; set; }
        public string PingAddress { get; set; }
        public string EFConnectionString { get; set; }
        public string ConnectionString { get; set; }
    }

#if USE_EXECUTION_STRATEGY
    public class DataContextConfiguration : DbConfiguration
    {
        public DataContextConfiguration()
        {
            SetExecutionStrategy("System.Data.SqlClient", () => new SqlAzureExecutionStrategy(5, TimeSpan.FromSeconds(30)));
        }
    }
#endif

    /// <summary>
    /// Factory class that makes a DungbeetleEntities object
    /// </summary>
    [Injection(Lifetime.Singleton)]
    public class DatabaseFactory
    {
        private QuestDbConnection _connection;

        ILifetimeScope _scope;
        public DatabaseFactory(ILifetimeScope scope, QuestDbConnection connection)
        {
            _scope = scope;
            _connection = connection;
        }

        /// <summary>
        /// make a new database object. You must dispose this object after you have used it.
        /// </summary>
        /// <returns></returns>
        public QuestEntities Get()
        {
            return _scope.Resolve<QuestEntities>();
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connection.ConnectionString);
        }
    }

#if USE_EXECUTION_STRATEGY
    [DbConfigurationType(typeof(ExecutionStrategy))]
#endif
    public partial class QuestEntities : DbContext
    {
        public QuestEntities(string connstring)
            : base(connstring)
        {
        }

        public QuestEntities(QuestDbConnection conn)
                : base(conn.EFConnectionString)
        {
        }

        public QuestEntities(EntityConnection conn)
                : base(conn, true)
        {
        }
    }
}
#endif