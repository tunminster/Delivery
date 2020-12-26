using System;
using System.Linq;
using FluentValidation;
using FluentValidation.Internal;

namespace Delivery.Azure.Library.Contracts.Extensions
{
    public static class FluentValidationExtensions
    {
        /// <summary>
        ///  Validation with the allowed special characters
        /// </summary>
        /// <param name="ruleBuilder"></param>
        /// <param name="placeHolderTranslationMessage"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IRuleBuilderOptions<T, string> WithValidCharacters<T>(this IRuleBuilder<T, string?> ruleBuilder, string placeHolderTranslationMessage)
        {
            // the following special characters are not allowed
            const string blacklistedSpecialCharacters = "{}[]<>";

            var ruleBuilderTyped = (RuleBuilder<T, string>) ruleBuilder;
            var memberName = ruleBuilderTyped.Rule.PropertyName;
            var message = placeHolderTranslationMessage;

            var ruleBuilderOptions = ruleBuilder.Must(inputString =>
            {
                message = string.Format(placeHolderTranslationMessage, memberName, blacklistedSpecialCharacters, inputString);

                if (string.IsNullOrEmpty(inputString))
                {
                    return true;
                }

                var validatedString = new string(inputString.Where(c =>
                    !blacklistedSpecialCharacters.Contains(c)).ToArray());

                return validatedString == inputString;
            });

            return message == placeHolderTranslationMessage ? ruleBuilderOptions : ruleBuilderOptions.WithMessage(message);
        }

        /// <summary>
        ///     Validates that the enum is not equal to the default value, unless it is null.
        /// </summary>
        public static IRuleBuilderOptions<T, TEnum> WithValidEnum<T, TEnum>(this IRuleBuilder<T, TEnum> ruleBuilder, string message = null, string? placeHolderTranslationMessage = null)
            where TEnum : struct, Enum
        {
            if (placeHolderTranslationMessage != null)
            {
                var ruleBuilderTyped = (RuleBuilder<T, TEnum>) ruleBuilder;
                var ruleMember = ruleBuilderTyped.Rule.Member.Name;

                message = string.Format(placeHolderTranslationMessage, ruleMember);
            }

            var ruleBuilderOptions = ruleBuilder.Must(p => !p.Equals(default(TEnum)));

            return !string.IsNullOrEmpty(message) ? ruleBuilderOptions : ruleBuilderOptions.WithMessage(message);
        }
    }
}