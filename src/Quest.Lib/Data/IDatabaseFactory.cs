using Microsoft.EntityFrameworkCore;
using System;

namespace Quest.Lib.Data
{
    public interface IDatabaseFactory
    {
        void Execute<DB>(Action<DB> action) where DB : DbContext;
        void ExecuteNoTracking<DB>(Action<DB> action) where DB : DbContext;
        T Execute<DB, T>(Func<DB, T> action) where DB : DbContext;
        T ExecuteNoTracking<DB, T>(Func<DB, T> action) where DB : DbContext;
    }
}