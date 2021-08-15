using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Delivery.Azure.Library.WebApi.Swagger.Attributes;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Delivery.Azure.Library.WebApi.Swagger.Filters
{
    public class OpenApiQueryTypeOperationFilter : IOperationFilter
	{
		public void Apply(OpenApiOperation operation, OperationFilterContext context)
		{
			if (!(context.ApiDescription.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor))
			{
				return;
			}

			var swaggerQueryTypeAttribute = controllerActionDescriptor.MethodInfo.GetCustomAttribute<SwaggerQueryTypeAttribute>() ??
			                                controllerActionDescriptor.ControllerTypeInfo.GetCustomAttribute<SwaggerQueryTypeAttribute>();

			var properties = swaggerQueryTypeAttribute?.Type.GetProperties();

			if (properties == null)
			{
				return;
			}

			operation.Parameters ??= new List<OpenApiParameter>();

			properties.ToList().ForEach(info =>
			{
				var type = info.PropertyType;
				if (IsNullable(type))
				{
					type = Nullable.GetUnderlyingType(type)!;
				}

				var schemaType = ConvertTypeToString(type);
				var openApiParameter = new OpenApiParameter
				{
					Name = info.GetCustomAttribute<DisplayAttribute>()?.Name,
					Schema = new OpenApiSchema
					{
						Type = schemaType
					},
					Required = info.GetCustomAttribute<RequiredAttribute>() != null,
					In = ParameterLocation.Query,
					AllowReserved = true
				};

				var examples = GenerateExamples(info);
				if (examples?.Any() ?? false)
				{
					openApiParameter.Example = schemaType switch
					{
						"string" => new OpenApiString(examples.First().Key),
						"integer" => new OpenApiInteger(int.Parse(examples.First().Key)),
						_ => openApiParameter.Example
					};
				}

				operation.Parameters.Add(openApiParameter);
			});
		}

		private static string ConvertTypeToString(MemberInfo type)
		{
			return type.Name switch
			{
				"String" => "string",
				"Int32" => "integer",
				_ => throw new InvalidOperationException($"Type with name {type.Name} is not implemented")
			};
		}

		private static Dictionary<string, OpenApiExample>? GenerateExamples(MemberInfo info)
		{
			return info.GetCustomAttribute<DisplayAttribute>()?
				.Description?.Split("|")
				.ToDictionary(s => s.Split("=").First(), s => new OpenApiExample
				{
					Description = s.Split("=").Last()
				});
		}

		private static bool IsNullable(Type type)
		{
			return Nullable.GetUnderlyingType(type) != null;
		}
	}
}