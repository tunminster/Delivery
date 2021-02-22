using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Delivery.Azure.Library.Core.Exceptions;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Telemetry;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Delivery.Azure.Library.WebApi.ModelBinders
{
    public class JsonModelBinder : IModelBinder
	{
		public async Task BindModelAsync(ModelBindingContext bindingContext)
		{
			// Test if a value is received
			var valueProviderResult = GetValueProviderResult(bindingContext);
			if (valueProviderResult == ValueProviderResult.None)
			{
				return;
			}

			bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

			// Get the json serialized value as string
			var serialized = valueProviderResult.FirstValue;

			// Return a successful binding for empty strings or nulls
			if (string.IsNullOrEmpty(serialized))
			{
				bindingContext.Result = ModelBindingResult.Success(model: null);
				return;
			}

			try
			{
				// Deserialize json string using custom json options defined in startup, if available
				var deserialized = JsonSerializer.Deserialize(serialized, bindingContext.ModelType, JsonExtensions.GetDefaultJsonSerializerOptions());
				if (deserialized == null)
				{
					throw new InvalidOperationException($"Expected to find a deserialized object given that there was an input: {serialized}");
				}

				// Run data annotation validation to validate properties and fields on deserialized model
				var validationResultProps = from property in TypeDescriptor.GetProperties(deserialized).Cast<PropertyDescriptor>()
					from attribute in property.Attributes.OfType<ValidationAttribute>()
					where !attribute.IsValid(property.GetValue(deserialized))
					select new
					{
						Member = property.Name,
						ErrorMessage = attribute.FormatErrorMessage(string.Empty)
					};

				var validationResultFields = from field in TypeDescriptor.GetReflectionType(deserialized).GetFields()
					from attribute in field.GetCustomAttributes<ValidationAttribute>()
					where !attribute.IsValid(field.GetValue(deserialized))
					select new
					{
						Member = field.Name,
						ErrorMessage = attribute.FormatErrorMessage(string.Empty)
					};

				// Add the validation results to the model state
				var errors = validationResultFields.Concat(validationResultProps);
				foreach (var validationResultItem in errors)
				{
					bindingContext.ModelState.AddModelError(validationResultItem.Member, validationResultItem.ErrorMessage);
				}

				// Set successful binding result
				bindingContext.Result = ModelBindingResult.Success(deserialized);
				bindingContext.HttpContext.Items[HttpTelemetryWriter.ModelBindingContentKey] = serialized;
				bindingContext.BindingSource = BindingSource.Body;

				await Task.CompletedTask;
			}
			catch (JsonException jsonException)
			{
				throw new ValidationSerializationException(jsonException.Message, jsonException);
			}
		}

		private static ValueProviderResult GetValueProviderResult(ModelBindingContext bindingContext)
		{
			if (bindingContext is null)
			{
				throw new ArgumentNullException(nameof(bindingContext));
			}

			// Test if a value is received
			var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
			return valueProviderResult;
		}
	}
}