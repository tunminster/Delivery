using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Delivery.Azure.Library.Storage.Cosmos.Contracts;
using Microsoft.Azure.Cosmos;

namespace Delivery.Azure.Library.Storage.Cosmos.Interfaces
{
    public interface IPlatformCosmosDbService
    {
        /// <summary>
		///     Add an item to the collection
		/// </summary>
		Task AddItemAsync<TDocument, TContract>(TDocument item) where TDocument : DocumentContract<TContract>, new() where TContract : class;

		/// <summary>
		///     Delete an item from the collection
		/// </summary>
		Task DeleteItemAsync<TDocument, TContract>(string id, string partitionKey, bool throwIfNotFound = true) where TDocument : DocumentContract<TContract>, new() where TContract : class;

		/// <summary>
		///     Queries cosmos for the latest <see cref="DocumentContract{TData}" /> matching common search parameters
		/// </summary>
		/// <param name="partitionKey">The partition key to use. Required</param>
		/// <param name="isDocumentRequired">Throws an error if a document was expected but could not be found</param>
		/// <param name="validFromDate">The date in which the data is considered valid. Defaults to now</param>
		/// <param name="validToDate">Allows data to expire</param>
		/// <typeparam name="TContract">The type of the data node</typeparam>
		/// <typeparam name="TDocument">The type of the full json document stored in cosmos</typeparam>
		/// <returns>The latest document contract, if any exists</returns>
		Task<TDocument?> GetLatestDocumentAsync<TDocument, TContract>(string partitionKey, bool isDocumentRequired = true, DateTimeOffset? validFromDate = null, DateTimeOffset? validToDate = null)
			where TDocument : DocumentContract<TContract>, new()
			where TContract : class;

		/// <summary>
		///     Get the item by its unique partition and id
		/// </summary>
		Task<TContract?> GetItemAsync<TContract>(string partitionKey, Guid id) where TContract : class;

		/// <summary>
		///     Gets a list of items matching the query definition
		/// </summary>
		Task<IEnumerable<TContract?>> GetItemsAsync<TContract>(QueryDefinition queryDefinition, QueryRequestOptions? queryRequestOptions = null, string? executedQuery = null) where TContract : class;

		/// <summary>
		///     Replace an item with a new representation
		/// </summary>
		Task UpdateItemAsync<TDocument, TContract>(string partitionKey, TContract item) where TDocument : DocumentContract<TContract>, new() where TContract : class;
    }
}