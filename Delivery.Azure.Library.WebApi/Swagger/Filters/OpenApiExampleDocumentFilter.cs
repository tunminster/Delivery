using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Delivery.Azure.Library.WebApi.Swagger.Parameterization;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Delivery.Azure.Library.WebApi.Swagger.Filters
{
    internal class OpenApiExampleDocumentFilter : IDocumentFilter
	{
		private readonly Dictionary<string, int> incrementingIntegersPerSchema = new();

		public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
		{
			ParameterizeProperties(swaggerDoc);

			AddParameterLocationExamples(swaggerDoc);

			FormatDates(swaggerDoc);
		}

		private void ParameterizeProperties(OpenApiDocument swaggerDoc)
		{
			// ReSharper disable once CollectionNeverUpdated.Local
			var openApiExampleParameters = new OpenApiExampleParameters();
			var propertyExamples = swaggerDoc.Components
				.Schemas
				.Values
				.SelectMany(openApiSchema => openApiSchema.Properties)
				.Select(p => p.Value);

			foreach (var propertyExample in propertyExamples)
			{
				UpdateExamples(propertyExample, openApiExampleParameters);
			}
		}

		private void UpdateExamples(OpenApiSchema propertyExample, OpenApiExampleParameters openApiExampleParameters)
		{
			var propertyExampleType = propertyExample.Type;
			if (propertyExample.Example is OpenApiString apiString)
			{
				var xmlDecodedValue = HttpUtility.HtmlDecode(apiString.Value);
				propertyExample.Example = new OpenApiString(xmlDecodedValue);
				UpdateOpenApiStrings(propertyExample, (OpenApiString) propertyExample.Example, propertyExampleType);
			}

			// allow dynamic date parameterization
			if (propertyExample.Format?.ToLowerInvariant() == "date-time" && !string.IsNullOrEmpty(propertyExample.Description) && propertyExample.Description.Contains(OpenApiExampleFormatter.EnclosedSubstitutionStart) && propertyExample.Description.Contains(OpenApiExampleFormatter.EnclosedSubstitutionEnd))
			{
				UpdateOpenApiDates(propertyExample, openApiExampleParameters, propertyExampleType);
			}
		}

		private static void UpdateOpenApiDates(OpenApiSchema propertyExample, OpenApiExampleParameters openApiExampleParameters, string propertyExampleType)
		{
			var key = GetKey(propertyExample.Description);
			var fullKey = $"{OpenApiExampleFormatter.EnclosedSubstitutionStart}{key}{OpenApiExampleFormatter.EnclosedSubstitutionEnd}";

			if (!openApiExampleParameters.TryGetValue(key, out var value))
			{
				throw new InvalidOperationException($"Expected to find a key {fullKey} in the document for type {propertyExampleType} with description {propertyExample.Description}");
			}

			var rawValue = value.Invoke(string.Empty, arg2: default);
			var formattedValue = DateTimeOffset.Parse(rawValue);
			propertyExample.Example = new OpenApiString(FormatDateTime(formattedValue));
			propertyExample.Description = propertyExample.Description.Replace(fullKey, string.Empty).Trim();
		}

		private void UpdateOpenApiStrings(OpenApiSchema propertyExample, OpenApiString apiString, string propertyExampleType)
		{
			var value = apiString.Value;
			if (value.Contains(OpenApiExampleFormatter.EnclosedSubstitutionStart) && value.Contains(OpenApiExampleFormatter.EnclosedSubstitutionEnd))
			{
				var key = GetKey(value);
				var fullKey = $"{OpenApiExampleFormatter.EnclosedSubstitutionStart}{key}{OpenApiExampleFormatter.EnclosedSubstitutionEnd}";
				if (key == "i")
				{
					var i = 0;
					if (incrementingIntegersPerSchema.ContainsKey(propertyExampleType))
					{
						i = incrementingIntegersPerSchema[propertyExampleType];
					}
					else
					{
						incrementingIntegersPerSchema.Add(propertyExampleType, value: 0);
					}

					propertyExample.Example = new OpenApiString(value.Replace(fullKey, i.ToString()));
					incrementingIntegersPerSchema[propertyExampleType]++;
				}
			}
		}

		private static string GetKey(string example)
		{
			var keyStart = example.IndexOf(OpenApiExampleFormatter.EnclosedSubstitutionStart, StringComparison.InvariantCultureIgnoreCase);
			var keyEnd = example.IndexOf(OpenApiExampleFormatter.EnclosedSubstitutionEnd, StringComparison.InvariantCultureIgnoreCase);
			var key = example.Substring(keyStart + OpenApiExampleFormatter.EnclosedSubstitutionStart.Length, keyEnd - keyStart - + OpenApiExampleFormatter.EnclosedSubstitutionEnd.Length);
			return key;
		}

		private static void AddParameterLocationExamples(OpenApiDocument swaggerDoc)
		{
			foreach (var openApiPathItem in swaggerDoc.Paths.Values)
			{
				foreach (var openApiParameter in openApiPathItem.Operations.SelectMany(p => p.Value.Parameters))
				{
					if (openApiParameter.In != ParameterLocation.Query && openApiParameter.In != ParameterLocation.Path)
					{
						continue;
					}

					// json will not accept a dollar sign at the start so they have to be stripped for open api
					var value = $"{{{{{openApiParameter.Name}}}}}".Replace("$", string.Empty);
					openApiParameter.Example = openApiParameter.Schema.Type?.ToLowerInvariant() switch
					{
						"string" => new OpenApiString(value),
						_ => openApiParameter.Example
					};
				}
			}
		}

		private static void FormatDates(OpenApiDocument swaggerDoc)
		{
			var propertyExamples = swaggerDoc.Components
				.Schemas
				.Values
				.SelectMany(openApiSchema => openApiSchema.Properties)
				.Select(p => p.Value);

			foreach (var propertyExample in propertyExamples)
			{
				if (propertyExample.Example is not OpenApiString openApiString)
				{
					continue;
				}

				if (DateTimeOffset.TryParse(openApiString.Value, out var dateTimeOffset))
				{
					propertyExample.Example = new OpenApiString(FormatDateTime(dateTimeOffset));
				}
			}
		}

		private static string FormatDateTime(DateTimeOffset dateTimeOffset)
		{
			return dateTimeOffset.ToString("s") + "Z";
		}
	}
}