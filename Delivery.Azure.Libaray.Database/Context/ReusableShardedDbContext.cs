namespace Delivery.Azure.Libaray.Database.Context
{
    public class ReusableShardedDbContext<TDatabaseContext> : IAsyncDisposableShardedDbContext
        where TDatabaseContext 
    {
        
    }
}