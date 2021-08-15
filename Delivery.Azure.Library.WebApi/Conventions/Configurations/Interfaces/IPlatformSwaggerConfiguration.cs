using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Delivery.Azure.Library.WebApi.Swagger;

namespace Delivery.Azure.Library.WebApi.Conventions.Configurations.Interfaces
{
    public interface IPlatformSwaggerConfiguration
    {
        /// <summary>
        ///     Adds a route serving a Swagger specification with all operations.
        /// </summary>
        /// <param name="additionalTypes">The additional types to add.</param>
        /// <param name="description">The API description.</param>
        void AddRouteWithAllOperations(IEnumerable<Type>? additionalTypes = null, string? description = null);

        /// <summary>
        ///     The api description the swagger document will produce
        /// </summary>
        string Description { get; }

        /// <summary>
        ///     The paths to the api documentation that will be used to produce the swagger document
        /// </summary>
        string[] DocumentationPaths { get; }

        /// <summary>
        ///     The api title the swagger document will produce
        /// </summary>
        string Title { get; }

        /// <summary>
        ///     The Swagger routes.
        /// </summary>
        ReadOnlyCollection<SwaggerRoute> Routes { get; }

        List<string> ApisToSortBySummary { get; }
    }
}