using System;
using System.Globalization;

namespace Delivery.Azure.Library.WebApi.Swagger.Parameterization
{
    public static class OpenApiExampleFormatter
    {
        public static readonly string EnclosedSubstitutionStart = "#{";
        public static readonly string EnclosedSubstitutionEnd = "}#";

        public static string ReplaceEnclosedPrimitives(string key, string value, string jsonDocument)
        {
            var substitutionKey = EnclosedSubstitutionStart + key + EnclosedSubstitutionEnd;

            if (substitutionKey.Contains(":bool"))
            {
                jsonDocument = jsonDocument.Replace($"\"{substitutionKey}\"", bool.Parse(value).ToString().ToLowerInvariant());
            }
            else if (substitutionKey.Contains(":number"))
            {
                jsonDocument = jsonDocument.Replace($"\"{substitutionKey}\"", double.Parse(value).ToString(CultureInfo.InvariantCulture));
            }
            else if (substitutionKey.Contains(":date"))
            {
                jsonDocument = jsonDocument.Replace($"{substitutionKey}", DateTimeOffset.Parse(value).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"));
            }
            else if (substitutionKey.Contains(":object"))
            {
                jsonDocument = jsonDocument.Replace($"\"{substitutionKey}\"", value);
            }
            else
            {
                jsonDocument = jsonDocument.Replace(substitutionKey, value);
            }

            return jsonDocument;
        }
    }
}