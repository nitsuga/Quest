using Autofac;
using Quest.Lib.DependencyInjection;
using System;

namespace Quest.Lib.Data
{
    /// <summary>
    /// Factory class that makes a DungbeetleEntities object
    /// </summary>
    [Injection(typeof(IDatabaseFactory), Lifetime.Singleton)]
    public class DatabaseFactory : IDatabaseFactory 
    {
        ILifetimeScope _scope;

        public DatabaseFactory(ILifetimeScope scope)
        {
            _scope = scope;
        }

        /// <summary>
        /// execute an action using the database context
        /// </summary>
        /// <param name="action"></param>
        public void Execute<DB>(Action<DB> action) where DB : IDisposable
        {
            using (var localscope = _scope.BeginLifetimeScope())
            {
                using (var db = localscope.Resolve<DB>())
                {
                    action(db);
                }
            }
        }

        /// <summary>
        /// execute a function using the database context
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public T Execute<DB, T>(Func<DB, T> action) where DB : IDisposable
        {
            using (var localscope = _scope.BeginLifetimeScope())
            {
                DB t;
                using (var db = localscope.Resolve<DB>())
                {
                    return action(db);
                }
            }
        }
    }
}
