using System;
using System.Collections.Generic;
using System.Linq;

namespace Delivery.Azure.Library.WebApi.Swagger
{
    /// <summary>
	///     Describes a route which serves a Swagger specification.
	/// </summary>
	public class SwaggerRoute
	{
		private SwaggerRoute(string? title, string? category, string? routeSegment, string? pathPrefix, IEnumerable<Type>? additionalTypes, string? description)
		{
			TitleSuffix = title;
			Category = category;
			RouteSegment = routeSegment;
			PathPrefix = pathPrefix;
			AdditionalTypes = additionalTypes?.ToList() ?? new List<Type>();
			Description = description;
		}

		/// <summary>
		///     Creates a route which serves a Swagger specification with all routes.
		/// </summary>
		/// <param name="additionalTypes">The additional types to add.</param>
		/// <param name="description">The API description.</param>
		/// <returns>The route.</returns>
		public static SwaggerRoute WithAllOperations(IEnumerable<Type>? additionalTypes, string? description = null)
		{
			return new(title: null, category: null, routeSegment: null, pathPrefix: null, additionalTypes, description);
		}

		/// <summary>
		///     Creates a route which serves a Swagger specification with all routes.
		/// </summary>
		/// <param name="titleSuffix">The title.</param>
		/// <param name="additionalTypes">The additional types to add.</param>
		/// <param name="description">The API description.</param>
		/// <returns>The route.</returns>
		public static SwaggerRoute WithAllOperations(string titleSuffix, IEnumerable<Type>? additionalTypes, string? description = null)
		{
			return new(titleSuffix, category: null, routeSegment: null, pathPrefix: null, additionalTypes, description);
		}

		/// <summary>
		///     Creates a route serving a Swagger specification operations in the given category.
		/// </summary>
		/// <param name="category">The category.</param>
		/// <param name="swaggerRouteSegment">The route segment.</param>
		/// <param name="additionalTypes">The additional types to add.</param>
		/// <param name="description">The API description.</param>
		/// <returns>The route.</returns>
		public static SwaggerRoute WithCategoryFilter(string category, string swaggerRouteSegment, IEnumerable<Type>? additionalTypes, string? description = null)
		{
			return new(title: null, category, swaggerRouteSegment, pathPrefix: null, additionalTypes, description);
		}

		/// <summary>
		///     Creates a route serving a Swagger specification operations in the given category.
		/// </summary>
		/// <param name="titleSuffix">The title suffix.</param>
		/// <param name="category">The category.</param>
		/// <param name="swaggerRouteSegment">The route segment.</param>
		/// <param name="additionalTypes">The additional types to add.</param>
		/// <param name="description">The API description.</param>
		/// <returns>The route.</returns>
		public static SwaggerRoute WithCategoryFilter(string titleSuffix, string category, string swaggerRouteSegment, IEnumerable<Type>? additionalTypes, string? description = null)
		{
			return new(titleSuffix, category, swaggerRouteSegment, pathPrefix: null, additionalTypes, description);
		}

		/// <summary>
		///     Creates a route serving a Swagger specification operations in the given category.
		/// </summary>
		/// <param name="category">The category.</param>
		/// <param name="swaggerRouteSegment">The route segment.</param>
		/// <param name="removedPathPrefix">The path prefix.</param>
		/// <param name="additionalTypes">The additional types to add.</param>
		/// <param name="description">The API description.</param>
		/// <returns>The route.</returns>
		public static SwaggerRoute WithCategoryFilterAndPathTransformation(string category, string swaggerRouteSegment, string removedPathPrefix, IEnumerable<Type>? additionalTypes, string? description = null)
		{
			return new(title: null, category, swaggerRouteSegment, removedPathPrefix, additionalTypes, description);
		}

		/// <summary>
		///     Creates a route serving a Swagger specification operations in the given category.
		/// </summary>
		/// <param name="titleSuffix">The title suffix.</param>
		/// <param name="category">The category.</param>
		/// <param name="swaggerRouteSegment">The route segment.</param>
		/// <param name="removedPathPrefix">The path prefix.</param>
		/// <param name="additionalTypes">The additional types to add.</param>
		/// <param name="description">The API description.</param>
		/// <returns>The route.</returns>
		public static SwaggerRoute WithCategoryFilterAndPathTransformation(string titleSuffix, string category, string swaggerRouteSegment, string removedPathPrefix, IEnumerable<Type>? additionalTypes, string? description = null)
		{
			return new(titleSuffix, category, swaggerRouteSegment, removedPathPrefix, additionalTypes, description);
		}

		/// <summary>
		///     Gets the route title suffix.
		/// </summary>
		public virtual string? TitleSuffix { get; }

		/// <summary>
		///     Gets the category filter.
		/// </summary>
		public virtual string? Category { get; }

		/// <summary>
		///     Gets the route segment to be used in the Swagger specification route.
		/// </summary>
		public string? RouteSegment { get; }

		/// <summary>
		///     Gets the path prefix which will be removed from the operations and added to as basePath.
		/// </summary>
		public virtual string? PathPrefix { get; }

		/// <summary>
		///     The additional schema types.
		/// </summary>
		public virtual List<Type> AdditionalTypes { get; } = new();

		/// <summary>
		///     The description (null/empty to use default).
		/// </summary>
		public string? Description { get; }
	}
}