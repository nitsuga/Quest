using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Lib.DataModel
{

#if USE_EXECUTION_STRATEGY
    public class ExecutionStrategy : DbConfiguration
    {
        public ExecutionStrategy()
        {
            SetExecutionStrategy("System.Data.SqlClient", () => new SqlAzureExecutionStrategy(5,TimeSpan.FromSeconds(30)));
        }
    }
#endif
}
