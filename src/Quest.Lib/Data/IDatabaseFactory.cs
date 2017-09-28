using System;

namespace Quest.Lib.Data
{
    public interface IDatabaseFactory
    {
        void Execute<DB>(Action<DB> action) where DB : IDisposable;
        T Execute<DB, T>(Func<DB, T> action) where DB : IDisposable;
    }
}