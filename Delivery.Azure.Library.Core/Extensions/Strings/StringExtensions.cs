using System;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Delivery.Azure.Library.Core.Extensions.Strings
{
    /// <summary>
	///     Extends strings with some conveniences
	/// </summary>
	public static class StringExtensions
	{
		/// <summary>
		///     Convert the string representation of an enum value to the enum value itself
		///     If the integer value is provided this will also work
		/// </summary>
		/// <typeparam name="TEnum">Enum type</typeparam>
		/// <param name="enumString">enum value to convert</param>
		/// <returns>the enum value</returns>
		public static TEnum ConvertToEnum<TEnum>(this string enumString) where TEnum : struct
		{
			if (Enum.TryParse<TEnum>(enumString, ignoreCase: true, out var enumValue))
			{
				return enumValue;
			}

			throw new InvalidOperationException($"Unable to cast {enumString} to type of {nameof(TEnum)}");
		}

		/// <summary>
		///     Converts a string to camelcase
		/// </summary>
		public static string ToCamelCase(this string target)
		{
			if (target.Length == 0)
			{
				return target;
			}

			var camelCase = $"{char.ToLowerInvariant(target[index: 0])}{target.Substring(startIndex: 1)}";
			return camelCase;
		}

		/// <summary>
		///     Converts a string to pascalcase
		/// </summary>
		public static string ToPascalCase(this string target)
		{
			if (target.Length == 0)
			{
				return target;
			}

			var camelCase = $"{char.ToUpperInvariant(target[index: 0])}{target.Substring(startIndex: 1)}";
			return camelCase;
		}

		/// <summary>
		///     Compare two strings
		/// </summary>
		/// <param name="a">String a to compare</param>
		/// <param name="b">String b to compare</param>
		/// <param name="trimAndInvariantCultureIgnoreCase">Set to true by default</param>
		/// <returns></returns>
		public static bool AreEqual(this string? a, string? b, bool trimAndInvariantCultureIgnoreCase = true)
		{
			return trimAndInvariantCultureIgnoreCase ? string.Equals(a?.Trim(), b?.Trim(), StringComparison.InvariantCultureIgnoreCase) : string.Equals(a, b);
		}

		/// <summary>
		///     Removes whitespaces in a string
		/// </summary>
		public static string RemoveWhitespaces(this string target)
		{
			if (target.Length == 0)
			{
				return target;
			}

			if (target == " ")
			{
				return string.Empty;
			}

			var chars = target.Where(c => !char.IsWhiteSpace(c)).ToArray();

			return new string(chars);
		}

		/// <summary>
		///     Adds spaces before capital letters in a string
		/// </summary>
		public static string ToSpacedString(this string target)
		{
			if (target.Length == 0)
			{
				return target;
			}

			if (target == " ")
			{
				return string.Empty;
			}

			var updatedString = Regex.Replace(target, "([a-z])([A-Z])", "$1 $2");

			return updatedString;
		}

		/// <summary>
		///     Checks to see if a string validates as an email address
		/// </summary>
		public static bool IsEmailAddressValid(this string emailAddress)
		{
			try
			{
				var mailAddress = new MailAddress(emailAddress);
				return mailAddress.Address == emailAddress;
			}
			catch
			{
				return false;
			}
		}
	}
}