using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Delivery.Azure.Library.Core.Extensions.Collections;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Core.Extensions.Strings;
using Delivery.Azure.Library.Exceptions.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Storage.Cosmos.Contracts;
using Delivery.Azure.Library.Storage.Cosmos.Interfaces;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Dependencies;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Dependencies.Models;
using Microsoft.Azure.Cosmos;
using Polly;

namespace Delivery.Azure.Library.Storage.Cosmos.Services
{
    public class PlatformCosmosDbService : IPlatformCosmosDbService
    {
        public static readonly QueryRequestOptions DefaultQueryRequestOptions = new()
        {
            EnableScanInQuery = false,
            MaxConcurrency = 8
        };
        
        private readonly Container container;
        private readonly string databaseName;
        private readonly CosmosClient dbClient;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        private readonly IServiceProvider serviceProvider;
        
        public PlatformCosmosDbService(
            IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter,
            CosmosClient dbClient,
            Container container)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
            this.dbClient = dbClient;
            databaseName = container.Database.Id;
            this.container = container;
        }
        
        public async Task AddItemAsync<TDocument, TContract>(TDocument item) where TDocument : DocumentContract<TContract>, new() where TContract : class
        {
            var dependencyName = $"Cosmos-{container.Id}";
            var dependencyData = new DependencyData("AddItem");
            var dependencyTarget = container.Id;

            item.Id ??= Guid.NewGuid();

            await new DependencyMeasurement(serviceProvider)
                .ForDependency(dependencyName, MeasuredDependencyType.AzureCosmosDb, dependencyData.ConvertToJson(), dependencyTarget)
                .WithContextualInformation(GetCosmosTelemetryProperties())
                .TrackAsync(async () => await container.CreateItemAsync(item, new PartitionKey(item.PartitionKey)));
        }

        public async Task DeleteItemAsync<TDocument, TContract>(string id, string partitionKey, bool throwIfNotFound = true) where TDocument : DocumentContract<TContract>, new() where TContract : class
        {
            var dependencyName = $"Cosmos-{container.Id}";
            var dependencyData = new DependencyData("DeleteItem");
            var dependencyTarget = container.Id;

            await new DependencyMeasurement(serviceProvider)
                .ForDependency(dependencyName, MeasuredDependencyType.AzureCosmosDb, dependencyData.ConvertToJson(), dependencyTarget)
                .WithContextualInformation(GetCosmosTelemetryProperties())
                .TrackAsync(async () =>
                {
                    try
                    {
                        await container.DeleteItemAsync<TContract>(id, new PartitionKey(partitionKey));
                    }
                    catch (CosmosException cosmosException) when (cosmosException.StatusCode == HttpStatusCode.NotFound)
                    {
                        // delete can act as a desired-state operation so if delete is called on something not existing multiple times it shouldn't throw an error
                        if (throwIfNotFound)
                        {
                            throw;
                        }
                    }
                });
        }

        public async Task<TDocument?> GetLatestDocumentAsync<TDocument, TContract>(string partitionKey, bool isDocumentRequired = true,
            DateTimeOffset? validFromDate = null, DateTimeOffset? validToDate = null) where TDocument : DocumentContract<TContract>, new() where TContract : class
        {
            var validFromPropertyName = nameof(DocumentContract<TContract>.ValidFromDate).ToCamelCase();
            var validToPropertyName = nameof(DocumentContract<TContract>.ValidToDate).ToCamelCase();
            var partitionKeyName = nameof(DocumentContract<TContract>.PartitionKey).ToCamelCase();

            validFromDate ??= DateTimeOffset.UtcNow;
            validToDate ??= DateTimeOffset.UtcNow;

            var sqlQuery = @$"SELECT TOP 1 * FROM c WHERE LOWER(c.{partitionKeyName}) = @{partitionKeyName} and c.{validFromPropertyName} <= @{validFromPropertyName} and c.{validToPropertyName} > @{validToPropertyName} ORDER BY c.{validFromPropertyName} DESC";

            var queryDefinition = new QueryDefinition(sqlQuery);

            var preparedStatements = new Dictionary<string, string>
            {
                {$"@{partitionKeyName}", partitionKey.Trim().ToLowerInvariant()},
                {$"@{validFromPropertyName}", $"{validFromDate:yyyy-MM-ddTHH:mm:ssZ}"},
                {$"@{validToPropertyName}", $"{validToDate:yyyy-MM-ddTHH:mm:ssZ}"}
            };

            preparedStatements.ForEach(keyValuePair => queryDefinition.WithParameter(keyValuePair.Key, keyValuePair.Value));
            var queryRequestOptions = DefaultQueryRequestOptions.DeepClone<QueryRequestOptions>();
            queryRequestOptions.MaxItemCount = 1;

            var executedText = GetExecutedQuery(queryDefinition, preparedStatements);
            var items = await GetItemsAsync<TDocument>(queryDefinition, queryRequestOptions, executedText);
            var latestDocument = items.SingleOrDefault();
            if (latestDocument == null && isDocumentRequired)
            {
                throw new InvalidOperationException($"No documents found for query '{executedText}'").WithTelemetry(GetCosmosTelemetryProperties());
            }

            return latestDocument;
        }

