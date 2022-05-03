using Delivery.Domain.Documentation;

namespace Delivery.Customer.Domain.Documentation
{
    public static class WebAppApiDocumentation
    {
        public static string GetApiGeneralDocumentationMarkdown(string environment, string category) => @$"
{GenericApiDocumentation.GetApiGeneralAboutDocumentationMarkdown(category)}

{GenericApiDocumentation.GetBoilerplateDocumentationMarkdown(environment, category)}
		";
    }
}