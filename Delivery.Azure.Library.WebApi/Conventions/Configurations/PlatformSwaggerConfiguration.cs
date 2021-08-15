using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.WebApi.Conventions.Configurations.Interfaces;
using Delivery.Azure.Library.WebApi.Swagger;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Azure.Library.WebApi.Conventions.Configurations
{
    public class PlatformSwaggerConfiguration : IPlatformSwaggerConfiguration
	{
		public IServiceProvider ServiceProvider { get; }
		private readonly List<SwaggerRoute> routes = new();
		private readonly List<string> documentationPaths = new();
		private readonly List<string> apisToSortBySummary = new();

		public PlatformSwaggerConfiguration(IServiceProvider serviceProvider)
		{
			ServiceProvider = serviceProvider;
		}

		/// <summary>
		///     Adds the XML documentation file for a given assembly.
		/// </summary>
		/// <param name="documentationPath">The XML documentation file path.</param>
		/// <exception cref="FileNotFoundException">The xml documentation file could not be found.</exception>
		public void AddXmlDocumentationPath(string documentationPath)
		{
			if (File.Exists(documentationPath))
			{
				documentationPaths.Add(documentationPath);
			}
			else
			{
				throw new FileNotFoundException($"The xml documentation file '{documentationPath}'could not be found for assembly.");
			}
		}

		/// <summary>
		///     Adds the XML documentation file for a given assembly.
		/// </summary>
		/// <param name="assembly">The assembly to load the XML documentation file for.</param>
		/// <exception cref="FileNotFoundException">The xml documentation file could not be found.</exception>
		public void AddXmlDocumentationPath(Assembly assembly)
		{
			var documentationPath = assembly.Location.Replace(".dll", ".xml");
			AddXmlDocumentationPath(documentationPath);
		}

		/// <summary>
		///     Adds a route serving a Swagger specification with all operations.
		/// </summary>
		/// ß
		/// <param name="additionalTypes">The additional types to add.</param>
		/// <param name="description">The API description.</param>
		public void AddRouteWithAllOperations(IEnumerable<Type>? additionalTypes = null, string? description = null)
		{
			routes.Add(SwaggerRoute.WithAllOperations(additionalTypes, description));
		}

		/// <summary>
		///     Adds a route serving a Swagger specification with operations in the given category and path transformation
		///     (i.e. remove <code>pathPrefix</code> from paths and add it to <code>baseUrl</code>).
		/// </summary>
		/// <param name="titleSuffix">The title suffix of the route.</param>
		/// <param name="category">The category.</param>
		/// <param name="swaggerRouteSegment">The route segment.</param>
		/// <param name="removedPathPrefix">The path prefix to remove from the operation paths and set on the baseUrl.</param>
		/// <param name="sortBySummary"></param>
		/// <param name="additionalTypes">The additional types to add.</param>
		/// <param name="description">The API description.</param>
		/// ß
		public void AddRouteWithCategoryFilterAndPathTransformation(string titleSuffix, string category, string swaggerRouteSegment, string removedPathPrefix, bool sortBySummary, IEnumerable<Type>? additionalTypes = null, string? description = null)
		{
			routes.Add(SwaggerRoute.WithCategoryFilterAndPathTransformation(titleSuffix, category, swaggerRouteSegment, removedPathPrefix, additionalTypes, description));
			if (sortBySummary)
			{
				apisToSortBySummary.Add(category);
			}
		}

		/// <summary>
		///     The api description the swagger document will produce
		/// </summary>
		public virtual string Description => ServiceProvider.GetRequiredService<IConfigurationProvider>().GetSetting("Environment_ApplicationDescription", isMandatory: false);

		/// <summary>
		///     The paths to the api documentation that will be used to produce the swagger document
		/// </summary>
		public virtual string[] DocumentationPaths => documentationPaths.ToArray();

		/// <summary>
		///     The api title the swagger document will produce
		/// </summary>
		public virtual string Title => ServiceProvider.GetRequiredService<IConfigurationProvider>().GetSetting("Environment_ApplicationDisplayName", isMandatory: false);

		/// <summary>
		///     The Swagger routes.
		/// </summary>
		public virtual ReadOnlyCollection<SwaggerRoute> Routes => routes.AsReadOnly();

		public virtual List<string> ApisToSortBySummary => apisToSortBySummary;
	}
}