        public async Task<TContract?> GetItemAsync<TContract>(string partitionKey, Guid id) where TContract : class
        {
            var dependencyName = $"Cosmos-{container.Id}";
            var dependencyData = new DependencyData("GetItem");
            var dependencyTarget = container.Id;

            var query = $"SELECT * FROM c WHERE c.partitionKey = '{partitionKey}'";
            if (id != Guid.Empty)
            {
                query += $" AND c.id = '{id}'";
            }

            var result = await new DependencyMeasurement(serviceProvider)
                .ForDependency(dependencyName, MeasuredDependencyType.AzureCosmosDb, dependencyData.ConvertToJson(), dependencyTarget)
                .WithContextualInformation(GetCosmosTelemetryProperties(query))
                .TrackAsync(async () =>
                {
                    try
                    {
                        return await container.ReadItemAsync<TContract>(id.ToString(), new PartitionKey(partitionKey));
                    }
                    catch (CosmosException cosmosException) when (cosmosException.StatusCode == HttpStatusCode.NotFound)
                    {
                        return null;
                    }
                });

            return result?.Resource;
        }

        public async Task<IEnumerable<TContract?>> GetItemsAsync<TContract>(QueryDefinition queryDefinition, QueryRequestOptions? queryRequestOptions = null,
            string? executedQuery = null) where TContract : class
        {
            var dependencyName = $"Cosmos-{container.Id}";
            var dependencyData = new DependencyData("GetItems");
            var dependencyTarget = container.Id;

            executedQuery ??= queryDefinition.QueryText;

            var results = await Policy.HandleResult<List<TContract>>(x => x == null)
                .Or<CosmosException>()
                .WaitAndRetryAsync(retryCount: 5, _ => TimeSpan.FromSeconds(value: 2))
                .ExecuteAsync(async () =>
                {
                    var itemList = await new DependencyMeasurement(serviceProvider)
                        .ForDependency(dependencyName, MeasuredDependencyType.AzureCosmosDb, dependencyData.ConvertToJson(), dependencyTarget)
                        .WithContextualInformation(GetCosmosTelemetryProperties(executedQuery))
                        .TrackAsync(async () =>
                        {
                            var query = container.GetItemQueryStreamIterator(queryDefinition, requestOptions: queryRequestOptions ?? DefaultQueryRequestOptions);
                            if (query == null)
                            {
                                return new List<TContract>();
                            }

                            var items = new List<TContract>();
                            while (query.HasMoreResults)
                            {
                                var page = await query.ReadNextAsync();

                                if (page.StatusCode == HttpStatusCode.BadRequest)
                                {
                                    throw new InvalidOperationException(page.ErrorMessage);
                                }

                                if (page.StatusCode < HttpStatusCode.OK || (int) page.StatusCode >= 300 || page.Content == null)
                                {
                                    continue;
                                }

                                using var jsonDocument = await JsonDocument.ParseAsync(page.Content);
                                var dataProperty = jsonDocument.RootElement.GetProperty("Documents");
                                var documentsItems = dataProperty.GetRawText();
                                var returnedItems = documentsItems.ConvertFromJson<List<TContract>>();
                                items.AddRange(returnedItems);
                            }

                            return items;
                        });

                    return itemList;
                });

            return results;
        }

        public async Task UpdateItemAsync<TDocument, TContract>(string partitionKey, TContract item) where TDocument : DocumentContract<TContract>, new() where TContract : class
        {
            var dependencyName = $"Cosmos-{container.Id}";
            var dependencyData = new DependencyData("UpdateItem");
            var dependencyTarget = container.Id;

            await new DependencyMeasurement(serviceProvider)
                .ForDependency(dependencyName, MeasuredDependencyType.AzureCosmosDb, dependencyData.ConvertToJson(), dependencyTarget)
                .WithContextualInformation(GetCosmosTelemetryProperties())
                .TrackAsync(async () => await container.UpsertItemAsync(item, new PartitionKey(partitionKey)));
        }
        
        private Dictionary<string, string> GetCosmosTelemetryProperties(string? executedQuery = null)
        {
            var telemetryProperties = new Dictionary<string, string>(executingRequestContextAdapter.GetTelemetryProperties())
            {
                {"Cosmos-Database", databaseName},
                {"Cosmos-Container", container.Id},
                {"Cosmos-Endpoint", dbClient.Endpoint.ToString()}
            };

            if (!string.IsNullOrEmpty(executedQuery))
            {
                telemetryProperties.Add("Query", executedQuery);
            }

            return telemetryProperties;
        }

        private static string GetExecutedQuery(QueryDefinition queryDefinition, Dictionary<string, string> preparedStatements)
        {
            var executedText = queryDefinition.QueryText;
            preparedStatements.ForEach(keyValuePair => executedText = executedText.Replace(keyValuePair.Key, $"'{keyValuePair.Value}'"));
            return executedText;
        }
        
    }
}