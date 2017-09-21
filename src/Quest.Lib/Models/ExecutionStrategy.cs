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
