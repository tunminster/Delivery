using Delivery.Domain.Documentation;

namespace Delivery.Driver.Domain.Documentation
{
    public static class DriverApiDocumentation
    {
        public static string GetApiGeneralDocumentationMarkdown(string environment, string category) => @$"
{GenericApiDocumentation.GetApiGeneralAboutDocumentationMarkdown(category)}

{GenericApiDocumentation.GetBoilerplateDocumentationMarkdown(environment, category)}
		";
    }
}