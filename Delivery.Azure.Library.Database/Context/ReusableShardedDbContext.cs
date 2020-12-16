using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Database.Context.Interfaces;

namespace Delivery.Azure.Library.Database.Context
{
    /// <summary>
	///     Allows a database context to be used and shared across multiple repositories and disposed by the initial-calling
	///     dataAccess
	/// </summary>
	/// <typeparam name="TDatabaseContext">The type of database context to use</typeparam>
	public class ReusableShardedDbContext<TDatabaseContext> : IDisposableShardedDbContext
		where TDatabaseContext : ShardedDatabaseContext
	{
		private readonly Func<Task<TDatabaseContext>> createShardedDatabaseContextFunctionWithTask;

		private readonly TDatabaseContext outerDatabaseContext;
		private TDatabaseContext innerDatabaseContext;
		public bool IsDisposed { get; private set; }

		/// <summary>
		///     Constructor
		/// </summary>
		/// <param name="createShardedDatabaseContextFunctionWithTask">
		///     A factory to create a new <typeparamref name="TDatabaseContext" /> asynchronously
		/// </param>
		public ReusableShardedDbContext(Func<Task<TDatabaseContext>> createShardedDatabaseContextFunctionWithTask)
		{
			this.createShardedDatabaseContextFunctionWithTask = createShardedDatabaseContextFunctionWithTask;
		}

		/// <summary>
		///     Constructor
		/// </summary>
		/// <param name="outerDatabaseContext">Re-use an existing database context</param>
		/// <param name="createShardedDatabaseContextFunctionWithTask">
		///     A factory to create a new <typeparamref name="TDatabaseContext" /> asynchronously
		/// </param>
		public ReusableShardedDbContext(TDatabaseContext outerDatabaseContext, Func<Task<TDatabaseContext>> createShardedDatabaseContextFunctionWithTask)
		{
			this.outerDatabaseContext = outerDatabaseContext;
			this.createShardedDatabaseContextFunctionWithTask = createShardedDatabaseContextFunctionWithTask;
		}

		public async ValueTask DisposeAsync()
		{
			if (outerDatabaseContext == null)
			{
				if (innerDatabaseContext != null)
				{
					await innerDatabaseContext.DisposeAsync();
				}

				IsDisposed = true;
			}
		}

		/// <summary>
		///     Gets a context and creates a new one if required
		/// </summary>
		public async Task<TDatabaseContext> GetOrCreateContextAsync()
		{
			if (outerDatabaseContext != null)
			{
				return outerDatabaseContext;
			}

			if (innerDatabaseContext != null && !innerDatabaseContext.IsDisposed)
			{
				return innerDatabaseContext;
			}

			var createShardedContextTask = createShardedDatabaseContextFunctionWithTask.Invoke();
			innerDatabaseContext = await createShardedContextTask;

			return innerDatabaseContext;
		}
	}
}